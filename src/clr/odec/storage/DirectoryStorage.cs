using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec.storage
{
    /// <summary>
    /// An implementation of <see cref="IStorage"/>, 
    /// using a directory in the filesystem as storage for the container.
    /// </summary>
    public class DirectoryStorage : IStorage
    {
        private readonly DirectoryInfo rootDir;

        /// <summary>
        /// Gets the root directory of the container storage.
        /// </summary>
        /// <value>The root directory of the container.</value>
        public DirectoryInfo RootDirectory { get { return rootDir; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryStorage"/> class.
        /// </summary>
        /// <remarks>
        /// The specified directory will be created if it does not exist.
        /// </remarks>
        /// <param name="rootDir">The root directory of the container.</param>#
        /// <exception cref="ArgumentNullException">
        /// Is thrown, if <c>null</c> is given for <paramref name="rootDir"/>.
        /// </exception>
        public DirectoryStorage(DirectoryInfo rootDir)
        {
            if (rootDir == null) throw new ArgumentNullException("rootDir");
            if (!rootDir.Exists)
            {
                rootDir.Create();
                rootDir.Refresh();
            }
            this.rootDir = rootDir;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Resources.DirectoryStorage_Name;
        }

        #region Implementation of IStorage

        /// <summary>
        /// Opens an octet stream for the specified storage file.
        /// </summary>
        /// <param name="relativePath">The relative path to the storage file in the container.</param>
        /// <returns>
        /// A <see cref="Stream"/> for reading the content of the given storage file.
        /// </returns>
        /// <remarks>
        /// The returned <see cref="Stream"/> can not be written.
        /// </remarks>
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
        /// Is thrown, if the storage causes an error while opening the storage file.
        /// </exception>
        public Stream Read(string relativePath)
        {
            if (!Exists(relativePath))
            {
                throw new FileNotFoundException(Resources.Storage_StorageFileNotFound, relativePath);
            }
            var fullPath = Path.Combine(rootDir.FullName, relativePath);
            var file = new FileInfo(fullPath);
            try
            {
                return file.OpenRead();
            }
            catch (Exception ex)
            {
                throw new StorageException(Resources.Storage_Read_OpeningFailed, ex);
            }
        }

        /// <summary>
        /// Opens an octet stream for the specified storage file.
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
        /// Is thrown, if <c>null</c> is given for <paramref name="relativePath"/>.
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
            if (relativePath.Length == 0) throw new ArgumentException(
                Resources.Storage_ArgumentException_EmptyPath, "relativePath");
            if (source == null) throw new ArgumentNullException("source");
            if (!source.CanRead) throw new ArgumentException(
                Resources.Storage_Write_StreamNotReadable, "source");
            var fullPath = Path.Combine(rootDir.FullName, relativePath);
            var dirPath = Path.GetDirectoryName(fullPath);
            var file = new FileInfo(fullPath);
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                using (var target = file.Open(FileMode.Create))
                {
                    source.CopyTo(target);
                }
            }
            catch (Exception ex)
            {
                throw new StorageException(Resources.DirectoryStorage_Write_StorageException_WritingFailed, ex);
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
            if (relativePath.Length == 0) throw new ArgumentException(
                Resources.Storage_ArgumentException_EmptyPath, "relativePath");
            if (sourceFile == null) throw new ArgumentNullException("sourceFile");
            if (!File.Exists(sourceFile)) throw new FileNotFoundException(
                Resources.DirectoryStorage_Write_SourceFileNotFound, sourceFile);
            var fullPath = Path.Combine(rootDir.FullName, relativePath);
            var dirPath = Path.GetDirectoryName(fullPath);
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                File.Copy(sourceFile, fullPath, true);
            }
            catch (Exception ex)
            {
                throw new StorageException(Resources.DirectoryStorage_Write_StorageException_WritingFailed, ex);
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
        public void Remove(string relativePath)
        {
            if (!Exists(relativePath))
            {
                throw new FileNotFoundException(
                Resources.Storage_StorageFileNotFound,
                relativePath);
            }
            var fullPath = Path.Combine(rootDir.FullName, relativePath);
            var file = new FileInfo(fullPath);
            try
            {
                file.Delete();
            }
            catch (Exception ex)
            {
                throw new StorageException(Resources.DirectoryStorage_Remove_StorageException_RemovingFailed, ex);
            }
        }

        /// <summary>
        /// Gets a value, describing the existence of the specified storage file.
        /// </summary>
        /// <param name="relativePath">The relative path to a storage file in the container.</param>
        /// <returns><c>true</c>, if the specified storage file exists; otherwise <c>false</c>.</returns>
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
            var fullPath = Path.Combine(rootDir.FullName, relativePath);
            var file = new FileInfo(fullPath);
            return file.Exists;
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of all relative paths of the stored files.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of the stored files.</returns>
        public IEnumerable<string> GetFiles()
        {
            return GetFiles(null, rootDir.FullName);
        }

        private static IEnumerable<string> GetFiles(string relative, string dir)
        {
            foreach (var file in Directory.GetFiles(dir))
            {
                yield return CombinePath(relative, Path.GetFileName(file));
            }
            foreach (var directory in Directory.GetDirectories(dir))
            {
                var relativePath = CombinePath(relative, Path.GetFileName(directory));
                //var absolutePath = Path.Combine(dir, directory);
                foreach (var file in GetFiles(relativePath, directory))
                {
                    yield return file;
                }
            }
        }

        private static string CombinePath(string dir, string relative)
        {
            return string.IsNullOrEmpty(dir) ? relative : dir + "/" + relative;
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

            // nothing

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DirectoryStorage"/> is reclaimed by garbage collection.
        /// </summary>
        ~DirectoryStorage()
        {
            Dispose();
        }

        #endregion
    }
}
