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
    class IndexElementTest : AssertionHelper
    {
        [Test]
        public void AddTest()
        {
            var target = new IndexElement();
            var firstId = target.GetNextEntityId();
            Expect(firstId == 0);
            Expect(target.Count == 0);
            Expect(target.Items.Length == 0);

            Assert.Throws<ArgumentNullException>(
                () => target.Add(null));

            var invalidItem1 = TestHelper.CreateIndexItemElement(TestHelper.Key1, 1, 0);
            invalidItem1.Id = -1;

            Assert.Throws<ArgumentException>(
                () => target.Add(invalidItem1));

            Expect(target.GetNextEntityId() == firstId);
            Expect(target.Count == 0);
            Expect(target.Items.Length == 0);

            var validItem = TestHelper.CreateIndexItemElement(
                TestHelper.Key1, firstId, firstId + 3);
            target.Add(validItem);
            Expect(target.Count == 1);
            Expect(target.Items.Length == 1);
            Expect(ReferenceEquals(validItem, target.Items[0]));

            var secondId = target.GetNextEntityId();
            Expect(secondId > firstId);

            Assert.Throws<ArgumentException>(
                () => target.Add(validItem));
            var invalidItem3 = TestHelper.CreateIndexItemElement(
                TestHelper.Key1, firstId, 0);
            Assert.Throws<ArgumentException>(
                () => target.Add(invalidItem3));
        }

        [Test]
        public void GetItemTest()
        {
            var target = new IndexElement();

            Assert.Throws<ArgumentException>(
                () => target.GetItem(0));

            var id1 = target.GetNextEntityId();
            var item1 = TestHelper.CreateIndexItemElement(
                TestHelper.Key1, id1, 2);
            Expect(!target.Contains(id1));
            Assert.Throws<ArgumentException>(
                () => target.GetItem(id1));
            target.Add(item1);
            Expect(target.Contains(id1));
            Expect(ReferenceEquals(item1, target.GetItem(id1)));

            var id2 = target.GetNextEntityId();
            var item2 = TestHelper.CreateIndexItemElement(
                TestHelper.Key1, id2, 1);
            Expect(!target.Contains(id2));
            Assert.Throws<ArgumentException>(
                () => target.GetItem(id2));
            target.Add(item2);
            Expect(target.Contains(id2));
            Expect(ReferenceEquals(item2, target.GetItem(id2)));

            var id3 = target.GetNextEntityId();
            var item3 = TestHelper.CreateIndexItemElement(
                TestHelper.Key1, id3, 0);
            Expect(!target.Contains(id3));
            Assert.Throws<ArgumentException>(
                () => target.GetItem(id3));
            target.Add(item3);
            Expect(target.Contains(id3));

            Expect(target.Count == 3);
            Expect(new[] { item1, item2, item3 },
                   Is.EqualTo(target.Items.OrderBy(item => item.Id)));

            Expect(ReferenceEquals(item1, target.GetItem(id1)));
            Expect(ReferenceEquals(item2, target.GetItem(id2)));
            Expect(ReferenceEquals(item3, target.GetItem(id3)));
        }

        [Test]
        public void RemoveTest()
        {
            var target = TestHelper.CreateIndexElement(TestHelper.Key1, 4);
            Expect(target.Count == 4);

            var items = target.Items;
            var nextId = target.GetNextEntityId();

            Assert.Throws<ArgumentException>(
                () => target.Remove(nextId));

            target.Remove(items[3].Id);
            Expect(target.Count == 3);
            Expect(target.Items.Length == 3);
            Expect(target.GetNextEntityId() == nextId);
            Expect(!target.Contains(items[3].Id));
        }

        [Test]
        public void ReadFromXmlParamTest()
        {
            var target = new IndexElement();
            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromXml(null));
        }

        [Test]
        public void WriteToXmlParamTest()
        {
            var target = new IndexElement();
            Assert.Throws<ArgumentNullException>(
                () => target.WriteToXml(null));
        }

        [Test]
        public void WriteToXmlTest()
        {
            var target = new IndexElement();
            Expect(!target.IsValid);

            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                Assert.Throws<InvalidOperationException>(
                    () => target.WriteToXml(w));
            }
        }

        [Test]
        public void SchemaValidationTest()
        {
            var target = TestHelper.CreateIndexElement(TestHelper.Key1, 4);
            TestHelper.CheckSchemaConformity(IndexElement.XML_NAME, Model.ContainerNamespace, target);
        }

        [Test]
        public void ReadFromXmlAndWriteToXmlTest()
        {
            var original = TestHelper.CreateIndexElement(TestHelper.Key1, 4);
            TestHelper.CheckIXmlStorable(original);
        }

        [Test]
        public void IsValidTest()
        {
            var target = new IndexElement();
            Expect(!target.IsValid);

            target = TestHelper.CreateIndexElement(TestHelper.Key1, 1);
            Expect(target.IsValid);
            target = TestHelper.CreateIndexElement(TestHelper.Key1, 2);
            Expect(target.IsValid);
            target = TestHelper.CreateIndexElement(TestHelper.Key1, 3);
            Expect(target.IsValid);
        }

        [Test]
        public void EqualsAndGetHashCodeTest()
        {
            var target1 = new IndexElement();
            var target2 = new IndexElement();
            var target3 = TestHelper.CreateIndexElement(TestHelper.Key1, 2);
            var target4 = TestHelper.CreateIndexElement(TestHelper.Key1, 2);
            var target5 = TestHelper.CreateIndexElement(TestHelper.Key1, 1);
            var target6 = TestHelper.CreateIndexElement(TestHelper.Key2, 2);

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
