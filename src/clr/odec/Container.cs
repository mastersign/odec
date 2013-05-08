using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.Properties;
using de.mastersign.odec.crypto;
using de.mastersign.odec.model;
using de.mastersign.odec.storage;
using de.mastersign.odec.utils;

namespace de.mastersign.odec
{
    /// <summary>
    /// Represents a forensic container.
    /// </summary>
    public partial class Container : IDisposable
    {
        private const int EDITION_SALT_LENGTH = 128; // in bytes -> 1024 bits

        #region Fields

        private readonly CompatibilityFlags compatibilityFlags;

        private readonly IStorage storage;

        private ProcessPhase phase;

        private EditionElement currentEditionE;
        private HistoryElement historyE;
        private IndexElement indexE;

        private IRSAProvider signingProvider;
        private IRSAProvider verifyingProvider;

        private readonly List<Entity> addedEntities = new List<Entity>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="storage">The storage for the container.</param>
        /// <param name="compatibilityFlags">The compatibility flags for the container.</param>
        private Container(IStorage storage, CompatibilityFlags compatibilityFlags)
        {
            this.compatibilityFlags = compatibilityFlags;
            this.storage = storage;
            this.phase = ProcessPhase.Empty;
        }

        #region Internal

        internal IRSAProvider SigningRSAProvider { get { return signingProvider; } }

        internal IRSAProvider VerifyingRSAProvider { get { return verifyingProvider; } }

        internal EditionElement GetEditionForEntity(int id)
        {
            if (CurrentEdition.NewEntities.Contains(id)) return CurrentEdition;
            var r =
                from item in historyE.Items
                from newEntityId in item.Edition.NewEntities
                where newEntityId == id
                select item.Edition;
            return r.FirstOrDefault();
        }

        internal void SetEntitySignatureForIndex(Entity entity, SignatureWrapper wrapper)
        {
            var indexItem = indexE.GetItem(entity.Id);
            indexItem.EntitySignature = wrapper;
        }

        internal SignatureWrapper SerializeAndSign(IXmlStorable2 value, string docElementName, string relativePath)
        {
            var buffer = SerializeXmlStorable2(value, docElementName);

            StoreSerializedXml(relativePath, buffer);

            // create a signature for the serialized XML element
            using (var ms = new MemoryStream(buffer, false))
            {
                Stream stream;
                // if compatibility dictates to use binary data of XML document
                if (CompatibilityFlags.SuppressStructureXmlCanonicalization)
                {
                    stream = ms;
                }
                else // if canonicalization of XML document is allowed
                {
                    var doc = new XmlDocument();
                    doc.Load(ms);
                    stream = Configuration.Canonicalizer.Canonize(doc.DocumentElement);
                }

                var signWrapper = new SignatureWrapper();
                signWrapper.BuildSignature(
                    Path.GetFileName(relativePath),
                    stream,
                    Configuration.HashProvider, SigningRSAProvider, Configuration.Canonicalizer);

                if (!CompatibilityFlags.SuppressStructureXmlCanonicalization)
                {
                    stream.Dispose();
                }

                StoreSignature(relativePath, signWrapper);

                return signWrapper;
            }
        }

        internal void SerializeWithOldSignature(IXmlStorable2 element, string docElementName, string relativePath, SignatureWrapper oldSignWrapper)
        {
            // Write the serialized XML element to the container storage
            var buffer = SerializeXmlStorable2(element, docElementName);
            StoreSerializedXml(relativePath, buffer);

            // Write the signature to the container storage
            StoreSignature(relativePath, oldSignWrapper);
        }

        private void StoreSerializedXml(string relativePath, byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer, false))
            {
                Storage.Write(relativePath, ms);
            }
        }

        private byte[] SerializeXmlStorable2(IXmlStorable2 value, string docElementName)
        {
            byte[] signatureBuffer;
            using (var ms = new MemoryStream())
            {
                var ws = new XmlWriterSettings { Indent = true, IndentChars = "  " };
                using (var w = XmlWriter.Create(ms, ws))
                {
                    w.WriteStartDocument();
                    w.WriteObject(docElementName, Model.ContainerNamespace,
                                  value, CompatibilityFlags.WriteXmlSignatureCanonicalized);
                    w.WriteEndDocument();
                }
                signatureBuffer = ms.ToArray();
            }
            return signatureBuffer;
        }

        private void StoreSignature(string relativePath, SignatureWrapper signWrapper)
        {
            using (var sigStream = new MemoryStream())
            {
                signWrapper.WriteToStream(sigStream, CompatibilityFlags.WriteXmlSignatureCanonicalized);
                sigStream.Position = 0L;
                Storage.Write(relativePath + ".sig", sigStream);
            }
        }

        internal void Deserialize(IXmlStorable value, string relativePath)
        {
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            using (var src = Storage.Read(relativePath))
            {
                doc.Load(src);
            }
            value.ReadFromXml(doc.DocumentElement);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Loads and validates the container.
        /// </summary>
        /// <returns><c>true</c> if the validation suceeds; otherwise <c>false</c>.</returns>
        private bool LoadAndValidate(ValidationHandler messageHandler)
        {
            var result = true;
            var editionOK = true;
            if (Storage.Exists(EDITION_FILE))
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure,
                    Resources.Container_LoadAndValidate_CurrentEditionFound, EDITION_FILE);

                var editionDoc = TryLoadXml(EDITION_FILE, messageHandler);
                if (ValidateXml(editionDoc, EDITION_FILE, messageHandler))
                {
                    currentEditionE = new EditionElement();
                    currentEditionE.ReadFromXml(editionDoc.DocumentElement);

                    if (currentEditionE.Validate("Current", messageHandler))
                    {
                        verifyingProvider = Configuration.CryptoFactory.CreateRSAProviderFromPemEncodedCertificate(
                            currentEditionE.Owner.X509Certificate);

                        if (!ValidateSignature(EDITION_FILE,
                            !CompatibilityFlags.SuppressStructureXmlCanonicalization,
                            true, VerifyingRSAProvider, messageHandler))
                        {
                            result = false;
                        }
                    }
                    else editionOK = false;
                }
                else editionOK = false;
            }
            else
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.Container_LoadAndValidate_CurrentEditionIsMissing, EDITION_FILE);
                editionOK = false;
            }
            if (!editionOK)
            {
                messageHandler.Error(ValidationMessageClass.Global,
                    Resources.Container_LoadAndValidate_MasterSignatureCanNotBeVerified);
                result = false;
            }

            if (VerifyingRSAProvider == null)
            {
                messageHandler.Error(ValidationMessageClass.Certificate,
                    Resources.Container_LoadAndValidate_NoCertificateFound);
                phase = ProcessPhase.Sealed;
                return false;
            }

            if (Storage.Exists(HISTORY_FILE))
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure,
                    Resources.Container_LoadAndValidate_ContainerHistoryFound, HISTORY_FILE);

                if (!ValidateDoubleSignature(HISTORY_FILE,
                    currentEditionE != null ? currentEditionE.HistorySignature : null,
                    !CompatibilityFlags.SuppressStructureXmlCanonicalization,
                    true, VerifyingRSAProvider, messageHandler))
                {
                    result = false;
                }

                var historyDoc = TryLoadXml(HISTORY_FILE, messageHandler);
                if (ValidateXml(historyDoc, HISTORY_FILE, messageHandler))
                {
                    if (historyDoc.DocumentElement.Name == HistoryElement.XML_NAME &&
                        historyDoc.DocumentElement.NamespaceURI == Model.ContainerNamespace)
                    {
                        historyE = new HistoryElement();
                        historyE.ReadFromXml(historyDoc.DocumentElement);

                        if (!historyE.Validate(messageHandler)) result = false;
                    }
                    else
                    {
                        messageHandler.Error(ValidationMessageClass.ContainerStructure,
                            Resources.Container_LoadAndValidate_HistoryElementNotFound);
                        result = false;
                    }
                }
                else result = false;
            }
            else
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.Container_LoadAndValidate_ContainerHistoryMissing, HISTORY_FILE);
                result = false;
            }

            if (Storage.Exists(INDEX_FILE))
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure,
                    Resources.Container_LoadAndValidate_EntityIndexFound, INDEX_FILE);

                if (!ValidateDoubleSignature(INDEX_FILE,
                    currentEditionE != null ? currentEditionE.IndexSignature : null,
                    !CompatibilityFlags.SuppressStructureXmlCanonicalization,
                    true, VerifyingRSAProvider, messageHandler))
                {
                    result = false;
                }

                var indexDoc = TryLoadXml(INDEX_FILE, messageHandler);
                if (ValidateXml(indexDoc, INDEX_FILE, messageHandler))
                {
                    if (indexDoc.DocumentElement.Name == IndexElement.XML_NAME &&
                        indexDoc.DocumentElement.NamespaceURI == Model.ContainerNamespace)
                    {
                        indexE = new IndexElement();
                        indexE.ReadFromXml(indexDoc.DocumentElement);

                        if (!indexE.Validate(messageHandler) ||
                            !ValidateEntities(messageHandler))
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        messageHandler.Error(ValidationMessageClass.ContainerStructure,
                            Resources.Container_LoadAndValidate_IndexElementNotFound);
                        result = false;
                    }
                }
                else result = false;
            }
            else
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.Container_LoadAndValidate_EntityIndexMissing, INDEX_FILE);
                result = false;
            }

            phase = ProcessPhase.Sealed;

            return result;
        }

        /// <summary>
        /// Validates the double signature for a data file.
        /// </summary>
        /// <param name="targetFile">The target file.</param>
        /// <param name="copy">The copy of the signature.</param>
        /// <param name="xmlCanonicalize">If set to <c>true</c> the target file will be loaded as XML and canonicalized.</param>
        /// <param name="validateDigest">if set to <c>true</c> the digest will be validated; otherwise not.</param>
        /// <param name="verifier">The verifying RSA provider.
        /// This provider needs access to the certificate of the associated owner.</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns>
        /// 	<c>true</c> if the signature is valid; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// A double signature consists of a *.sig file with a XML signature for the target file
        /// and a copy of the signature in the manner of a <see cref="SignatureWrapper"/> object.
        /// </remarks>
        internal bool ValidateDoubleSignature(string targetFile, SignatureWrapper copy,
            bool xmlCanonicalize, bool validateDigest,
            IRSAProvider verifier, ValidationHandler messageHandler)
        {
            var result = true;
            var signatureFile = targetFile + ".sig";

            SignatureWrapper original = null;
            SignatureWrapper sig;
            if (Storage.Exists(signatureFile))
            {
                original = new SignatureWrapper();
                using (var signatureStream = Storage.Read(signatureFile))
                {
                    original.ReadFromStream(signatureStream);
                }
                sig = original;
            }
            else
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.Container_ValidateDoubleSignature_SignatureIsMissing, targetFile);
                result = false;

                sig = copy;
            }

            if (copy == null)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.Container_ValidateDoubleSignature_CopyOfSignatureIsMissing, targetFile);
                result = false;
            }

            if (sig == null) return false;

            if (original != null && copy != null &&
                !original.Equals(copy))
            {
                messageHandler.Error(ValidationMessageClass.Signature,
                    Resources.Container_ValidateDoubleSignature_InvalidSignatureCopy, targetFile);
                result = false;
            }

            if (!ValidateSignature(sig, targetFile, xmlCanonicalize, validateDigest, verifier, messageHandler))
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Validates all entities in the entity index.
        /// </summary>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns><c>true</c> if all entities are valid; otherwise <c>false</c>.</returns>
        private bool ValidateEntities(ValidationHandler messageHandler)
        {
            var result = true;
            foreach (var item in indexE.Items)
            {
                var entityElement = new EntityElement();
                entityElement.Id = item.Id;
                var entity = new Entity(this, entityElement, false);

                if (Storage.Exists(entity.EntityHeaderPath))
                {
                    messageHandler.Success(ValidationMessageClass.ContainerStructure,
                        Resources.Container_ValidateEntities_EntityHeaderFound, item.Id);

                    var entityHeaderDoc = TryLoadXml(entity.EntityHeaderPath, messageHandler);
                    if (ValidateXml(entityHeaderDoc, entity.EntityHeaderPath, messageHandler))
                    {
                        if (!ValidateSignature(entity.EntityHeaderPath,
                            !CompatibilityFlags.SuppressStructureXmlCanonicalization,
                            true,
                            entity.VerifyingRSAProvider, messageHandler))
                        {
                            result = false;
                        }

                        Deserialize(entityElement, entity.EntityHeaderPath);

                        if (entity.Label != item.Label)
                        {
                            messageHandler.Error(ValidationMessageClass.ContainerStructure,
                                Resources.Container_ValidateEntities_EntityLabelDiffersFromIndexItem,
                                entity.Label, item.Label);
                            result = false;
                        }

                        if (!entity.ValidateStructure(messageHandler)) result = false;
                    }
                    else result = false;
                }
                else
                {
                    messageHandler.Error(ValidationMessageClass.ContainerStructure,
                        Resources.Container_ValidateEntities_EntityHeaderIsMissing, item.Id);
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Validates a signature for a data file.
        /// </summary>
        /// <param name="sigWrapper">The <see cref="SignatureWrapper"/> with the signature to validate.</param>
        /// <param name="targetFile">The target file.</param>
        /// <param name="xmlCanonicalize">If set to <c>true</c> the target file will be loaded as XML and canonicalized.</param>
        /// <param name="validateDigest">if set to <c>true</c> the digest will be validated; otherwise not.</param>
        /// <param name="verifier">The verifying RSA provider.
        /// This provider needs access to the certificate of the associated owner.</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns>
        /// 	<c>true</c> if the signature is valid; otherwise <c>false</c>.
        /// </returns>
        internal bool ValidateSignature(SignatureWrapper sigWrapper, string targetFile,
            bool xmlCanonicalize, bool validateDigest,
            IRSAProvider verifier, ValidationHandler messageHandler)
        {
            var resultAuth = sigWrapper.VerifySignature(
                Configuration.HashProvider, verifier,
                Configuration.Canonicalizer, targetFile, messageHandler);

            bool resultInt = true;
            if (validateDigest)
            {
                using (var targetStream = Storage.Read(targetFile))
                {
                    if (xmlCanonicalize)
                    {
                        var doc = new XmlDocument();
                        doc.Load(targetStream);

                        using (var canonicalizedTarget = Configuration.Canonicalizer.Canonize(doc.DocumentElement))
                        {
                            resultInt = sigWrapper.VerifyDigest(
                                Path.GetFileName(targetFile), canonicalizedTarget,
                                Configuration.HashProvider, targetFile, messageHandler);
                        }
                    }
                    else
                    {
                        resultInt = sigWrapper.VerifyDigest(
                            Path.GetFileName(targetFile), targetStream, Configuration.HashProvider,
                            targetFile, messageHandler);
                    }
                }
            }

            return resultAuth && resultInt;
        }

        /// <summary>
        /// Validates a signature for a data file.
        /// </summary>
        /// <param name="targetFile">The target file.</param>
        /// <param name="xmlCanonicalize">If set to <c>true</c> the target data
        /// will be loaded as XML and canonicalized.</param>
        /// <param name="validateDigest">if set to <c>true</c> the digest will be validated; otherwise not.</param>
        /// <param name="verifier">The verifying RSA provider.
        /// This provider needs access to the certificate of the associated owner.</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns>
        /// 	<c>true</c> if the signature is valid; otherwise <c>false</c>.
        /// </returns>
        internal bool ValidateSignature(string targetFile, bool xmlCanonicalize, bool validateDigest,
            IRSAProvider verifier, ValidationHandler messageHandler)
        {
            var sigWrapper = new SignatureWrapper();
            using (var signatureStream = Storage.Read(targetFile + ".sig"))
            {
                sigWrapper.ReadFromStream(signatureStream);
            }
            return ValidateSignature(sigWrapper, targetFile, validateDigest,
                xmlCanonicalize, verifier, messageHandler);
        }

        /// <summary>
        /// Validates the certificate from current owner and all history owners of the container.
        /// </summary>
        /// <param name="caDirectory">A directory with trusted certification authorities.</param>
        /// <param name="rules">Certificate validation rules.</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns>
        ///   <c>true</c> if the validation suceeds; otherwise <c>false</c>.
        /// </returns>
        public bool ValidateCertificates(CertificationAuthorityDirectory caDirectory, CertificateValidationRules rules, ValidationHandler messageHandler)
        {
            var result = true;
            var currentCert = Configuration.CryptoFactory.CreateRSAProviderFromPemEncodedCertificate(currentEditionE.Owner.X509Certificate);
            if (!currentCert.ValidateCertificate(caDirectory, rules, CurrentEdition.Timestamp, messageHandler))
            {
                result = false;
            }
            foreach (var item in historyE.Items)
            {
                var cert = Configuration.CryptoFactory.CreateRSAProviderFromPemEncodedCertificate(item.Edition.Owner.X509Certificate);
                if (!cert.ValidateCertificate(caDirectory, rules, item.Edition.Timestamp, messageHandler))
                {
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks the signatures of the entity values.
        /// </summary>
        /// <remarks>
        /// <see cref="CurrentProcessPhase"/> must be <see cref="ProcessPhase.Sealed"/> to call this method.
        /// </remarks>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns><c>true</c> if the entity values are integer; otherwise <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Is throw, if <see cref="CurrentProcessPhase"/> is not <see cref="ProcessPhase.Sealed"/>.</exception>
        public bool VerifyEntityValueSignatures(ValidationHandler messageHandler)
        {
            if (phase != ProcessPhase.Sealed)
            {
                throw new InvalidOperationException(
                    Resources.Container_VerifyEntityValueSignatures_InvalidOperation_ContainerNotSealed);
            }

            var result = true;

            foreach (var id in GetEntityIds())
            {
                var entity = GetEntity(id);
                if (!entity.VerifyValueSignatures(messageHandler)) result = false;
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.Signature,
                    Resources.Container_VerifyEntityValueSignatures_AllEntityValuesIntegerAndAuthentic);
            }

            return result;
        }

        /// <summary>
        /// Trys to load the given storage file as XML.
        /// If parsing the XML fails, error messages will be send by calling <paramref name="messageHandler"/>.
        /// </summary>
        /// <param name="relativePath">The relative path to the storage file.</param>
        /// <param name="messageHandler">The message handler..</param>
        /// <returns>An instance of <see cref="XmlDocument"/> or <c>null</c>.</returns>
        internal XmlDocument TryLoadXml(string relativePath, ValidationHandler messageHandler)
        {
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            try
            {
                using (var src = Storage.Read(relativePath))
                {
                    doc.Load(src);
                }
            }
            catch (XmlException ex)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.Container_TryLoadXml_ErrorReadingXMLDocument, relativePath, ex.Message);
                return null;
            }
            return doc;
        }

        /// <summary>
        /// Validates the given <see cref="XmlDocument"/> with the XML Schema
        /// of the container model. In case of validation errors,
        /// <paramref name="messageHandler"/> is called to display them.
        /// </summary>
        /// <param name="doc">The XML document to validate.</param>
        /// <param name="docName">A display name for the document.</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns></returns>
        internal bool ValidateXml(XmlDocument doc, string docName, ValidationHandler messageHandler)
        {
            if (doc == null) return false;
            var result = true;

            string errorMsg;
            if (!doc.IsSchemaConform(out errorMsg))
            {
                messageHandler.Error(ValidationMessageClass.XmlSchema,
                    Resources.Container_ValidateXml_SchemaError, docName, errorMsg);
                result = false;
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.XmlSchema,
                    Resources.Container_ValidateXml_XMLValid, docName);
            }
            return result;
        }

        #endregion

        #region Workflow

        /// <summary>
        /// Starts the initialization phase of the container.
        /// This phase must be completet with a call to <see cref="FinishInitialization"/>.
        /// </summary>
        /// <param name="initialEdition">The initial edition of the container.</param>
        /// <param name="settings">Optional a set of settings for the initialization phase or <c>null</c>.</param>
        /// <param name="privateKey">A <see cref="IRSAProvider"/> with the private key.</param>
        /// <param name="certificate">A <see cref="IRSAProvider"/> with the X509 certificate.</param>
        private void StartInitialization(EditionElement initialEdition, InitializationSettings settings, IRSAProvider privateKey, IRSAProvider certificate)
        {
            signingProvider = privateKey;
            verifyingProvider = certificate;

            settings = settings ?? new InitializationSettings();

            // Optionally create a salt for the new edition
            if (settings.CreateSaltForNewEdition)
            {
                CreateSaltFor(initialEdition);
            }

            // Load the certificate into the owner description
            initialEdition.Owner.X509Certificate = verifyingProvider.ExportCertificate();
            currentEditionE = initialEdition;

            // Create an empty history
            historyE = new HistoryElement();

            // Create an empty entity index
            indexE = new IndexElement();

            // Mark the container as initializing
            phase = ProcessPhase.Initialization;
        }

        /// <summary>
        /// Finishes the initialization phase.
        /// </summary>
        /// <remarks>
        /// Must be called to complete the initialization phase after creating a new container.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if no entity was added while the initialization phase.
        /// </exception>
        public void FinishInitialization()
        {
            if (IsDisposed) throw new ObjectDisposedException("Container");
            if (indexE.Count == 0)
            {
                throw new InvalidOperationException(
                    Resources.Container_FinishInitialization_InvalidOperation_NoEntity);
            }

            SealContainer();
        }

        /// <summary>
        /// Starts the transformation phase of an existing container.
        /// </summary>
        /// <param name="newEdition">The new edition of the container.</param>
        /// <param name="settings">Optional a set of settings for the transformation phase or <c>null</c>.</param>
        /// <param name="privateKey">A <see cref="IRSAProvider"/> with the private key.</param>
        /// <param name="certificate">A <see cref="IRSAProvider"/> with the X509 certificate.</param>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="newEdition"/>, <paramref name="privateKey"/>, or <paramref name="certificate"/>.
        ///   </exception>
        /// <remarks>
        /// The transformation phase must be completed with a call to <see cref="FinishTransformation"/>.
        /// </remarks>
        public void StartTransformation(EditionElement newEdition, TransformationSettings settings, IRSAProvider privateKey, IRSAProvider certificate)
        {
            if (newEdition == null) throw new ArgumentNullException("newEdition");
            if (privateKey == null) throw new ArgumentNullException("privateKey");
            if (certificate == null) throw new ArgumentNullException("certificate");

            if (IsDisposed) throw new ObjectDisposedException("Container");

            signingProvider = privateKey;
            verifyingProvider = certificate;

            // Load the certificate into the owner description
            newEdition.Owner.X509Certificate = verifyingProvider.ExportCertificate();

            StartTransformation(newEdition, settings ?? new TransformationSettings());
        }

        /// <summary>
        /// Starts the transformation phase of an existing container.
        /// The certificate of the current owner will be reused.
        /// </summary>
        /// <remarks>
        /// The transformation phase must be completed with a call to <see cref="FinishTransformation"/>.
        /// </remarks>
        /// <param name="newEdition">The new edition of the container.</param>
        /// <param name="settings">Optional a set of settings for the transformation phase or <c>null</c>.</param>
        /// <param name="privateKey">A <see cref="IRSAProvider"/> with the private key.</param>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="newEdition"/> or <paramref name="privateKey"/>.
        /// </exception>
        public void StartTransformation(EditionElement newEdition, TransformationSettings settings, IRSAProvider privateKey)
        {
            if (newEdition == null) throw new ArgumentNullException("newEdition");
            if (privateKey == null) throw new ArgumentNullException("privateKey");

            if (IsDisposed) throw new ObjectDisposedException("Container");

            signingProvider = privateKey;
            verifyingProvider = Configuration.CryptoFactory.CreateRSAProviderFromPemEncodedCertificate(
                CurrentEdition.Owner.X509Certificate);

            StartTransformation(newEdition, settings ?? new TransformationSettings());
        }

        private void StartTransformation(EditionElement newEdition, TransformationSettings settings)
        {
            // Optionally create a salt for the new edition
            if (settings.CreateSaltForNewEdition)
            {
                CreateSaltFor(newEdition);
            }

            // Optionally remove the salt from the former current edition
            if (settings.PreventReinstatingFormerCurrentEdition &&
                currentEditionE.SaltState == EditionSaltState.Present)
            {
                currentEditionE.Salt = string.Empty;
            }

            // Create a history item
            var pastMasterSignature = new SignatureWrapper();
            using (var sigSrc = Storage.Read(EDITION_FILE + ".sig"))
            {
                pastMasterSignature.ReadFromStream(sigSrc);
            }
            var historyItem = new HistoryItemElement
                {
                    Edition = currentEditionE,
                    PastMasterSignature = pastMasterSignature
                };

            // Push the history item on to the history stack
            historyE.Push(historyItem);

            // Use the new edition as current edition
            currentEditionE = newEdition;

            // Mark the container as transforming
            phase = ProcessPhase.Transformation;
        }

        private static void CreateSaltFor(EditionElement edition)
        {
            var buffer = new byte[EDITION_SALT_LENGTH];
            Configuration.RandomGenerator.GenerateRandomData(buffer);
            edition.Salt = Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// Finishes the transformation phase.
        /// </summary>
        /// <remarks>
        /// Must be called to complete the transformation phase after calling <see cref="StartTransformation(de.mastersign.odec.model.EditionElement,TransformationSettings,IRSAProvider)"/>
        /// or <see cref="StartTransformation(de.mastersign.odec.model.EditionElement,TransformationSettings,IRSAProvider, IRSAProvider)"/>.
        /// </remarks>
        public void FinishTransformation()
        {
            if (IsDisposed) throw new ObjectDisposedException("Container");

            SealContainer();
        }

        /// <summary>
        /// Stores the history, the index and the edition
        /// and creates the necessary signatures including the master signature.
        /// </summary>
        private void SealContainer()
        {
            // Store and sign the container history
            CurrentEdition.HistorySignature =
                SerializeAndSign(historyE, HistoryElement.XML_NAME, HISTORY_FILE);

            // Store and sign the entity index
            CurrentEdition.IndexSignature =
                SerializeAndSign(indexE, IndexElement.XML_NAME, INDEX_FILE);

            // Store and sign the current edition
            // The signature of the current edition is the master signature
            SerializeAndSign(CurrentEdition, EditionElement.XML_NAME, EDITION_FILE);

            // Mark the container as sealed
            phase = ProcessPhase.Sealed;
        }

        /// <summary>
        /// Removes all editions younger than the specified history edition 
        /// with all added entities and reinstates the specified edition as current edition.
        /// </summary>
        /// <param name="editionId">The ID of the history edition to reinstate as current edition.</param>
        /// <exception cref="ObjectDisposedException">if the container is disposed</exception>
        /// <exception cref="ArgumentException">if the given ID is no valid history edition ID</exception>
        /// <exception cref="InvalidOperationException">
        /// if <see cref="CurrentProcessPhase"/> is not <see cref="ProcessPhase.Sealed"/>
        /// or reinstating the specified edition is impossible
        /// </exception>
        public void ReinstateHistoryEdition(Guid editionId)
        {
            string msg;
            if (!CanReinstateHistoryEdition(editionId, out msg))
            {
                throw new InvalidOperationException(string.Format(
                    Resources.Container_ReinstateHistoryEdition_ReinstatingEditionImpossible, msg));
            }

            // Remove current edition
            DeleteEdition(currentEditionE);
            currentEditionE = null;

            // Remove all junger history editions
            EditionElement edition = null;
            SignatureWrapper pastMasterSignature = null;
            while (historyE.Count > 0)
            {
                var historyItem = historyE.Pop();
                if (historyItem.Edition.Guid == editionId)
                {
                    // reached target edition
                    edition = historyItem.Edition;
                    pastMasterSignature = historyItem.PastMasterSignature;
                    break;
                }

                DeleteEdition(historyItem.Edition);
            }

            if (edition == null)
            {
                throw new ContainerException("The target edition was not found.");
            }


            // Reinstate history edition with signatures
            currentEditionE = edition;
            ResetLastEntityId();
            SerializeWithOldSignature(historyE, HistoryElement.XML_NAME, HISTORY_FILE, currentEditionE.HistorySignature);
            SerializeWithOldSignature(indexE, IndexElement.XML_NAME, INDEX_FILE, currentEditionE.IndexSignature);
            SerializeWithOldSignature(currentEditionE, EditionElement.XML_NAME, EDITION_FILE, pastMasterSignature);
        }

        private void DeleteEdition(EditionElement edition)
        {
            foreach (var entityId in edition.NewEntities)
            {
                UnsafeDeleteEntity(entityId);
            }
        }

        private void UnsafeDeleteEntity(int id)
        {
            if (!indexE.Contains(id)) { return; }

            // Retrieve the entity which will be removed
            var entity = GetEntity(id);

            // Delete the entity related files in the container
            entity.RemoveFromStorage();

            // Remove the entity from the index
            indexE.Remove(id);
        }

        private void ResetLastEntityId()
        {
            int maxEntitiyId = 0;
            foreach (var histItem in historyE.Items)
            {
                maxEntitiyId = Math.Max(maxEntitiyId, histItem.Edition.NewEntities.Max());
            }
            maxEntitiyId = Math.Max(maxEntitiyId, currentEditionE.NewEntities.Max());
            indexE.LastId = maxEntitiyId;
        }

        #endregion

        #region Entity Operations

        /// <summary>
        /// Creates a new entity in the container.
        /// </summary>
        /// <param name="type">The type of the entity or <see cref="Guid.Empty"/> if no profile is used.</param>
        /// <param name="provenance">The description of the provenance.</param>
        /// <param name="label">The label of the entity or <c>null</c>.</param>
        /// <param name="predecessors">The predecessors as array of IDs.</param>
        /// <returns>An <see cref="Entity"/> object.</returns>
        /// <remarks>
        /// This method can only be called in an initialization or transformation phase.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if this method is called outside of an initialization or transformation phase.
        /// </exception>
        /// <exception cref="ArgumentNullException">Is thrown, if <c>null</c> is given for <paramref name="provenance"/>.</exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the given label is allready in use, or one of the given predecessores does not exists.
        /// </exception>
        public Entity NewEntity(Guid type, ProvenanceElement provenance, string label, params int[] predecessors)
        {
            if (provenance == null) throw new ArgumentNullException("provenance");
            if (IsDisposed) throw new ObjectDisposedException("Container");

            if (phase != ProcessPhase.Initialization && phase != ProcessPhase.Transformation)
            {
                throw new InvalidOperationException(Resources.Container_NewEntity_InvalidOperation_WrongPhase);
            }
            if (label != null && IsEntityLabelExisting(label))
            {
                throw new ArgumentException(Resources.Container_NewEntity_LabelAllreadyInUse);
            }
            if (indexE.LastId == 99999)
            {
                throw new InvalidOperationException(Resources.Container_NewEntity_MaximumNumberExceeded);
            }

            // Create a new entity element
            var entityElement =
                new EntityElement
                    {
                        Id = indexE.GetNextEntityId(),
                        Label = label,
                        Type = type,
                        Provenance = provenance,
                    };

            // Iterate over all predecessor IDs
            if (predecessors != null)
            {
                foreach (var predId in predecessors)
                {
                    // Check if predecessor exists
                    if (!indexE.Contains(predId))
                    {
                        throw new ArgumentException(
                            Resources.Container_NewEntity_ArgumentException_PredecessorDoesNotExist, "predecessors");
                    }
                    // Add new entity as successor to its predecessor
                    var predecessorE = indexE.GetItem(predId);
                    predecessorE.Successors.Add(entityElement.Id);

                    // Add predecessor ID to entity
                    entityElement.Predecessors.Add(predId);
                }
            }

            // Create a new index item and add it to the index
            var indexItem = new IndexItemElement { Id = entityElement.Id, Label = label };
            indexE.Add(indexItem);

            // Add it to the edition
            CurrentEdition.NewEntities.Add(entityElement.Id);

            // Create a new entity handler and return it to the caller);
            return new Entity(this, entityElement, true);
        }

        /// <summary>
        /// Removes an existing entity from the container.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <remarks>
        /// This method can only be called in a transformation phase.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if this method is called outside of a transformation phase.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the specified entity does not exists.
        /// </exception>
        public void RemoveEntity(int id)
        {
            if (IsDisposed) throw new ObjectDisposedException("Container");

            if (phase != ProcessPhase.Transformation)
            {
                throw new InvalidOperationException(Resources.Container_RemoveEntity_InvalidOperation_WrongPhase);
            }
            if (!indexE.Contains(id))
            {
                throw new ArgumentException(Resources.Container_ArgumentException_EntityDoesNotExist);
            }

            // Retrieve the entity which will be removed
            var entity = GetEntity(id);

            // Delete the entity related files in the container
            entity.RemoveFromStorage();

            // Remove the entity from the index
            indexE.Remove(id);

            // Add to edition
            CurrentEdition.RemovedEntities.Add(id);

            if (addedEntities != null && addedEntities.Contains(entity))
            {
                addedEntities.Remove(entity);
            }
        }

        /// <summary>
        /// Gets an entity by the specified ID.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        /// <returns>An <see cref="Entity"/> object.</returns>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the specified entity does not exist.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if <see cref="CurrentProcessPhase"/> is <see cref="ProcessPhase.Empty"/>.
        /// </exception>
        public Entity GetEntity(int id)
        {
            if (IsDisposed) throw new ObjectDisposedException("Container");

            if (phase == ProcessPhase.Empty)
            {
                throw new InvalidOperationException("The container is empty.");
            }
            if (!indexE.Contains(id))
            {
                throw new ArgumentException(Resources.Container_ArgumentException_EntityDoesNotExist, "id");
            }

            // Try to use the cache of added entities
            if (addedEntities != null)
            {
                var entity = addedEntities.FirstOrDefault(e => e.Id == id);
                if (entity != null)
                {
                    return entity;
                }
            }

            // Create a new entity element
            var entityElement = new EntityElement();

            // Identify the element with the specified ID
            entityElement.Id = id;

            // Create a wrapper for the entity element
            var result = new Entity(this, entityElement, false);

            // Read the entity header
            Deserialize(entityElement, result.EntityHeaderPath);

            return result;
        }

        #endregion

        #region Queries

        /// <summary>
        /// Gets the compatibility flags for the container.
        /// </summary>
        /// <value>The compatibility flags for the container.</value>
        public CompatibilityFlags CompatibilityFlags { get { return compatibilityFlags; } }

        /// <summary>
        /// Gets the current process phase.
        /// </summary>
        /// <value>The current process phase.</value>
        public ProcessPhase CurrentProcessPhase { get { return phase; } }

        /// <summary>
        /// Gets the storage of the container.
        /// </summary>
        /// <value>The storage.</value>
        public IStorage Storage { get { return storage; } }

        /// <summary>
        /// Gets the current edition of the container.
        /// </summary>
        /// <value>The current edition.</value>
        public EditionElement CurrentEdition { get { return currentEditionE; } }

        /// <summary>
        /// Gets the initial edition of the container.
        /// </summary>
        /// <value>The initial edition.</value>
        public EditionElement InitalEdition
        {
            get { return HistoryEditionCount > 0 ? GetHistoryEdition(0) : CurrentEdition; }
        }

        /// <summary>
        /// Gets the number of history editions.
        /// </summary>
        /// <value>The history edition count.</value>
        public int HistoryEditionCount
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("Container");
                return historyE != null ? historyE.Count : 0;
            }
        }

        /// <summary>
        /// Gets the relative path of the history signature file.
        /// </summary>
        public string HistorySignaturePath { get { return HISTORY_FILE + ".sig"; } }

        /// <summary>
        /// Gets the relative path of the index signature file.
        /// </summary>
        public string IndexSignaturePath { get { return INDEX_FILE + ".sig"; } }

        /// <summary>
        /// Gets the relative path of the master signature path (signature of current edition).
        /// </summary>
        public string MasterSignaturePath { get { return EDITION_FILE + ".sig"; } }

        /// <summary>
        /// Reads the signature of the container history.
        /// </summary>
        /// <returns>The signature of the container history.</returns>
        public SignatureWrapper ReadHistorySignature()
        {
            return ReadSignature(HistorySignaturePath);
        }

        /// <summary>
        /// Reads the signature of the entity index.
        /// </summary>
        /// <returns>The signature of the entity index.</returns>
        public SignatureWrapper ReadIndexSignature()
        {
            return ReadSignature(IndexSignaturePath);
        }

        /// <summary>
        /// Reads the master signature.
        /// </summary>
        /// <returns>The signature of the current edition.</returns>
        public SignatureWrapper ReadMasterSignature()
        {
            return ReadSignature(MasterSignaturePath);
        }

        private SignatureWrapper ReadSignature(string path)
        {
            var sigWr = new SignatureWrapper();
            using (var s = Storage.Read(path))
            {
                sigWr.ReadFromStream(s);
            }
            return sigWr;
        }

        /// <summary>
        /// Gets a history edition specified by an index.
        /// The zero-based index addresses the items on the history stack. 
        /// Thereby zero addresses the initial edition and 
        /// <see cref="HistoryEditionCount"/> - 1 addresses the most recent edition.
        /// </summary>
        /// <param name="index">The index of the past edition.</param>
        /// <returns>An <see cref="EditionElement"/> object.</returns>
        public EditionElement GetHistoryEdition(int index)
        {
            if (IsDisposed) throw new ObjectDisposedException("Container");
            return historyE.Items[index].Edition;
        }

        /// <summary>
        /// Gets an array of all entity IDs.
        /// </summary>
        /// <returns>An array of all entity IDs.</returns>
        public int[] GetEntityIds()
        {
            if (IsDisposed) throw new ObjectDisposedException("Container");
            return indexE.Items.Select(i => i.Id).ToArray();
        }

        /// <summary>
        /// Gets the number of entities in the container.
        /// </summary>
        /// <value>The entity count.</value>
        public int EntityCount { get { return indexE.Count; } }

        /// <summary>
        /// Determines whether the given entity label is existing.
        /// </summary>
        /// <param name="label">The entity label.</param>
        /// <returns>
        /// 	<c>true</c> if the entity label is existing; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <paramref name="label"/> is <c>null</c>.
        /// </exception>
        public bool IsEntityLabelExisting(string label)
        {
            return indexE.Items.Any(i => i.Label == label);
        }

        /// <summary>
        /// Gets the entity id for the given label.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <returns>The ID of the described entity.</returns>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the given label was not found in the index.
        /// </exception>
        public int GetEntityId(string label)
        {
            foreach (var item in indexE.Items)
            {
                if (item.Label == label) return item.Id;
            }
            throw new ArgumentNullException("label", Resources.Container_ArgumentNullException_LabelNotFound);
        }

        /// <summary>
        /// Checks wheter the specified edition ca be reinstated as current edition.
        /// If the it is not possible to reinstate the specified edition, 
        /// the reason is given through the parameter <paramref name="message"/>.
        /// </summary>
        /// <param name="editionId">The ID of the edition.</param>
        /// <param name="message">The reason why the specified edition can not be reinstated or <c>null</c>.</param>
        /// <returns><c>true</c> if the specified edition can be reinstated as current edition; otherwise <c>false</c>.</returns>
        /// <exception cref="ObjectDisposedException">if the container is disposed</exception>
        /// <exception cref="ArgumentException">if the given ID is no valid history edition ID</exception>
        /// <exception cref="InvalidOperationException">
        /// if <see cref="CurrentProcessPhase"/> is not <see cref="ProcessPhase.Sealed"/>
        /// </exception>
        public bool CanReinstateHistoryEdition(Guid editionId, out string message)
        {
            if (IsDisposed) throw new ObjectDisposedException("Container");
            if (phase != ProcessPhase.Sealed) throw new InvalidOperationException(
                Resources.Container_CanReinstateHistoryEdition_InvalidOperation_WrongPhase);

            var editionNo = -1;
            for (var i = 0; i < HistoryEditionCount; i++)
            {
                if (GetHistoryEdition(i).Guid == editionId)
                {
                    editionNo = i;
                    break;
                }
            }
            if (editionNo < 0)
            {
                throw new ArgumentException(Resources.Container_CanReinstateHistoryEdition_ArgumentException_EditionNotFound, "editionId");
            }

            var result = true;
            var sb = new StringBuilder();
            var edition = GetHistoryEdition(editionNo);
            if (edition.SaltState == EditionSaltState.Removed)
            {
                sb.AppendLine(Resources.Container_CanReinstateHistoryEdition_Reason_SaltRemoved);
                result = false;
            }
            var entities = GetEntityIdsForEdition(editionNo);
            for (var i = editionNo + 1; i < HistoryEditionCount; i++)
            {
                var e = GetHistoryEdition(i);
                if (HasRemovedIncludedEntity(e, entities))
                {
                    sb.AppendLine(
                        Resources.Container_CanReinstateHistoryEdition_Reason_EntityRemoved);
                    result = false;
                    break;
                }
            }

            // further checks necessary?

            message = result ? null : sb.ToString();
            return result;
        }

        private int[] GetEntityIdsForEdition(int editionNo)
        {
            var entities = new List<int>();
            for (var i = 0; i <= editionNo; i++)
            {
                var e = GetHistoryEdition(i);
                entities.AddRange(e.NewEntities);
                foreach (var removedEntity in e.RemovedEntities)
                {
                    entities.Remove(removedEntity);
                }
            }
            return entities.ToArray();
        }

        private bool HasRemovedIncludedEntity(EditionElement edition, int[] includedEntityIds)
        {
            return edition.RemovedEntities.Any(includedEntityIds.Contains);
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            Storage.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DirectoryStorage"/> is reclaimed by garbage collection.
        /// </summary>
        ~Container()
        {
            Dispose();
        }

        #endregion
    }
}
