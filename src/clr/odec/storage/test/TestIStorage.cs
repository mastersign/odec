using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.odec.storage.test
{
    abstract class TestIStorage : AssertionHelper
    {
        protected abstract IStorage CreateStorage();

        protected abstract void DisposeStorage(IStorage storage);

        public abstract void ExistsParamTest();
        protected void CheckExistsParam()
        {
            var target = CreateStorage();

            Assert.Throws<ArgumentNullException>(
                () => target.Exists(null));

            foreach (var ic in Path.GetInvalidPathChars())
            {
                var icCopy = ic;
                Assert.Throws<ArgumentException>(
                    () => target.Exists(string.Format("test{0}/invalid.bin", icCopy)));
            }

            DisposeStorage(target);
        }

        public abstract void WriteParamTest();
        protected void CheckWriteParam()
        {
            var ms = new MemoryStream(GetRandomData(1024));
            var target = CreateStorage();

            Assert.Throws<ArgumentNullException>(
                () => target.Write(null, ms));

            Assert.Throws<ArgumentException>(
                () => target.Write("", ms));

            Assert.Throws<ArgumentNullException>(
                () => target.Write((string)"writeparamtest.bin", (Stream)null));

            var tmpFile = Path.GetTempFileName();
            using (var invalidStream = File.OpenWrite(tmpFile))
            {
                Assert.Throws<ArgumentException>(
                    () => target.Write("writeparamtest.bin", invalidStream));
            }
            File.Delete(tmpFile);

            ms.Position = 0L;

            foreach (var ic in Path.GetInvalidPathChars())
            {
                var icCopy = ic;
                Assert.Throws<ArgumentException>(
                    () => target.Write(string.Format("test{0}/invalid.bin", icCopy), ms));
            }

            ms.Dispose();

            DisposeStorage(target);
        }

        private static byte[] GetRandomData(int size)
        {
            var data = new byte[size];
            new Random().NextBytes(data);
            return data;
        }

        private void WriteTestFile(IStorage store, string file, bool byFile)
        {
            WriteTestFile(store, file, GetRandomData(1024), byFile);
        }

        private void WriteTestFile(IStorage store, string file, byte[] data, bool byFile)
        {
            if (byFile)
            {
                var tmpFile = Path.GetTempFileName();
                File.WriteAllBytes(tmpFile, data);
                store.Write(file, tmpFile);
                File.Delete(tmpFile);
            }
            else
            {
                var ms = new MemoryStream(data);
                store.Write(file, ms);
                Assert.Throws<ObjectDisposedException>(
                    () => ms.Position = 0L);
            }
        }

        public abstract void WriteTest();
        protected void CheckWrite()
        {
            var target = CreateStorage();

            const string file1 = "test.bin";
            const string file2 = "00001/test.bin";

            Expect(!target.Exists(file1));
            WriteTestFile(target, file1, false);
            Expect(target.Exists(file1));

            Expect(!target.Exists(file2));
            WriteTestFile(target, file2, true);
            Expect(target.Exists(file2));
            WriteTestFile(target, file2, true);
            Expect(target.Exists(file2));

            DisposeStorage(target);
        }

        public abstract void ReadParamTest();
        protected void CheckReadParam()
        {
            var target = CreateStorage();

            Assert.Throws<ArgumentNullException>(
                () => target.Read(null));

            Assert.Throws<ArgumentException>(
                () => target.Read(""));

            foreach (var ic in Path.GetInvalidPathChars())
            {
                var icCopy = ic;
                Assert.Throws<ArgumentException>(
                    () => target.Read(string.Format("test{0}/invalid.bin", icCopy)));
            }

            Assert.Throws<FileNotFoundException>(
                () => target.Read("notexisting.bin"));

            DisposeStorage(target);
        }

        public abstract void ReadTest();
        protected void CheckRead()
        {
            var data1 = GetRandomData(1024);
            var data2 = GetRandomData(1024);

            var target = CreateStorage();

            const string file1 = "test1.bin";
            const string file2 = "subdir/test2.bin";
            WriteTestFile(target, file1, data1, false);
            Expect(target.Exists(file1));
            WriteTestFile(target, file2, data2, true);
            Expect(target.Exists(file2));

            using (var s = target.Read(file1))
            {
                Expect(s, !Null);
                Expect(s.CanRead);
                Expect(!s.CanWrite);

                var copy = new byte[data1.Length];
                s.Read(copy, 0, data1.Length);
                Expect(copy, Is.EqualTo(data1));
            }

            using (var s = target.Read(file2))
            {
                Expect(s, !Null);
                Expect(s.CanRead);
                Expect(!s.CanWrite);

                var copy = new byte[data2.Length];
                s.Read(copy, 0, data2.Length);
                Expect(copy, Is.EqualTo(data2));
            }

            DisposeStorage(target);
        }

        public abstract void RemoveParamTest();
        protected void CheckRemoveParam()
        {
            var target = CreateStorage();

            Assert.Throws<ArgumentNullException>(
                () => target.Remove(null));

            Assert.Throws<ArgumentException>(
                () => target.Remove(""));

            foreach (var ic in Path.GetInvalidPathChars())
            {
                var icCopy = ic;
                Assert.Throws<ArgumentException>(
                    () => target.Read(string.Format("test{0}/invalid.bin", icCopy)));
            }

            Assert.Throws<FileNotFoundException>(
                () => target.Remove("notexisting.bin"));

            DisposeStorage(target);
        }

        public abstract void RemoveTest();
        protected void CheckRemove()
        {
            var target = CreateStorage();

            const string file1 = "removetest.bin";
            const string file2 = "sub1/sub2/removetest.bin";

            Expect(!target.Exists(file1));
            WriteTestFile(target, file1, true);
            Expect(target.Exists(file1));

            Expect(!target.Exists(file2));
            WriteTestFile(target, file2, false);
            Expect(target.Exists(file2));

            target.Remove(file1);
            Expect(!target.Exists(file1));
            Expect(target.Exists(file2));

            target.Remove(file2);
            Expect(!target.Exists(file2));

            DisposeStorage(target);
        }

        public abstract void GetFilesTest();
        protected void CheckGetFiles()
        {
            var target = CreateStorage();

            var files = new[] {
                "enumeration1.bin", 
                "enumeration2.bin", 
                "test/enumeration1.bin", 
                "test/enumeration2.bin",
                "test2/enumeration.bin",
            };

            foreach (var file in files)
            {
                WriteTestFile(target, file, false);
            }

            var e = target.GetFiles();
            var files2 = e.ToArray();

            Array.Sort(files);
            Array.Sort(files2);

            Expect(files, Is.EqualTo(files2));
        }
    }
}
