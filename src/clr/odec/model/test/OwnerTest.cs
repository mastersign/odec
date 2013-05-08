using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.test;
using NUnit.Framework;

namespace de.mastersign.odec.model.test
{
    [TestFixture]
    internal class OwnerTest : AssertionHelper
    {
        [Test]
        public void ReadFromXmlParamTest()
        {
            var target = new Owner();

            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromXml(null));
        }

        [Test]
        public void WriteToXmlParamTest()
        {
            var target = new Owner();

            Assert.Throws<ArgumentNullException>(
                () => target.WriteToXml(null));
        }

        [Test]
        public void WriteToXmlTest()
        {
            var target = new Owner();
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                Expect(!target.IsValid);
                Assert.Throws<InvalidOperationException>(
                    () => target.WriteToXml(w));
            }
        }

        [Test]
        public void ReadFromXmlWriteToXmlTest()
        {
            var original = TestHelper.CreateOwnerType(1);

            var copy = TestHelper.CopyByXmlSerialization(original);

            Expect(copy, Is.EqualTo(original));
        }

        [Test]
        public void IsValidTest()
        {
            var target = new Owner();
            Expect(!target.IsValid);

            target = TestHelper.CreateOwnerType(1);
            Expect(target.IsValid);

            target.Role = null;
            Expect(target.IsValid);
        }
    }
}