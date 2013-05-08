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
    internal class EditionElementTest : AssertionHelper
    {
        [Test]
        public void StaticCreateTest()
        {
            const string software = "Test Software";
            const string profile = "Test Profile";
            const string version = "1.2.3.4000b";
            var owner = TestHelper.CreateOwnerType(1);
            const string copyright = "Test \x00A9 Copyright";
            const string comments = "Test comments\nwith multiple\nlines.";

            Assert.Throws<ArgumentNullException>(
                () => EditionElement.Create(null, profile, version, owner, copyright, comments));
            Assert.Throws<ArgumentNullException>(
                () => EditionElement.Create(software, null, version, owner, copyright, comments));
            Assert.Throws<ArgumentNullException>(
                () => EditionElement.Create(software, profile, null, owner, copyright, comments));
            Assert.Throws<ArgumentNullException>(
                () => EditionElement.Create(software, profile, version, null, copyright, comments));
            Assert.Throws<ArgumentNullException>(
                () => EditionElement.Create(software, profile, version, owner, null, comments));

            var result = EditionElement.Create(software, profile, version, owner, copyright, null);
            result = EditionElement.Create(software, profile, version, owner, copyright, comments);

            Expect(result.Guid, Is.Not.EqualTo(Guid.Empty));
            Expect(result.Salt, Is.Null);
            Expect(result.Software, Is.EqualTo(software));
            Expect(result.Profile, Is.EqualTo(profile));
            Expect(result.Version, Is.EqualTo(version));
            Expect(ReferenceEquals(result.Owner, owner));
            Expect(result.Copyright, Is.EqualTo(copyright));
            Expect(result.Comments, Is.EqualTo(comments));
            Expect(result.Timestamp > DateTime.Now - new TimeSpan(0, 0, 0, 5));
            Expect(result.HistorySignature, Null);
            Expect(result.IndexSignature, Null);
        }

        [Test]
        public void ReadFromXmlParamTest()
        {
            var target = new EditionElement();
            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromXml(null));
        }

        [Test]
        public void WriteToXmlParamTest()
        {
            var target = new EditionElement();
            Assert.Throws<ArgumentNullException>(
                () => target.WriteToXml(null));
        }

        [Test]
        public void WriteToXmlTest()
        {
            var target = new EditionElement();
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
            var target = TestHelper.CreateEditionElement(TestHelper.Key1, 1, 0, 0);
            TestHelper.CheckSchemaConformity(EditionElement.XML_NAME, Model.ContainerNamespace, target);
            
            target = TestHelper.CreateEditionElement(TestHelper.Key1, 1, 1, 1);
            TestHelper.CheckSchemaConformity(EditionElement.XML_NAME, Model.ContainerNamespace, target); 
            
            target = TestHelper.CreateEditionElement(TestHelper.Key1, 1, 5, 5);
            TestHelper.CheckSchemaConformity(EditionElement.XML_NAME, Model.ContainerNamespace, target);
        }

        [Test]
        public void ReadFromXmlAndWriteToXmlTest()
        {
            var original = TestHelper.CreateEditionElement(TestHelper.Key1, 1, 3, 3);

            var copy = TestHelper.CopyByXmlSerialization(original);

            Expect(copy, Is.EqualTo(original));
        }

        [Test]
        public void IsValidTest()
        {
            var target = new EditionElement();
            Expect(!target.IsValid);

            target = TestHelper.CreateEditionElement(TestHelper.Key1, 1, 3, 3);
            Expect(target.IsValid);
        }

        [Test]
        public void EqualsAndGetHashCodeTest()
        {
            var target1 = new EditionElement();
            var target2 = TestHelper.CreateEditionElement(TestHelper.Key1, 1, 3, 3);
            var target3 = TestHelper.CreateEditionElement(TestHelper.Key1, 1, 3, 3);
            var target4 = TestHelper.CreateEditionElement(TestHelper.Key1, 2, 3, 3);
            var target5 = TestHelper.CreateEditionElement(TestHelper.Key1, 1, 1, 3);
            var target6 = TestHelper.CreateEditionElement(TestHelper.Key1, 1, 3, 1);

            var hash1 = target1.GetHashCode();
            var hash2 = target2.GetHashCode();
            var hash3 = target3.GetHashCode();
            var hash4 = target4.GetHashCode();
            var hash5 = target5.GetHashCode();
            var hash6 = target6.GetHashCode();

            Expect(hash1, Is.Not.EqualTo(hash2));
            Expect(target1, Is.Not.EqualTo(target2));

            Expect(hash1, Is.Not.EqualTo(hash3));
            Expect(target1, Is.Not.EqualTo(target3));

            Expect(hash1, Is.Not.EqualTo(hash4));
            Expect(target1, Is.Not.EqualTo(target4));

            Expect(hash2, Is.EqualTo(hash3));
            Expect(target2, Is.EqualTo(target3));

            Expect(hash2, Is.Not.EqualTo(hash4));
            Expect(target2, Is.Not.EqualTo(target4));

            Expect(hash2, Is.Not.EqualTo(hash5));
            Expect(target2, Is.Not.EqualTo(target5));

            Expect(hash2, Is.Not.EqualTo(hash6));
            Expect(target2, Is.Not.EqualTo(target6));
        }
    }
}