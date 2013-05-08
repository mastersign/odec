using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// This class provides cryptographic factories depending on the cryptographic configuration.
    /// </summary>
    public static class Configuration
    {
        private static readonly ICryptoFactory cryptoFactory;
        private static readonly IHashProvider hashProvider;
        private static readonly IRandomGenerator randomGenerator;
        private static readonly IXmlCanonicalizer canonicalizer;

        static Configuration()
        {
            cryptoFactory = new BouncyCastleCryptoFactory();
            hashProvider = cryptoFactory.CreateHashProvider();
            randomGenerator = new BouncyCastleRandomGenerator();
            canonicalizer = new BclC14NCanonicalizer();
        }

        /// <summary>
        /// Gets a factory for cryptographic providers.
        /// </summary>
        public static ICryptoFactory CryptoFactory { get { return cryptoFactory; } }

        /// <summary>
        /// Gets a provider to build cryptographic hashes.
        /// </summary>
        public static IHashProvider HashProvider { get { return hashProvider; } }

        /// <summary>
        /// Gets a cryptographic random generator.
        /// </summary>
        public static IRandomGenerator RandomGenerator { get { return randomGenerator; } }

        /// <summary>
        /// Gets a canonicalizer to normalize XML documents.
        /// </summary>
        public static IXmlCanonicalizer Canonicalizer { get { return canonicalizer; } }

    }
}
