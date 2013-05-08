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
    class ValueReferenceTest : AssertionHelper
    {
        [Test]
        public void ReadFromXmlParamTest()
        {
            var target = new ValueReference();
            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromXml(null));
        }

        [Test]
        public void WriteToXmlParamTest()
        {
            var target = new ValueReference();
            Assert.Throws<ArgumentNullException>(
                () => target.WriteToXml(null));
        }

        [Test]
        public void WriteToXmlTest()
        {
            var target = new ValueReference();
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
            var original = TestHelper.CreateValueReference(TestHelper.Key1, 1);
            var copy = TestHelper.CopyByXmlSerialization(original);
            Expect(copy, Is.EqualTo(original));
        }

        [Test]
        public void IsValidTest()
        {
            var target = new ValueReference();
            Expect(!target.IsValid);

            target = TestHelper.CreateValueReference(TestHelper.Key1, 1);
            Expect(target.IsValid);
        }

        [Test]
        public void EqualsAndGetHashCodeTest()
        {
            var target1 = new ValueReference();
            var target2 = new ValueReference();
            var target3 = TestHelper.CreateValueReference(TestHelper.Key1, 1);
            var target4 = TestHelper.CreateValueReference(TestHelper.Key1, 1);
            var target5 = TestHelper.CreateValueReference(TestHelper.Key1, 2);
            var target6 = TestHelper.CreateValueReference(TestHelper.Key2, 1);

            var hash1 = target1.GetHashCode();
            var hash2 = target2.GetHashCode();
            var hash3 = target3.GetHashCode();
            var hash4 = target4.GetHashCode();
            var hash5 = target5.GetHashCode();
            var hash6 = target6.GetHashCode();

            Expect(hash1, Is.EqualTo(hash2));
            Expect(target1, Is.EqualTo(target2));

            Expect(hash1, Is.Not.EqualTo(hash3));
            Expect(target1, Is.Not.EqualTo(target3));

            Expect(hash3, Is.EqualTo(hash4));
            Expect(target3, Is.EqualTo(target4));

            Expect(hash3, Is.Not.EqualTo(hash5));
            Expect(target3, Is.Not.EqualTo(target5));

            Expect(hash3, Is.Not.EqualTo(hash6));
            Expect(target3, Is.Not.EqualTo(target6));
        }
    }
}
