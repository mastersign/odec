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
    /// This class is representing the XML element for an item in the container history.
    /// The name of the XML element is <c>HistoryItem</c> and it resides in the namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    public class HistoryItemElement : IXmlStorable2, IEquatable<HistoryItemElement>
    {
        ///<summary>
        /// The name of the XML element, represented by this class.
        ///</summary>
        public const string XML_NAME = "HistoryItem";

        /// <summary>
        /// Gets or sets the edition, associated with the history item.
        /// </summary>
        /// <value>The edition.</value>
        public EditionElement Edition { get; set; }

        /// <summary>
        /// Gets or sets the past master signature of the container
        /// from the point of time, when <see cref="Edition"/> was the
        /// current edition of the container.
        /// </summary>
        /// <value>The past master signature.</value>
        public SignatureWrapper PastMasterSignature { get; set; }

        /// <summary>
        /// Checks the validity of this instance.
        /// The given <see cref="ValidationHandler"/> is called while the validation process,
        /// to display errors, warnings and informal messages.
        /// </summary>
        /// <param name="displayName">The display name for the history item.</param>
        /// <param name="messageHandler">A message handler.</param>
        /// <returns>
        /// 	<c>true</c> if this instance is valid; otherwise <c>false</c>.
        /// </returns>
        public bool Validate(string displayName, ValidationHandler messageHandler)
        {
            var result = true;

            if (Edition == null)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure, 
                    Resources.HistoryItemElement_Validate_NoEdition, displayName);
                result = false;
            }
            else if (!Edition.Validate(
                string.Format(Resources.HistoryItemElement_Validate_EditionInHistoryItem, displayName), 
                messageHandler))
            {
                result = false; 
            }

            if (PastMasterSignature == null)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure, 
                    Resources.HistoryItemElement_Validate_NoPastMasterSignature, displayName);
                result = false;
            }
            else if (!PastMasterSignature.ValidateStructure(
                string.Format(Resources.HistoryItemElement_Validate_PastMasterSignatureInHistoryItem, displayName), 
                messageHandler))
            {
                result = false;
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure, 
                    Resources.HistoryItemElement_Validate_ValidStructure);
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

            Edition = e.ReadObject("c:" + EditionElement.XML_NAME, Edition);
            PastMasterSignature = e.ReadObject("c:PastMasterSignature", PastMasterSignature);
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
        /// <param name="canonicalizedSignatures"><c>true</c>, if the embeded signatures
        /// must be written unindented and canonicalized; otherwise <c>false</c>.</param>
        /// <remarks>The parameter <paramref name="canonicalizedSignatures"/> is needed for
        /// compatibility reasons.</remarks>
        /// <seealso cref="CompatibilityFlags"/>
        public void WriteToXml(XmlWriter w, bool canonicalizedSignatures)
        {
            if (w == null) throw new ArgumentNullException("w");
            if (!IsValid)
            {
                throw new InvalidOperationException("The object is not valid and can not be written to XML.");
            }

            w.WriteObject(EditionElement.XML_NAME, Model.ContainerNamespace, 
                Edition, canonicalizedSignatures);

            w.WriteObject("PastMasterSignature", Model.ContainerNamespace, 
                PastMasterSignature, canonicalizedSignatures);
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
                    Edition != null && Edition.IsValid &&
                    PastMasterSignature != null && PastMasterSignature.IsValid;
            }
        }

        #endregion

        #region Implementation of IEquatable<HistoryItemElement>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(HistoryItemElement other)
        {
            if (other == null) return false;
            if (!Equals(Edition, other.Edition)) return false;
            if (!Equals(PastMasterSignature, other.PastMasterSignature)) return false;
            return true;
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is HistoryItemElement ? Equals((HistoryItemElement)obj) : false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            return 7
                * (Edition != null ? Edition.GetHashCode() + 23 : 1)
                * (PastMasterSignature != null ? PastMasterSignature.GetHashCode() + 23 : 1);
        }

        #endregion
    }
}