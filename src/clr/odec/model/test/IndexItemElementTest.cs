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
    class IndexItemElementTest : AssertionHelper
    {
        [Test]
        public void ReadFromXmlParamTest()
        {
            var target = new IndexItemElement();
            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromXml(null));
        }

        [Test]
        public void WriteToXmlParamTest()
        {
            var target = new IndexItemElement();
            Assert.Throws<ArgumentNullException>(
                () => target.WriteToXml(null));
        }

        [Test]
        public void WriteToXmlTest()
        {
            var target = new IndexItemElement();
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                Expect(!target.IsValid);
                Assert.Throws<InvalidOperationException>(
                    () => target.WriteToXml(w));
            }
        }

        [Test]
        public void ReadFromXmlAndWriteToXmlTest()
        {
            var original = TestHelper.CreateIndexItemElement(TestHelper.Key1, 3, 3);
            TestHelper.CheckIXmlStorable(original);
        }

        [Test]
        public void IsValidTest()
        {
            var target = new IndexItemElement();
            Expect(!target.IsValid);

            target = TestHelper.CreateIndexItemElement(TestHelper.Key1, 1, 3);
            target.Id = -1;
            Expect(!target.IsValid);

            target = TestHelper.CreateIndexItemElement(TestHelper.Key1, 1, 3);
            Expect(target.IsValid);

            target = TestHelper.CreateIndexItemElement(TestHelper.Key1, 3, 3);
            Expect(target.IsValid);
        }

        [Test]
        public void EqualsAndGetHashCodeTest()
        {
            var target1 = new IndexItemElement();
            var target2 = new IndexItemElement();
            var target3 = TestHelper.CreateIndexItemElement(TestHelper.Key1, 1, 2);
            var target4 = TestHelper.CreateIndexItemElement(TestHelper.Key1, 1, 2);
            var target5 = TestHelper.CreateIndexItemElement(TestHelper.Key1, 1, 3);
            var target6 = TestHelper.CreateIndexItemElement(TestHelper.Key2, 1, 2);
            var target7 = TestHelper.CreateIndexItemElement(TestHelper.Key1, 2, 2);
            var target8 = TestHelper.CreateIndexItemElement(TestHelper.Key1, 2, 2);
            target8.Label = "testlabel";

            var hash1 = target1.GetHashCode();
            var hash2 = target2.GetHashCode();
            var hash3 = target3.GetHashCode();
            var hash4 = target4.GetHashCode();
            var hash5 = target5.GetHashCode();
            var hash6 = target6.GetHashCode();
            var hash7 = target7.GetHashCode();
            var hash8 = target8.GetHashCode();

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

            Expect(hash1, Is.Not.EqualTo(hash7));
            Expect(target1, Is.Not.EqualTo(target7));

            Expect(hash7, Is.Not.EqualTo(hash8));
            Expect(target7, Is.Not.EqualTo(target8));
        }
    }
}
