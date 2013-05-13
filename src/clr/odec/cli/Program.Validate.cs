using System;
using System.IO;
using de.mastersign.odec.crypto;

namespace de.mastersign.odec.cli
{
    partial class Program
    {
        private static bool DisplayValidationErrorsOnly { get; set; }
        private static bool ValidationErrorOccured { get; set; }

        private static int Validate()
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

            Container container;
            var errC = OpenAndValidate(out container);
            container.Dispose();
            return errC;
        }

        private static int OpenAndValidate(out Container container)
        {
            ValidationErrorOccured = false;

            if (Directory.Exists(StartupInfo.ContainerPath))
            {
                container = Container.OpenDirectory(StartupInfo.ContainerPath, ValidationHandler, StartupInfo.Compatibility);
            }
            else
            {
                container = Container.OpenZip(StartupInfo.ContainerPath, ValidationHandler, StartupInfo.Compatibility);
            }

            if (ValidationErrorOccured)
            {
                WriteError(Resources.Error_ValidationStep1Failed);
                return ERR_VALIDATION_STEP_1;
            }

            if (StartupInfo.ValidateValue)
            {
                if (!container.VerifyEntityValueSignatures(ValidationHandler))
                {
                    WriteError(Resources.Error_ValidationStep2Failed);
                    return ERR_VALIDATION_STEP_2;
                }
            }

            if (StartupInfo.ValidateCertificate)
            {
                var rules = 
                    new CertificateValidationRules
                        {
                            DateValidationScheme = DateValidationScheme.ModifiedPemShell,
                            AllowSelfSignedCertificate = StartupInfo.CertificationAuthority == null
                        };
                CertificationAuthorityDirectory caDir;
                if (StartupInfo.CertificationAuthority == null)
                {
                    caDir = new CertificationAuthorityDirectory();
                }
                else if (Directory.Exists(StartupInfo.CertificationAuthority))
                {
                    caDir = CertificationAuthorityDirectory.CreateFromFileSystem(
                        StartupInfo.CertificationAuthority, Configuration.CryptoFactory);
                }
                else if (File.Exists(StartupInfo.CertificationAuthority))
                {
                    caDir = new CertificationAuthorityDirectory(
                        Configuration.CryptoFactory.CreateRSAProviderFromCertificateFile(
                            StartupInfo.CertificationAuthority));
                }
                else
                {
                    caDir = new CertificationAuthorityDirectory();
                }

                if (!container.ValidateCertificates(caDir, rules, ValidationHandler))
                {
                    WriteError(Resources.Error_ValidationStep3Failed);
                    return ERR_VALIDATION_STEP_3;
                }
            }

            if (StartupInfo.ValidateProfile)
            {
                var profile = LoadProfile(StartupInfo.ProfileFile);
                if (profile == null ||
                    !profile.ValidateContainer(container, ValidationHandler))
                {
                    WriteError(Resources.Error_ValidationStep4Failed);
                    return ERR_VALIDATION_STEP_4;
                }
            }

            return OK;
        }

        private static void ValidationHandler(ContainerValidationEventArgs ea)
        {
            if (ea.Severity == ValidationSeverity.Error)
            {
                ValidationErrorOccured = true;
            }
            if (!DisplayValidationErrorsOnly || ea.Severity == ValidationSeverity.Error)
            {
#if COLOR
                switch (ea.Severity)
                {
                    case ValidationSeverity.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.BackgroundColor = ConsoleColor.Black;
                        break;
                    case ValidationSeverity.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = ConsoleColor.Black;
                        break;
                }
#endif
                Console.WriteLine("[{0}] {1}: {2}",
                    ea.Severity, ea.MessageClass, ea.Message);
#if COLOR
                Console.ResetColor();
#endif
            }
        }
    }
}
