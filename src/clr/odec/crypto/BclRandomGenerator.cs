using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// An implementation of <see cref="IRandomGenerator"/> using the BLC implementation
    /// for a cryptographic random generator.
    /// </summary>
    public class BclRandomGenerator : IRandomGenerator
    {
        private readonly RandomNumberGenerator rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="BclRandomGenerator"/> class.
        /// </summary>
        public BclRandomGenerator()
        {
            rng = RandomNumberGenerator.Create();
        }

        /// <summary>
        /// Generates cryptographic random data.
        /// </summary>
        /// <param name="buffer">The buffer to write the random data.</param>
        public void GenerateRandomData(byte[] buffer)
        {
            rng.GetBytes(buffer);
        }
    }
}
