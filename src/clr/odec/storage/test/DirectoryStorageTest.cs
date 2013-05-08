using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.odec.storage.test
{
    [TestFixture]
    class DirectoryStorageTest : TestIStorage
    {
        private static DirectoryInfo CreateStorageDir()
        {
            var rootDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var dir = new DirectoryInfo(rootDir);
            dir.Create();
            return dir;
        }

        private static void DestroyStorageDir(DirectoryInfo dir)
        {
            dir.Delete(true);
        }

        [Test]
        public void ConstructionTest()
        {
            Assert.Throws<ArgumentNullException>(
                () => new DirectoryStorage(null));

            var notExistent = new DirectoryInfo(Path.Combine(
                Path.GetTempPath(), Path.GetRandomFileName()));
            Expect(!notExistent.Exists);

            // Test with existing directory
            var dir2 = CreateStorageDir();
            using (new DirectoryStorage(dir2)) { }
            DestroyStorageDir(dir2);

            // Test with not existing directory
            var dir1 = CreateStorageDir();
            dir1.Delete(true);
            dir1.Refresh();
            Expect(!dir1.Exists);
            using (new DirectoryStorage(dir1)) { }
            Expect(dir1.Exists);
            DestroyStorageDir(dir1);
        }

        #region TestIStorable

        protected override IStorage CreateStorage()
        {
            return new DirectoryStorage(CreateStorageDir());
        }

        protected override void DisposeStorage(IStorage storage)
        {
            DestroyStorageDir(((DirectoryStorage)storage).RootDirectory);
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
