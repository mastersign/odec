using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// This interface describes a factory for cryptographic providers.
    /// </summary>
    public interface ICryptoFactory
    {
        /// <summary>
        /// Creates an <see cref="IRSAProvider"/> from a PEM encoded X509 certificate file.
        /// </summary>
        /// <param name="certFile">The name of the certificate file.</param>
        /// <returns>An <see cref="IRSAProvider"/> or <c>null</c> if the creation failes.</returns>
        IRSAProvider CreateRSAProviderFromCertificateFile(string certFile);

        /// <summary>
        /// Creates an <see cref="IRSAProvider"/> from a PEM encoded X509 certificate.
        /// </summary>
        /// <param name="pemCert">The PEM encoded certificate.</param>
        /// <returns>An <see cref="IRSAProvider"/> or <c>null</c> if the creation failes.</returns>
        IRSAProvider CreateRSAProviderFromPemEncodedCertificate(string pemCert);

        /// <summary>
        /// Creates an <see cref="IRSAProvider"/> from a PEM encoded private key file.
        /// </summary>
        /// <param name="keyFile">The name of the key file.</param>
        /// <param name="passwordSrc">The password source or <c>null</c>.</param>
        /// <returns>
        /// An <see cref="IRSAProvider"/> or <c>null</c> if the creation failes.
        /// </returns>
        IRSAProvider CreateRSAProviderFromPrivateKeyFile(string keyFile, IPasswordSource passwordSrc);

        /// <summary>
        /// Creates an <see cref="IRSAProvider"/> from a PEM encoded private key.
        /// </summary>
        /// <param name="pemKey">The PEM encoded private key.</param>
        /// <param name="passwordSrc">The password source or <c>null</c>.</param>
        /// <returns>
        /// An <see cref="IRSAProvider"/> or <c>null</c> if the creation failes.
        /// </returns>
        IRSAProvider CreateRSAProviderFromPemEncodedPrivateKey(string pemKey, IPasswordSource passwordSrc);

        /// <summary>
        /// Creates an <see cref="IHashProvider"/> object.
        /// </summary>
        /// <returns>A hash provider.</returns>
        IHashProvider CreateHashProvider();
    }
}
