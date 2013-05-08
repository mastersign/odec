using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using de.mastersign.odec.test;
using NUnit.Framework;

namespace de.mastersign.odec.model.test
{
    [TestFixture]
    internal class HistoryItemElementTest : AssertionHelper
    {
        [Test]
        public void ReadFromXmlParamTest()
        {
            var target = new HistoryItemElement();

            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromXml(null));
        }

        [Test]
        public void WriteToXmlParamTest()
        {
            var target = new HistoryItemElement();

            Assert.Throws<ArgumentNullException>(
                () => target.WriteToXml(null));
        }

        [Test]
        public void WriteToXmlTest()
        {
            var target = new HistoryItemElement();
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
            var original = TestHelper.CreateHistoryItemElement(TestHelper.Key1, 1);
            var copy = TestHelper.CopyByXmlSerialization(original);
            Expect(copy, Is.EqualTo(original));
        }

        [Test]
        public void IsValidTest()
        {
            var target = new HistoryItemElement();
            Expect(!target.IsValid);

            target = TestHelper.CreateHistoryItemElement(TestHelper.Key1, 1);
            Expect(target.IsValid);
        }

        [Test]
        public void EqualsAndGetHashCodeTest()
        {
            var target1 = new HistoryItemElement();
            var target2 = TestHelper.CreateHistoryItemElement(TestHelper.Key1, 1);
            var target3 = TestHelper.CreateHistoryItemElement(TestHelper.Key2, 1);
            var target4 = new HistoryItemElement();
            target4.Edition = target3.Edition;
            target4.PastMasterSignature = target3.PastMasterSignature;

            var hash1 = target1.GetHashCode();
            var hash2 = target2.GetHashCode();
            var hash3 = target3.GetHashCode();
            var hash4 = target4.GetHashCode();

            Expect(hash1, Is.Not.EqualTo(hash2));
            Expect(hash1, Is.Not.EqualTo(hash3));
            Expect(hash1, Is.Not.EqualTo(hash4));
            Expect(target1, Is.Not.EqualTo(target2));
            Expect(target1, Is.Not.EqualTo(target3));
            Expect(target1, Is.Not.EqualTo(target4));

            Expect(hash2, Is.Not.EqualTo(hash3));
            Expect(target2, Is.Not.EqualTo(target3));

            Expect(hash3, Is.EqualTo(hash4));
            Expect(target3, Is.EqualTo(target4));
        }
    }
}