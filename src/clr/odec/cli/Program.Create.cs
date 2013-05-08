using System;
using System.IO;
using System.Reflection;
using System.Xml;
using de.mastersign.odec.crypto;
using de.mastersign.odec.model;
using de.mastersign.odec.process;

namespace de.mastersign.odec.cli
{
    partial class Program
    {
        private static int Create()
        {
            var ok = true;

            if (StartupInfo.ContainerPath == null)
            {
                WriteWarning(Resources.Warning_NoContainerPathGiven);
                ok = false;
            }
            if (StartupInfo.OwnerDescriptionFile == null)
            {
                WriteWarning(Resources.Warning_NoOwnerDescriptionFile);
                ok = false;
            }
            var entityDescriptions = StartupInfo.EntityDescriptionFiles;
            if (entityDescriptions.Length == 0)
            {
                WriteWarning(Resources.Warning_NoEntityDescriptionFile);
                ok = false;
            }

            if (!ok)
            {
                WriteHelpHint();
                return ERR_ARGUMENT_MISSING;
            }

            if (File.Exists(StartupInfo.ContainerPath) ||
                Directory.Exists(StartupInfo.ContainerPath))
            {
                WriteWarning(Resources.Warning_ContainerAllreadyExists,
                    StartupInfo.ContainerPath);
                return ERR_CONTAINER_EXISTS;
            }

            Profile profile = null;
            if (StartupInfo.ProfileFile != null)
            {
                profile = LoadProfile(StartupInfo.ProfileFile);
                if (profile == null) return ERR_XML;
            }

            string errMsg;
            XmlDocument ownerDoc;
            if (!XmlHelper.LoadOwnerDescription(
                    StartupInfo.OwnerDescriptionFile,
                    out ownerDoc, out errMsg))
            {
                WriteError(errMsg);
                return ERR_XML;
            }

            var copyright = ReadTextFile(StartupInfo.CopyrightTextFile);
            var comments = ReadTextFile(StartupInfo.CommentsTextFile);

            var edition = CreateEdition(profile, copyright, comments);
            edition.Owner = CreateOwner(ownerDoc);

            IRSAProvider certificate;
            try
            {
                certificate = ReadCertificate(ownerDoc);
            }
            catch (Exception ex)
            {
                WriteError(Resources.Error_ReadingCertificate, ex);
                return ERR_CERTIFICATE_MISSING;
            }
            IRSAProvider privateKey;
            try
            {
                privateKey = ReadPrivateKey(ownerDoc);
            }
            catch (Exception ex)
            {
                WriteError(Resources.Error_ReadingPrivateKey, ex);
                return ERR_PRIVATE_KEY_MISSING;
            }

            Container container;
            try
            {
                container = CreateContainer(
                    StartupInfo.StorageType, StartupInfo.ContainerPath,
                    edition, certificate, privateKey);
            }
            catch (Exception ex)
            {
                WriteError(Resources.Error_InitializingContainer, ex);
                return ERR_INITIALIZATION;
            }

            int errC = OK;
            foreach (var entityDescription in entityDescriptions)
            {
                errC = AddEntity(container, entityDescription);
                if (errC != OK) break;
            }

            try
            {
                container.FinishInitialization();

                PostNewEdition(container);
            }
            catch (Exception ex)
            {
                WriteError(Resources.Error_SealingContainer, ex);
                return ERR_SEALING;
            }
            finally
            {
                container.Dispose();
            }
            return errC;
        }

        private static Container CreateContainer(StorageType type, string path, EditionElement edition, IRSAProvider certificate, IRSAProvider privateKey)
        {
            var settings = new InitializationSettings { CreateSaltForNewEdition = StartupInfo.CreateEditionWithSalt };
            switch (type)
            {
                case StorageType.ZipFile:
                    return Container.CreateZip(path, edition, settings, privateKey, certificate, StartupInfo.Compatibility);
                case StorageType.Directory:
                    return Container.CreateDirectory(path, edition, settings, privateKey, certificate, StartupInfo.Compatibility);
                default:
                    throw new NotSupportedException(Resources.StorageTypeNotSupported);
            }
        }

        private static EditionElement CreateEdition(Profile profile, string copyright, string comments)
        {
            var ee = new EditionElement();
            ee.Guid = Guid.NewGuid();

            if (profile != null)
            {
                ee.Profile = profile.Name;
                ee.Version = profile.Version;
            }

            ee.Timestamp = DateTime.Now;
            ee.Copyright = copyright;
            ee.Comments = comments;

            var an = Assembly.GetExecutingAssembly().GetName();
            ee.Software = string.Format("{0}, {1}", an.Name, an.Version);

            return ee;
        }

        private static Owner CreateOwner(XmlDocument ownerDoc)
        {
            var owner = new Owner();
            owner.Institute = ownerDoc.ReadString("/Owner/Institute");
            owner.Operator = ownerDoc.ReadString("/Owner/Operator");
            if (ownerDoc.ElementExists("/Owner/Role"))
            {
                owner.Role = ownerDoc.ReadString("/Owner/Role");
            }
            owner.Email = ownerDoc.ReadString("/Owner/Email");
            return owner;
        }

        private static IRSAProvider ReadCertificate(XmlDocument ownerDoc)
        {
            var fileTag = ownerDoc.SelectSingleNode("/Owner/CertificateFile");
            if (fileTag != null)
            {
                var path = fileTag.InnerText;
                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(Path.GetDirectoryName(StartupInfo.OwnerDescriptionFile), path);
                }
                if (!File.Exists(path)) throw new FileNotFoundException(Resources.CertificateFileNotFound, path);
                return Configuration.CryptoFactory.CreateRSAProviderFromCertificateFile(path);
            }
            var pemTag = ownerDoc.SelectSingleNode("/Owner/Certificate");
            return Configuration.CryptoFactory.CreateRSAProviderFromPemEncodedCertificate(pemTag.InnerText);
        }

        private static IRSAProvider ReadPrivateKey(XmlDocument ownerDoc)
        {
            var fileTag = ownerDoc.SelectSingleNode("/Owner/PrivateKeyFile");
            if (fileTag != null)
            {
                var path = fileTag.InnerText;
                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(Path.GetDirectoryName(StartupInfo.OwnerDescriptionFile), path);
                }
                if (!File.Exists(path)) throw new FileNotFoundException(Resources.PrivateKeyNotFound, path);
                return Configuration.CryptoFactory.CreateRSAProviderFromPrivateKeyFile(
                    path, new PasswordSource());
            }
            var pemTag = ownerDoc.SelectSingleNode("/Owner/PrivateKey");
            return Configuration.CryptoFactory.CreateRSAProviderFromPemEncodedPrivateKey(
                pemTag.InnerText, new PasswordSource());
        }

        private static string ReadTextFile(string file)
        {
            if (file == null) return null;
            return File.ReadAllText(file);
        }

        private static Profile LoadProfile(string file)
        {
            using (var r = XmlReader.Create(file))
            {
                try
                {
                    var p = new Profile();
                    p.LoadFromXml(r);
                    return p;
                }
                catch (FormatException fe)
                {
                    WriteError(fe.Message);
                    return null;
                }
            }
        }

        private static int AddEntity(Container container, string entityDescription)
        {
            string errMsg;
            XmlDocument entityDoc;
            if (!XmlHelper.LoadEntityDescription(
                entityDescription, out entityDoc, out errMsg))
            {
                return ERR_XML;
            }

            var provenance = new ProvenanceElement();
            if (entityDoc.ElementExists("/Entity/Provenance/Guid"))
            {
                provenance.Guid = new Guid(entityDoc.ReadString("/Entity/Provenance/Guid"));
            }
            var label = entityDoc.ReadString("/Entity/Label");
            var type = new Guid(entityDoc.ReadString("/Entity/Type"));
            var predecessors = entityDoc.ReadIdList("/Entity/Predecessors");

            Entity entity;

            try
            {
                entity = container.NewEntity(type, provenance, label, predecessors);
            }
            catch (Exception ex)
            {
                WriteError(Resources.Error_CreatingEntity, ex);
                return ERR_ENTITY_CREATION;
            }

            string baseDir = Path.GetDirectoryName(entityDescription);

            var parameterSetDescr = entityDoc.SelectSingleNode("/Entity/ParameterSet") as XmlElement;
            if (parameterSetDescr != null)
            {
                var res = AddValue(entity, parameterSetDescr, baseDir, true);
                if (res != OK) return res;
            }

            var valueDescriptions = entityDoc.SelectNodes("/Entity/Value");
            if (valueDescriptions != null)
            {
                foreach (XmlElement valueDescription in valueDescriptions)
                {
                    var res = AddValue(entity, valueDescription, baseDir);
                    if (res != OK) return res;
                }
            }

            try
            {
                entity.Close();
            }
            catch (Exception ex)
            {
                WriteError(Resources.Error_StoringEntity, ex);
                return ERR_ENTITY_STORAGE;
            }

            return OK;
        }

        private static int AddValue(Entity entity, XmlElement valueDescription, string basePath, bool parameterSet = false)
        {
            var name = valueDescription.ReadString("Name");
            if (string.IsNullOrEmpty(name))
            {
                WriteError(parameterSet
                    ? Resources.Error_NameOfProvenanceParameterSetIsEmpty
                    : Resources.Error_NameOfValueIsEmpty);
                return ERR_DESCRIPTION_INVALID;
            }

            var type = Guid.Empty;
            if (valueDescription.ElementExists("Type"))
            {
                type = new Guid(valueDescription.ReadString("Type"));
            }

            var srcPath = valueDescription.ReadString("SourceFile");
            var appearance = ValueAppearance.plain;
            if (string.IsNullOrEmpty(srcPath))
            {
                appearance = ValueAppearance.suppressed;
            }
            else
            {
                srcPath = Environment.ExpandEnvironmentVariables(srcPath);
                if (!Path.IsPathRooted(srcPath))
                {
                    srcPath = Path.Combine(basePath, srcPath);
                }
                if (!File.Exists(srcPath))
                {
                    WriteError(parameterSet
                        ? Resources.Error_ProvenanceParameterSetSourceFileNotFound
                        : Resources.Error_ValueSourceFileNotFound,
                        srcPath);
                    return ERR_VALUE_SOURCE_FILE_MISSING;
                }
            }

            if (parameterSet)
            {
                entity.SetProvenanceParameterSet(name, type, appearance, srcPath);
            }
            else
            {
                entity.AddValue(name, type, appearance, srcPath);
            }
            return OK;
        }

        private static void PostNewEdition(Container container)
        {
            Console.Out.WriteLine(container.CurrentEdition.Guid.ToString("D"));
        }
    }
}
