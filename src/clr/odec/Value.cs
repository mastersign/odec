using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using de.mastersign.odec.Properties;
using de.mastersign.odec.crypto;
using de.mastersign.odec.model;

namespace de.mastersign.odec
{
    /// <summary>
    /// Represents an entity value or a provenance parameter set.
    /// </summary>
    public class Value
    {
        private const string DEF_HASH = AlgorithmIdentifier.SHA1;

        private readonly Entity parent;
        private readonly ValueReference valueRef;
        private readonly string sourcePath;

        private bool isStored;

        /// <summary>
        /// Initializes a new instance of the <see cref="Value"/> class.
        /// </summary>
        /// <param name="parent">The parent entity.</param>
        /// <param name="valueRef">The value reference to manage.</param>
        /// <param name="sourcePath">The source file of the value or <c>null</c>.</param>
        internal Value(Entity parent, ValueReference valueRef, string sourcePath)
        {
            this.parent = parent;
            this.valueRef = valueRef;
            this.sourcePath = sourcePath;
            isStored = sourcePath == null;
        }

        /// <summary>
        /// Writes the data of the value to the container storage, calculates the signature
        /// writes the signature to the container storage and copies the signature to the value reference.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if no source file is given to the constructor or if the value was allready written.
        /// </exception>
        internal void WriteAndSignValue()
        {
            if (sourcePath == null)
            {
                throw new InvalidOperationException(
                    Resources.Value_WriteAndSignValue_InvalidOperation_NoSourceFileGiven);
            }
            if (isStored)
            {
                throw new InvalidOperationException(
                    Resources.Value_WriteAndSignValue_InvalidOperation_ValueAllreadyStored);
            }
            var container = parent.Parent;
            var sourceFileInfo = new FileInfo(sourcePath);
            valueRef.Size = sourceFileInfo.Length;

            // TODO make more efficient (double read: storing, hash computing)
            // tip: IHashBuilder and IStorage.Write are incompatible

            if (Appearance != ValueAppearance.suppressed)
            {
                // Write the value data
                container.Storage.Write(ValuePath, sourcePath);
            }
            // Compute the signature
            var signWrapper = new SignatureWrapper();
            using (var fs = File.OpenRead(sourcePath))
            {
                signWrapper.BuildSignature(Name, fs,
                    Configuration.HashProvider, container.SigningRSAProvider,
                    Configuration.Canonicalizer);
            }
            valueRef.ValueSignature = signWrapper;

            // Write the signature
            using (var ms = new MemoryStream())
            {
                signWrapper.WriteToStream(ms, parent.Parent.CompatibilityFlags.WriteXmlSignatureCanonicalized);
                ms.Position = 0L;
                container.Storage.Write(SignaturePath, ms);
            }

            isStored = true;
        }

        /// <summary>
        /// Gets the relative path of the storage file for this value.
        /// </summary>
        /// <value>The relative path of the storage file for this value.</value>
        public string ValuePath { get { return Parent.DirectoryName + "/" + Name; } }

        /// <summary>
        /// Gets the relative path of the signature file for this value.
        /// </summary>
        /// <value>The relative path of the signature file for this value.</value>
        public string SignaturePath { get { return ValuePath + ".sig"; } }

        #region Wrapper

        /// <summary>
        /// Gets the name of the value.
        /// </summary>
        /// <value>The name of the value.</value>
        public string Name { get { return valueRef.Name; } }

        /// <summary>
        /// Gets the data type of the value.
        /// </summary>
        /// <value>The data type of the value.</value>
        public Guid Type { get { return valueRef.Type; } }

        /// <summary>
        /// Gets the size of the value.
        /// </summary>
        /// <value>The size of the value.</value>
        public long Size { get { return valueRef.Size; } }

        /// <summary>
        /// Gets the appearance of the value.
        /// </summary>
        /// <value>The appearance of the value.</value>
        public ValueAppearance Appearance { get { return valueRef.Appearance; } }

        #endregion

        /// <summary>
        /// Gets the entity, containing this value.
        /// </summary>
        /// <value>The parent entity.</value>
        public Entity Parent { get { return parent; } }

        /// <summary>
        /// Gets a value indicating whether the value can be read.
        /// </summary>
        /// <value><c>true</c> if the value is stored in the container and can be read; otherwise, <c>false</c>.</value>
        public bool CanRead { get { return isStored && Appearance != ValueAppearance.suppressed; } }

        /// <summary>
        /// Gets a read only octet stream of the specified value.
        /// </summary>
        /// <returns>A <see cref="Stream"/> to read the specified value.</returns>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if the value is not yet written to the container storage
        /// or the value appearance is <see cref="ValueAppearance.suppressed"/>.
        /// </exception>
        public Stream Read()
        {
            if (!isStored)
            {
                throw new InvalidOperationException(Resources.Value_InvalidOperation_ValueNotStoredYet);
            }
            if (Appearance == ValueAppearance.suppressed)
            {
                throw new InvalidOperationException(Resources.Value_Read_InvalidOperation_ValueAppearanceIsSuppressed);
            }
            return parent.Parent.Storage.Read(ValuePath);
        }

        /// <summary>
        /// Copies the data of the value to a file.
        /// </summary>
        /// <param name="targetPath">The path of the target file.</param>
        /// <remarks>
        /// The specified target file will be overridden if it allready exists.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if the value is not yet written to the container storage
        /// or the value appaerance is <see cref="ValueAppearance.suppressed"/>
        /// </exception>
        public void CopyToFile(string targetPath)
        {
            using (var source = Read())
            using (var target = File.Open(targetPath, FileMode.Create))
            {
                source.CopyTo(target);
            }
        }

        /// <summary>
        /// Validates the structure of the entity value.
        /// </summary>
        /// <returns><c>true</c> if the structure of the value is valid; otherwise <c>false</c>.</returns>
        public bool ValidateStructure(ValidationHandler messageHandler)
        {
            if (!isStored)
            {
                throw new InvalidOperationException(Resources.Value_InvalidOperation_ValueNotStoredYet);
            }
            var stor = parent.Parent.Storage;

            var result = true;

            var valueFileExists = stor.Exists(ValuePath);
            if (Appearance == ValueAppearance.suppressed && valueFileExists)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.Value_ValidateStructure_ValueFileNotExpected, ValuePath);
                result = false;
            }
            if (Appearance != ValueAppearance.suppressed && !valueFileExists)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.Value_ValidateStructure_ValueFileMissing, ValuePath);
                result = false;
            }
            if (!stor.Exists(SignaturePath))
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.Value_ValidateStructure_SignatureMissing, DisplayName);
                result = false;
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure,
                    Resources.Value_ValidateStructure_StructureIsValid, DisplayName);
            }

            return result;
        }

        /// <summary>
        /// Verifies the signature of the entity value.
        /// </summary>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns><c>true</c> if the signature is valid</returns>
        public bool VerifySignature(ValidationHandler messageHandler)
        {
            var container = parent.Parent;

            return container.ValidateDoubleSignature(
                ValuePath, valueRef.ValueSignature, false, valueRef.Appearance!=ValueAppearance.suppressed,
                parent.VerifyingRSAProvider, messageHandler);
        }

        /// <summary>
        /// Reads the signature of the value from the storage.
        /// </summary>
        /// <returns>The signature of the value.</returns>
        public SignatureWrapper ReadSignature()
        {
            var sigWr = new SignatureWrapper();
            using (var signatureStream = Parent.Parent.Storage.Read(SignaturePath))
            {
                sigWr.ReadFromStream(signatureStream);
            }
            return sigWr;
        }

        internal void RemoveFromStorage()
        {
            if (!isStored)
            {
                throw new InvalidOperationException(Resources.Value_InvalidOperation_ValueNotStoredYet);
            }

            var storage = parent.Parent.Storage;
            if (storage.Exists(ValuePath))
            {
                storage.Remove(ValuePath);
            }
            var signaturePath = ValuePath + ".sig";
            if (storage.Exists(signaturePath))
            {
                storage.Remove(signaturePath);
            }
        }

        private string DisplayName { get { return string.Format("Entity[{0}]/{1}", parent.Id, Name); } }
    }
}
