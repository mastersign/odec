using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.Properties;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// This class is representing the XML element, describing the provenance of an entity.
    /// The name of the XML element is <c>Provenance</c> and it resides in the namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    public class ProvenanceElement : IXmlStorable, IEquatable<ProvenanceElement>
    {
        /// <summary>
        /// Gets or sets the GUID which identifies the provenance.
        /// </summary>
        /// <value>The GUID of the provenance.</value>
        public Guid Guid { get; set; }

        #region Implementation of IXmlStorable

        /// <summary>
        /// Loads the state of the object from an XML source.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> used as source.</param>
        public void ReadFromXml(XmlNode e)
        {
            if (e == null) throw new ArgumentNullException("e");
            Guid = e.ReadParsedObject("c:Guid", Guid.Empty, v => new Guid(v));
        }

        /// <summary>
        /// Writes the state of the object to an XML target.
        /// </summary>
        /// <param name="w">The <see cref="XmlWriter"/> used as target.</param>
        public void WriteToXml(XmlWriter w)
        {
            if (w == null) throw new ArgumentNullException("w");
            if (!IsValid)
            {
                throw new InvalidOperationException(Resources.XmlStorable_WriteToXml_ThisInstanceIsInvalid);
            }
            if (Guid != Guid.Empty)
            {
                w.WriteElementString("Guid", Model.ContainerNamespace, Guid.ToString("D"));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// The instance is valid, if a call to <see cref="WriteToXml"/>
        /// produces schema conform XML.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get { return true; }
        }

        #endregion

        #region Implementation of IEquatable<ProvenanceElement>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(ProvenanceElement other)
        {
            if (other == null) return false;
            return Guid == other.Guid;
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is ProvenanceElement && Equals((ProvenanceElement)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            return 7
                * (Guid.GetHashCode() + 23);
        }

        #endregion
    }
}
