using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using de.mastersign.odec.test;
using NUnit.Framework;

namespace de.mastersign.odec.crypto.test
{
    [TestFixture]
    class BclKeyHelperTest : AssertionHelper
    {
        [Test]
        public void LoadCertificateParamTest()
        {
            Assert.Throws<ArgumentNullException>(
                () => BclKeyHelper.LoadCertificateFile(null));

            string notExistingFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.Throws<FileNotFoundException>(
                () => BclKeyHelper.LoadCertificateFile(notExistingFile));
        }

        [Test]
        public void LoadCertificateTest()
        {
            string file = TestHelper.GetResFilePath("Test.crt");
            var cert = BclKeyHelper.LoadCertificateFile(file);
            Expect(cert, !Null);
        }

        [Test]
        public void LoadPrivateKeyParamTest()
        {
            Assert.Throws<ArgumentNullException>(
                () => BclKeyHelper.LoadPrivateKeyFile(null));

            string notExistingFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.Throws<FileNotFoundException>(
                () => BclKeyHelper.LoadPrivateKeyFile(notExistingFile));
        }

        [Test]
        public void LoadPrivateKeyTest()
        {
            string file = TestHelper.GetResFilePath("Test.key");
            var key = BclKeyHelper.LoadPrivateKeyFile(file);
            Expect(key, !Null);
        }

        [Test]
        public void CertificatePemEncodingTest()
        {
            var crtFile = TestHelper.GetResFilePath("Test.crt");

            var cert1 = BclKeyHelper.LoadCertificateFile(crtFile);
            var pemCert1 = BclKeyHelper.EncodeCertificate(cert1);
            var certData = BclKeyHelper.DecodePem("CERTIFICATE", pemCert1);
            var cert2 = new X509Certificate2(certData);

            Expect(cert1.IssuerName.Name, Is.EqualTo(cert2.IssuerName.Name));
        }
    }
}
