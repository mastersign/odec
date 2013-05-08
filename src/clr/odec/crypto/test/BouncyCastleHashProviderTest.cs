using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.odec.crypto.test
{
    [TestFixture]
    internal class BouncyCastleHashProviderTest : AssertionHelper
    {
        [Test]
        public void CreateHashBuilderParamTest()
        {
            IHashProviderTest.CreateHashBuilderParamTest(
                new BouncyCastleHashProvider());
        }

        [Test]
        public void CreateHashBuilderTest()
        {
            IHashProviderTest.CreateHashBuilderTest(new BouncyCastleHashProvider());
        }
    }
}