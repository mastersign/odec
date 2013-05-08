using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.test
{
    [TestFixture]
    internal class UtilsTest : AssertionHelper
    {
        [Test]
        public void ToArrayTest()
        {
            var data1 = File.ReadAllBytes(TestHelper.GetResFilePath("DemoFile.xml"));
            var data2 = TestHelper.GetResFile("DemoFile.xml").ToArray();

            Expect(data1, Is.EqualTo(data2));
        }

        [Test]
        public void AreEqualTest1()
        {
            var a = new byte[] {1, 2, 3};
            var b = new byte[] {1, 2, 3};
            var c = new byte[] {1, 4, 3};
            var d = new byte[] {};

            Expect(ObjectUtils.AreEqual((byte[]) null, null));
            Expect(!ObjectUtils.AreEqual(a, null));
            Expect(!ObjectUtils.AreEqual(null, a));
            Expect(ObjectUtils.AreEqual(a, a));
            Expect(ObjectUtils.AreEqual(a, b));
            Expect(!ObjectUtils.AreEqual(a, c));
            Expect(!ObjectUtils.AreEqual(a, d));
        }

        [Test]
        public void GetHashCodeTest1()
        {
            var a = new byte[] {1, 2, 3};
            var b = new byte[] {1, 2, 3};
            var c = new byte[] {1, 4, 3};
            var d = new byte[] {1, 1};
            var e = new byte[] {1, 1, 1};

            Assert.Throws<ArgumentNullException>(
                () => ObjectUtils.GetHashCode((byte[]) null));

            var hashA = ObjectUtils.GetHashCode(a);
            var hashB = ObjectUtils.GetHashCode(b);
            var hashC = ObjectUtils.GetHashCode(c);
            var hashD = ObjectUtils.GetHashCode(d);
            var hashE = ObjectUtils.GetHashCode(e);

            Expect(hashA, Is.EqualTo(hashB));
            Expect(hashA, Is.Not.EqualTo(hashC));
            Expect(hashA, Is.Not.EqualTo(hashD));
            Expect(hashD, Is.Not.EqualTo(hashE));
        }
    }
}