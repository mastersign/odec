using System.Xml;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// Represents the capability to load the state of the object from a <see cref="XmlElement"/> and to write the state to a <see cref="XmlWriter"/>.
    /// </summary>
    public interface IXmlStorable
    {
        /// <summary>
        /// Loads the state of the object from an XML source.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> used as source.</param>
        void ReadFromXml(XmlNode e);

        /// <summary>
        /// Writes the state of the object to an XML target.
        /// </summary>
        /// <param name="w">The <see cref="XmlWriter"/> used as target.</param>
        void WriteToXml(XmlWriter w);

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// The instance is valid, if a call to <see cref="WriteToXml"/>
        /// produces schema conform XML.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        bool IsValid { get; }
    }
}