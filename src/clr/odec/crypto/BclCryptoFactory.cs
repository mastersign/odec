using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// A crypto factory which delivers crypto implementations based on the .NET base class library.
    /// </summary>
    public class BclCryptoFactory : ICryptoFactory
    {
        /// <summary>
        /// Creates an <see cref="IRSAProvider"/> from a PEM encoded X509 certificate file.
        /// </summary>
        /// <param name="certFile">The name of the certificate file.</param>
        /// <returns>
        /// An <see cref="IRSAProvider"/> or <c>null</c> if the creation failes.
        /// </returns>
        public IRSAProvider CreateRSAProviderFromCertificateFile(string certFile)
        {
            return 
                new BclRSAProvider
                    {
                        Certificate = BclKeyHelper.LoadCertificateFile(certFile)
                    };
        }

        /// <summary>
        /// Creates an <see cref="IRSAProvider"/> from a PEM encoded X509 certificate.
        /// </summary>
        /// <param name="pemCert">The PEM encoded certificate.</param>
        /// <returns>
        /// An <see cref="IRSAProvider"/> or <c>null</c> if the creation failes.
        /// </returns>
        public IRSAProvider CreateRSAProviderFromPemEncodedCertificate(string pemCert)
        {
            return
                new BclRSAProvider
                    {
                        Certificate = BclKeyHelper.LoadCertificate(pemCert)
                    };
        }

        /// <summary>
        /// Creates an <see cref="IRSAProvider"/> from a PEM encoded private key file.
        /// </summary>
        /// <param name="keyFile">The name of the key file.</param>
        /// <param name="passwordSrc">The password source or <c>null</c>.</param>
        /// <returns>
        /// An <see cref="IRSAProvider"/> or <c>null</c> if the creation failes.
        /// </returns>
        public IRSAProvider CreateRSAProviderFromPrivateKeyFile(string keyFile, IPasswordSource passwordSrc)
        {
            return
                new BclRSAProvider
                    {
                        Key = BclKeyHelper.LoadPrivateKeyFile(keyFile)
                    };
        }

        /// <summary>
        /// Creates an <see cref="IRSAProvider"/> from a PEM encoded private key.
        /// </summary>
        /// <param name="pemKey">The PEM encoded private key.</param>
        /// <param name="passwordSrc">The password source or <c>null</c>.</param>
        /// <returns>
        /// An <see cref="IRSAProvider"/> or <c>null</c> if the creation failes.
        /// </returns>
        public IRSAProvider CreateRSAProviderFromPemEncodedPrivateKey(string pemKey, IPasswordSource passwordSrc)
        {
            return 
                new BclRSAProvider
                    {
                        Key = BclKeyHelper.LoadPrivateKey(pemKey)
                    };
        }

        /// <summary>
        /// Creates an <see cref="IHashProvider"/> object.
        /// </summary>
        /// <returns>An hash provider.</returns>
        public IHashProvider CreateHashProvider()
        {
            return new BclHashProvider();
        }
    }
}
