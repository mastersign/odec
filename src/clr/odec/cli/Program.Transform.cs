using System;
using System.IO;
using de.mastersign.odec.storage;

namespace de.mastersign.odec.cli
{
    partial class Program
    {
        private static int Transform()
        {
            var ok = true;

            if (StartupInfo.ContainerPath == null)
            {
                WriteWarning(Resources.Warning_NoContainerPathGiven);
                ok = false;
            }
            if (StartupInfo.TargetPath == null)
            {
                WriteWarning(Resources.Warning_NoTargetPath);
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

            if (File.Exists(StartupInfo.TargetPath) ||
                Directory.Exists(StartupInfo.TargetPath))
            {
                WriteWarning(Resources.Warning_TargetPathAllreadyExists,
                    StartupInfo.TargetPath);
                return ERR_CONTAINER_EXISTS;
            }

            if (StartupInfo.TargetPath == StartupInfo.ContainerPath)
            {
                WriteWarning(Resources.Warning_InPlaceTransformationNotSupported);
                return ERR_ARGUMENT_INVALID;
            }

            IStorage sourceStorage;
            try
            {
                if (Directory.Exists(StartupInfo.ContainerPath))
                {
                    sourceStorage = new DirectoryStorage(new DirectoryInfo(StartupInfo.ContainerPath));
                }
                else
                {
                    sourceStorage = new ZipStorage(StartupInfo.ContainerPath);
                }
            }
            catch (Exception ex)
            {
                WriteError(Resources.Error_OpeningSourceContainer, ex);
                return ERR_CONTAINER_OPENING;
            }

            IStorage targetStorage;
            try
            {
                switch (StartupInfo.StorageType)
                {
                    case StorageType.ZipFile:
                        targetStorage = new ZipStorage(StartupInfo.TargetPath);
                        break;
                    case StorageType.Directory:
                        targetStorage = new DirectoryStorage(new DirectoryInfo(StartupInfo.TargetPath));
                        break;
                    default:
                        WriteWarning(Resources.Warning_StorageTypeNotSupported);
                        return ERR_ARGUMENT_INVALID;
                }
            }
            catch (Exception ex)
            {
                WriteError(Resources.Error_CreatingTargetContainer, ex);
                return ERR_CONTAINER_OPENING;
            }

            int errC = OK;
            foreach (var file in sourceStorage.GetFiles())
            {
                try
                {
                    targetStorage.Write(file, sourceStorage.Read(file));
                }
                catch (Exception ex)
                {
                    WriteError(Resources.Error_CopyingStorageFile, file, ex);
                    errC = ERR_COPY_STORAGE_FILE;
                    break;
                }
            }

            sourceStorage.Dispose();
            targetStorage.Dispose();

            return errC;
        }
    }
}
