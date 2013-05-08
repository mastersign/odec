using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// The default implementation of <see cref="IHashProvider"/> using the BCL
    /// implementation of the SHA1 algorithm.
    /// </summary>
    public class BclHashProvider : IHashProvider
    {
        private static readonly Dictionary<string, Func<Stream, IHashBuilder>> factories =
            new Dictionary<string, Func<Stream, IHashBuilder>>();

        static BclHashProvider()
        {
            AddMethod(AlgorithmIdentifier.MD5, new MD5CryptoServiceProvider());
            AddMethod(AlgorithmIdentifier.SHA1, new SHA1Managed());
            AddMethod(AlgorithmIdentifier.SHA256, new SHA256Managed());
            AddMethod(AlgorithmIdentifier.SHA384, new SHA384Managed());
            AddMethod(AlgorithmIdentifier.SHA512, new SHA512Managed());
            AddMethod(AlgorithmIdentifier.RIPEMD160, new RIPEMD160Managed());
        }

        private static void AddMethod(string id, HashAlgorithm digest)
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
            if (!target.CanWrite)
            {
                throw new ArgumentException(
                Resources.HashProvider_CreateHashBuilder_ArgumentException_StreamIsNotWritable, "target");
            }
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
            if (!factories.ContainsKey(hashMethod))
            {
                throw new NotSupportedException(Resources.HashProvider_NotSupported_HashAlgorithm);
            }
            return factories[hashMethod](Stream.Null);
        }

        /// <summary>
        /// Gets the supported hash methods.
        /// </summary>
        /// <returns>An array with all supported hash algorithms.</returns>
        public string[] GetSupportedMethods()
        {
            return factories.Keys.ToArray();
        }

        #endregion

        private class HashBuilder : IHashBuilder
        {
            private readonly ProxyStream proxy;
            private readonly HashAlgorithm algo;
            private readonly string id;
            private byte[] hashBuffer;

            public HashBuilder(Stream target, HashAlgorithm algo, string id)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target", Resources.HashBuilder_ArgumentNullException_Target);
                }
                this.id = id;
                this.algo = algo;
                this.algo.Initialize();
                proxy = new ProxyStream(target, algo);
            }

            public Stream Stream
            {
                get { return proxy; }
            }

            public byte[] ComputeHash()
            {
                if (hashBuffer == null)
                {
                    proxy.Close();
                    algo.TransformFinalBlock(new byte[0], 0, 0);
                    hashBuffer = algo.Hash;
                }
                return hashBuffer;
            }

            public string HashMethod { get { return id; } }
        }

        private class ProxyStream : Stream
        {
            private readonly HashAlgorithm algo;
            private Stream output;
            private bool closed;

            public ProxyStream(Stream output, HashAlgorithm algo)
            {
                this.output = output;
                this.algo = algo;
            }

            #region Overrides of Stream

            public override void Close()
            {
                base.Close();
                closed = true;
                output = null;
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (closed)
                {
                    throw new ObjectDisposedException(GetType().FullName + "_" + GetHashCode());
                }
                if (output != null)
                {
                    output.Write(buffer, offset, count);
                }
                algo.TransformBlock(buffer, offset, count, buffer, offset);
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override long Length
            {
                get { throw new NotSupportedException(); }
            }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            #endregion
        }
    }
}