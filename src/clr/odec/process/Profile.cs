using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.Properties;
using de.mastersign.odec.model;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.process
{
    /// <summary>
    /// A process profile for the forensic container.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// Gets the name of the profile.
        /// </summary>
        /// <value>The name of the profile.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the version of the profile.
        /// </summary>
        /// <value>The version of the profile.</value>
        public string Version { get; private set; }

        /// <summary>
        /// Gets the description of the profile.
        /// </summary>
        /// <value>The description of the profile.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the data type catalog of the profile.
        /// </summary>
        /// <value>The data types in the profile.</value>
        public DataTypeCatalog DataTypes { get; private set; }

        /// <summary>
        /// Gets the entity type catalog of the profile.
        /// </summary>
        /// <value>The entity types in the profile.</value>
        public EntityTypeCatalog EntityTypes { get; private set; }

        /// <summary>
        /// Gets the provenance interface catalog of the profile.
        /// </summary>
        /// <value>The provenance interfaces in the profile.</value>
        public ProvenanceInterfaceCatalog ProvenanceInterfaces { get; private set; }

        /// <summary>
        /// Loads the profile from a <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="profileElement">The <see cref="XmlElement"/> <c>Profile</c> 
        /// from the namespace <see cref="Model.ProfileNamespace"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="profileElement"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the given <see cref="XmlElement"/> is not a <c>Profile</c> element
        /// in the namespace <see cref="Model.ProfileNamespace"/>.
        /// </exception>
        public void LoadFromXml(XmlElement profileElement)
        {
            if (profileElement == null) throw new ArgumentNullException("profileElement");

            if (profileElement.NamespaceURI != Model.ProfileNamespace)
            {
                throw new ArgumentException(Resources.Profile_LoadFromXml_WrongNamespace, "profileElement");
            }
            if (profileElement.Name != "Profile")
            {
                throw new ArgumentException(Resources.Profile_LoadFromXml_WrongXmlElement, "profileElement");
            }

            Name = profileElement.ReadElementString("p:Name", null);
            Version = profileElement.ReadElementString("p:Version", null);
            Description = profileElement.ReadElementString("p:Description", null);

            DataTypes = new DataTypeCatalog();
            DataTypes.LoadFromXml(profileElement.SelectSingleNode(
                "p:DataTypeCatalog", Model.NamespaceManager) as XmlElement);

            EntityTypes = new EntityTypeCatalog();
            EntityTypes.LoadFromXml(profileElement.SelectSingleNode(
                "p:EntityTypeCatalog", Model.NamespaceManager) as XmlElement);

            ProvenanceInterfaces = new ProvenanceInterfaceCatalog();
            ProvenanceInterfaces.LoadFromXml(profileElement.SelectSingleNode(
                "p:ProvenanceInterfaceCatalog", Model.NamespaceManager) as XmlElement);
        }

        /// <summary>
        /// Loads the profile from a <see cref="XmlReader"/> and validates it with the XMLSchema.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="reader"/>.
        /// </exception>
        /// <exception cref="FormatException">
        /// Is thrown, if the given XML document is not schema conform.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the XML document does not has a Profile XML element at the root.
        /// </exception>
        public void LoadFromXml(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            var doc = new XmlDocument();
            doc.Load(reader);

            string errMsg;
            if (!doc.IsSchemaConform(out errMsg))
            {
                throw new FormatException(
                    string.Format(Resources.Profile_LoadFromXml_FormatException_ProfileNotSchemaConform, errMsg));
            }
            LoadFromXml(doc.DocumentElement);
        }

        /// <summary>
        /// Validates the container with this profile description.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> to be validated.</param>
        /// <param name="handler">A <see cref="ValidationHandler"/> to receive validation messages.
        /// At least one error message is sent to the <paramref name="handler"/> if the validation failes.</param>
        /// <returns><c>true</c> if the container is valid; otherwise <c>false</c>.</returns>
        public bool ValidateContainer(Container container, ValidationHandler handler)
        {
            if (container == null) throw new ArgumentNullException("container");

            if (container.CurrentEdition.Profile != Name)
            {
                handler.Error(ValidationMessageClass.ContainerProfile, 
                    Resources.Profile_ValidateContainer_ProfileMismatch,
                    container.CurrentEdition.Profile, Name);
                return false;
            }
            if (container.CurrentEdition.Version != Version)
            {
                handler.Success(ValidationMessageClass.ContainerProfile,
                    Resources.Profile_ValidateContainer_ProfileVersionMismatch,
                    container.CurrentEdition.Version, Version);
//                return false;
            }

            var valid = true;
            foreach (var id in container.GetEntityIds())
            {
                var entity = container.GetEntity(id);
                if (!ValidateEntity(entity, handler))
                {
                    valid = false;
                }
            }
            return valid;
        }

        /// <summary>
        /// Validates an <see cref="Entity"/> against this <see cref="Profile"/>.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> which will be validated.</param>
        /// <param name="handler">A <see cref="ValidationHandler"/> to receive validation messages.
        /// At least one error message will be sent to the <paramref name="handler"/> if the validation failes.</param>
        /// <returns>
        /// 	<c>true</c> if the entity is valid; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="entity"/>.
        /// </exception>
        public bool ValidateEntity(Entity entity, ValidationHandler handler)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            if (!EntityTypes.Contains(entity.Type))
            {
                handler.Error(ValidationMessageClass.Entity, 
                    Resources.Profile_ValidateEntity_UnkownEntityType, entity.Type.ToString("D"), entity.Id);
                return false;
            }
            return EntityTypes[entity.Type].ValidateEntity(this, entity, handler);
        }
    }
}
