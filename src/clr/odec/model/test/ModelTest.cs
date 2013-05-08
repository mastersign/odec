using NUnit.Framework;

namespace de.mastersign.odec.model.test
{
    [TestFixture]
    internal class ModelTest : AssertionHelper
    {
        [Test]
        public void SchemaTest()
        {
            Expect(Model.Schema, !Null);
        }

        [Test]
        public void ContainerNamespaceTest()
        {
            Expect(Model.ContainerNamespace, !Null);
        }

        [Test]
        public void NamespaceManagerTest()
        {
            var nm = Model.NamespaceManager;
            Expect(nm.LookupNamespace("c"), Is.EqualTo(Model.ContainerNamespace));
            Expect(nm.LookupNamespace("sig"), Is.EqualTo(Model.XMLSIGD_NS));
            Expect(nm.LookupPrefix(Model.ContainerNamespace), Is.EqualTo("c"));
            Expect(nm.LookupPrefix(Model.XMLSIGD_NS), Is.EqualTo("sig"));
        }
    }
}