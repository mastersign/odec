using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// Represents the capability to provide a method to compute hashes.
    /// </summary>
    public interface IHashProvider
    {
        /// <summary>
        /// Creates a <see cref="IHashBuilder"/> with a target <see cref="Stream"/>.
        /// By calling this method, the data, written to the <see cref="IHashBuilder"/>,
        /// is passed on to the given target stream.
        /// </summary>
        /// <param name="hashMethod">The requested hash method.</param>
        /// <param name="target">A target stream.</param>
        /// <returns>A <see cref="IHashBuilder"/> instance.</returns>
        /// <exception cref="NotSupportedException">
        /// Is thrown, if the requested Hash Algorithm is not supported.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="hashMethod"/> or <paramref name="target"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the given stream is not writeable.
        /// </exception>
        IHashBuilder CreateHashBuilder(string hashMethod, Stream target);

        /// <summary>
        /// Creates a <see cref="IHashBuilder"/>.
        /// By calling this method, the data, written to the <see cref="IHashBuilder"/>,
        /// is rejected after using it to compute the hash.
        /// </summary>
        /// <param name="hashMethod">The requested hash method.</param>
        /// <returns>A <see cref="IHashBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="hashMethod"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Is thrown, if the requested hash algorithm is not supported.
        /// </exception>
        IHashBuilder CreateHashBuilder(string hashMethod);

        /// <summary>
        /// Gets the supported hash methods.
        /// </summary>
        /// <returns>An array with all supported hash algorithms.</returns>
        string[] GetSupportedMethods();
    }
}