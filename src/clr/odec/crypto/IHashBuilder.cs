using System.IO;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// Represents the capability to compute a hash.
    /// </summary>
    public interface IHashBuilder
    {
        /// <summary>
        /// Gets the stream, the data must be written to.
        /// </summary>
        /// <value>The stream.</value>
        Stream Stream { get; }

        /// <summary>
        /// Finalizes the computation of the hash, after all data is written to the <see cref="Stream"/>.
        /// </summary>
        /// <remarks>
        /// After calling this method, no more data can be written to the <see cref="Stream"/>.
        /// </remarks>
        /// <returns>The computed hash.</returns>
        byte[] ComputeHash();

        /// <summary>
        /// Gets a <see cref="string"/>, identifying the hash method.
        /// </summary>
        /// <remarks>
        /// For the algorithm SHA1, for example, the URI
        /// <c>http://www.w3.org/2000/09/xmldsig#sha1</c> should be returned.
        /// </remarks>
        /// <value>The hash method.</value>
        string HashMethod { get; }
    }
}