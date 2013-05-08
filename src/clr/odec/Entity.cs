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
    /// Represents an entity in a container.
    /// </summary>
    /// <remarks>
    /// Most of the entity properties can be found in <see cref="EntityElement"/>.
    /// </remarks>
    public class Entity
    {
        private readonly Container parent;
        private readonly EntityElement entityElement;

        private Value addedParameterSet;
        private readonly List<Value> addedValues;

        /// <summary>
        /// Gets or sets the entity element representing the XML description of the entity.
        /// </summary>
        /// <value>The entity XML element.</value>
        internal EntityElement EntityElement { get { return entityElement; } }

        /// <summary>
        /// Gets the verifying RSA provider from the certificate of the owner of the entity.
        /// </summary>
        /// <value>The verifying RSA provider.</value>
        internal IRSAProvider VerifyingRSAProvider
        {
            get { return Configuration.CryptoFactory.CreateRSAProviderFromPemEncodedCertificate(CreatingEdition.Owner.X509Certificate); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        /// <param name="entityElement">The entity XML element to manage.</param>
        /// <param name="open">if set to <c>true</c> the entity can be changed.</param>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="parent"/> or <paramref name="entityElement"/>.
        /// </exception>
        internal Entity(Container parent, EntityElement entityElement, bool open)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            if (entityElement == null) throw new ArgumentNullException("entityElement");

            this.parent = parent;
            this.entityElement = entityElement;
            IsOpen = open;

            if (IsOpen)
            {
                addedValues = new List<Value>();
            }
        }

        #region Wrapper

        /// <summary>
        /// Gets the id of the entity.
        /// </summary>
        /// <value>The entity id.</value>
        public int Id { get { return EntityElement.Id; } }

        /// <summary>
        /// Gets the label of the entity.
        /// </summary>
        /// <value>The label of the entity or <c>null</c>.</value>
        public string Label { get { return EntityElement.Label; } }

        /// <summary>
        /// Gets an enumeration of predecessors.
        /// </summary>
        /// <value>The predecessors of this entity.</value>
        public IEnumerable<int> Predecessors { get { return EntityElement.Predecessors; } }

        /// <summary>
        /// Gets a GUID, which identifies the type of the entity.
        /// </summary>
        /// <value>The type of the entity.</value>
        public Guid Type { get { return EntityElement.Type; } }

        /// <summary>
        /// Gets the description of the provenance.
        /// </summary>
        /// <value>The provenance of the entity.</value>
        public ProvenanceElement Provenance { get { return EntityElement.Provenance; } }

        #endregion

        /// <summary>
        /// Gets the container, containing this entity.
        /// </summary>
        /// <value>The parent container.</value>
        public Container Parent { get { return parent; } }

        /// <summary>
        /// Gets the edition in which this entity was added to the container.
        /// </summary>
        /// <value>The creating edition or <c>null</c>, if no matching edition was found.</value>
        public EditionElement CreatingEdition { get { return parent.GetEditionForEntity(Id); } }

        /// <summary>
        /// Gets the name of the entity directory in the container.
        /// </summary>
        /// <value>The name of the entity directory.</value>
        public string DirectoryName { get { return Id.ToString("00000"); } }

        /// <summary>
        /// Gets a value indicating whether this entity is open for changes.
        /// </summary>
        /// <remarks>
        /// If the entity is open, than values and provenance parameter can added to the entity.
        /// If the entity is closed no permanent changes can be made to the entity.
        /// </remarks>
        /// <value><c>true</c> if the entity is open and the entity can be changed; otherwise, <c>false</c>.</value>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Sets the provenance parameter set.
        /// </summary>
        /// <remarks>
        /// If <paramref name="appearance"/> is set to <see cref="ValueAppearance.suppressed"/>
        /// the data at the source path is used to compute the signature, but is not stored in the container.
        /// Set <paramref name="appearance"/> to <see cref="ValueAppearance.encrypted"/>, 
        /// if the data in the source path is encrypted in any way.
        /// </remarks>
        /// <param name="name">The name of the provenance parameter set.</param>
        /// <param name="type">The data type of the provenance parameter set.</param>
        /// <param name="appearance">The appearance of the data.</param>
        /// <param name="sourcePath">The path of the file, containing the data.</param>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="name"/> or <paramref name="sourcePath"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the specified value name is allready in use
        /// or if <paramref name="type"/> is <see cref="Guid.Empty"/>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Is thrown, if the specified source file in <paramref name="sourcePath"/> can not be found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if the provenance parameter set is allready set or the entity is not open.
        /// </exception>
        public void SetProvenanceParameterSet(string name, Guid type, ValueAppearance appearance, string sourcePath)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (sourcePath == null) throw new ArgumentNullException("sourcePath");

            if ((EntityElement.ParameterSet != null && EntityElement.ParameterSet.Name == name)
                || EntityElement.Values.Any(vr => vr.Name == name))
            {
                throw new InvalidOperationException(
                    string.Format(Resources.Entity_SetProvenanceParameterSet_InvalidOperation_ValueNameAllreadyInUse, name));
            }
            if (type == Guid.Empty) throw new ArgumentException(
                Resources.Entity_SetProvenanceParameterSet_ArgumentException_TypeIdInvalid, "type");
            if (!File.Exists(sourcePath)) throw new FileNotFoundException(
                Resources.Entity_SourceFileNotFound, sourcePath);

            var valueRef = new ValueReference { Name = name, Type = type, Appearance = appearance };
            EntityElement.ParameterSet = valueRef;
            addedParameterSet = new Value(this, valueRef, sourcePath);
        }

        /// <summary>
        /// Gets the handler for the provenance parameter set.
        /// </summary>
        /// <returns>A <see cref="Value"/> object to read the provenance parameter set.</returns>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if the entity does not have a provenance parameter set.
        /// </exception>
        public Value GetProvenanceParameterSet()
        {
            if (entityElement.ParameterSet == null)
            {
                throw new InvalidOperationException(
                    Resources.Entity_GetProvenanceParameterSet_NoProvenanceParameterSet);
            }

            return addedParameterSet ?? new Value(this, entityElement.ParameterSet, null);
        }

        /// <summary>
        /// Gets a value indicating whether this entity has a provenance parameter set.
        /// </summary>
        /// <value><c>true</c> if this instance has a provenance parameter set; otherwise, <c>false</c>.</value>
        public bool HasProvenanceParameterSet
        {
            get { return EntityElement.ParameterSet != null; }
        }

        /// <summary>
        /// Adds an entity value.
        /// </summary>
        /// <remarks>
        /// If <paramref name="appearance"/> is set to <see cref="ValueAppearance.suppressed"/>
        /// the data at the source path is used to compute the signature, but is not stored in the container.
        /// Set <paramref name="appearance"/> to <see cref="ValueAppearance.encrypted"/>, 
        /// if the data in the source path is encrypted in any way.
        /// </remarks>
        /// <param name="name">The name of the value.</param>
        /// <param name="type">The data type of the value.</param>
        /// <param name="appearance">The appearance of the data.</param>
        /// <param name="sourcePath">The path of the file, containing the data.</param>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="name"/> or <paramref name="sourcePath"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the specifies value name allready exists,
        /// <paramref name="name"/> is equal to the name of the provenance parameter set or
        /// <paramref name="type"/> is <see cref="Guid.Empty"/>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Is thrown, if the specified source file in <paramref name="sourcePath"/> can not be found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if the entity is not open.
        /// </exception>
        public void AddValue(string name, Guid type, ValueAppearance appearance, string sourcePath)
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException(Resources.Entity_AddValue_InvalidOperation_NotOpenForChanges);
            }

            if (name == null) throw new ArgumentNullException("name");
            if (sourcePath == null) throw new ArgumentNullException("sourcePath");

            if ((EntityElement.ParameterSet != null && EntityElement.ParameterSet.Name == name)
                || EntityElement.Values.Any(vr => vr.Name == name))
            {
                throw new ArgumentException(
                    string.Format(Resources.Entity_AddValue_ArgumentException_NameAllreadyInUse, name), "name");
            }
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(Resources.Entity_SourceFileNotFound, sourcePath);
            }

            var valueRef = new ValueReference { Name = name, Type = type, Appearance = appearance };
            EntityElement.Values.Add(valueRef);
            addedValues.Add(new Value(this, valueRef, sourcePath));
        }

        /// <summary>
        /// Gets a handler for the specified value.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <returns>A <see cref="Value"/> object to read the value</returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="name"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the specified value does not exist.
        /// </exception>
        public Value GetValue(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (addedValues != null)
            {
                var addedValueRef = addedValues.FirstOrDefault(vr => vr.Name == name);
                if (addedValueRef != null)
                {
                    return addedValueRef;
                }
            }

            var valueRef = EntityElement.Values.FirstOrDefault(vr => vr.Name == name);
            if (valueRef == null)
            {
                throw new ArgumentException(
                    string.Format(Resources.Entity_GetValue_ArgumentException_NameNotFound, name), "name");
            }
            return new Value(this, valueRef, null);
        }

        /// <summary>
        /// Gets an enumeration of the names of all values in the entity.
        /// </summary>
        /// <value>The value names.</value>
        public IEnumerable<string> ValueNames
        {
            get { return EntityElement.Values.Select(vRef => vRef.Name); }
        }

        /// <summary>
        /// Gets the relative path to the entity header .
        /// </summary>
        /// <value>The entity header path.</value>
        public string EntityHeaderPath { get { return DirectoryName + "/entity.xml"; } }

        /// <summary>
        /// Closes this entity and writes the entity header, the provenance parameter and the values to the container storage.
        /// </summary>
        /// <remarks>
        /// This writes the entity to the storage and creates the necessary signatures.
        /// After calling <see cref="Close"/>, no further changes can be made to the entity.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Is thrown, if <see cref="IsOpen"/> is <c>false</c>.
        /// </exception>
        public void Close()
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException(Resources.Entity_Close_InvalidOperation_EntityAllreadyClosed);
            }

            if (addedParameterSet != null)
            {
                // Write and sign the provenance parameter set
                addedParameterSet.WriteAndSignValue();
                addedParameterSet = null;
            }

            // Write and sign the entity values
            foreach (var addedValue in addedValues)
            {
                addedValue.WriteAndSignValue();
            }
            addedValues.Clear();

            // Write and sign the entity header
            var signWrapper = parent.SerializeAndSign(EntityElement, EntityElement.XML_NAME, EntityHeaderPath);

            // Give the signature to the container index
            parent.SetEntitySignatureForIndex(this, signWrapper);
        }

        /// <summary>
        /// Gets the relative path of the signature file for this entity.
        /// </summary>
        /// <value>The relative path of the signature file for this entity.</value>
        public string SignaturePath { get { return EntityHeaderPath + ".sig"; } }


        /// <summary>
        /// Reads the signature of the entity from the storage.
        /// </summary>
        /// <returns>The signature of the entity.</returns>
        public SignatureWrapper ReadSignature()
        {
            var sigWr = new SignatureWrapper();
            using (var signatureStream = Parent.Storage.Read(SignaturePath))
            {
                sigWr.ReadFromStream(signatureStream);
            }
            return sigWr;
        }

        internal void RemoveFromStorage()
        {
            if (IsOpen)
            {
                throw new InvalidOperationException(
                    Resources.Entity_RemoveFromStorage_InvalidOperation_EntityNotClosedYet);
            }

            if (entityElement.ParameterSet != null)
            {
                GetProvenanceParameterSet().RemoveFromStorage();
            }
            var names = ValueNames.ToArray();
            foreach (var name in names)
            {
                GetValue(name).RemoveFromStorage();
            }

            var storage = parent.Storage;
            if (storage.Exists(EntityHeaderPath))
            {
                storage.Remove(EntityHeaderPath);
            }
            var signaturePath = EntityHeaderPath + ".sig";
            if (storage.Exists(signaturePath))
            {
                storage.Remove(signaturePath);
            }
        }

        /// <summary>
        /// Checks the validity of this instance.
        /// The given <see cref="ValidationHandler"/> is called while the validation process,
        /// to display errors, warnings and informal messages.
        /// </summary>
        /// <param name="messageHandler">A message handler.</param>
        /// <returns><c>true</c> if this instance is valid; otherwise <c>false</c>.</returns>
        public bool ValidateStructure(ValidationHandler messageHandler)
        {
            var result = true;

            if (!entityElement.Validate(messageHandler)) result = false;

            if (HasProvenanceParameterSet)
            {
                var ps = GetProvenanceParameterSet();
                if (!ps.ValidateStructure(messageHandler)) result = false;
            }
            foreach (var name in ValueNames)
            {
                var v = GetValue(name);
                if (!v.ValidateStructure(messageHandler)) result = false;
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure,
                    Resources.Entity_ValidateStructure_StructureIsValid, Id);
            }

            return result;
        }

        /// <summary>
        /// Verifies the digests for the provenance parameter set and the values.
        /// </summary>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns><c>true</c> if the values are integer and authentic; otherwise <c>false</c>.</returns>
        public bool VerifyValueSignatures(ValidationHandler messageHandler)
        {
            var result = true;

            if (HasProvenanceParameterSet)
            {
                var ps = GetProvenanceParameterSet();
                if (!ps.VerifySignature(messageHandler)) result = false;
            }
            foreach (var name in ValueNames)
            {
                var v = GetValue(name);
                if (!v.VerifySignature(messageHandler)) result = false;
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.Signature,
                    Resources.Entity_VerifyValueSignatures_AllValueSignaturesAuthentic, Id);
            }
            return result;
        }
    }
}
