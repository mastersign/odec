using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// This class supports the reading of PEM encoded certificate and key files
    /// in a way that the read certificate or key objects are compatible 
    /// with cryptographic providers based on the Bouncy Castle crypto library.
    /// </summary>
    public static class BouncyCastleKeyHelper
    {
        private const string PEM_ID_CERTIFICATE = "CERTIFICATE";
        private const string PEM_ID_PRIVATE_KEY = "RSA PRIVATE KEY";

        /// <summary>
        /// Loads a X509 certificate file in a <see cref="X509Certificate"/> object.
        /// </summary>
        /// <param name="file">The absolute path to the certificate file.</param>
        /// <returns>The read certificate as a <see cref="X509Certificate"/> object.</returns>
        public static X509Certificate LoadCertificateFile(string file)
        {
            return LoadCertificate(File.ReadAllText(file, Encoding.ASCII));
        }

        /// <summary>
        /// Loads a X509 certificate from PEM encoded data in a <see cref="X509Certificate"/> object.
        /// </summary>
        /// <param name="pemCert">The encoded certificate as a string.</param>
        /// <returns>The read certificate as a <see cref="X509Certificate"/> object.</returns>
        public static X509Certificate LoadCertificate(string pemCert)
        {
            using (var sr = new StringReader(ExtractPem(pemCert, PEM_ID_CERTIFICATE)))
            {
                var pemReader = new PemReader(sr);
                return (X509Certificate)pemReader.ReadObject();
            }
        }

        /// <summary>
        /// Encodes the given X509 certificate in PEM style.
        /// </summary>
        /// <param name="cert">The certificate.</param>
        /// <returns>A string with the PEM encoded certificate.</returns>
        public static string EncodeCertificate(X509Certificate cert)
        {
            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
            {
                var pw = new PemWriter(tw);
                pw.WriteObject(cert);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Loads a private key file in a <see cref="ICipherParameters"/> object.
        /// </summary>
        /// <param name="file">The absolute path to the key file.</param>
        /// <param name="passFinder">The password finder or <c>null</c>.</param>
        /// <returns>
        /// The read key as a <see cref="ICipherParameters"/> object.
        /// </returns>
        public static ICipherParameters LoadPrivateKeyFile(string file, IPasswordFinder passFinder = null)
        {
            return LoadPrivateKey(File.ReadAllText(file, Encoding.ASCII), passFinder);
        }

        /// <summary>
        /// Loads a PEM encoded private key in a <see cref="ICipherParameters"/> object.
        /// </summary>
        /// <param name="pemKey">The PEM encoded private key.</param>
        /// <param name="passFinder">The password finder or <c>null</c>.</param>
        /// <returns>The read key as a <see cref="ICipherParameters"/> object.</returns>
        public static ICipherParameters LoadPrivateKey(string pemKey, IPasswordFinder passFinder = null)
        {
            using (var keyStream = new StringReader(ExtractPem(pemKey, PEM_ID_PRIVATE_KEY)))
            {
                var r = new PemReader(keyStream, passFinder);
                var key = (AsymmetricCipherKeyPair)r.ReadObject();
                return key.Private;
            }
        }

        /// <summary>
        /// Extracts the first PEM encoded data block with a given ID from a string.
        /// </summary>
        /// <param name="text">The text to search in.</param>
        /// <param name="id">The id of the PEM encoded data block.</param>
        /// <returns>The string containing nothing else than the PEM encoded data.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="text"/> or <paramref name="id"/> is <c>null</c>.</exception>
        /// <exception cref="ApplicationException">if <paramref name="text"/> does not contain the </exception>
        private static string ExtractPem(string text, string id)
        {
            if (text == null) throw new ArgumentNullException("text");
            if (id == null) throw new ArgumentNullException("id");

            var header = string.Format("-----BEGIN {0}-----", id);
            var footer = string.Format("-----END {0}-----", id);
            var headPos = text.IndexOf(header, StringComparison.Ordinal);
            if (headPos < 0)
            {
                throw new ArgumentException(
                    string.Format(Resources.BouncyCastleKeyHelper_ExtractPem_NoPemData, id));
            }
            var footPos = text.IndexOf(footer, headPos, StringComparison.Ordinal);
            if (footPos < 0)
            {
                throw new ArgumentException(
                    string.Format(Resources.BouncyCastleKeyHelper_ExtractPem_NoPemData, id));
            }
            return text.Substring(headPos, (footPos - headPos) + footer.Length);
        }
    }
}