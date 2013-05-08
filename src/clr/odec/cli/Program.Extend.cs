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
        private static int Extend()
        {
            var ok = true;

            if (StartupInfo.ContainerPath == null)
            {
                WriteWarning(Resources.Warning_NoContainerPathGiven);
                ok = false;
            }
            if (StartupInfo.ValidateProfile && StartupInfo.ProfileFile == null)
            {
                WriteWarning(Resources.Warning_NoProfileFile);
                ok = false;
            }

            if (!ok)
            {
                WriteHelpHint();
                return ERR_ARGUMENT_MISSING;
            }

            if (!File.Exists(StartupInfo.ContainerPath) &&
                !Directory.Exists(StartupInfo.ContainerPath))
            {
                WriteWarning(Resources.Warning_ConatinerNotFound,
                    StartupInfo.ContainerPath);
                return ERR_CONTAINER_MISSING;
            }

            Profile profile = null;
            if (StartupInfo.ProfileFile != null)
            {
                profile = LoadProfile(StartupInfo.ProfileFile);
                if (profile == null) return ERR_XML;
            }

            string errMsg;
            XmlDocument ownerDoc = null;
            if (StartupInfo.OwnerDescriptionFile != null &&
                !XmlHelper.LoadOwnerDescription(StartupInfo.OwnerDescriptionFile, out ownerDoc, out errMsg))
            {
                WriteError(errMsg);
                return ERR_XML;
            }

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
            var errC = OpenAndValidate(out container);
            if (errC != OK)
            {
                container.Dispose();
                return errC;
            }

            if (profile != null)
            {
                if (container.CurrentEdition.Profile != profile.Name)
                {
                    WriteError(Resources.Error_ProfileMismatch,
                        profile.Name, container.CurrentEdition.Profile);
                    return ERR_PROFILE_MISMATCH;
                }
                if (container.CurrentEdition.Version != profile.Version)
                {
                    WriteError(Resources.Error_ProfileVersionMismatch,
                        profile.Version, container.CurrentEdition.Version);
                    return ERR_PROFILE_MISMATCH;
                }
            }

            var copyright = ReadTextFile(StartupInfo.CopyrightTextFile);
            var comments = ReadTextFile(StartupInfo.CommentsTextFile);

            var edition = CreateEdition(container.CurrentEdition, profile, copyright, comments);
            edition.Owner = CreateOwner(ownerDoc);

            var settings = new TransformationSettings
                               {
                                   CreateSaltForNewEdition = StartupInfo.CreateEditionWithSalt,
                                   PreventReinstatingFormerCurrentEdition = StartupInfo.RemoveSaltFromFormerEdition,
                               };

            container.StartTransformation(edition, settings, privateKey, certificate);

            var entityDescriptions = StartupInfo.EntityDescriptionFiles;
            foreach (var entityDescription in entityDescriptions)
            {
                errC = AddEntity(container, entityDescription);
                if (errC != OK) break;
            }

            try
            {
                container.FinishTransformation();

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

        private static EditionElement CreateEdition(EditionElement lastEdition, Profile profile, string copyright, string comments)
        {
            var ee = new EditionElement();
            ee.Guid = Guid.NewGuid();

            if (profile != null)
            {
                ee.Profile = profile.Name;
                ee.Version = profile.Version;
            }
            else
            {
                ee.Profile = lastEdition.Profile;
                ee.Version = lastEdition.Version;
            }

            ee.Timestamp = DateTime.Now;
            ee.Copyright = copyright;
            ee.Comments = comments;

            var an = Assembly.GetExecutingAssembly().GetName();
            ee.Software = string.Format("{0}, {1}", an.Name, an.Version);

            return ee;
        }
    }
}
