using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.odec.storage.test
{
    [TestFixture]
    class ZipStorageTest : TestIStorage
    {
        private static readonly List<string> zipFiles
            = new List<string>();

        private static string CreateZipFile()
        {
            var tmpFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            zipFiles.Add(tmpFilePath);
            return tmpFilePath;
        }

        private static void DestroyZipFile(string zipFile)
        {
            if (zipFiles.Contains(zipFile))
            {
                File.Delete(zipFile);
                zipFiles.Remove(zipFile);
            }
        }

        [Test]
        public void ConstructionTest()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ZipStorage(null));

            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            ZipStorage zip = null;
            try
            {
                zip = new ZipStorage(path);
            }
            finally
            {
                if (zip != null) zip.Dispose();
                File.Delete(path);
            }
        }

        #region Overrides of TestIStorage

        protected override IStorage CreateStorage()
        {
            return new ZipStorage(CreateZipFile());
        }

        protected override void DisposeStorage(IStorage storage)
        {
            storage.Dispose();
            DestroyZipFile(((ZipStorage)storage).ZipFilePath);
        }

        [Test]
        public override void ExistsParamTest() { CheckExistsParam(); }

        [Test]
        public override void WriteParamTest() { CheckWriteParam(); }

        [Test]
        public override void WriteTest() { CheckWrite(); }

        [Test]
        public override void ReadParamTest() { CheckReadParam(); }

        [Test]
        public override void ReadTest() { CheckRead(); }

        [Test]
        public override void RemoveParamTest() { CheckRemoveParam(); }

        [Test]
        public override void RemoveTest() { CheckRemove(); }

        [Test]
        public override void GetFilesTest() { CheckGetFiles(); }

        #endregion
    }
}
