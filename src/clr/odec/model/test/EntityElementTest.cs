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
    class EntityElementTest : AssertionHelper
    {
        [Test]
        public void ReadFromXmlParamTest()
        {
            var target = new EntityElement();
            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromXml(null));
        }

        [Test]
        public void WriteToXmlParamTest()
        {
            var target = new EntityElement();
            Assert.Throws<ArgumentNullException>(
                () => target.WriteToXml(null));
        }

        [Test]
        public void WriteToXmlTest()
        {
            var target = new EntityElement();
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                Expect(!target.IsValid);
                Assert.Throws<InvalidOperationException>(
                    () => target.WriteToXml(w));
            }
        }

        [Test]
        public void SchemaValidationTest()
        {
            var target = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 1, false);
            TestHelper.CheckSchemaConformity(EntityElement.XML_NAME, Model.ContainerNamespace, target);
            target = TestHelper.CreateEntityElement(TestHelper.Key1, 2, 4, false);
            TestHelper.CheckSchemaConformity(EntityElement.XML_NAME, Model.ContainerNamespace, target);
            target = TestHelper.CreateEntityElement(TestHelper.Key1, 3, 4, true);
            TestHelper.CheckSchemaConformity(EntityElement.XML_NAME, Model.ContainerNamespace, target);
        }

        [Test]
        public void ReadFromXmlWriteToXmlTest()
        {
            var target1 = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 1, false);
            var target2 = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 2, false);
            var target3 = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 2, true);
            TestHelper.CheckIXmlStorable(target1, target2, target3);
        }

        [Test]
        public void IsValidTest()
        {
            var target = new EntityElement();
            Expect(!target.IsValid);
            target = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 1, false);
            target.Id = -1;
            Expect(!target.IsValid);
            Expect(!target.Validate(null));

            target.Id = 0;
            Expect(target.IsValid);
            Expect(target.Validate(null));
            target.Id = 99999;
            Expect(target.IsValid);
            Expect(target.Validate(null));

            target.Id = 100000;
            Expect(!target.IsValid);
            Expect(!target.Validate(null));

            target = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 1, false);
            target.Values.Clear();
            Expect(!target.IsValid);
            Expect(!target.Validate(null));

            target = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 1, false);
            Expect(target.IsValid);
            Expect(target.Validate(null));
            target = TestHelper.CreateEntityElement(TestHelper.Key1, 2, 1, true);
            Expect(target.IsValid);
            Expect(target.Validate(null));
            target = TestHelper.CreateEntityElement(TestHelper.Key1, 3, 10, true);
            Expect(target.IsValid);
            Expect(target.Validate(null));
        }

        [Test]
        public void EqualsAndGetHashCodeTest()
        {
            var target1 = new EntityElement();
            var target2 = new EntityElement();
            var target3 = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 1, true);
            var target4 = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 1, true);
            var target5 = TestHelper.CreateEntityElement(TestHelper.Key1, 1, 1, false);
            var target6 = TestHelper.CreateEntityElement(TestHelper.Key2, 1, 2, true);
            var target7 = TestHelper.CreateEntityElement(TestHelper.Key2, 2, 1, true);

            var hash1 = target1.GetHashCode();
            var hash2 = target2.GetHashCode();
            var hash3 = target3.GetHashCode();
            var hash4 = target4.GetHashCode();
            var hash5 = target5.GetHashCode();
            var hash6 = target6.GetHashCode();
            var hash7 = target7.GetHashCode();

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

            Expect(hash3, Is.Not.EqualTo(hash7));
            Expect(target3, Is.Not.EqualTo(target7));
        }
    }
}
