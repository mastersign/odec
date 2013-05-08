using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// This class is used to build an index for CAs.
    /// </summary>
    public class CertificationAuthorityDirectory
    {
        private readonly Dictionary<string, IRSAProvider> dict = new Dictionary<string, IRSAProvider>();

        /// <summary>
        /// Creates a <see cref="CertificationAuthorityDirectory"/> from a file system directory.
        /// All CRT files with PEM encoded certificates are used as trusted certification authorities.
        /// </summary>
        /// <param name="directoryPath">The path to the ca directory.</param>
        /// <param name="cryptoFactory">A cryptographic factory.</param>
        /// <returns></returns>
        public static CertificationAuthorityDirectory CreateFromFileSystem(string directoryPath, ICryptoFactory cryptoFactory)
        {
            return new CertificationAuthorityDirectory(GetCertificatesFromFileSystem(directoryPath, cryptoFactory));
        }

        private static IEnumerable<IRSAProvider> GetCertificatesFromFileSystem(string directoryPath, ICryptoFactory cryptoFactory)
        {
            foreach (var f in Directory.GetFiles(directoryPath, "*.crt", SearchOption.TopDirectoryOnly))
            {
                IRSAProvider cert = null;
                try
                {
                    cert = cryptoFactory.CreateRSAProviderFromCertificateFile(f);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Error while loading certificate '{0}':\n{1}", f, ex);
                }
                if (cert != null) yield return cert;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificationAuthorityDirectory"/> class.
        /// </summary>
        /// <param name="cas">An array of certification authorities.</param>
        public CertificationAuthorityDirectory(params IRSAProvider[] cas)
        {
            Initialize(cas);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificationAuthorityDirectory"/> class.
        /// </summary>
        /// <param name="cas">An enumeration with certification authorities.</param>
        public CertificationAuthorityDirectory(IEnumerable<IRSAProvider> cas)
        {
            Initialize(cas);
        }

        private void Initialize(IEnumerable<IRSAProvider> cas)
        {
            if (cas == null) return;
            foreach (var ca in cas)
            {
                dict[ca.GetCertificateInfo().SubjectDistinguishedName] = ca;
            }
        }

        /// <summary>
        /// Gets the signing certificate or <c>null</c> if the signing certificate is not in the directory.
        /// </summary>
        /// <param name="cert">The signed certificate.</param>
        /// <returns>The certificate of the signing CA or <c>null</c>.</returns>
        public IRSAProvider GetSigningCertificate(IRSAProvider cert)
        {
            if (cert == null) throw new ArgumentNullException("cert");
            if (!cert.CanVerifySignature) throw new ArgumentException(Resources.CertificationAuthorityDirectory_GetSigningCertificate_NoCertificate, "cert");
            IRSAProvider res;
            return dict.TryGetValue(
                cert.GetCertificateInfo().IssuerDistingushedName,
                out res) ? res : null;
        }
    }
}
