using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.storage
{
    /// <summary>
    /// This interface represents the capability to read and write the storage files of a container.
    /// </summary>
    public interface IStorage : IDisposable
    {
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
        /// <exception cref="FormatException">
        /// Is thrown, if the given relative path contains invalid characters or has the length of zero.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Is thrown, if the specified storage file does not exist.
        /// </exception>
        /// <exception cref="StorageException">
        /// Is thrown, if the storage causes an error while opening the storage file.
        /// </exception>
        Stream Read(string relativePath);

        /// <summary>
        /// Writes the content of the given <see cref="Stream"/> to the storage file.
        /// </summary>
        /// <param name="relativePath">The relative path to the storage file in the container.</param>
        /// <param name="source">The source of the data to be written.</param>
        /// <remarks>
        /// If no storage file exists under the given path, a new storage file
        /// is created. If a storage file exists under the given path,
        /// its content is replaced with the data, from the given stream.
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
        void Write(string relativePath, Stream source);

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
        void Write(string relativePath, string sourceFile);

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
        void Remove(string relativePath);

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
        bool Exists(string relativePath);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of all relative paths of the stored files.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of the stored files.</returns>
        IEnumerable<string> GetFiles();
    }
}
