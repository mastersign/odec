using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.Properties;
using de.mastersign.odec.model;

namespace de.mastersign.odec.process
{
    /// <summary>
    /// The abstract base class for catalogs.
    /// </summary>
    /// <typeparam name="T">The type of the items in the catalog.</typeparam>
    public abstract class CatalogBase<T> : ICollection<T>
        where T : CatalogItem, new()
    {
        private readonly Dictionary<Guid, T> dict = new Dictionary<Guid, T>();

        /// <summary>
        /// Loads the catalog items from a <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> containing the catalog items.</param>
        public virtual void LoadFromXml(XmlElement e)
        {
            dict.Clear();
            var template = new T();
            var items = e.SelectNodes("p:" + template.XmlName, Model.NamespaceManager);
            if (items != null)
            {
                foreach (XmlElement itemE in items)
                {
                    var item = new T();
                    item.LoadFromXml(itemE);
                    Add(item);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="CatalogItem"/> of type <typeparamref name="T"/> 
        /// with the specified ID.
        /// </summary>
        /// <value>The element with the given ID.</value>
        public T this[Guid id]
        {
            get
            {
                T res;
                if (!dict.TryGetValue(id, out res)) throw new KeyNotFoundException();
                return res;
            }
        }

        /// <summary>
        /// Determines whether the catalog contains an element with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the element.</param>
        /// <returns>
        /// 	<c>true</c> if the catalog contains an element with the the specified ID; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Guid id)
        {
            return dict.ContainsKey(id);
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return dict.Values.GetEnumerator();
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

        #region Implementation of ICollection<T>

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(T item)
        {
            dict.Add(item.Id, item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Clear()
        {
            dict.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> in <see cref="T:System.Collections.Generic.ICollection`1"/> is found in the 
        /// <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(T item)
        {
            return dict.ContainsValue(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> 
        /// to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination 
        /// of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. 
        /// The <see cref="T:System.Array"/> must have zero-based indexing. </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="IndexOutOfRangeException">if <paramref name="arrayIndex"/> exceeds the limits of the array.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="array"/> is multidimensional.
        /// – oder –The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/>
        /// is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// – oder –Type <typeparamref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("arrayIndex");
            if (array.Rank > 1) throw new ArgumentException(
                Resources.CatalogBase_CopyTo_ArgumentException_MoreThanOneDimension, "array");
            if (arrayIndex >= array.Length) throw new IndexOutOfRangeException();
            if (array.Length - arrayIndex < dict.Count) throw new ArgumentException(
                Resources.CatalogBase_CopyTo_ArgumentException_NotEnoughSpace);
            foreach (var kvp in dict)
            {
                array[arrayIndex++] = kvp.Value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>;
        /// otherwise, <c>false</c>.
        /// This method also returns <c>false</c> if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(T item)
        {
            return dict.ContainsKey(item.Id) && dict.Remove(item.Id);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count { get { return dict.Count; } }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/>is read-only.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <see cref="T:System.Collections.Generic.ICollection`1"/>is read-only; otherwise <c>false</c>.
        /// </returns>
        public bool IsReadOnly { get { return false; } }

        #endregion
    }
}
