using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// An implementation of <see cref="IRSAProvider"/> based on the
    /// Bouncy Castle Crypto library.
    /// </summary>
    /// <remarks>
    /// <a href="http://www.bouncycastle.org/csharp/">Bouncy Castle Website</a>
    /// </remarks>
    public class BouncyCastleRSAProvider : IRSAProvider
    {
        /// <summary>
        /// Gets or sets a X509 certificate.
        /// </summary>
        /// <remarks>
        /// <para>The certificate must be set, if <see cref="VerifySignature"/> will be called.</para>
        /// </remarks>
        public X509Certificate Certificate { get; set; }

        /// <summary>
        /// Gets or sets a private key.
        /// </summary>
        /// <remarks>
        /// <para>The private key must be set, if <see cref="ComputeSignature"/> will be called.</para>
        /// </remarks>
        public ICipherParameters PrivateKey { get; set; }

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
            get { return PrivateKey != null; }
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
            var sha1 = new Sha1Digest();
            var signer = new RsaDigestSigner(sha1);
            signer.Init(true, PrivateKey);
            using (var signerStream = new SignerStream(source, signer, null))
            {
                signerStream.CopyTo(Stream.Null);
            }
            return signer.GenerateSignature();
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
            if (!CanVerifySignature)
            {
                throw new NotSupportedException(Resources.RSAProvider_NotSupported_NoCertificate);
            }
            var sha1 = new Sha1Digest();
            var signer = new RsaDigestSigner(sha1);
            signer.Init(false, Certificate.GetPublicKey());
            using (var signerStream = new SignerStream(source, signer, null))
            {
                signerStream.CopyTo(Stream.Null);
            }
            return signer.VerifySignature(signature);
        }

        /// <summary>
        /// Gets a <see cref="String"/>, identifying the signature method.
        /// </summary>
        /// <value>The signature method.</value>
        public string SignatureMethod
        {
            get { return System.Security.Cryptography.Xml.SignedXml.XmlDsigRSASHA1Url; }
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
            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
            {
                var pemW = new PemWriter(tw);
                pemW.WriteObject(Certificate);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Creates a new instance of <see cref="IRSAProvider"/> with the given PEM encoded certificate.
        /// </summary>
        /// <param name="pem">The pem encoded certificate.</param>
        /// <returns>A new <see cref="IRSAProvider"/>.</returns>
        public IRSAProvider CreateWithCertificate(string pem)
        {
            var sr = new StringReader(pem);
            var pemR = new PemReader(sr);
            var result = new BouncyCastleRSAProvider();
            result.Certificate = (X509Certificate)pemR.ReadObject();
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

            var now = DateTime.Now;

            if (!CheckValidity(Certificate,
                rules.DateValidationScheme == DateValidationScheme.PemShell ? now : usageTime,
                messageHandler))
            {
                return false;
            }
            if (IsCertificateSelfSigned())
            {
                if (!rules.AllowSelfSignedCertificate)
                {
                    messageHandler.Error(ValidationMessageClass.Certificate,
                        Resources.BouncyCastleRSAProvider_ValidateCertificate_NoSelfSignedCertsAllowed);
                    return false;
                }
                return CheckCertificateSignature(Certificate, Certificate, messageHandler);
            }

            var ca = GetCA(caDirectory);
            if (ca == null)
            {
                messageHandler.Error(ValidationMessageClass.Certificate,
                    Resources.BouncyCastleRSAProvider_ValidateCertificate_NoCAFound,
                    Certificate.SubjectDN);
                return false;
            }
            messageHandler.Success(ValidationMessageClass.Certificate,
                Resources.BouncyCastleRSAProvider_ValidateCertificate_CAFound,
                Certificate.IssuerDN);

            if (!CheckCertificateSignature(ca, Certificate, messageHandler))
            {
                return false;
            }
            var caCheckTime = DateTime.MaxValue;
            switch (rules.DateValidationScheme)
            {
                case DateValidationScheme.PemShell:
                    caCheckTime = now;
                    break;
                case DateValidationScheme.ModifiedPemShell:
                    caCheckTime = usageTime;
                    break;
                case DateValidationScheme.Chain:
                    caCheckTime = Certificate.NotBefore;
                    break;
            }
            if (!CheckValidity(ca, caCheckTime, messageHandler))
            {
                return false;
            }
            messageHandler.Success(ValidationMessageClass.Certificate,
                Resources.BouncyCastleRSAProvider_ValidateCertificate_Success,
                Certificate.SubjectDN);
            return true;
        }

        private bool CheckValidity(X509Certificate cert, DateTime time, ValidationHandler messageHandler)
        {
            var result = true;
            Exception exc = null;
            try
            {
                cert.CheckValidity(time);
            }
            catch (Exception e)
            {
                exc = e;
                result = false;
            }
            if (result)
            {
                messageHandler.Success(ValidationMessageClass.Certificate,
                    Resources.RSAProvider_ValidateCertificate_Succeeded,
                    cert.SubjectDN);
            }
            else
            {
                messageHandler.Error(ValidationMessageClass.Certificate,
                    Resources.RSAProvider_ValidateCertificate_Failed,
                    cert.SubjectDN, exc.Message);
            }
            return result;
        }

        private bool IsCertificateSelfSigned()
        {
            return IsNameEqual(
                Certificate.SubjectDN, Certificate.SubjectUniqueID,
                Certificate.IssuerDN, Certificate.IssuerUniqueID);
        }

        private bool IsNameEqual(
            Org.BouncyCastle.Asn1.X509.X509Name dn1, Org.BouncyCastle.Asn1.DerBitString uniqueId1,
            Org.BouncyCastle.Asn1.X509.X509Name dn2, Org.BouncyCastle.Asn1.DerBitString uniqueId2)
        {
            if (dn1 == null) throw new ArgumentNullException("dn1");
            if (dn2 == null) throw new ArgumentNullException("dn2");
            if (!dn1.Equivalent(dn2)) return false;
            if (uniqueId1 == null)
            {
                return uniqueId2 == null;
            }
            return uniqueId2 != null && Equals(uniqueId1.GetBytes(), uniqueId2.GetBytes());
        }

        private X509Certificate GetCA(CertificationAuthorityDirectory caDirectory)
        {
            var ca = caDirectory.GetSigningCertificate(this);
            return ca != null ? ((BouncyCastleRSAProvider)ca).Certificate : null;
        }

        private bool CheckCertificateSignature(X509Certificate ca, X509Certificate cert, ValidationHandler messageHandler)
        {
            var result = true;
            Exception exc = null;
            try
            {
                cert.Verify(ca.GetPublicKey());
            }
            catch (Exception e)
            {
                exc = e;
                result = false;
            }
            if (result)
            {
                messageHandler.Success(ValidationMessageClass.Certificate,
                    Resources.BouncyCastleRSAProvider_CheckCertificateSignature_Success,
                    cert.SubjectDN);
            }
            else
            {
                messageHandler.Error(ValidationMessageClass.Certificate,
                    Resources.BouncyCastleRSAProvider_CheckCertificateSignature_Failed,
                    cert.SubjectDN, exc.Message);
            }
            return result;
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
                SubjectDistinguishedName = Certificate.SubjectDN.ToString(),
                IssuerDistingushedName = Certificate.IssuerDN.ToString(),
                SerialNumber = Certificate.SerialNumber.ToString(),
                SignatureAlgorithm = Certificate.SigAlgName,
                Version = Certificate.Version,
                NotBefore = Certificate.NotBefore,
                NotAfter = Certificate.NotAfter,
            };
        }

        #endregion
    }
}