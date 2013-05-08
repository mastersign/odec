using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// Represents the capability to provide a method to compute cryptographic random data.
    /// </summary>
    public interface IRandomGenerator
    {
        /// <summary>
        /// Generates cryptographic random data.
        /// </summary>
        /// <param name="buffer">The buffer to write the random data.</param>
        /// <exception cref="ArgumentNullException">if <c>null</c> is given for <paramref name="buffer"/></exception>
        void GenerateRandomData(byte[] buffer);
    }
}
