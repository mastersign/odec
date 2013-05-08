using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using de.mastersign.odec.Properties;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// Repesents the XML element <c>IndexItem</c>, which is an item in the entity index.
    /// It resides in the namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    public class IndexItemElement : IXmlStorable2, IEquatable<IndexItemElement>
    {
        ///<summary>
        /// The name of the XML element, represented by this class.
        ///</summary>
        public const string XML_NAME = "IndexItem";

        private readonly List<int> successors = new List<int>();

        /// <summary>
        /// Gets or sets the id of the referenced entity.
        /// </summary>
        /// <value>The id of the referenced entity.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the label of the referenced entity.
        /// </summary>
        /// <value>The label of the referenced entity or <c>null</c>.</value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the signature of the referenced entity.
        /// </summary>
        /// <value>The entity signature.</value>
        public SignatureWrapper EntitySignature { get; set; }

        /// <summary>
        /// Gets the ids of the successor entities.
        /// </summary>
        /// <value>The successor entities.</value>
        public ICollection<int> Successors
        {
            get { return successors; }
        }

        /// <summary>
        /// Checks the validity of this instance.
        /// The given <see cref="ValidationHandler"/> is called while the validation process,
        /// to display errors, warnings and informal messages.
        /// </summary>
        /// <param name="messageHandler">A message handler.</param>
        /// <returns>
        /// 	<c>true</c> if this instance is valid; otherwise <c>false</c>.
        /// </returns>
        public bool Validate(ValidationHandler messageHandler)
        {
            var result = true;
            if (Id <= 0)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.IndexItemElement_Validate_InvalidId);
                result = false;
            }
            if (EntitySignature == null)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.IndexItemElement_Validate_NoEntitySignature, Id);
                result = false;
            }
            else if (EntitySignature.ValidateStructure(
                string.Format(Resources.IndexItemElement_Validate_EntitySignatureInIndexItem, Id), 
                messageHandler))
            {
                result = false;
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure, 
                    Resources.IndexItemElement_Validate_ValidStructure, Id);
            }

            return result;
        }

        #region Implementation of IXmlStorable

        /// <summary>
        /// Loads the state of the object from a XML source.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> used as source.</param>
        public void ReadFromXml(XmlNode e)
        {
            if (e == null) throw new ArgumentNullException("e");
            Id = e.ReadParsedObject("c:Id", Id, int.Parse);
            Label = e.ReadElementString("c:Label", null);

            var listString = e.ReadElementString("c:Successors", null);
            if (listString != null)
            {
                var list = listString.Split(' ');
                successors.Clear();
                successors.AddRange(
                    from s in list
                    where !string.IsNullOrEmpty(s)
                    select int.Parse(s));
            }

            EntitySignature = e.ReadObject("c:EntitySignature", EntitySignature);
        }

        /// <summary>
        /// Writes the state of the object to a XML target.
        /// </summary>
        /// <param name="w">The <see cref="XmlWriter"/> used as target.</param>
        public void WriteToXml(XmlWriter w)
        {
            WriteToXml(w, false);
        }

        /// <summary>
        /// Writes the state of the object to a XML target.
        /// </summary>
        /// <param name="w">The <see cref="XmlWriter"/> used as target.</param>
        /// <param name="canonicalizedSignatures"><c>true</c>, if the embeded signatures
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

            w.WriteElementString("Successors", Model.ContainerNamespace,
                string.Join(" ", successors.Select(id => id.ToString(CultureInfo.InvariantCulture)).ToArray()));

            w.WriteObject("EntitySignature", Model.ContainerNamespace,
                EntitySignature, canonicalizedSignatures);
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
                    Id >= 0 &&
                    EntitySignature != null && EntitySignature.IsValid;
            }
        }

        #endregion

        #region Implementation of IEquatable<IndexItemElement>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(IndexItemElement other)
        {
            return
                Id == other.Id &&
                Label == other.Label &&
                Equals(EntitySignature, other.EntitySignature) &&
                ObjectUtils.AreEqual(successors.ToArray(), other.successors.ToArray());
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is IndexItemElement && Equals((IndexItemElement)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            return 7
                * (Id.GetHashCode() + 23)
                * (Label != null ? Label.GetHashCode() + 23 : 1)
                * (EntitySignature != null ? EntitySignature.GetHashCode() + 23 : 1)
                * ObjectUtils.GetHashCode(successors.ToArray());
        }

        #endregion
    }
}