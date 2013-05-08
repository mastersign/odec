using System;
using System.Text;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.IO;
using de.mastersign.odec.Properties;
using de.mastersign.odec.crypto;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// Wraps a W3C XML signature for a single file. 
    /// This class represents the XML type <c>SIGNATUREWRAPPER</c>
    /// in the namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    public class SignatureWrapper : IXmlStorable2, IEquatable<SignatureWrapper>
    {
        private static readonly string DEF_HASH = AlgorithmIdentifier.SHA256;

        /// <summary>
        /// Gets or sets a <see cref="Signature"/> object representing the W3C XML signature.
        /// </summary>
        /// <value>The signature.</value>
        public Signature Signature { get; set; }

        /// <summary>
        /// Builds a signature with a given hash.
        /// </summary>
        /// <param name="uri">The URI, referencing the signed data.</param>
        /// <param name="digest">The hash of the data.</param>
        /// <param name="digestMethod">The URI, specifiing the hash algorithm.</param>
        /// <param name="rsaProvider">The crypto provider.</param>
        /// <param name="canonicalizer">The canonicalizer.</param>
        public void BuildSignature(string uri, byte[] digest, string digestMethod,
                                   IRSAProvider rsaProvider, IXmlCanonicalizer canonicalizer)
        {
            if (string.IsNullOrEmpty(uri)) throw new ArgumentNullException("uri");
            if (digest == null) throw new ArgumentNullException("digest");
            if (string.IsNullOrEmpty(digestMethod)) throw new ArgumentNullException("digestMethod");
            if (rsaProvider == null) throw new ArgumentNullException("rsaProvider");
            if (canonicalizer == null) throw new ArgumentNullException("canonicalizer");

            var si =
                new SignedInfo
                    {
                        SignatureMethod = rsaProvider.SignatureMethod,
                        CanonicalizationMethod = canonicalizer.CanonicalizationMethod,
                    };
            si.AddReference(
                new Reference
                    {
                        DigestMethod = digestMethod,
                        DigestValue = digest,
                        Uri = uri
                    });

            Signature =
                new Signature
                    {
                        SignedInfo = si,
                        SignatureValue = rsaProvider.ComputeSignature(canonicalizer.Canonize(si.GetXml()))
                    };
        }

        /// <summary>
        /// Builds a signature including the SHA1 hash for a data source.
        /// </summary>
        /// <param name="uri">The URI, referencing the signed data.</param>
        /// <param name="dataSource">A stream with the data to sign.</param>
        /// <param name="hashProvider">The hash provider.</param>
        /// <param name="rsaProvider">The crypto provider.</param>
        /// <param name="canonicalizer">The canonicalizer.</param>
        public void BuildSignature(string uri, Stream dataSource, IHashProvider hashProvider,
                                   IRSAProvider rsaProvider, IXmlCanonicalizer canonicalizer)
        {
            if (string.IsNullOrEmpty(uri)) throw new ArgumentNullException("uri");
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (!dataSource.CanRead) throw new ArgumentException(Resources.SignatureWrapper_ArgumentException_SourceNotReadable, "dataSource");
            if (hashProvider == null) throw new ArgumentNullException("hashProvider");
            if (rsaProvider == null) throw new ArgumentNullException("rsaProvider");
            if (canonicalizer == null) throw new ArgumentNullException("canonicalizer");

            var hashBuilder = hashProvider.CreateHashBuilder(DEF_HASH);
            dataSource.CopyTo(hashBuilder.Stream);
            var digest = hashBuilder.ComputeHash();
            BuildSignature(uri, digest, DEF_HASH, rsaProvider, canonicalizer);
        }

        /// <summary>
        /// Verifies the digest included in the signed info of the XML signature.
        /// </summary>
        /// <param name="uri">The URI, referencing the signed data.</param>
        /// <param name="digest">The hash of the data.</param>
        /// <param name="digestMethod">The URI, specifiing the hash algorithm.</param>
        /// <param name="displayName">The display name for the signed data or <c>null</c>.</param>
        /// <param name="messageHandler">A message handler or <c>null</c>.</param>
        /// <returns><c>true</c>, if the signed data is integer; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="uri"/>,
        /// <paramref name="digest"/> or <paramref name="digestMethod"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if given digest method does not match the method used in the
        /// signature.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if the wrapped <see cref="Signature"/> is <c>null</c>.
        /// </exception>
        public bool VerifyDigest(string uri, byte[] digest, string digestMethod,
            string displayName = null, ValidationHandler messageHandler = null)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (digest == null) throw new ArgumentNullException("digest");
            if (digestMethod == null) throw new ArgumentNullException("digestMethod");
            if (Signature == null) throw new InvalidOperationException("No signature set.");

            var reference = (Reference)Signature.SignedInfo.References[0];
            if (!uri.Equals(reference.Uri))
            {
                messageHandler.Error(ValidationMessageClass.Signature,
                    "The URI in the signature of '{0}' does not match the given URI.", displayName);
                return false;
            }

            if (digestMethod != reference.DigestMethod)
            {
                throw new ArgumentException(
                    string.Format("The given digest method '{0}' does not match the method used in the signature '{1}'.",
                    digestMethod, reference.DigestMethod), "digestMethod");
            }

            if (!ObjectUtils.AreEqual(reference.DigestValue, digest))
            {
                messageHandler.Error(ValidationMessageClass.Signature,
                    "The data of '{0}' is not integer.", displayName);
                return false;
            }
            messageHandler.Success(ValidationMessageClass.Signature,
                "The data of '{0}' is integer.", displayName);
            return true;
        }

        /// <summary>
        /// Verifies the digest included in the signed info of the XML signature.
        /// </summary>
        /// <param name="uri">The URI, referencing the signed data.</param>
        /// <param name="dataSource">A stream with the data to verify.</param>
        /// <param name="hashProvider">The hash provider.</param>
        /// <param name="displayName">The display name for the signed data or <c>null</c>.</param>
        /// <param name="messageHandler">A message handler or <c>null</c>.</param>
        /// <returns><c>true</c>, if the signed data is integer; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="uri"/>,
        /// <paramref name="dataSource"/> or <paramref name="hashProvider"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the given <paramref name="dataSource"/> is not readable.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if the wrapped <see cref="Signature"/> is <c>null</c>.
        /// </exception>
        public bool VerifyDigest(string uri, Stream dataSource, IHashProvider hashProvider,
            string displayName = null, ValidationHandler messageHandler = null)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (hashProvider == null) throw new ArgumentNullException("hashProvider");
            if (Signature == null) throw new InvalidOperationException("No signature set.");

            if (!dataSource.CanRead) throw new ArgumentException(
                Resources.SignatureWrapper_ArgumentException_SourceNotReadable, "dataSource");

            if (messageHandler != null && displayName == null) displayName = "Unkown";

            var si = Signature.SignedInfo;
            var reference = (Reference)si.References[0];
            if (!uri.Equals(reference.Uri))
            {
                messageHandler.Error(ValidationMessageClass.Signature,
                    Resources.SignatureWrapper_VerifyDigest_UriMismatch, displayName);
                return false;
            }

            if (!hashProvider.GetSupportedMethods().Contains(reference.DigestMethod))
            {
                messageHandler.Error(ValidationMessageClass.Signature,
                    Resources.SignatureWrapper_VerifyDigest_DigestAlgorithmNotSupported, displayName);
                return false;
            }
            var hashBuilder = hashProvider.CreateHashBuilder(reference.DigestMethod);
            dataSource.CopyTo(hashBuilder.Stream);
            var digest = hashBuilder.ComputeHash();
            if (!ObjectUtils.AreEqual(reference.DigestValue, digest))
            {
                messageHandler.Error(ValidationMessageClass.Signature,
                    Resources.SignatureWrapper_VerifyDigest_DataNotInteger, displayName);
                return false;
            }
            messageHandler.Success(ValidationMessageClass.Signature,
                Resources.SignatureWrapper_VerifyDigest_DataIsInteger, displayName);

            return true;
        }

        /// <summary>
        /// Checks if the signature is authentic.
        /// </summary>
        /// <remarks>
        /// The digest included in the signed info of the XML signature is not validated.
        /// </remarks>
        /// <param name="hashProvider">A hash provider.</param>
        /// <param name="rsaProvider">
        /// The verifying RSA provider. 
        /// This provider needs access to the certificate of the associated edition owner.
        /// </param>
        /// <param name="canonicalizer">An XML canonicalizer.</param>
        /// <param name="displayName">The display name for the signed data or <c>null</c>.</param>
        /// <param name="messageHandler">A message handler or <c>null</c>.</param>
        /// <returns><c>true</c> if the signature is authentic; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="hashProvider"/>, 
        /// <paramref name="rsaProvider"/> or <paramref name="canonicalizer"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if the wrapped <see cref="Signature"/> is <c>null</c>.
        /// </exception>
        public bool VerifySignature(IHashProvider hashProvider, IRSAProvider rsaProvider, IXmlCanonicalizer canonicalizer,
            string displayName = null, ValidationHandler messageHandler = null)
        {
            if (hashProvider == null) throw new ArgumentNullException("hashProvider");
            if (rsaProvider == null) throw new ArgumentNullException("rsaProvider");
            if (canonicalizer == null) throw new ArgumentNullException("canonicalizer");
            if (Signature == null) throw new InvalidOperationException("No signature set.");

            if (messageHandler != null && displayName == null) displayName = "Unkown";

            var si = Signature.SignedInfo;
            if (si.CanonicalizationMethod != canonicalizer.CanonicalizationMethod)
            {
                messageHandler.Error(ValidationMessageClass.Signature,
                    Resources.SignatureWrapper_VerifySignature_CanonicalizerNotSupported,
                    si.CanonicalizationMethod, displayName);
                return false;
            }

            if (si.SignatureMethod != rsaProvider.SignatureMethod)
            {
                messageHandler.Error(ValidationMessageClass.Signature,
                    Resources.SignatureWrapper_VerifySignature_SignatureAlgorithmNotSupported,
                    si.SignatureMethod, displayName);
                return false;
            }
            var sigResult = rsaProvider.VerifySignature(
                canonicalizer.Canonize(si.GetXml()),
                Signature.SignatureValue);

            if (sigResult)
            {
                messageHandler.Success(ValidationMessageClass.Signature,
                    Resources.SignatureWrapper_VerifySignature_SignatureIsAuthentic, displayName);
                return true;
            }
            else
            {
                messageHandler.Error(ValidationMessageClass.Signature,
                    Resources.SignatureWrapper_VerifySignature_SignatureNotAuthentic, displayName);
                return false;
            }
        }

        /// <summary>
        /// Saves the signature to an octet stream.
        /// </summary>
        /// <param name="stream">A writeable <see cref="Stream"/> object.</param>
        /// <param name="canonicalized"><c>true</c>, if the signature
        /// must be written unindented and canonicalized; otherwise <c>false</c>.</param>
        /// <remarks>The parameter <paramref name="canonicalized"/> is intended 
        /// to be used for compatibility reasons.</remarks>
        /// <seealso cref="CompatibilityFlags"/>
        public void WriteToStream(Stream stream, bool canonicalized)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            if (canonicalized)
            {
                using (var cs = Configuration.Canonicalizer.Canonize(Signature.GetXml()))
                {
                    cs.CopyTo(stream);
                }
            }
            else
            {
                var ws = new XmlWriterSettings { Indent = true, IndentChars = "  " };
                using (var w = XmlWriter.Create(stream, ws))
                {
                    w.WriteStartDocument();
                    WriteToXml(w);
                    w.WriteEndDocument();
                }
            }
        }

        /// <summary>
        /// Restores the signature fromo an octet stream.
        /// </summary>
        /// <param name="stream">A readable <see cref="Stream"/> object.</param>
        public void ReadFromStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            var doc = new XmlDocument();
            doc.Load(stream);
            ReadFromXml(doc);
        }

        /// <summary>
        /// Checks the validity of this instance.
        /// The given <see cref="ValidationHandler"/> is called while the validation process,
        /// to display errors, warnings and informal messages.
        /// </summary>
        /// <param name="displayName">The display name for the signed data.</param>
        /// <param name="messageHandler">A message handler.</param>
        /// <returns>
        /// 	<c>true</c> if this instance is valid; otherwise <c>false</c>.
        /// </returns>
        public bool ValidateStructure(string displayName, ValidationHandler messageHandler)
        {
            bool result = true;
            if (Signature == null)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.SignatureWrapper_ValidateStructure_NoSignature, displayName);
                result = false;
            }
            else
            {
                if (Signature.SignedInfo == null)
                {
                    messageHandler.Error(ValidationMessageClass.ContainerStructure,
                        Resources.SignatureWrapper_ValidateStructure_NoSignedInfo, displayName);
                    result = false;
                }
                else if (Signature.SignedInfo.References.Count == 0)
                {
                    messageHandler.Error(ValidationMessageClass.ContainerStructure,
                        Resources.SignatureWrapper_ValidateStructure_NoDataReference, displayName);
                    result = false;
                }
                else if (Signature.SignedInfo.References.Count > 1)
                {
                    messageHandler.Error(ValidationMessageClass.ContainerStructure,
                        Resources.SignatureWrapper_ValidateStructure_MoreThanOneDataReference, displayName);
                    result = false;
                }
                if (Signature.SignatureValue == null)
                {
                    messageHandler.Error(ValidationMessageClass.ContainerStructure,
                        Resources.SignatureWrapper_ValidateStructure_NoSignatureValue, displayName);
                    result = false;
                }
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure,
                    Resources.SignatureWrapper_ValidateStructure_ValidStructure, displayName);
            }

            return result;
        }

        #region Implementation of IXmlStorable

        /// <summary>
        /// Loads the state of the object from an XML source.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> used as source.</param>
        public void ReadFromXml(XmlNode e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            var sEl = e.SelectSingleNode("sig:Signature", Model.NamespaceManager) as XmlElement;
            if (sEl == null)
            {
                throw new ArgumentException(Resources.SignatureWrapper_ReadFromXml_NoSignatureElement);
            }

            var s = new Signature();
            s.LoadXml(sEl);
            Signature = s;
        }

        /// <summary>
        /// Writes the state of the object to an XML target.
        /// </summary>
        /// <param name="w">The <see cref="XmlWriter"/> used as target.</param>
        public void WriteToXml(XmlWriter w)
        {
            WriteToXml(w, false);
        }

        /// <summary>
        /// Writes the signature to an XML target.
        /// </summary>
        /// <param name="w">The <see cref="XmlWriter"/> used as target.</param>
        /// <param name="canonicalized"><c>true</c>, if the signature must be written unindented 
        /// and canonicalized; otherwise <c>false</c>.</param>
        /// <remarks>The parameter <paramref name="canonicalized"/> is needed for
        /// compatibility reasons.</remarks>
        /// <seealso cref="CompatibilityFlags"/>
        public void WriteToXml(XmlWriter w, bool canonicalized)
        {
            if (w == null)
            {
                throw new ArgumentNullException("w");
            }
            if (Signature == null)
            {
                throw new InvalidOperationException(Resources.SignatureWrapper_WriteToXml_InvalidOperation_NoSignature);
            }
            if (!IsValid)
            {
                throw new InvalidOperationException(Resources.XmlStorable_WriteToXml_ThisInstanceIsInvalid);
            }

            if (canonicalized)
            {
                string rawSignatureText;
                using (var cs = Configuration.Canonicalizer.Canonize(Signature.GetXml()))
                using (var r = new StreamReader(cs))
                {
                    rawSignatureText = r.ReadToEnd();
                }
                w.WriteRaw(rawSignatureText);
            }
            else
            {
                Signature.GetXml().WriteTo(w);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// The instance is valid, if a call to <see cref="WriteToXml(System.Xml.XmlWriter)"/>
        /// produces schema conform XML.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                return
                    Signature != null &&
                    Signature.SignedInfo != null &&
                    Signature.SignedInfo.References.Count == 1 &&
                    Signature.SignatureValue != null;
            }
        }

        #endregion

        #region Implementation of IEquatable<SignatureWrapperType>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(SignatureWrapper other)
        {
            if (other == null) return false;
            if (Signature == null) return other.Signature == null;
            if (other.Signature == null) return false;
            var canonicalizer = new BclC14NCanonicalizer();
            var e1 = Signature.GetXml();
            var e2 = other.Signature.GetXml();
            var d1 = canonicalizer.Canonize(e1).ToArray();
            var d2 = canonicalizer.Canonize(e2).ToArray();
            return ObjectUtils.AreEqual(d1, d2);
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is SignatureWrapper && Equals((SignatureWrapper)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            if (Signature == null)
            {
                return 7;
            }
            var canonicalizer = new BclC14NCanonicalizer();
            using (var s = canonicalizer.Canonize(Signature.GetXml()))
            {
                return ObjectUtils.GetHashCode(s.ToArray());
            }
        }

        #endregion

    }
}