using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.Properties;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// This class is representing the XML element, describing an entity.
    /// The name of the XML element is <c>Entity</c> and it resides in the namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    public class EntityElement : IXmlStorable2, IEquatable<EntityElement>
    {
        ///<summary>
        /// The name of the XML element, represented by this class.
        ///</summary>
        public const string XML_NAME = "Entity";

        /// <summary>
        /// Gets or sets the id of the entity.
        /// </summary>
        /// <value>The entity id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the label of the entity.
        /// </summary>
        /// <value>The label of the entity or <c>null</c>.</value>
        public string Label { get; set; }

        private readonly List<int> predecessors = new List<int>();

        /// <summary>
        /// Gets the collection of predecessors.
        /// </summary>
        /// <value>The predecessors of this entity.</value>
        public ICollection<int> Predecessors { get { return predecessors; } }

        /// <summary>
        /// Gets or sets a GUID, which identifies the type of the entity.
        /// </summary>
        /// <value>The type of the entity.</value>
        public Guid Type { get; set; }

        /// <summary>
        /// Gets or sets the description of the provenance.
        /// </summary>
        /// <value>The provenance of the entity.</value>
        public ProvenanceElement Provenance { get; set; }

        /// <summary>
        /// Gets or sets the reference to the provenance parameter set.
        /// </summary>
        /// <value>The parameter set or <c>null</c>, if no parameter set is stored in the entity.</value>
        public ValueReference ParameterSet { get; set; }

        private readonly List<ValueReference> values = new List<ValueReference>();

        /// <summary>
        /// Gets the collection with all value references.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<ValueReference> Values { get { return values; } }

        /// <summary>
        /// Checks the validity of this instance.
        /// The given <see cref="ValidationHandler"/> is called while the validation process,
        /// to display errors, warnings and informal messages.
        /// </summary>
        /// <param name="messageHandler">A message handler.</param>
        /// <returns><c>true</c> if this instance is valid; otherwise <c>false</c>.</returns>
        public bool Validate(ValidationHandler messageHandler)
        {
            var result = true;
            if (Id < 0 || Id >= 100000)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.EntityElement_Validate_InvalidId);
                result = false;
            }
            else if (Provenance == null || !Provenance.IsValid)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.EntityElement_Validate_InvalidProvenanceDescription, Id);
                result = false;
            }
            if (ParameterSet != null && !ParameterSet.IsValid)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.EntityElement_Validate_InvalidProvenanceParameterSetReference, ParameterSet.Name, Id);
                result = false;
            }
            if (values.Count == 0)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.EntityElement_Validate_NoValues, Id);
                result = false;
            }
            foreach (var valueRef in values)
            {
                if (!valueRef.IsValid)
                {
                    messageHandler.Error(ValidationMessageClass.ContainerStructure,
                        Resources.EntityElement_Validate_InvalidValueReference, valueRef.Name, Id);
                    result = false;
                }
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure,
                    Resources.EntityElement_Validate_StructureIsValid, Id);
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
            if (e == null) throw new ArgumentNullException("e");
            Id = e.ReadParsedObject("c:Id", Id, int.Parse);
            Label = e.ReadElementString("c:Label", null);
            var listString = e.ReadElementString("c:Predecessors", "");
            if (listString != null)
            {
                var list = listString.Split(' ');
                predecessors.Clear();
                predecessors.AddRange(
                    from s in list
                    where !string.IsNullOrEmpty(s)
                    select int.Parse(s));
            }
            Type = e.ReadParsedObject("c:Type", Guid.Empty, v => new Guid(v));
            Provenance = e.ReadObject("c:Provenance", new ProvenanceElement());
            ParameterSet = e.ReadObject<ValueReference>("c:ParameterSet", null);
            var valueE = (XmlElement)e.SelectSingleNode("c:Values", Model.NamespaceManager);
            values.Clear();
            values.AddRange(valueE.ReadObjects<ValueReference>("c:Value"));
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
        /// Writes the state of the object to an XML target.
        /// </summary>
        /// <param name="w">The <see cref="XmlWriter"/> used as target.</param>
        /// <param name="canonicalizedSignatures"><c>true</c>, if the embeded signature
        /// must be written unindented and canonicalized; otherwise <c>false</c>.</param>
        /// <remarks>The parameter <paramref name="canonicalizedSignatures"/> is needed for
        /// compatibility reasons.</remarks>
        /// <seealso cref="CompatibilityFlags"/>
        public void WriteToXml(XmlWriter w, bool canonicalizedSignatures)
        {
            if (w == null) throw new ArgumentNullException("w");
            if (!IsValid) throw new InvalidOperationException(Resources.XmlStorable_WriteToXml_ThisInstanceIsInvalid);
            w.WriteElementString("Id", Model.ContainerNamespace, Id.ToString(CultureInfo.InvariantCulture));
            if (Label != null) w.WriteElementString("Label", Model.ContainerNamespace, Label);
            w.WriteElementString("Predecessors", Model.ContainerNamespace,
                string.Join(" ", predecessors.Select(id => id.ToString(CultureInfo.InvariantCulture)).ToArray()));

            if (Type != Guid.Empty)
            {
                w.WriteElementString("Type", Model.ContainerNamespace, Type.ToString("D"));
            }
            w.WriteObject("Provenance", Model.ContainerNamespace, Provenance);
            if (ParameterSet != null)
            {
                w.WriteObject("ParameterSet", Model.ContainerNamespace, ParameterSet);
            }
            w.WriteStartElement("Values", Model.ContainerNamespace);
            foreach (var value in values)
            {
                w.WriteObject("Value", Model.ContainerNamespace, value, canonicalizedSignatures);
            }
            w.WriteEndElement();
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
                    Id >= 0 && Id <= 99999 &&
                    Provenance != null && Provenance.IsValid &&
                    (ParameterSet == null || ParameterSet.IsValid) &&
                    values.Count > 0 && values.All(v => v.IsValid);
            }
        }

        #endregion

        #region Implementation of IEquatable<EntityElement>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(EntityElement other)
        {
            return
                Id == other.Id &&
                Label == other.Label &&
                Type == other.Type &&
                Equals(Provenance, other.Provenance) &&
                Equals(ParameterSet, other.ParameterSet) &&
                ObjectUtils.AreEqual(Predecessors.ToArray(), other.Predecessors.ToArray()) &&
                ObjectUtils.AreEqual(values.ToArray(), other.values.ToArray());
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is EntityElement && Equals((EntityElement)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            var result = 7 * (Id + 23);
            result *= Label != null ? Label.GetHashCode() + 23 : 1;
            result *= Type.GetHashCode() + 23;
            result *= Provenance != null ? Provenance.GetHashCode() + 23 : 1;
            result *= predecessors.Aggregate(result, (current, v) => current * (v + 23));
            result *= Type.GetHashCode() + 23;
            if (ParameterSet != null)
            {
                result *= ParameterSet.GetHashCode() + 23;
            }
            result = values.Aggregate(result, (current, value) => current * (value.GetHashCode() + 23));
            return result;
        }

        #endregion
    }
}
