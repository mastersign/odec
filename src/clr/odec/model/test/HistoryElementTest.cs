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
    internal class HistoryElementTest : AssertionHelper
    {
        [Test]
        public void PushParamTest()
        {
            var target = new HistoryElement();

            Assert.Throws<ArgumentNullException>(
                () => target.Push(null));
        }

        [Test]
        public void PushTest()
        {
            var target = new HistoryElement();

            var items = new HistoryItemElement[4];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = TestHelper.CreateHistoryItemElement(TestHelper.Key1, i);
            }

            Expect(target.Count == 0);
            for (int i = 0; i < items.Length; i++)
            {
                Expect(target.Count == i);
                target.Push(items[i]);
                Expect(target.Count == i + 1);
                Expect(target.Peek() == items[i]);
                Expect(target.Items[target.Count - 1] == items[i]);
            }

            Expect(target.Items, Is.EqualTo(items));
        }

        [Test]
        public void PopTest()
        {
            var target = new HistoryElement();
            Expect(target.Count == 0);
            Assert.Throws<InvalidOperationException>(
                () => target.Pop());

            var items = new HistoryItemElement[4];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = TestHelper.CreateHistoryItemElement(TestHelper.Key1, i);
                target.Push(items[i]);
            }

            Expect(target.Count == items.Length);
            for (int i = items.Length - 1; i >= 0; i--)
            {
                var item = target.Pop();
                Expect(target.Count == i);
                Expect(item == items[i]);
            }

            var emptyArray = new HistoryItemElement[0];
            Expect(target.Items, Is.EqualTo(emptyArray));

            Expect(target.Count == 0);
            Assert.Throws<InvalidOperationException>(
                () => target.Pop());
        }

        [Test]
        public void PeekTest()
        {
            var target = new HistoryElement();
            Expect(target.Count == 0);
            Assert.Throws<InvalidOperationException>(
                () => target.Peek());

            for (int i = 0; i < 4; i++)
            {
                var item = TestHelper.CreateHistoryItemElement(TestHelper.Key1, i);
                target.Push(item);
                Expect(target.Peek() == item);
            }
        }

        [Test]
        public void ClearTest()
        {
            var target = new HistoryElement();
            Expect(target.Count == 0);
            target.Clear();
            Expect(target.Count == 0);

            target = TestHelper.CreateHistoryElement(TestHelper.Key1, 4);
            Expect(target.Count == 4);
            Expect(target.Items.Length == 4);
            target.Clear();
            Expect(target.Count == 0);
            Expect(target.Items.Length == 0);
        }

        [Test]
        public void ReadFromXmlParamTest()
        {
            var target = new HistoryElement();
            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromXml(null));
        }

        [Test]
        public void WriteToXmlParamTest()
        {
            var target = new HistoryElement();
            Assert.Throws<ArgumentNullException>(
                () => target.WriteToXml(null));
        }

        [Test]
        public void WriteToXmlTest()
        {
            var target = new HistoryElement();
            Expect(target.IsValid);

            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                target.WriteToXml(w);
            }
        }

        [Test]
        public void SchemaValidationTest()
        {
            var target = TestHelper.CreateHistoryElement(TestHelper.Key1, 4);
            TestHelper.CheckSchemaConformity(HistoryElement.XML_NAME, Model.ContainerNamespace, target);
        }

        [Test]
        public void ReadFromXmlAndWriteToXmlTest()
        {
            var original = TestHelper.CreateHistoryElement(TestHelper.Key1, 4);

            TestHelper.CheckIXmlStorable(original);
        }

        [Test]
        public void IsValidTest()
        {
            var target = new HistoryElement();
            Expect(target.IsValid);

            target = TestHelper.CreateHistoryElement(TestHelper.Key1, 1);
            Expect(target.IsValid);
            target = TestHelper.CreateHistoryElement(TestHelper.Key1, 2);
            Expect(target.IsValid);
            target = TestHelper.CreateHistoryElement(TestHelper.Key1, 3);
            Expect(target.IsValid);
        }

        [Test]
        public void EqualsAndGetHashCodeTest()
        {
            var target1 = new HistoryElement();
            var target2 = new HistoryElement();
            var target3 = TestHelper.CreateHistoryElement(TestHelper.Key1, 1);
            var target4 = TestHelper.CreateHistoryElement(TestHelper.Key1, 2);
            var target5 = TestHelper.CreateHistoryElement(TestHelper.Key1, 2);

            var hash1 = target1.GetHashCode();
            var hash2 = target2.GetHashCode();
            Expect(hash1, Is.EqualTo(hash2));
            Expect(target1, Is.EqualTo(target2));

            var hash3 = target3.GetHashCode();
            Expect(hash3, Is.Not.EqualTo(hash2));
            Expect(target3, Is.Not.EqualTo(target2));

            var hash4 = target4.GetHashCode();
            Expect(hash4, Is.Not.EqualTo(hash3));
            Expect(target4, Is.Not.EqualTo(target3));

            var hash5 = target4.GetHashCode();
            Expect(hash5, Is.EqualTo(hash4));
            Expect(target5.Equals(target4));
        }
    }
}