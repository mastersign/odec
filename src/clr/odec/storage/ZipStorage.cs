using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec.storage
{
    /// <summary>
    /// An implementation of <see cref="IStorage"/>, 
    /// using a ZIP file as storage for the container.
    /// This Implementation uses the SharpZipLib.
    /// </summary>
    public class ZipStorage : IStorage
    {
        private class StaticDataSource : IStaticDataSource
        {
            private readonly Stream stream;

            public StaticDataSource(Stream stream)
            {
                this.stream = stream;
            }

            public Stream GetSource() { return stream; }
        }

        private readonly string file;
        private readonly ZipFile zip;

        ///// <summary>
        ///// Gets the <see cref="ZipFile"/> object, representing
        ///// the ZIP archive.
        ///// </summary>
        ///// <value>The handle for the ZIP archive.</value>
        //public ZipFile ZipFile { get { return zip; } }

        /// <summary>
        /// Gets the path to the ZipFile.
        /// </summary>
        /// <value>The path to the ZipFile.</value>
        public string ZipFilePath { get { return file; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipStorage"/> class.
        /// </summary>
        /// <param name="zipFile">The path to the ZIP archive for the container.</param>
        public ZipStorage(string zipFile)
        {
            if (zipFile == null) throw new ArgumentNullException("zipFile");
            file = zipFile;
            zip = File.Exists(zipFile)
                ? new ZipFile(zipFile)
                : ZipFile.Create(zipFile);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Resources.ZipStorage_Name;
        }

        #region Implementation of IStorage

        /// <summary>
        /// Opens an octet stream for the specified storage file.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="Stream"/> can not be written.
        /// </remarks>
        /// <param name="relativePath">The relative path to the storage file in the container.</param>
        /// <returns>A <see cref="Stream"/> for reading the content of the given storage file.</returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="relativePath"/>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Is thrown, if the specified storage file does not exist.
        /// </exception>
        /// <exception cref="FormatException">
        /// Is thrown, if the given relative path contains invalid characters or has the length of zero.
        /// </exception>
        public Stream Read(string relativePath)
        {
            if (!Exists(relativePath))
            {
                throw new FileNotFoundException(Resources.Storage_StorageFileNotFound, relativePath);
            }
            var ze = zip.GetEntry(relativePath);
            try
            {
                return zip.GetInputStream(ze);
            }
            catch (Exception ex)
            {
                throw new StorageException(Resources.Storage_Read_OpeningFailed, ex);
            }
        }

        /// <summary>
        /// Writes the content of the given <see cref="Stream"/> to the storage file.
        /// </summary>
        /// <param name="relativePath">The relative path to the storage file in the container.</param>
        /// <param name="source">The source of the data to be written.</param>
        /// <remarks>
        /// The returned <see cref="Stream"/> can not be read.
        /// If no storage file exists under the given path, a new storage file
        /// is created. If a storage file exists under the given path,
        /// its content is replaced with the data, written to the returned stream.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="relativePath"/> or <paramref name="source"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if <paramref name="source"/> is not readable.
        /// </exception>
        /// <exception cref="FormatException">
        /// Is thrown, if the given relative path contains invalid characters or has the length of zero.
        /// </exception>
        /// <exception cref="StorageException">
        /// Is thrown, if the storage causes an error while opening the storage file.
        /// </exception>
        public void Write(string relativePath, Stream source)
        {
            if (relativePath == null) throw new ArgumentNullException("relativePath");
            if (relativePath.Length == 0) throw new ArgumentException(Resources.Storage_ArgumentException_EmptyPath, "relativePath");
            if (source == null) throw new ArgumentNullException("source");
            if (!source.CanRead) throw new ArgumentException(Resources.Storage_Write_StreamNotReadable, "source");
            if (IsInvalidPath(relativePath))
            {
                throw new ArgumentException(Resources.ZipStorage_ArgumentException_InvalidPathCharacters);
            }

            try
            {
                zip.BeginUpdate();
                zip.Add(new StaticDataSource(source), relativePath);
                zip.CommitUpdate();
            }
            catch (Exception ex)
            {
                throw new StorageException("The writing of the storage file failed.", ex);
            }
            finally
            {
                source.Dispose();
            }
        }

        /// <summary>
        /// Writes the content of the given source file to the storage file.
        /// </summary>
        /// <param name="relativePath">The relative path to the storage file in the container.</param>
        /// <param name="sourceFile">The absolute file system path of the data to be written.</param>
        /// <remarks>
        /// The returned <see cref="Stream"/> can not be read.
        /// If no storage file exists under the given path, a new storage file
        /// is created. If a storage file exists under the given path,
        /// its content is replaced with the data, written to the returned stream.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="relativePath"/> or <paramref name="sourceFile"/>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Is thrown, if <paramref name="sourceFile"/> does not exist.
        /// </exception>
        /// <exception cref="FormatException">
        /// Is thrown, if the given relative path contains invalid characters or has the length of zero.
        /// </exception>
        /// <exception cref="StorageException">
        /// Is thrown, if the storage causes an error while opening the storage file.
        /// </exception>
        public void Write(string relativePath, string sourceFile)
        {
            if (relativePath == null) throw new ArgumentNullException("relativePath");
            if (relativePath.Length == 0) throw new ArgumentException("The given path is empty.", "relativePath");
            if (sourceFile == null) throw new ArgumentNullException("source");
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("The given source file does not exist.", sourceFile);
            if (IsInvalidPath(relativePath))
            {
                throw new ArgumentException(Resources.ZipStorage_ArgumentException_InvalidPathCharacters);
            }

            try
            {
                zip.BeginUpdate();
                zip.Add(sourceFile, relativePath);
                zip.CommitUpdate();
            }
            catch (Exception ex)
            {
                throw new StorageException("The writing of the storage file failed.", ex);
            }
        }

        /// <summary>
        /// Removes the specified storage file.
        /// </summary>
        /// <param name="relativePath">The relative path to the storage file.</param>
        /// <returns>A <see cref="Stream"/> for reading the content of the given storage file.</returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="relativePath"/>.
        /// </exception>
        /// <exception cref="FormatException">
        /// Is thrown, if the given relative path contains invalid characters or has the length of zero.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Is thrown, if the specified storage file does not exist.
        /// </exception>
        /// <exception cref="StorageException">
        /// Is thrown, if the storage causes an error while removing the storage file.
        /// </exception>
        public void Remove(string relativePath)
        {
            if (!Exists(relativePath))
            {
                throw new FileNotFoundException(
                    "The specified storage file was not found.",
                    relativePath);
            }
            try
            {
                zip.BeginUpdate();
                var ze = zip.GetEntry(relativePath);
                zip.Delete(ze);
                zip.CommitUpdate();
            }
            catch (Exception ex)
            {
                throw new StorageException("The removing of the storage file failed.", ex);
            }
        }

        /// <summary>
        /// Gets a value, describing the existence of the specified storage file.
        /// </summary>
        /// <param name="relativePath">The relative path to a storage file in the container.</param>
        /// <returns>
        /// 	<c>true</c>, if the specified storage file exists; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="relativePath"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Is thrown, if the given relative path contains invalid characters or has the length of zero.
        /// </exception>
        public bool Exists(string relativePath)
        {
            if (relativePath == null) throw new ArgumentNullException("relativePath");
            if (relativePath.Length == 0) throw new ArgumentException(
                Resources.Storage_ArgumentException_EmptyPath, "relativePath");
            if (IsInvalidPath(relativePath))
            {
                throw new ArgumentException(Resources.ZipStorage_ArgumentException_InvalidPathCharacters);
            }
            var index = zip.FindEntry(relativePath, true);
            return index >= 0;
        }

        private static bool IsInvalidPath(string path)
        {
            return Path.GetInvalidPathChars().Any(path.Contains);
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of all relative paths of the stored files.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of the stored files.</returns>
        public IEnumerable<string> GetFiles()
        {
            for (int i = 0; i < zip.Count; i++)
            {
                var e = zip[i];
                yield return e.Name;
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            if (zip != null)
            {
                zip.Close();
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DirectoryStorage"/> is reclaimed by garbage collection.
        /// </summary>
        ~ZipStorage()
        {
            Dispose();
        }

        #endregion
    }
}
