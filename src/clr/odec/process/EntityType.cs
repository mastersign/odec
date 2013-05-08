using System;
using System.Collections;
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
    /// The definition of an entity type with all value descriptions.
    /// </summary>
    public class EntityType : CatalogItem, IEnumerable<EntityType.Value>
    {
        /// <summary>
        /// Gets the name of the XML element representing this class of catalog items.
        /// </summary>
        /// <value>The name of the XML element representing this class of catalog items.</value>
        public override string XmlName { get { return "EntityType"; } }

        /// <summary>
        /// Gets or sets the severity of the entity type.
        /// An open entity type can have more than the specified entity values.
        /// A strict entity type must not have more than the specified entity values.
        /// </summary>
        /// <value>The severity of the entity type.</value>
        public ValueCheckSeverity Severity { get; set; }

        private readonly List<Value> values = new List<Value>();

        /// <summary>
        /// Loads the catalog item definition from a <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> containing the catalog item definition.</param>
        /// <exception cref="FormatException">Is thrown, if the value of the <c>guid</c> attribute
        /// in the given <see cref="XmlElement"/> is no valid <see cref="Guid"/>.</exception>
        public override void LoadFromXml(XmlElement e)
        {
            base.LoadFromXml(e);

            values.Clear();

            var valueCollectionE = (XmlElement)e.SelectSingleNode("p:Values", Model.NamespaceManager);
            if (valueCollectionE == null)
            {
                return;
            }

            Severity = ValueCheckSeverity.Strict;
            if (valueCollectionE.HasAttribute("severity"))
            {
                var severityTxt = valueCollectionE.GetAttribute("severity");
                switch (severityTxt.ToLower())
                {
                    case "strict":
                        Severity = ValueCheckSeverity.Strict;
                        break;
                    case "open":
                        Severity = ValueCheckSeverity.Open;
                        break;
                }
            }

            var valueEList = valueCollectionE.SelectNodes("p:Value", Model.NamespaceManager);
            if (valueEList != null)
            {
                foreach (XmlElement valueE in valueEList)
                {
                    var value = new Value();
                    value.LoadFromXml(valueE);
                    values.Add(value);
                }
            }
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<Value> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Gets an <see cref="Array"/> with all value descriptions for the entity type.
        /// </summary>
        /// <returns>All values of the entity type.</returns>
        public Value[] GetValues()
        {
            return values.ToArray();
        }

        /// <summary>
        /// Determines whether a value with the specified name is part of the entity type.
        /// </summary>
        /// <param name="valueName">Name of the value.</param>
        /// <returns>
        /// 	<c>true</c> if a value with the specified name is part of this entity type; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string valueName)
        {
            return values.Any(v => v.Name == valueName);
        }

        internal bool ValidateEntity(Profile profile, Entity entity, ValidationHandler handler)
        {
            bool valid = true;

            // Check all values accordingg to the entity type
            var entityValueNames = entity.ValueNames.ToList();
            foreach (var value in values)
            {
                if (entityValueNames.Contains(value.Name))
                {
                    var valueHandle = entity.GetValue(value.Name);
                    if (valueHandle.Type != value.DataTypeRef)
                    {
                        handler.Error(ValidationMessageClass.Entity,
                            Resources.EntityType_ValidateEntity_DataTypeMismatch,
                            value.Name, entity.Id);
                        valid = false;
                    }
                    else
                    {
                        // TODO aktivate entity value validation

                        // var dataType = profile.DataTypes[value.DataTypeRef];
                        //if (!dataType.ValidateValue(entity.Parent.Storage.Read(valueHandle.ValuePath), handler))
                        //{
                        //    valid = false;
                        //}
                    }
                }
                else if (value.Usage == ValueUsage.Required)
                {
                    handler.Error(ValidationMessageClass.Entity,
                        Resources.EntityType_ValidateEntity_RequiredValueMissing,
                        value.Name, entity.Id, Name);
                    valid = false;
                }
            }

            if (Severity == ValueCheckSeverity.Strict)
            {
                // Check for values, which don't belong to the entity type
                foreach (var valueName in entityValueNames)
                {
                    if (!Contains(valueName))
                    {
                        handler.Error(ValidationMessageClass.Entity,
                            Resources.EntityType_ValidateEntity_UnexpectedValue,
                            valueName, entity.Id, Name);
                        valid = false;
                    }
                }
            }

            // Check for provenance
            if (profile.ProvenanceInterfaces.Contains(entity.Provenance.Guid))
            {
                var provenance = profile.ProvenanceInterfaces[entity.Provenance.Guid];
                if (provenance.OutputType != entity.Type)
                {
                    handler.Error(ValidationMessageClass.Entity, 
                        Resources.EntityType_ValidateEntity_ProvenanceMismatch,
                        Name, entity.Id, provenance.Name);
                    valid = false;
                }
            }
            else
            {
                handler.Error(ValidationMessageClass.Entity, 
                    Resources.EntityType_ValidateEntity_UnknownProvenance,
                    entity.Provenance.Guid, entity.Id);
                valid = false;
            }

            if (valid)
            {
                handler.Success(ValidationMessageClass.Entity, 
                    Resources.EntityType_ValidateEntity_Valid, entity.Id, entity.Label ?? Resources.Unlabeled, Name);
            }
            return valid;
        }

        /// <summary>
        /// The definition of an entity value as part of an entity type.
        /// </summary>
        public class Value
        {
            /// <summary>
            /// Gets the name of the value.
            /// </summary>
            /// <value>The name of the value.</value>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the description of the value.
            /// </summary>
            /// <value>The description of the value.</value>
            public string Description { get; private set; }

            /// <summary>
            /// Gets the reference to the data type of the value.
            /// </summary>
            /// <value>The data type reference.</value>
            public Guid DataTypeRef { get; private set; }

            /// <summary>
            /// Gets a value, describing the usage of the value.
            /// </summary>
            /// <value>The usage of the value.</value>
            public ValueUsage Usage { get; private set; }

            /// <summary>
            /// Loads the value description from a <see cref="XmlElement"/>.
            /// </summary>
            /// <param name="e">The <see cref="XmlElement"/> containing the description of the entity value.</param>
            public void LoadFromXml(XmlElement e)
            {
                Name = e.ReadElementString("p:Name", null);
                Description = e.ReadElementString("p:Description", null);
                DataTypeRef = e.ReadParsedObject(
                    "p:DataTypeRef", Guid.Empty, 
                    v => new Guid(v));
                Usage = e.ReadParsedObject(
                    "p:Usage", ValueUsage.Required,
                    v => (ValueUsage) Enum.Parse(typeof (ValueUsage), v, true));
            }
        }

        /// <summary>
        /// An enumeration with all possible severity values for the value collection of an entity type.
        /// </summary>
        public enum ValueCheckSeverity
        {
            /// <summary>
            /// Only the values listed in the value collection of the entity type are permitted.
            /// Additional values in an entity causes a validation error.
            /// </summary>
            Strict,

            /// <summary>
            /// Additional values, not listed in the value collection of the entity type,
            /// are ignored while validating the container against the profile.
            /// </summary>
            Open,
        }

        /// <summary>
        /// An enumeration with all usage settings for an entity value.
        /// </summary>
        public enum ValueUsage
        {
            /// <summary>
            /// The value needs to be present in an entity.
            /// </summary>
            /// <remarks>
            /// This setting is independent of the value appearance.
            /// </remarks>
            Required,

            /// <summary>
            /// The value is optional and an entity is valid without it.
            /// </summary>
            /// <remarks>
            /// This setting is independent of the value appearance.
            /// </remarks>
            Optional
        }

    }
}
