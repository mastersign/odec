using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.test;
using NUnit.Framework;

namespace de.mastersign.odec.process.test
{
    [TestFixture]
    class ProfileTest : AssertionHelper
    {
        [Test]
        public void LoadFromXml()
        {
            var target = new Profile();

            Assert.Throws<ArgumentNullException>(
                () => target.LoadFromXml((XmlReader)null));
            Assert.Throws<ArgumentNullException>(
                () => target.LoadFromXml((XmlElement) null));

            using (var xr = XmlReader.Create(TestHelper.GetResFile("DemoProfile.xml")))
            {
                target.LoadFromXml(xr);
            }

            Expect(target.Name, Is.EqualTo("Demo Profile"));
            Expect(target.Version, Is.EqualTo("1.0"));
            Expect(target.Description, !Null);
            
            Expect(target.DataTypes.Count == 3);
            Expect(target.EntityTypes.Count == 2);
            Expect(target.ProvenanceInterfaces.Count == 3);
        }
    }
}
