using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using de.mastersign.odec.Properties;
using de.mastersign.odec.crypto;
using de.mastersign.odec.model;
using de.mastersign.odec.storage;

namespace de.mastersign.odec
{
    partial class Container
    {
        internal const string EDITION_FILE = "edition.xml";
        internal const string HISTORY_FILE = "history.xml";
        internal const string INDEX_FILE = "index.xml";

        /// <summary>
        /// Checks if the given directory appears to be a container.
        /// Only the existance of the main container structure files is checked.
        /// No validation is done.
        /// </summary>
        /// <param name="path">A path to the container directory.</param>
        /// <returns><c>true</c> if the given directory appears to be a container; otherwise <c>false</c>.</returns>
        public static bool IsDirectoryContainer(string path)
        {
            return
                Directory.Exists(path) &&
                File.Exists(Path.Combine(path, EDITION_FILE)) &&
                File.Exists(Path.Combine(path, EDITION_FILE + ".sig")) &&
                File.Exists(Path.Combine(path, INDEX_FILE)) &&
                File.Exists(Path.Combine(path, INDEX_FILE + ".sig")) &&
                File.Exists(Path.Combine(path, HISTORY_FILE)) &&
                File.Exists(Path.Combine(path, HISTORY_FILE + ".sig"));
        }

        /// <summary>
        /// Checks if the given file appears to be a ZIP container.
        /// Only the existance of the main container structure files is checked.
        /// No validation is done.
        /// </summary>
        /// <remarks>
        /// To check the existance of the container structure files, the ZIP archive needs to be opened and read.
        /// This can lead to serious IO workload while checking big container files.
        /// </remarks>
        /// <param name="path">A path to the container ZIP file.</param>
        /// <returns><c>true</c> if the given file appears to be a ZIP container; otherwise <c>false</c>.</returns>
        public static bool IsZipContainer(string path)
        {
            if (!File.Exists(path)) return false;
            try
            {
                using (var storage = new ZipStorage(path))
                {
                    return
                        storage.Exists(EDITION_FILE) &&
                        storage.Exists(EDITION_FILE + ".sig") &&
                        storage.Exists(INDEX_FILE) &&
                        storage.Exists(INDEX_FILE + ".sig") &&
                        storage.Exists(HISTORY_FILE) &&
                        storage.Exists(HISTORY_FILE + ".sig");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the given path appears to be a container.
        /// Only the existance of the main container structure files is checked.
        /// No validation is done.
        /// </summary>
        /// <remarks>
        /// To check the existance of the container structure files, a ZIP archive needs to be opened and read.
        /// This can lead to serious IO workload while checking big container ZIP files.
        /// </remarks>
        /// <remarks>
        /// To verify if the given path leads to a valid container, the container must be opened and validated.
        /// Use <see cref="OpenContainer(string,de.mastersign.odec.ValidationHandler)"/> for that purpose.
        /// </remarks>
        /// <seealso cref="IsDirectoryContainer"/>
        /// <seealso cref="IsZipContainer"/>
        /// <param name="path">A path to a possible container. This can be a directory or a ZIP file.</param>
        /// <returns><c>true</c> if the given path leads to something what appears to be a container.</returns>
        public static bool IsContainer(string path)
        {
            return IsDirectoryContainer(path) || IsZipContainer(path);
        }

        /// <summary>
        /// Opens an existing container in the given storage with default compatibility 
        /// conform to the container format specification.
        /// </summary>
        /// <param name="storage">The storage, containing the container data.</param>
        /// <param name="messageHandler">The message handler for validation messages.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="OpenDirectory(string,ValidationHandler)"/>
        /// <seealso cref="OpenZip(string,ValidationHandler)"/>
        /// <exception cref="ArgumentNullException">
        /// If <c>null</c> is given for <paramref name="storage"/>.
        /// </exception>
        public static Container Open(IStorage storage, ValidationHandler messageHandler)
        {
            return Open(storage, messageHandler, CompatibilityFlags.DefaultFlags);
        }

        /// <summary>
        /// Opens an existing container in the given storage.
        /// </summary>
        /// <param name="storage">The storage, containing the container data.</param>
        /// <param name="messageHandler">The message handler for validation messages.</param>
        /// <param name="compatibilityFlags">The compatibility flags for the container.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="OpenDirectory(string,ValidationHandler)"/>
        /// <seealso cref="OpenZip(string,ValidationHandler)"/>
        /// <exception cref="ArgumentNullException">
        /// If <c>null</c> is given for <paramref name="storage"/>.
        /// </exception>
        public static Container Open(IStorage storage,
            ValidationHandler messageHandler, CompatibilityFlags compatibilityFlags)
        {
            var result = new Container(storage, compatibilityFlags);
            if (result.LoadAndValidate(messageHandler))
            {
                messageHandler.Success(ValidationMessageClass.Global,
                    Resources.Container_Open_ContainerIsValid);
            }
            else
            {
                messageHandler.Error(ValidationMessageClass.Global,
                    Resources.Container_Open_ContainerInvalid);
            }
            return result;
        }

        /// <summary>
        /// Opens an existing container in the given ZIP file with default compatibility 
        /// conform to the container format specification.
        /// </summary>
        /// <param name="fileName">The path of the ZIP file.</param>
        /// <param name="messageHandler">The message handler for validation messages.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Open(IStorage,ValidationHandler)"/>
        /// <seealso cref="OpenDirectory(string,ValidationHandler)"/>
        /// <exception cref="ArgumentNullException">
        /// If <c>null</c> is given for <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="fileName"/> is no valid file system path.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// If the given file does not exists.
        /// </exception>
        public static Container OpenZip(string fileName, ValidationHandler messageHandler)
        {
            return OpenZip(fileName, messageHandler, CompatibilityFlags.DefaultFlags);
        }

        /// <summary>
        /// Opens an existing container in the given ZIP file.
        /// </summary>
        /// <param name="fileName">The path of the ZIP file.</param>
        /// <param name="messageHandler">The message handler for validation messages.</param>
        /// <param name="compatibilityFlags">The compatibility flags for the container.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Open(IStorage,ValidationHandler)"/>
        /// <seealso cref="OpenDirectory(string,ValidationHandler)"/>
        /// <exception cref="ArgumentNullException">
        /// If <c>null</c> is given for <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="fileName"/> is no valid file system path.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// If the given file does not exists.
        /// </exception>
        public static Container OpenZip(string fileName, ValidationHandler messageHandler,
            CompatibilityFlags compatibilityFlags)
        {
            return Open(new ZipStorage(fileName), messageHandler, compatibilityFlags);
        }

        /// <summary>
        /// Opens an existing container in the given directory with default compatibility 
        /// conform to the container format specification.
        /// </summary>
        /// <param name="directoryName">The path of the directory.</param>
        /// <param name="messageHandler">The message handler for validation messages.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Open(IStorage,ValidationHandler)"/>
        /// <seealso cref="OpenZip(string,ValidationHandler)"/>
        public static Container OpenDirectory(string directoryName, ValidationHandler messageHandler)
        {
            return OpenDirectory(directoryName, messageHandler, CompatibilityFlags.DefaultFlags);
        }

        /// <summary>
        /// Opens an existing container in the given directory.
        /// </summary>
        /// <param name="directoryName">The path of the directory.</param>
        /// <param name="messageHandler">The message handler for validation messages.</param>
        /// <param name="compatibilityFlags">The compatibility flags for the container.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Open(IStorage,ValidationHandler)"/>
        /// <seealso cref="OpenZip(string,ValidationHandler)"/>
        public static Container OpenDirectory(string directoryName, ValidationHandler messageHandler, CompatibilityFlags compatibilityFlags)
        {
            return Open(new DirectoryStorage(new DirectoryInfo(directoryName)),
                messageHandler, compatibilityFlags);
        }

        /// <summary>
        /// Opens an existing container in the given directory or the given ZIP file with default compatibility.
        /// </summary>
        /// <param name="path">The path of the directory or the ZIP file.</param>
        /// <param name="messageHandler">The message handler for validation messages.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Open(IStorage,ValidationHandler)"/>
        /// <seealso cref="OpenDirectory(string,ValidationHandler)"/>
        /// <seealso cref="OpenZip(string,ValidationHandler)"/>
        public static Container OpenContainer(string path, ValidationHandler messageHandler)
        {
            if (File.Exists(path))
            {
                return OpenZip(path, messageHandler, CompatibilityFlags.DefaultFlags);
            }
            if (Directory.Exists(path))
            {
                return OpenDirectory(path, messageHandler, CompatibilityFlags.DefaultFlags);
            }
            throw new ArgumentException(
                string.Format(Resources.Container_OpenContainer_ArgumentException_PathNotFound, path), "path");
        }

        /// <summary>
        /// Opens an existing container in the given directory or the given ZIP file.
        /// </summary>
        /// <param name="path">The path of the directory or the ZIP file.</param>
        /// <param name="messageHandler">The message handler for validation messages.</param>
        /// <param name="compatibilityFlags">The compatibility flags for the container.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Open(IStorage,ValidationHandler)"/>
        /// <seealso cref="OpenDirectory(string,ValidationHandler)"/>
        /// <seealso cref="OpenZip(string,ValidationHandler)"/>
        public static Container OpenContainer(string path, ValidationHandler messageHandler, CompatibilityFlags compatibilityFlags)
        {
            if (File.Exists(path))
            {
                return OpenZip(path, messageHandler, compatibilityFlags);
            }
            if (Directory.Exists(path))
            {
                return OpenDirectory(path, messageHandler, compatibilityFlags);
            }
            throw new ArgumentException(
                string.Format(Resources.Container_OpenContainer_ArgumentException_PathNotFound, path), "path");
        }

        /// <summary>
        /// Creates a new container in the given storage with default compatibility 
        /// conform to the container format specification.
        /// </summary>
        /// <remarks>
        /// After creation, the container is in the initialization phase. 
        /// This phase must be completed with a call to <see cref="FinishInitialization"/>.
        /// </remarks>
        /// <param name="storage">The storage, where the container structure will be created.</param>
        /// <param name="initialEdition">The initial edition.</param>
        /// <param name="settings">Optional a set of settings for the initialization phase or <c>null</c>.</param>
        /// <param name="privateKey">A <see cref="IRSAProvider"/> with the private key.</param>
        /// <param name="certificate">A <see cref="IRSAProvider"/> with the X509 certificate.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="CreateDirectory(string,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        /// <seealso cref="CreateZip(string,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        public static Container Create(IStorage storage,
            EditionElement initialEdition, InitializationSettings settings,
            IRSAProvider privateKey, IRSAProvider certificate)
        {
            return Create(storage, initialEdition, settings, privateKey, certificate, CompatibilityFlags.DefaultFlags);
        }

        /// <summary>
        /// Creates a new container in the given storage.
        /// </summary>
        /// <remarks>
        /// After creation, the container is in the initialization phase. 
        /// This phase must be completed with a call to <see cref="FinishInitialization"/>.
        /// </remarks>
        /// <param name="storage">The storage, where the container structure will be created.</param>
        /// <param name="initialEdition">The initial edition.</param>
        /// <param name="settings">Optional a set of settings for the initialization phase or <c>null</c>.</param>
        /// <param name="privateKey">A <see cref="IRSAProvider"/> with the private key.</param>
        /// <param name="certificate">A <see cref="IRSAProvider"/> with the X509 certificate.</param>
        /// <param name="compatibilityFlags">The compatibility flags for the container.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="CreateDirectory(string,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        /// <seealso cref="CreateZip(string,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        public static Container Create(IStorage storage, EditionElement initialEdition, InitializationSettings settings, IRSAProvider privateKey, IRSAProvider certificate, CompatibilityFlags compatibilityFlags)
        {
            var result = new Container(storage, compatibilityFlags);
            result.StartInitialization(initialEdition, settings, privateKey, certificate);
            return result;
        }

        /// <summary>
        /// Creates a new container in a ZIP file with default compatibility 
        /// conform to the container format specification. 
        /// If the ZIP file allready exists, its content will be rejected.
        /// </summary>
        /// <remarks>
        /// After creation, the container is in the initialization phase. 
        /// This phase must be completed with a call to <see cref="FinishInitialization"/>.
        /// </remarks>
        /// <param name="fileName">The path of the ZIP file.</param>
        /// <param name="initialEdition">The initial edition.</param>
        /// <param name="settings">Optional a set of settings for the initialization phase or <c>null</c>.</param>
        /// <param name="privateKey">A <see cref="IRSAProvider"/> with the private key.</param>
        /// <param name="certificate">A <see cref="IRSAProvider"/> with the X509 certificate.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Create(IStorage,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        /// <seealso cref="CreateDirectory(string,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        public static Container CreateZip(string fileName,
            EditionElement initialEdition, InitializationSettings settings,
            IRSAProvider privateKey, IRSAProvider certificate)
        {
            return CreateZip(fileName, initialEdition, settings, privateKey, certificate, CompatibilityFlags.DefaultFlags);
        }

        /// <summary>
        /// Creates a new container in a ZIP file. 
        /// If the ZIP file allready exists, its content will be rejected.
        /// </summary>
        /// <remarks>
        /// After creation, the container is in the initialization phase. 
        /// This phase must be completed with a call to <see cref="FinishInitialization"/>.
        /// </remarks>
        /// <param name="fileName">The path of the ZIP file.</param>
        /// <param name="initialEdition">The initial edition.</param>
        /// <param name="settings">Optional a set of settings for the initialization phase or <c>null</c>.</param>
        /// <param name="privateKey">A <see cref="IRSAProvider"/> with the private key.</param>
        /// <param name="certificate">A <see cref="IRSAProvider"/> with the X509 certificate.</param>
        /// <param name="compatibilityFlags">The compatibility flags for the container.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Create(IStorage,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        /// <seealso cref="CreateDirectory(string,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        public static Container CreateZip(string fileName,
            EditionElement initialEdition, InitializationSettings settings,
            IRSAProvider privateKey, IRSAProvider certificate, CompatibilityFlags compatibilityFlags)
        {
            if (File.Exists(fileName)) File.Delete(fileName);
            return Create(new ZipStorage(fileName), initialEdition, settings, privateKey, certificate, compatibilityFlags);
        }

        /// <summary>
        /// Creates a new container in a directory with default compatibility 
        /// conform to the container format specification.
        /// If the directory does not exists, it will be created.
        /// </summary>
        /// <remarks>
        /// After creation, the container is in the initialization phase. 
        /// This phase must be completed with a call to <see cref="FinishInitialization"/>.
        /// </remarks>
        /// <param name="directoryName">The path of the directory.</param>
        /// <param name="initialEdition">The initial edition.</param>
        /// <param name="settings">Optional a set of settings for the initialization phase or <c>null</c>.</param>
        /// <param name="privateKey">A <see cref="IRSAProvider"/> with the private key.</param>
        /// <param name="certificate">A <see cref="IRSAProvider"/> with the X509 certificate.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Create(IStorage,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        /// <seealso cref="CreateZip(string,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        public static Container CreateDirectory(string directoryName,
            EditionElement initialEdition, InitializationSettings settings,
            IRSAProvider privateKey, IRSAProvider certificate)
        {
            return CreateDirectory(directoryName, initialEdition, settings,
                privateKey, certificate, CompatibilityFlags.DefaultFlags);
        }

        /// <summary>
        /// Creates a new container in a directory.
        /// If the directory does not exists, it will be created.
        /// </summary>
        /// <remarks>
        /// After creation, the container is in the initialization phase. 
        /// This phase must be completed with a call to <see cref="FinishInitialization"/>.
        /// </remarks>
        /// <param name="directoryName">The path of the directory.</param>
        /// <param name="initialEdition">The initial edition.</param>
        /// <param name="settings">Optional a set of settings for the initialization phase or <c>null</c>.</param>
        /// <param name="privateKey">A <see cref="IRSAProvider"/> with the private key.</param>
        /// <param name="certificate">A <see cref="IRSAProvider"/> with the X509 certificate.</param>
        /// <param name="compatibilityFlags">The compatibility flags for the container.</param>
        /// <returns>A <see cref="Container"/> object.</returns>
        /// <seealso cref="Create(IStorage,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        /// <seealso cref="CreateZip(string,EditionElement,InitializationSettings,IRSAProvider,IRSAProvider)"/>
        public static Container CreateDirectory(string directoryName,
            EditionElement initialEdition, 
            InitializationSettings settings,
            IRSAProvider privateKey, IRSAProvider certificate,
            CompatibilityFlags compatibilityFlags)
        {
            var dir = new DirectoryInfo(directoryName);
            if (!dir.Exists)
            {
                dir.Create();
            }
            return Create(new DirectoryStorage(dir),
                initialEdition, settings, privateKey, certificate, compatibilityFlags);
        }

    }
}
