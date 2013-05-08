using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.IO;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// An implementation of <see cref="IHashProvider"/> based on the
    /// Bouncy Castle Crypto library.
    /// </summary>
    /// <remarks>
    /// <a href="http://www.bouncycastle.org/csharp/">Bouncy Castle Website</a>
    /// </remarks>
    public class BouncyCastleHashProvider : IHashProvider
    {
        private static readonly Dictionary<string, Func<Stream, IHashBuilder>> factories =
            new Dictionary<string, Func<Stream, IHashBuilder>>();

        static BouncyCastleHashProvider()
        {
            AddMethod(AlgorithmIdentifier.MD5, new MD5Digest());
            AddMethod(AlgorithmIdentifier.SHA1, new Sha1Digest());
            AddMethod(AlgorithmIdentifier.SHA256, new Sha256Digest());
            AddMethod(AlgorithmIdentifier.SHA384, new Sha384Digest());
            AddMethod(AlgorithmIdentifier.SHA512, new Sha512Digest());
            AddMethod(AlgorithmIdentifier.RIPEMD160, new RipeMD160Digest());
            AddMethod(AlgorithmIdentifier.RIPEMD256, new RipeMD256Digest());
            AddMethod(AlgorithmIdentifier.RIPEMD320, new RipeMD320Digest());
            AddMethod(AlgorithmIdentifier.WHIRLPOOL, new WhirlpoolDigest());
        }

        private static void AddMethod(string id, IDigest digest)
        {
            factories.Add(id, s => new HashBuilder(s, digest, id));
        }

        #region Implementation of IHashProvider

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
        public IHashBuilder CreateHashBuilder(string hashMethod, Stream target)
        {
            if (hashMethod == null) throw new ArgumentNullException("hashMethod");
            if (target == null) throw new ArgumentNullException("target");
            if (!target.CanWrite) throw new ArgumentException(
                Resources.HashProvider_CreateHashBuilder_ArgumentException_StreamIsNotWritable, "target");
            if (!factories.ContainsKey(hashMethod))
            {
                throw new NotSupportedException(Resources.HashProvider_NotSupported_HashAlgorithm);
            }
            return factories[hashMethod](target);
        }

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
        public IHashBuilder CreateHashBuilder(string hashMethod)
        {
            if (hashMethod == null) throw new ArgumentNullException("hashMethod");
            if (!factories.ContainsKey(hashMethod))
            {
                throw new NotSupportedException(Resources.HashProvider_NotSupported_HashAlgorithm);
            }
            return factories[hashMethod](Stream.Null);
        }

        /// <summary>
        /// Gets the supported hash methods.
        /// </summary>
        /// <returns>
        /// An array with all supported hash algorithms.
        /// </returns>
        public string[] GetSupportedMethods()
        {
            return factories.Keys.ToArray();
        }

        #endregion

        /// <summary>
        /// The <see cref="IHashBuilder"/> implementation for <see cref="BouncyCastleHashProvider"/>.
        /// </summary>
        private class HashBuilder : IHashBuilder
        {
            private readonly DigestStream proxy;
            private readonly IDigest digest;
            private readonly string id;
            private byte[] hashBuffer;

            /// <summary>
            /// Initializes a new instance of <see cref="HashBuilder"/>.
            /// It takes a <see cref="Stream"/> as a target for written data.
            /// </summary>
            /// <param name="target">The stream as target for written data or <c>null</c>.</param>
            /// <param name="digest">The digest.</param>
            /// <param name="id">The identifier for the algorithm.</param>
            public HashBuilder(Stream target, IDigest digest, string id)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target", Resources.HashBuilder_ArgumentNullException_Target);
                }
                this.digest = digest;
                this.id = id;
                proxy = new DigestStream(target, null, digest);
            }

            #region Implementation of IHashBuilder

            public Stream Stream
            {
                get { return proxy; }
            }

            public byte[] ComputeHash()
            {
                if (hashBuffer == null)
                {
                    proxy.Close();
                    proxy.Dispose();
                    hashBuffer = new byte[digest.GetDigestSize()];
                    digest.DoFinal(hashBuffer, 0);
                }
                return hashBuffer;
            }

            public string HashMethod { get { return id; } }

            #endregion
        }
    }
}