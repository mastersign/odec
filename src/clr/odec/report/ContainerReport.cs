using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec;
using de.mastersign.odec.model;
using de.mastersign.odec.process;
using de.mastersign.odec.storage;

namespace de.mastersign.odec.report
{
    /// <summary>
    /// Represents a report for the container including validation messages and structural information.
    /// </summary>
    public class ContainerReport
    {
        /// <summary>
        /// The namespace of the report XML markup.
        /// </summary>
        public const string NS = "http://www.mastersign.de/odec/report/";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerReport"/> class.
        /// </summary>
        public ContainerReport()
        {
            StructureValidationMessages = new ValidationMessageCollection("Structure");
            StructureValidationMessages.MessageAdded += MessageCollectionMessageAdded;
            ValueValidationMessages = new ValidationMessageCollection("Values");
            ValueValidationMessages.MessageAdded += MessageCollectionMessageAdded;
            CertificateValidationMessages = new ValidationMessageCollection("Certificates");
            CertificateValidationMessages.MessageAdded += MessageCollectionMessageAdded;
            ProfileValidationMessages = new ValidationMessageCollection("Profile");
            ProfileValidationMessages.MessageAdded += MessageCollectionMessageAdded;
        }

        /// <summary>
        /// Gets or sets the handler which is called for all messages after adding the message to its message collection.
        /// </summary>
        public ValidationHandler DefaultValidationHandler { get; set; }

        /// <summary>
        /// Gets the message collection for structual validation.
        /// </summary>
        public ValidationMessageCollection StructureValidationMessages { get; private set; }

        /// <summary>
        /// Gets the message collection for the validation of value content.
        /// </summary>
        public ValidationMessageCollection ValueValidationMessages { get; private set; }

        /// <summary>
        /// Gets the message collection for the validation of the certificates.
        /// </summary>
        public ValidationMessageCollection CertificateValidationMessages { get; private set; }

        /// <summary>
        /// Gets the message collection for the validation of the container against a profile.
        /// </summary>
        public ValidationMessageCollection ProfileValidationMessages { get; private set; }

        /// <summary>
        /// Gets the handler for structural validation. 
        /// This handler is supposed to be given to 
        /// <see cref="Container.Open(IStorage,ValidationHandler)"/>
        /// and related open methods.
        /// </summary>
        public ValidationHandler StructureValidationHandler { get { return StructureValidationMessages.MessageHandler; } }

        /// <summary>
        /// Gets the handler for validation of the value content.
        /// This handler is supposed to be given to <see cref="Container.VerifyEntityValueSignatures"/>.
        /// </summary>
        public ValidationHandler ValueValidationHandler { get { return ValueValidationMessages.MessageHandler; } }

        /// <summary>
        /// Gets the handler for validation of the certificates.
        /// This handler is supposed to be given to <see cref="Container.ValidateCertificates"/>.
        /// </summary>
        public ValidationHandler CertificateValidationHandler { get { return CertificateValidationMessages.MessageHandler; } }

        /// <summary>
        /// Gets the handler for validation of the container against a profile.
        /// This handler is supposed to be given to <see cref="Profile.ValidateContainer"/>
        /// and <see cref="Profile.ValidateEntity"/>.
        /// </summary>
        public ValidationHandler ProfileValidationHandler { get { return ProfileValidationMessages.MessageHandler; } }

        /// <summary>
        /// Gets a value indicating if the report contains any error messages.
        /// </summary>
        public bool ContainsError
        {
            get
            {
                return StructureValidationMessages.ContainsError
                    || ValueValidationMessages.ContainsError
                    || CertificateValidationMessages.ContainsError
                    || ProfileValidationMessages.ContainsError;
            }
        }

        private void MessageCollectionMessageAdded(object sender, ContainerValidationEventArgs e)
        {
            if (DefaultValidationHandler != null) DefaultValidationHandler(e);
        }

        /// <summary>
        /// This method uses the messages collected until now and the given container to create a report in XML format.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="c">The container or <c>null</c> if the containers structure is invalid.</param>
        /// <param name="w">The <see cref="XmlWriter"/> to write the XML markup.</param>
        public void WriteXmlReport(string sourcePath, Container c, XmlWriter w)
        {
            w.WriteStartElement("ContainerReport", NS);

            w.WriteElementString("Source", NS, sourcePath);
            w.WriteElementString("Timestamp", NS, DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
            w.WriteElementString("Summary", NS, ContainsError ? "error" : "success");

            WriteValidationMessages(StructureValidationMessages, w);
            WriteValidationMessages(ValueValidationMessages, w);
            WriteValidationMessages(CertificateValidationMessages, w);
            WriteValidationMessages(ProfileValidationMessages, w);

            WriteContainerStructure(c, w);

            w.WriteEndElement();
        }

        private static void WriteValidationMessages(ValidationMessageCollection vmc, XmlWriter w)
        {
            if (vmc.Count == 0) return;
            w.WriteStartElement("ValidationMessageCollection", NS);
            w.WriteAttributeString("name", vmc.Name);
            w.WriteAttributeString("count", vmc.Count.ToString(CultureInfo.InvariantCulture));
            w.WriteAttributeString("summary", vmc.ContainsError ? "error" : "success");
            foreach (var msg in vmc)
            {
                w.WriteStartElement("ValidationMessage", NS);
                w.WriteAttributeString("severity", msg.Severity == ValidationSeverity.Error ? "error" : "success");
                w.WriteAttributeString("topic", msg.MessageClass.ToString());
                w.WriteString(msg.Message);
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        private void WriteContainerStructure(Container c, XmlWriter w)
        {
            if (c == null) return;

            w.WriteStartElement("Container", NS);

            WriteEdition(c.CurrentEdition, w);

            w.WriteStartElement("History", NS);
            w.WriteAttributeString("count", c.HistoryEditionCount.ToString(CultureInfo.InvariantCulture));
            for (var i = 0; i < c.HistoryEditionCount; i++)
            {
                WriteEdition(c.GetHistoryEdition(i), w);
            }
            w.WriteEndElement();

            w.WriteStartElement("Index", NS);
            w.WriteAttributeString("count", c.EntityCount.ToString(CultureInfo.InvariantCulture));
            foreach (var entityId in c.GetEntityIds())
            {
                WriteEntity(c.GetEntity(entityId), w);
            }
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private static void WriteEdition(EditionElement e, XmlWriter w)
        {
            w.WriteStartElement("Edition", NS);
            w.WriteAttributeString("guid", e.Guid.ToString("D"));
            w.WriteAttributeString("timestamp", e.Timestamp.ToString("O", CultureInfo.InvariantCulture));
            w.WriteElementString("Software", NS, e.Software);
            w.WriteElementString("Profile", NS, e.Profile);
            w.WriteElementString("Version", NS, e.Version);
            w.WriteElementString("SaltState", NS, e.SaltState.ToString());

            WriteOwner(e.Owner, w);

            if (e.Copyright != null)
            {
                w.WriteElementString("Copyright", NS, e.Copyright);
            }
            if (e.Comments != null)
            {
                w.WriteElementString("Comments", NS, e.Comments);
            }

            if (e.NewEntities.Count > 0)
            {
                w.WriteStartElement("NewEntities", NS);
                w.WriteAttributeString("count", e.NewEntities.Count.ToString(CultureInfo.InvariantCulture));
                foreach (var id in e.NewEntities)
                {
                    w.WriteElementString("EntityId", NS, id.ToString(CultureInfo.InvariantCulture));
                }
                w.WriteEndElement();
            }
            if (e.RemovedEntities.Count > 0)
            {
                w.WriteStartElement("RemovedEntities", NS);
                w.WriteAttributeString("count", e.RemovedEntities.Count.ToString(CultureInfo.InvariantCulture));
                foreach (var id in e.RemovedEntities)
                {
                    w.WriteElementString("EntityId", NS, id.ToString(CultureInfo.InvariantCulture));
                }
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        private static void WriteOwner(Owner o, XmlWriter w)
        {
            w.WriteStartElement("Owner", NS);
            w.WriteElementString("Institute", NS, o.Institute);
            w.WriteElementString("Operator", NS, o.Operator);
            w.WriteElementString("Email", NS, o.Email);
            if (o.Role != null)
            {
                w.WriteElementString("Role", NS, o.Role);
            }
            w.WriteEndElement();
        }

        private static void WriteSignature(SignatureWrapper sw, XmlWriter w)
        {
            // nothing
        }

        private static void WriteEntity(Entity e, XmlWriter w)
        {
            w.WriteStartElement("Entity", NS);
            w.WriteAttributeString("id", e.Id.ToString(CultureInfo.InvariantCulture));
            if (e.Label != null) w.WriteAttributeString("label", e.Label);
            w.WriteStartElement("Provenance", NS);
            if (e.Provenance.Guid != Guid.Empty)
            {
                w.WriteElementString("Interface", NS, e.Provenance.Guid.ToString("D"));
            }
            w.WriteEndElement();
            if (e.Type != Guid.Empty)
            {
                w.WriteElementString("Type", NS, e.Type.ToString("D"));
            }
            if (e.Predecessors.Any())
            {
                w.WriteStartElement("Predecessors", NS);
                w.WriteAttributeString("count", e.Predecessors.Count().ToString(CultureInfo.InvariantCulture));
                foreach (var id in e.Predecessors)
                {
                    w.WriteElementString("EntityId", NS, id.ToString(CultureInfo.InvariantCulture));
                }
                w.WriteEndElement();
            }
            if (e.HasProvenanceParameterSet)
            {
                WriteValue(e.GetProvenanceParameterSet(), w, true);
            }
            if (e.ValueNames.Any())
            {
                w.WriteStartElement("Values", NS);
                w.WriteAttributeString("count", e.ValueNames.Count().ToString(CultureInfo.InvariantCulture));
                foreach (var name in e.ValueNames)
                {
                    WriteValue(e.GetValue(name), w);
                }
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        private static void WriteValue(Value v, XmlWriter w, bool isParameterSet = false)
        {
            w.WriteStartElement(isParameterSet ? "ProvenanceParameterSet" : "Value", NS);
            w.WriteAttributeString("name", v.Name);
            w.WriteAttributeString("appearance", v.Appearance.ToString());
            if (v.Type != Guid.Empty)
            {
                w.WriteElementString("Type", NS, v.Type.ToString("D"));
            }
            w.WriteElementString("Size", NS, v.Size.ToString(CultureInfo.InvariantCulture));
            w.WriteEndElement();
        }
    }
}
