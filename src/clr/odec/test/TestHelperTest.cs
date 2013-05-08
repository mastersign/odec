using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.odec.test
{
    [TestFixture]
    internal class TestHelperTest : AssertionHelper
    {
        [Test]
        public void KeyTest()
        {
            var k1 = TestHelper.Key1;
            var k2 = TestHelper.Key2;
            Expect(k1.D, Is.Not.EqualTo(k2.D));
        }
    }
}