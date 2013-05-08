using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using de.mastersign.odec.test;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Signers;

namespace de.mastersign.odec.crypto.test
{
    [TestFixture]
    class CompatibilityTest : AssertionHelper
    {
        [Test]
        public void SignatureBouncyCastleToBclTest()
        {
            // Prepare test data
            var msg = new byte[10240];
            new Random().NextBytes(msg);
            var msgS = new MemoryStream(msg);

            // Retrieve file paths for PEM encoded certificate and key
            var crtFile = TestHelper.GetResFilePath("Test.crt");
            var keyFile = TestHelper.GetResFilePath("Test.key");

            // Create a Signature with Bouncy Castle
            var bKey = BouncyCastleKeyHelper.LoadPrivateKeyFile(keyFile);
            var bRsa = new RsaEngine();
            var bSha1 = new Sha1Digest();
            var bSigner1 = new RsaDigestSigner(bSha1);
            bSigner1.Init(true, bKey);
            var bSignerS = new SignerStream(msgS, bSigner1, null);
            bSignerS.CopyTo(Stream.Null);
            var sign = bSigner1.GenerateSignature();

            // Verify the Signature with Bouncy Castle
            var bCert = BouncyCastleKeyHelper.LoadCertificateFile(crtFile);
            bRsa.Init(false, bCert.GetPublicKey());
            bSha1.Reset();
            var bSigner2 = new RsaDigestSigner(bSha1);
            bSigner2.Init(false, bCert.GetPublicKey());
            msgS.Position = 0L;
            bSignerS = new SignerStream(msgS, bSigner2, null);
            bSignerS.CopyTo(Stream.Null);
            Expect(bSigner2.VerifySignature(sign));

            // Verify the Signature with BCL
            var cCert = BclKeyHelper.LoadCertificateFile(crtFile);
            var cRsa = new RSACryptoServiceProvider();
            cRsa.ImportParameters(BclKeyHelper.GetPublicKey(cCert));
            var cSha1 = new SHA1Managed();
            cSha1.Initialize();
            Expect(cRsa.VerifyData(msg, cSha1, sign));
        }

        [Test]
        public void SignatureBclToBouncyCastleTest()
        {
            // Prepare test data
            var msg = new byte[10240];
            new Random().NextBytes(msg);
            var msgS = new MemoryStream(msg);

            // Retrieve file paths for PEM encoded certificate and key
            var crtFile = TestHelper.GetResFilePath("Test.crt");
            var keyFile = TestHelper.GetResFilePath("Test.key");

            // Create a Signature with BCL
            var cKey = BclKeyHelper.LoadPrivateKeyFile(keyFile);
            var cRsa1 = new RSACryptoServiceProvider();
            var cSha1 = new SHA1Managed();
            cSha1.Initialize();
            cRsa1.ImportParameters(cKey);
            var sign = cRsa1.SignData(msg, cSha1);

            // Verify the Signature with BCL
            var cCert = BclKeyHelper.LoadCertificateFile(crtFile);
            var cRsa2 = (RSACryptoServiceProvider)cCert.PublicKey.Key;
            cSha1.Initialize();
            Expect(cRsa2.VerifyData(msg, cSha1, sign));

            // Verify the Signature with Bouncy Castle
            var bRsa = new RsaEngine();
            var bSha1 = new Sha1Digest();
            var bCert = BouncyCastleKeyHelper.LoadCertificateFile(crtFile);
            bRsa.Init(false, bCert.GetPublicKey());
            bSha1.Reset();
            var bSigner = new RsaDigestSigner(bSha1);
            bSigner.Init(false, bCert.GetPublicKey());
            msgS.Position = 0L;
            var bSignerS = new SignerStream(msgS, bSigner, null);
            bSignerS.CopyTo(Stream.Null);
            Expect(bSigner.VerifySignature(sign));
        }
    }
}
