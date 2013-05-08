using System.Xml;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// Extends the <see cref="IXmlStorable"/> interface with an alternative
    /// <see cref="WriteToXml"/> method for writing XML signatures in a canonicalized fashion.
    /// </summary>
    /// <seealso cref="CompatibilityFlags"/>
    public interface IXmlStorable2 : IXmlStorable
    {
        /// <summary>
        /// Writes the state of the object to an XML target.
        /// </summary>
        /// <param name="w">The <see cref="XmlWriter"/> used as target.</param>
        /// <param name="canonicalizedSignatures"><c>true</c>, if the embeded signatures
        /// must be written unindented and canonicalized; otherwise <c>false</c>.</param>
        /// <remarks>The parameter <paramref name="canonicalizedSignatures"/> is intended 
        /// to be used for compatibility reasons.</remarks>
        /// <seealso cref="CompatibilityFlags"/>
        void WriteToXml(XmlWriter w, bool canonicalizedSignatures);
    }
}