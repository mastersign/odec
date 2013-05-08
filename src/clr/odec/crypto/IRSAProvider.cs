using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// Represents the capability to sign data and to verify an existing signature.
    /// </summary>
    public interface IRSAProvider
    {
        /// <summary>
        /// Gets a value indicating whether this instance can compute a signature.
        /// </summary>
        /// <remarks>
        /// If, for example, this instance has only access to the public key of a public-private key pair,
        /// this instance will not be able to compute a signature.
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if this instance can compute a signature; otherwise, <c>false</c>.
        /// </value>
        bool CanComputeSignature { get; }

        /// <summary>
        /// Computes a signature for the given data stream.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <returns>The signature data.</returns>
        /// <exception cref="NotSupportedException">thrown, if <see cref="CanComputeSignature"/> is <c>false</c>.</exception>
        byte[] ComputeSignature(Stream source);

        /// <summary>
        /// Gets a value indicating whether this instance can verify a signature.
        /// </summary>
        /// <remarks>
        /// If, for example, this instance has only access to the private key of a public-private key pair,
        /// this instance will not be able to verify a signature.
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if this instance can verify a signature; otherwise, <c>false</c>.
        /// </value>
        bool CanVerifySignature { get; }

        /// <summary>
        /// Verifies the signature for the given data.
        /// </summary>
        /// <remarks>
        /// This method, however, is not validating the conditions for the verification, 
        /// e.g. the validity of a certificate, used to verify the signature.
        /// </remarks>
        /// <param name="source">The data.</param>
        /// <param name="signature">The signature.</param>
        /// <returns><c>true</c>, if the signature is valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">thrown, if <see cref="CanVerifySignature"/> is <c>false</c>.</exception>
        bool VerifySignature(Stream source, byte[] signature);

        /// <summary>
        /// Gets a <see cref="String"/>, identifying the signature method.
        /// </summary>
        /// <remarks>
        /// For the W3C recommended combination of RSA and SHA1, for example, the URI
        /// <c>http://www.w3.org/2000/09/xmldsig#rsa-sha1</c> should be returned.
        /// </remarks>
        /// <value>The signature method.</value>
        string SignatureMethod { get; }

        /// <summary>
        /// Exports the certificate as PEM encoded.
        /// </summary>
        /// <returns>A string, containing the PEM encoded certificate.</returns>
        /// <exception cref="NotSupportedException">
        /// Is thrown, if the crypto provider has no access to the certificate.
        /// </exception>
        string ExportCertificate();

        /// <summary>
        /// Creates a new instance of <see cref="IRSAProvider"/> with the given PEM encoded certificate.
        /// </summary>
        /// <param name="pem">The pem encoded certificate.</param>
        /// <returns>A new <see cref="IRSAProvider"/>.</returns>
        IRSAProvider CreateWithCertificate(string pem);

        /// <summary>
        /// Validates the certificate.
        /// </summary>
        /// <param name="caDirectory">A directory with trusted certification authorities.</param>
        /// <param name="rules">A rule set for the validation.</param>
        /// <param name="usageTime">The creation time of the signature associated with this validation.</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns>
        ///   <c>true</c> if the certificate is valid; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Is thrown, if the provider has no access to the certificate.
        /// In this case <see cref="CanVerifySignature"/> is <c>false</c>.
        ///   </exception>
        bool ValidateCertificate(CertificationAuthorityDirectory caDirectory, CertificateValidationRules rules, DateTime usageTime, ValidationHandler messageHandler);

        /// <summary>
        /// Gets a <see cref="CertificateInfo"/>, describing the certificate.
        /// </summary>
        /// <returns>A <see cref="CertificateInfo"/>, describing the certificate.</returns>
        /// <exception cref="NotSupportedException">
        /// Is thrown, if the <see cref="IRSAProvider"/> has no access to the certificate.
        /// </exception>
        CertificateInfo GetCertificateInfo();
    }
}