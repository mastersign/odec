using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Security;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// An implementation of <see cref="IRandomGenerator"/> based on the
    /// Bouncy Castle Crypto library.
    /// </summary>
    public class BouncyCastleRandomGenerator : IRandomGenerator
    {
        private readonly SecureRandom rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="BclRandomGenerator"/> class.
        /// </summary>
        public BouncyCastleRandomGenerator()
        {
            rng = new SecureRandom();
        }

        /// <summary>
        /// Generates cryptographic random data.
        /// </summary>
        /// <param name="buffer">The buffer to write the random data.</param>
        public void GenerateRandomData(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            rng.NextBytes(buffer);
        }
    }
}
