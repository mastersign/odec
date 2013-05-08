using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.odec.crypto.test
{
    [TestFixture]
    internal class BclHashProviderTest : AssertionHelper
    {
        [Test]
        public void CreateHashBuilderParamTest()
        {
            IHashProviderTest.CreateHashBuilderParamTest(
                new BclHashProvider());
        }

        [Test]
        public void CreateHashBuilderTest()
        {
            IHashProviderTest.CreateHashBuilderTest(new BclHashProvider());
        }
    }
}