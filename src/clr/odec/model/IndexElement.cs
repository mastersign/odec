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
    /// Repesents the XML element <c>Index</c>, which is a list of
    /// entites. 
    /// It resides in the namespace <see cref="Model.ContainerNamespace"/>.
    /// An entity is identified by an id.
    /// </summary>
    public class IndexElement : IXmlStorable2, IEquatable<IndexElement>
    {
        ///<summary>
        /// The name of the XML element, represented by this class.
        ///</summary>
        public const string XML_NAME = "Index";

        private readonly List<IndexItemElement> items = new List<IndexItemElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexElement"/> class.
        /// </summary>
        public IndexElement()
        {
            LastId = -1;
        }

        /// <summary>
        /// Gets the index items as array.
        /// </summary>
        /// <value>The index items.</value>
        public IndexItemElement[] Items
        {
            get { return items.ToArray(); }
        }

        /// <summary>
        /// Gets the number of items in the index.
        /// </summary>
        /// <value>The number of items in the index.</value>
        public int Count { get { return items.Count; } }

        /// <summary>
        /// Gets or sets the last used entity id.
        /// </summary>
        /// <value>The last used entity id or <c>-1</c> if no entity was added yet.</value>
        internal int LastId { get; set; }

        /// <summary>
        /// Gets an id for a new entity.
        /// </summary>
        /// <returns>The next entity id.</returns>
        /// <remarks>
        /// The result of this method changes,
        /// if <see cref="Add"/> is called and
        /// an entity reference with a valid id is given.
        /// </remarks>
        public int GetNextEntityId()
        {
            return LastId + 1;
        }

        /// <summary>
        /// Adds the specified new index item.
        /// </summary>
        /// <param name="newItem">The new index item.</param>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="newItem"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the given index item is referencing
        /// to an invalid entity id.
        /// An entity id is invalid, if it is allready used
        /// or if it is less than 1. To avoid the usage of an invalid 
        /// entity id, use <see cref="GetNextEntityId"/>.
        /// </exception>
        public void Add(IndexItemElement newItem)
        {
            if (newItem == null) throw new ArgumentNullException("newItem");
            if (newItem.Id <= LastId)
            {
                throw new ArgumentException(
                    Resources.IndexElement_Add_ArgumentException_InvalidEntityReference, "newItem");
            }
            items.Add(newItem);
            LastId = newItem.Id;
        }

        /// <summary>
        /// Determines whether the index contains an entity reference
        /// with the given id.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>
        /// 	<c>true</c> if the index contains an item with the specified id; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(int id)
        {
            return items.Select(item => item.Id).Contains(id);
        }

        /// <summary>
        /// Gets the index item for the specified entity id.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>The index item, referencing the entity with the given id.</returns>
        /// <exception cref="ArgumentException">
        /// Is thrown, if no index item exists for the given entity id.
        /// </exception>
        public IndexItemElement GetItem(int id)
        {
            if (!Contains(id))
            {
                throw new ArgumentException(
                    string.Format(Resources.IndexElement_ArgumentException_EntityIdNotFound, id),
                    "id");
            }
            return items.First(item => item.Id == id);
        }

        /// <summary>
        /// Removes the specified index item.
        /// </summary>
        /// <param name="id">The id of the index item.</param>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the index does not conatin the specified item.
        /// </exception>
        public void Remove(int id)
        {
            if (!Contains(id))
            {
                throw new ArgumentException(
                    string.Format(Resources.IndexElement_ArgumentException_EntityIdNotFound, id),
                    "id");
            }
            items.Remove(GetItem(id));
        }

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
            if (items.Count <= 0)
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure, 
                    Resources.IndexElement_Validate_IndexIsEmpty);
                result = false;
            }

            foreach (var item in items)
            {
                var itemR = item;
                if (!itemR.IsValid)
                {
                    messageHandler.Error(ValidationMessageClass.ContainerStructure, 
                        Resources.IndexElement_Validate_IndexItemIsInvalid, item.Id);
                    result = false;
                }
                if (items.Any(i => i != itemR && itemR.Label != null && i.Label == itemR.Label))
                {
                    messageHandler.Error(ValidationMessageClass.ContainerStructure,
                        Resources.IndexElement_Validate_MultipleUseOfLabel, item.Id);
                    result = false;
                }
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure, 
                    Resources.IndexElement_Validate_ValidStructure);
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
            LastId = e.ReadParsedObject("c:LastId", LastId, int.Parse);
            items.Clear();
            items.AddRange(e.ReadObjects<IndexItemElement>("c:" + IndexItemElement.XML_NAME));
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
                throw new InvalidOperationException(Resources.XmlStorable_WriteToXml_ThisInstanceIsInvalid);
            }
            w.WriteElementString("LastId", Model.ContainerNamespace, LastId.ToString(CultureInfo.InvariantCulture));
            foreach (var item in items)
            {
                w.WriteObject(IndexItemElement.XML_NAME, Model.ContainerNamespace, 
                    item, canonicalizedSignatures);
            }
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
                return items.Count > 0
                    && items.All(item => item.IsValid);
            }
        }

        #endregion

        #region Implementation of IEquatable<IndexElement>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(IndexElement other)
        {
            return
                LastId == other.LastId &&
                ObjectUtils.AreEqual(Items, other.Items);
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is IndexElement && Equals((IndexElement)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            return items.Aggregate(
                7 * (LastId + 23),
                (current, item) => current * (item.GetHashCode() + 23));
        }

        #endregion

    }
}