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
    /// This class is representing the XML element <c>Edition</c>, describing a container edition.
    /// It resides in the namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    public class EditionElement : IXmlStorable2, IEquatable<EditionElement>
    {
        #region Static

        /// <summary>
        /// Creates a new edition element.
        /// </summary>
        /// <param name="software">The name of the used software.</param>
        /// <param name="profile">The id of the container profile.</param>
        /// <param name="version">The version of the container profile.</param>
        /// <param name="owner">The owner of the container.</param>
        /// <param name="copyright">Copyright info.</param>
        /// <param name="comments">Comments or <c>null</c>.</param>
        /// <returns>An <see cref="EditionElement"/> object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="software"/>, <paramref name="profile"/>,
        /// <paramref name="version"/>, <paramref name="owner"/> or <paramref name="copyright"/>.
        /// </exception>
        public static EditionElement Create(
            string software, string profile, string version,
            Owner owner, string copyright, string comments)
        {
            if (software == null) throw new ArgumentNullException("software");
            if (profile == null) throw new ArgumentNullException("profile");
            if (version == null) throw new ArgumentNullException("version");
            if (owner == null) throw new ArgumentNullException("owner");
            if (copyright == null) throw new ArgumentNullException("copyright");
            return new EditionElement
                       {
                           Guid = Guid.NewGuid(),
                           Software = software,
                           Profile = profile,
                           Version = version,
                           Timestamp = DateTime.Now,
                           Owner = owner,
                           Copyright = copyright,
                           Comments = comments,
                       };
        }

        #endregion

        ///<summary>
        /// The name of the XML element, represented by this class.
        ///</summary>
        public const string XML_NAME = "Edition";

        /// <summary>
        /// Gets or sets the global unique identifier for the container edition.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets a salt (cryptographic random data) for the edition.
        /// </summary>
        /// <remarks>
        /// The salt is included in the signature computation and can be removed
        /// if the edition is pushed on to the history stack in case reinstating this
        /// edition must be prevented.
        /// To remove the salt from the edition set this property to <see cref="string.Empty"/>.
        /// </remarks>
        /// <value>
        /// <list type="bullet">
        ///     <item>
        ///         <term><c>null</c></term>
        ///         <description>if the edition has no salt for prevention of reinstating</description>
        ///     </item>
        ///     <item>
        ///         <term>A not empty string</term>
        ///         <description>if the edition has a salt which can be removed to prevent reinstation</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="String.Empty"/></term>
        ///         <description>if the salt of the edition was removed to prevent reinstating it</description>
        ///     </item>
        /// </list>
        /// </value>
        /// <seealso cref="SaltState"/>
        public string Salt { get; set; }

        /// <summary>
        /// Gets the state of the edition salt.
        /// </summary>
        /// <seealso cref="Salt"/>
        public EditionSaltState SaltState
        {
            get
            {
                if (Salt == null) return EditionSaltState.None;
                if (Salt == string.Empty) return EditionSaltState.Removed;
                return EditionSaltState.Present;
            }
        }

        /// <summary>
        /// Gets or sets a string, describing the software which created the edition.
        /// The creation of an edition can be the initialization of a new container or
        /// the extension of an existing one.
        /// </summary>
        public string Software { get; set; }

        /// <summary>
        /// Gets or sets a string, identifying the container profile.
        /// </summary>
        /// <remarks>
        /// The container profile references a description for the allowed
        /// data and entity types and entity provenance interfaces.
        /// </remarks>
        public string Profile { get; set; }

        /// <summary>
        /// Gets or sets a string, identifying the version of the container profile.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets a value indicating if this edition is referencing a container profile.
        /// </summary>
        /// <value><c>true</c> if a container profile is referenced; otherwise <c>false.</c></value>
        public bool IsReferencingProfile
        {
            get { return !string.IsNullOrEmpty(Profile) && !string.IsNullOrEmpty(Version); }
        }

        /// <summary>
        /// Gets or sets the date and time of the edition creation.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the owner of the edition.
        /// </summary>
        public Owner Owner { get; set; }

        /// <summary>
        /// Gets or sets copyright informations for the edition.
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Gets or sets unstructured comments for the edition.
        /// </summary>
        public string Comments { get; set; }

        private readonly List<int> newEntities = new List<int>();

        /// <summary>
        /// Gets a collection with the ids of all new entities.
        /// </summary>
        /// <value>The new entities.</value>
        public ICollection<int> NewEntities { get { return newEntities; } }

        private readonly List<int> removedEntities = new List<int>();

        /// <summary>
        /// Gets a collection with the ids of all removed entities.
        /// </summary>
        /// <value>The removed entities.</value>
        public List<int> RemovedEntities { get { return removedEntities; } }

        /// <summary>
        ///  Gets or sets a <see cref="SignatureWrapper"/>, which contains a
        /// <see cref="System.Security.Cryptography.Xml.Signature"/> signing
        /// the XML document with the container history.
        /// </summary>
        public SignatureWrapper HistorySignature { get; set; }

        /// <summary>
        ///  Gets or sets a <see cref="SignatureWrapper"/>, which contains a
        /// <see cref="System.Security.Cryptography.Xml.Signature"/> signing
        /// the XML document with the entity index.
        /// </summary>
        public SignatureWrapper IndexSignature { get; set; }

        /// <summary>
        /// Checks the validity of this instance.
        /// The given <see cref="ValidationHandler"/> is called while the validation process,
        /// to display errors, warnings and informal messages.
        /// </summary>
        /// <param name="displayName">The display name for the edition.</param>
        /// <param name="messageHandler">A message handler.</param>
        /// <returns>
        /// 	<c>true</c> if this instance is valid; otherwise <c>false</c>.
        /// </returns>
        public bool Validate(string displayName, ValidationHandler messageHandler)
        {
            var result = true;
            if (Guid == Guid.Empty)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.EditionElement_Validate_IdIsEmpty, displayName);
                result = false;
            }
            if (string.IsNullOrWhiteSpace(Software))
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.EditionElement_Validate_SoftwareFieldIsEmpty, displayName);
                result = false;
            }
            if (Owner == null)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.EditionElement_Validate_NoOwner, displayName);
                result = false;
            }
            else if (!Owner.Validate(messageHandler))
            {
                result = false;
            }
            if (HistorySignature == null)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.EditionElement_Validate_NoHistorySignature, displayName);
                result = false;
            }
            else if (!HistorySignature.ValidateStructure(
                string.Format(Resources.EditionElement_Validate_HistorySignatureInEdition, displayName),
                messageHandler))
            {
                result = false;
            }
            if (IndexSignature == null)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure,
                    Resources.EditionElement_Validate_NoIndexSignature, displayName);
                result = false;
            }
            else if (!IndexSignature.ValidateStructure(
                string.Format(Resources.EditionElement_Validate_IndexSignatureInEdition, displayName),
                messageHandler))
            {
                result = false;
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

            Guid = e.ReadParsedObject("c:Guid", Guid, t => new Guid(t));
            Salt = e.SelectSingleNode("c:Salt", Model.NamespaceManager) != null
                ? e.ReadElementString("c:Salt", string.Empty)
                : null;
            Software = e.ReadElementString("c:Software", Software);
            Profile = e.ReadElementString("c:Profile", null);
            Version = e.ReadElementString("c:Version", null);
            Timestamp = e.ReadParsedObject("c:Timestamp", Timestamp, DateTimeUtils.ParseDateTime);
            Owner = e.ReadObject("c:Owner", Owner);
            Copyright = e.ReadElementString("c:Copyright", null);
            Comments = e.ReadElementString("c:Comments", null);

            var listString = e.ReadElementString("c:AddedEntities", null);
            if (listString != null)
            {
                var list = listString.Split(' ');
                newEntities.Clear();
                newEntities.AddRange(
                    from s in list
                    where !string.IsNullOrEmpty(s)
                    select int.Parse(s));
            }

            listString = e.ReadElementString("c:RemovedEntities", null);
            if (listString != null)
            {
                var list = listString.Split(' ');
                removedEntities.Clear();
                removedEntities.AddRange(
                    from s in list
                    where !string.IsNullOrEmpty(s)
                    select int.Parse(s));
            }

            HistorySignature = e.ReadObject("c:HistorySignature", HistorySignature);
            IndexSignature = e.ReadObject("c:IndexSignature", IndexSignature);
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

            w.WriteElementString("Guid", Model.ContainerNamespace, Guid.ToString("D"));
            if (Salt != null)
            {
                w.WriteElementString("Salt", Model.ContainerNamespace, Salt);
            }
            w.WriteElementString("Software", Model.ContainerNamespace, Software);
            if (Profile != null)
            {
                w.WriteElementString("Profile", Model.ContainerNamespace, Profile);
                if (Version != null)
                {
                    w.WriteElementString("Version", Model.ContainerNamespace, Version);
                }
            }

            w.WriteElementString("Timestamp", Model.ContainerNamespace, DateTimeUtils.FormatDateTime(Timestamp));
            w.WriteObject("Owner", Model.ContainerNamespace, Owner);

            if (Copyright != null)
            {
                w.WriteElementString("Copyright", Model.ContainerNamespace, Copyright);
            }
            if (Comments != null)
            {
                w.WriteElementString("Comments", Model.ContainerNamespace, Comments);
            }

            w.WriteElementString("AddedEntities", Model.ContainerNamespace,
                string.Join(" ", newEntities.Select(id => id.ToString(CultureInfo.InvariantCulture)).ToArray()));

            w.WriteElementString("RemovedEntities", Model.ContainerNamespace,
                string.Join(" ", removedEntities.Select(id => id.ToString(CultureInfo.InvariantCulture)).ToArray()));

            w.WriteObject("HistorySignature", Model.ContainerNamespace, HistorySignature, canonicalizedSignatures);
            w.WriteObject("IndexSignature", Model.ContainerNamespace, IndexSignature, canonicalizedSignatures);
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
                    Guid != Guid.Empty &&
                    !string.IsNullOrWhiteSpace(Software) &&
                    Owner != null && Owner.IsValid &&
                    // !Copyright.IsNullOrWhite() &&
                    HistorySignature != null && HistorySignature.IsValid &&
                    IndexSignature != null && IndexSignature.IsValid;
            }
        }

        #endregion

        #region Implementation of IEquatable<EditionType>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(EditionElement other)
        {
            if (Guid != other.Guid) return false;
            if (Salt != other.Salt) return false;
            if (!string.Equals(Software, other.Software)) return false;
            if (Timestamp != other.Timestamp) return false;
            if (!Equals(Owner, other.Owner)) return false;
            if (!string.Equals(Copyright, other.Copyright)) return false;
            if (!string.Equals(Comments, other.Comments)) return false;
            if (!ObjectUtils.AreEqual(newEntities.ToArray(), other.newEntities.ToArray())) return false;
            if (!ObjectUtils.AreEqual(removedEntities.ToArray(), other.removedEntities.ToArray())) return false;
            if (!Equals(HistorySignature, other.HistorySignature)) return false;
            if (!Equals(IndexSignature, other.IndexSignature)) return false;
            return true;
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is EditionElement && Equals((EditionElement)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            return 7
                * Guid.GetHashCode()
                * (Salt != null ? Salt.GetHashCode() + 23 : 1)
                * (Software != null ? Software.GetHashCode() + 23 : 1)
                * (Timestamp.GetHashCode() + 23)
                * (Owner != null ? Owner.GetHashCode() + 23 : 1)
                * (Copyright != null ? Copyright.GetHashCode() + 23 : 1)
                * (Comments != null ? Comments.GetHashCode() + 23 : 1)
                * (ObjectUtils.GetHashCode(newEntities.ToArray()) + 23)
                * (ObjectUtils.GetHashCode(removedEntities.ToArray()) + 23)
                * (HistorySignature != null ? HistorySignature.GetHashCode() : 1)
                * (IndexSignature != null ? IndexSignature.GetHashCode() : 1);
        }

        #endregion

    }
}