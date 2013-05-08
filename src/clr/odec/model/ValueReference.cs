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
    /// This class is representing the XML type, describing the reference to an entity value
    /// or a provenance parameter set.
    /// The name of the XML type is <c>VALUEREFERENCE</c> and it resides in the namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    public class ValueReference : IXmlStorable2, IEquatable<ValueReference>
    {
        /// <summary>
        /// Gets or sets the name of the referenced value.
        /// </summary>
        /// <remarks>
        /// This is the relative path of the data file storing the value as an octet stream.
        /// The path is relative to the directory of the entity.
        /// </remarks>
        /// <value>The file name of the referenced value.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data type of the referenced value.
        /// </summary>
        /// <remarks>
        /// This <see cref="String"/> is an id, which allows the identification
        /// of the data type, used by the referenced value, in the context
        /// of the container profile.
        /// </remarks>
        /// <value>The data type of the referenced value.</value>
        public Guid Type { get; set; }

        /// <summary>
        /// Gets or sets the size of the referenced value as a number of octets.
        /// </summary>
        /// <value>The size of the value.</value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the appearance of the referenced value.
        /// </summary>
        /// <value>The appearance of the referenced value.</value>
        public ValueAppearance Appearance { get; set; }

        /// <summary>
        /// Gets or sets the signature of the referenced value.
        /// </summary>
        /// <value>The value signature.</value>
        public SignatureWrapper ValueSignature { get; set; }

        #region Implementation of IXmlStorable

        /// <summary>
        /// Loads the state of the object from an XML source.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> used as source.</param>
        public void ReadFromXml(XmlNode e)
        {
            if (e == null) throw new ArgumentNullException("e");
            Name = e.ReadElementString("c:Name", Name);
            Type = e.ReadParsedObject("c:Type", Guid.Empty, v => new Guid(v));
            Size = e.ReadParsedObject("c:Size", Size, long.Parse);
            Appearance = e.ReadParsedObject("c:Appearance", Appearance, 
                v => (ValueAppearance)Enum.Parse(typeof (ValueAppearance), v));
            ValueSignature = e.ReadObject("c:ValueSignature", ValueSignature);
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
        /// <param name="canonicalizedSignature"><c>true</c>, if the embeded signature
        /// must be written unindented and canonicalized; otherwise <c>false</c>.</param>
        /// <remarks>The parameter <paramref name="canonicalizedSignature"/> is needed for
        /// compatibility reasons.</remarks>
        /// <seealso cref="CompatibilityFlags"/>
        public void WriteToXml(XmlWriter w, bool canonicalizedSignature)
        {
            if (w == null) throw new ArgumentNullException("w");
            if (!IsValid) throw new InvalidOperationException(Resources.XmlStorable_WriteToXml_ThisInstanceIsInvalid);
            w.WriteElementString("Name", Model.ContainerNamespace, Name);
            if (Type != Guid.Empty)
            {
                w.WriteElementString("Type", Model.ContainerNamespace, Type.ToString("D"));
            }
            w.WriteElementString("Size", Model.ContainerNamespace, Size.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("Appearance", Model.ContainerNamespace, 
                Enum.GetName(typeof(ValueAppearance), Appearance));

            w.WriteStartElement("ValueSignature", Model.ContainerNamespace);
            ValueSignature.WriteToXml(w, canonicalizedSignature);
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
                    !string.IsNullOrWhiteSpace(Name) &&
                    Size >= 0 &&
                    Enum.IsDefined(typeof(ValueAppearance), Appearance) &&
                    ValueSignature != null && ValueSignature.IsValid;
            }
        }

        #endregion

        #region Implementation of IEquatable<ValueReferenceType>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(ValueReference other)
        {
            if (other == null) return false;
            return
                Name == other.Name &&
                Type == other.Type &&
                Size == other.Size &&
                Appearance == other.Appearance &&
                Equals(ValueSignature, other.ValueSignature);
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is ValueReference && Equals((ValueReference)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            return 7
                * (Name != null ? Name.GetHashCode() + 23 : 1)
                * (Type.GetHashCode() + 23)
                * (Size.GetHashCode() + 23)
                * (Appearance.GetHashCode() + 23)
                * (ValueSignature != null ? ValueSignature.GetHashCode() + 23 : 1);
        }

        #endregion
    }
}