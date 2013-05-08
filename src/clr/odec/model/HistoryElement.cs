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
    /// Represents the XML element <c>History</c>, which stores all past
    /// editions along with the past master signatures of the container.
    /// It resides in the namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    /// <remarks>The history element is implementing a stack for history items.
    /// If a container is extended, the past edition and the past master signature
    /// are wrapped into a <see cref="HistoryItemElement"/> and pasted into the history
    /// by calling <see cref="Push"/>. 
    /// The most recent <see cref="HistoryItemElement"/> 
    /// is available by calling <see cref="Peek"/>. 
    /// The history can be rewound by calling <see cref="Pop"/>. This removes
    /// the most recent item and returns it to the caller.
    /// </remarks>
    public class HistoryElement : IXmlStorable2, IEquatable<HistoryElement>
    {
        ///<summary>
        /// The name of the XML element, represented by this class.
        ///</summary>
        public const string XML_NAME = "History";

        private readonly List<HistoryItemElement> items = new List<HistoryItemElement>();

        /// <summary>
        /// Gets an array with all items of the history.
        /// </summary>
        /// <value>The history items.</value>
        public HistoryItemElement[] Items
        {
            get { return items.ToArray(); }
        }

        /// <summary>
        /// Pushes the specified item on to the history.
        /// </summary>
        /// <param name="item">The past item.</param>
        public void Push(HistoryItemElement item)
        {
            if (item == null) throw new ArgumentNullException("item");
            items.Add(item);
        }

        /// <summary>
        /// Takes the most recent item from the history and removes it.
        /// </summary>
        /// <returns>The most recent <see cref="HistoryItemElement"/>.</returns>
        /// <exception cref="InvalidOperationException">If the history contains no items.</exception>
        public HistoryItemElement Pop()
        {
            var lastIndex = Count - 1;
            if (lastIndex < 0)
            {
                throw new InvalidOperationException(Resources.HistoryElement_InvalidOperation_NoItems);
            }
            var result = items[lastIndex];
            items.RemoveAt(lastIndex);
            return result;
        }

        /// <summary>
        /// Gets the most recent item from the history, but does not remove it.
        /// </summary>
        /// <returns>The most recent <see cref="HistoryItemElement"/> from the history.</returns>
        public HistoryItemElement Peek()
        {
            var lastIndex = Count - 1;
            if (lastIndex < 0)
            {
                throw new InvalidOperationException(Resources.HistoryElement_InvalidOperation_NoItems);
            }
            return items[lastIndex];
        }

        /// <summary>
        /// Removes all items from the history.
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Gets the number of <see cref="HistoryItemElement"/> in the history.
        /// </summary>
        /// <value>The number of items.</value>
        public int Count
        {
            get { return items.Count; }
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
            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].IsValid)
                {
                    messageHandler.Error(ValidationMessageClass.ContainerStructure, 
                        Resources.HistoryElement_Validate_InvalidItem, i);
                    result = false;
                }
            }
            if (result)
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure, 
                    Resources.HistoryElement_Validate_StructureIsValid);
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
            items.Clear();
            foreach (var item in e.ReadObjects<HistoryItemElement>("c:" + HistoryItemElement.XML_NAME))
            {
                items.Add(item);
            }
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
            for (int i = 0; i < Count; i++)
            {
                w.WriteObject(HistoryItemElement.XML_NAME, Model.ContainerNamespace, 
                    items[i], canonicalizedSignatures);
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
            get { return items.All(i => i.IsValid); }
        }

        #endregion

        #region Implementation of IEquatable<HistoryElement>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(HistoryElement other)
        {
            return ObjectUtils.AreEqual(Items, other.Items);
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is HistoryElement && Equals((HistoryElement) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            return items.Aggregate(7, (current, item) => current*(item.GetHashCode() + 23));
        }

        #endregion

    }
}