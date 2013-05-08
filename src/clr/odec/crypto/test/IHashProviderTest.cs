using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using de.mastersign.odec.test;
using de.mastersign.odec.test.res;
using NUnit.Framework;

namespace de.mastersign.odec.crypto.test
{
    internal static class IHashProviderTest
    {
        public static void CreateHashBuilderParamTest(IHashProvider provider)
        {
            Assert.Throws<ArgumentNullException>(
                () => provider.CreateHashBuilder(null));

            using (var ms = new MemoryStream())
            {
                Assert.Throws<ArgumentNullException>(
                    () => provider.CreateHashBuilder(null, ms));
            }

            // Check with read only stream
            using (var fs = File.OpenRead(TestHelper.GetResFilePath("DemoFile.xml")))
            {
                Assert.Throws<ArgumentException>(
                    () => provider.CreateHashBuilder(AlgorithmIdentifier.SHA1, fs));
            }

            Assert.Throws<ArgumentNullException>(
                () => provider.CreateHashBuilder(AlgorithmIdentifier.SHA1, null));
        }

        public static void CreateHashBuilderTest(IHashProvider provider)
        {
            Assert.Throws<NotSupportedException>(
                () => provider.CreateHashBuilder("unknown algorithm"));

            foreach (var method in provider.GetSupportedMethods())
            {
                AlgorithmTest(provider, method);
            }
        }

        private static void AlgorithmTest(IHashProvider provider, string algoId)
        {
            var builder = provider.CreateHashBuilder(algoId);
            DigestBuilderTest(builder, algoId);

            var bufferStream = new MemoryStream();
            var digest1 = DigestBuilderTest(provider.CreateHashBuilder(algoId, bufferStream), algoId);

            Assert.AreEqual(
                bufferStream.ToArray(),
                File.ReadAllBytes(TestHelper.GetResFilePath("DemoFile.xml")));

            bufferStream.Dispose();

            var digest2 = DigestBuilderTest(provider.CreateHashBuilder(algoId), algoId);

            Assert.AreEqual(digest1, digest2);
        }

        private static byte[] DigestBuilderTest(IHashBuilder builder, string algoId)
        {
            Assert.IsNotNull(builder);
            Assert.AreEqual(algoId, builder.HashMethod);

            var stream = builder.Stream;
            Assert.IsNotNull(stream);
            Assert.AreEqual(stream, builder.Stream);

            Assert.IsTrue(stream.CanWrite);
            using (var resStream = TestHelper.GetResFile("DemoFile.xml"))
            {
                resStream.CopyTo(stream);
            }
            Assert.AreEqual(stream, builder.Stream);

            var digest = builder.ComputeHash();
            Assert.IsNotNull(digest);

            Assert.AreEqual(stream, builder.Stream);

            var digest2 = builder.ComputeHash();
            Assert.AreEqual(digest, digest2);

            return digest;
        }
    }
}