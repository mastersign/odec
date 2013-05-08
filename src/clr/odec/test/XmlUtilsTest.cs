using System;
using System.Xml;
using NUnit.Framework;
using de.mastersign.odec.model;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.test
{
    [TestFixture]
    internal class XmlUtilsTest : AssertionHelper
    {
        [Test]
        public void ParseValueTest()
        {
            var text = "1234";
            var invalidText = "abc";
            var def = 1;
            var expected = 1234;

            Assert.Throws<ArgumentNullException>(
                () => XmlUtils.ParseValue(null, def, int.Parse));

            Assert.Throws<ArgumentNullException>(
                () => XmlUtils.ParseValue(text, def, null));

            Expect(XmlUtils.ParseValue(text, def, int.Parse), Is.EqualTo(expected));
            Expect(XmlUtils.ParseValue(invalidText, def, int.Parse), Is.EqualTo(def));
        }

        [Test]
        public void ReadParsedObjectTest()
        {
            var eName = "c:x_Element";
            var def = 1;
            var expected = 1234;
            var doc = new XmlDocument();
            var e = doc.CreateElement(eName, Model.ContainerNamespace);
            doc.AppendChild(e);
            e.InnerText = "1234";

            Assert.Throws<ArgumentNullException>(
                () => XmlUtils.ReadParsedObject(null, eName, def, int.Parse));

            Assert.Throws<ArgumentNullException>(
                () => e.ReadParsedObject(null, def, int.Parse));

            Assert.Throws<ArgumentException>(
                () => e.ReadParsedObject("", def, int.Parse));

            Assert.Throws<ArgumentException>(
                () => e.ReadParsedObject(" ", def, int.Parse));

            Assert.Throws<ArgumentNullException>(
                () => e.ReadParsedObject(eName, def, null));

            Expect(e.ReadParsedObject(eName, def, int.Parse), Is.EqualTo(def));
            Expect(doc.ReadParsedObject(eName, def, int.Parse), Is.EqualTo(expected));
            e.InnerText = "abc";
            Expect(doc.ReadParsedObject(eName, def, int.Parse), Is.EqualTo(def));
        }
    }
}