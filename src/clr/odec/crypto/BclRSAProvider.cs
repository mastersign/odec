using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using de.mastersign.odec.Properties;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// The default implementation of the <see cref="IRSAProvider"/> interface,
    /// using the BCL implementation of the RSA and SHA1 algorithms.
    /// </summary>
    public class BclRSAProvider : IRSAProvider
    {
        /// <summary>
        /// Gets or sets the X509 certificate to support the verification of a signature.
        /// </summary>
        /// <remarks>
        /// If this is <c>null</c>, the verification of a signatures is not possible.
        /// </remarks>
        /// <value>The X509 certificate.</value>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// Gets or sets a private RSA key to support the creation of a signature.
        /// </summary>
        /// <remarks>
        /// If this is <c>null</c>, the creation of a signature is not possible.
        /// </remarks>
        public RSAParameters Key { get; set; }

        #region Implementation of IRSAProvider

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
        public bool CanComputeSignature
        {
            get
            {
                if (Key.D == null) return false;
                var rsa = new RSACryptoServiceProvider();
                rsa.ImportParameters(Key);
                var result = !rsa.PublicOnly;
                rsa.Clear();
                return result;
            }
        }

        /// <summary>
        /// Computes a signature for the given data stream.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <returns>The signature data.</returns>
        /// <exception cref="NotSupportedException">thrown, if <see cref="CanComputeSignature"/> is <c>false</c>.</exception>
        public byte[] ComputeSignature(Stream source)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (!CanComputeSignature)
            {
                throw new NotSupportedException(Resources.RSAProvider_NotSupported_NoPrivateKey);
            }

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(Key);
            var result = rsa.SignData(source, new SHA1Managed());
            rsa.Clear();
            return result;
        }

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
        public bool CanVerifySignature
        {
            get { return Certificate != null; }
        }

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
        public bool VerifySignature(Stream source, byte[] signature)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (signature == null) throw new ArgumentNullException("signature");
            if (!CanVerifySignature) throw new NotSupportedException();

            var rsa = (RSACryptoServiceProvider)Certificate.PublicKey.Key;
            var rsaParams = rsa.ExportParameters(false);
            rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);
            var result = rsa.VerifyData(source.ToArray(), new SHA1Managed(), signature);
            rsa.Clear();
            return result;
        }

        /// <summary>
        /// Gets a <see cref="String"/>, identifying the signature method.
        /// </summary>
        /// <value>The signature method.</value>
        public string SignatureMethod
        {
            get { return SignedXml.XmlDsigRSASHA1Url; }
        }

        /// <summary>
        /// Exports the certificate as PEM encoded.
        /// </summary>
        /// <returns>
        /// A string, containing the PEM encoded certificate.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Is thrown, if the crypto provider don't has access to the certificate.
        /// </exception>
        public string ExportCertificate()
        {
            if (Certificate == null)
            {
                throw new NotSupportedException(Resources.RSAProvider_NotSupported_NoCertificate);
            }
            return BclKeyHelper.EncodeCertificate(Certificate);
        }

        /// <summary>
        /// Creates a new instance of <see cref="IRSAProvider"/> with the given PEM encoded certificate.
        /// </summary>
        /// <param name="pem">The pem encoded certificate.</param>
        /// <returns>A new <see cref="IRSAProvider"/>.</returns>
        public IRSAProvider CreateWithCertificate(string pem)
        {
            var result =
                new BclRSAProvider
                    {
                        Certificate = BclKeyHelper.LoadCertificate(pem)
                    };
            return result;
        }

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
        /// In this case <see cref="IRSAProvider.CanVerifySignature"/> is <c>false</c>.
        ///   </exception>
        public bool ValidateCertificate(
            CertificationAuthorityDirectory caDirectory, 
            CertificateValidationRules rules, 
            DateTime usageTime, 
            ValidationHandler messageHandler)
        {
            if (Certificate == null)
            {
                throw new NotSupportedException(Resources.RSAProvider_NotSupported_NoCertificate);
            }

            var result = Certificate.Verify();
            if (!result)
            {
                messageHandler.Error(ValidationMessageClass.Certificate,
                    Resources.RSAProvider_ValidateCertificate_Failed, Certificate.SubjectName.Name, null);
            }
            else
            {
                messageHandler.Success(ValidationMessageClass.Certificate,
                    Resources.RSAProvider_ValidateCertificate_Succeeded, Certificate.SubjectName.Name);
            }

            throw new NotImplementedException();
            //return result;
        }

        /// <summary>
        /// Gets a <see cref="CertificateInfo"/>, describing the certificate.
        /// </summary>
        /// <returns>
        /// A <see cref="CertificateInfo"/>, describing the certificate.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Is thrown, if the <see cref="IRSAProvider"/> has no access to the certificate.
        /// </exception>
        public CertificateInfo GetCertificateInfo()
        {
            if (Certificate == null)
            {
                throw new NotSupportedException(Resources.RSAProvider_NotSupported_NoCertificate);
            }

            return new CertificateInfo
            {
                SubjectDistinguishedName = Certificate.SubjectName.Name,
                IssuerDistingushedName = Certificate.IssuerName.Name,
                SerialNumber = Certificate.SerialNumber,
                SignatureAlgorithm = Certificate.SignatureAlgorithm.FriendlyName,
                Version = Certificate.Version,
                NotBefore = Certificate.NotBefore,
                NotAfter = Certificate.NotAfter,
            };
        }

        #endregion
    }
}