using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.model;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.process
{
    /// <summary>
    /// The definition of a provenance interface.
    /// </summary>
    public class ProvenanceInterface : CatalogItem
    {
        /// <summary>
        /// Gets the name of the XML element representing this class of catalog items.
        /// </summary>
        /// <value>The name of the XML element representing this class of catalog items.</value>
        public override string XmlName { get { return "ProvenanceInterface"; } }

        /// <summary>
        /// Gets the description of the provenance parameter set or <c>null</c> if this provenance has no parameter set.
        /// </summary>
        /// <value>The parameter set or <c>null</c>.</value>
        public ParameterSetDescription ParameterSet { get; private set; }

        private readonly List<Guid> inputTypes = new List<Guid>();


        /// <summary>
        /// Gets an <see cref="Array"/> with references to the input entity types.
        /// </summary>
        /// <returns>An <see cref="Array"/> with references to the input entity types.</returns>
        public Guid[] GetInputTypeRefs()
        {
            return inputTypes.ToArray();
        }


        /// <summary>
        /// Gets a references to the output entity type.
        /// </summary>
        /// <returns>The reference to the output entity type.</returns>
        public Guid OutputType { get; private set; }

        /// <summary>
        /// Loads the catalog item definition from a <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> containing the catalog item definition.</param>
        /// <exception cref="FormatException">Is thrown, if the value of the <c>guid</c> attribute
        /// in the given <see cref="XmlElement"/> is no valid <see cref="Guid"/>.</exception>
        public override void LoadFromXml(XmlElement e)
        {
            base.LoadFromXml(e);

            inputTypes.Clear();
            var inputTypeEList = e.SelectNodes("p:Input/p:EntityTypeRef", Model.NamespaceManager);
            if (inputTypeEList != null)
            {
                foreach (XmlElement inputTypeE in inputTypeEList)
                {
                    inputTypes.Add(new Guid(inputTypeE.InnerText.Trim()));
                }
            }

            OutputType = e.ReadParsedObject("p:Output/p:EntityTypeRef", Guid.Empty, v => new Guid(v));

            var parameterSetE = e.SelectSingleNode("p:ParameterSet", Model.NamespaceManager) as XmlElement;
            if (parameterSetE != null)
            {
                ParameterSet = new ParameterSetDescription();
                ParameterSet.LoadFromXml(parameterSetE);
            }
            else
            {
                ParameterSet = null;
            }
        }

        /// <summary>
        /// The definition of a provenance parameter set as part of a provenance.
        /// </summary>
        public class ParameterSetDescription
        {
            /// <summary>
            /// Gets the name of the parameter set.
            /// </summary>
            /// <value>The name of the parameter set.</value>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the description of the parameter set.
            /// </summary>
            /// <value>The description of the parameter set.</value>
            public string Description { get; private set; }

            /// <summary>
            /// Gets the reference to the data type of the parameter set.
            /// </summary>
            /// <value>The data type reference.</value>
            public Guid DataTypeRef { get; private set; }

            /// <summary>
            /// Loads the parameter set description from a <see cref="XmlElement"/>.
            /// </summary>
            /// <param name="e">The <see cref="XmlElement"/> containing the description of the provenance parameter set.</param>
            public void LoadFromXml(XmlElement e)
            {
                Name = e.ReadElementString("p:Name", null);
                Description = e.ReadElementString("p:Description", null);
                DataTypeRef = e.ReadParsedObject(
                    "p:DataTypeRef", Guid.Empty,
                    v => new Guid(v));
            }
        }

    }
}
