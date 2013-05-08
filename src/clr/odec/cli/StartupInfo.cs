using System;
using System.Collections.Generic;
using System.IO;

namespace de.mastersign.odec.cli
{
    class StartupInfo
    {
        public bool ShowHelp { get; private set; }

        public bool ShowInfo { get; private set; }

        public WorkingMode Mode { get; private set; }

        public string ContainerPath { get; private set; }

        public string TargetPath { get; private set; }

        public StorageType StorageType { get; private set; }

        public string OwnerDescriptionFile { get; private set; }

        public string CopyrightTextFile { get; private set; }

        public string CommentsTextFile { get; private set; }

        private readonly List<string> entityFiles = new List<string>();

        public string[] EntityDescriptionFiles { get { return entityFiles.ToArray(); } }

        public string ProfileFile { get; private set; }

        public string CertificationAuthority { get; private set; }

        public bool ValidateValue { get; private set; }

        public bool ValidateCertificate { get; private set; }

        public bool ValidateProfile { get; private set; }

        public string TargetEdition { get; private set; }

        public CompatibilityFlags Compatibility { get; private set; }

        public bool CreateEditionWithSalt { get; private set; }

        public bool RemoveSaltFromFormerEdition { get; private set; }

        public bool ArgumentError { get; private set; }

        public StartupInfo(string[] args)
        {
            Mode = WorkingMode.None;
            StorageType = StorageType.ZipFile;
            CreateEditionWithSalt = true;
            var compatibility = CompatibilityFlags.DefaultFlags;

            var pos = 0;
            while (pos < args.Length)
            {
                switch (args[pos].ToLower())
                {
                    case "--help":
                    case "-?":
                        ShowHelp = true;
                        break;
                    case "--info":
                    case "-i":
                        ShowInfo = true;
                        break;
                    case "--mode":
                    case "-m":
                        pos++;
                        if (pos < args.Length) SetMode(args[pos]);
                        break;
                    case "--container":
                    case "-c":
                        pos++;
                        if (pos < args.Length)
                        {
                            ContainerPath = ExpandPath(args[pos]);
                        }
                        break;
                    case "--owner":
                    case "-o":
                        pos++;
                        if (pos < args.Length)
                        {
                            OwnerDescriptionFile = GetExistingFile(args[pos]);
                        }
                        break;
                    case "--copyright":
                    case "-cr":
                        pos++;
                        if (pos < args.Length)
                        {
                            CopyrightTextFile = GetExistingFile(args[pos]);
                        }
                        break;
                    case "--comments":
                    case "-cm":
                        pos++;
                        if (pos < args.Length)
                        {
                            CommentsTextFile = GetExistingFile(args[pos]);
                        }
                        break;
                    case "--storage":
                    case "-s":
                        pos++;
                        if (pos < args.Length) SetStorageType(args[pos]);
                        break;
                    case "--entity":
                    case "-e":
                        pos++;
                        if (pos < args.Length) AddEntityDescriptionFile(args[pos]);
                        break;
                    case "--profile":
                    case "-p":
                        pos++;
                        if (pos < args.Length)
                        {
                            ProfileFile = GetExistingFile(args[pos]);
                        }
                        break;
                    case "--certauthority":
                    case "-ca":
                        pos++;
                        if (pos < args.Length)
                        {
                            CertificationAuthority = GetExistingPath(args[pos]);
                        }
                        break;
                    case "--validatevalue":
                    case "-vv":
                        ValidateValue = true;
                        break;
                    case "--validatecert":
                    case "-vc":
                        ValidateCertificate = true;
                        break;
                    case "--validateprofile":
                    case "-vp":
                        ValidateProfile = true;
                        break;
                    case "--target":
                    case "-t":
                        pos++;
                        if (pos < args.Length)
                        {
                            TargetPath = ExpandPath(args[pos]);
                        }
                        break;
                    case "--targetedition":
                    case "-te":
                        pos++;
                        if (pos < args.Length)
                        {
                            TargetEdition = args[pos];
                        }
                        break;
                    case "--noback":
                    case "-nb":
                        RemoveSaltFromFormerEdition = true;
                        break;
                    case "--nosalt":
                    case "-ns":
                        CreateEditionWithSalt = false;
                        break;
                    case "--noxmlcanon":
                    case "-nxc":
                        compatibility.SuppressStructureXmlCanonicalization = true;
                        break;
                }
                pos++;
            }

            Compatibility = compatibility;
        }

        private static string ExpandPath(string value)
        {
            var path = Environment.ExpandEnvironmentVariables(value);
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Environment.CurrentDirectory, path);
            }
            return path;
        }

        private string GetExistingFile(string value)
        {
            var path = ExpandPath(value);
            if (!File.Exists(path))
            {
                Program.WriteWarning(Resources.Warning_FileNotFound, path);
                ArgumentError = true;
                return null;
            }
            return path;
        }

        private string GetExistingPath(string value)
        {
            var path = ExpandPath(value);
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Program.WriteWarning(Resources.Warning_PathNotFound, path);
                ArgumentError = true;
                return null;
            }
            return path;
        }

        private void SetMode(string value)
        {
            switch (value.ToLower())
            {
                case "create":
                case "c":
                    Mode = WorkingMode.Create;
                    break;
                case "extend":
                case "e":
                    Mode = WorkingMode.Extent;
                    break;
                case "inspect":
                case "i":
                    Mode = WorkingMode.Inspect;
                    break;
                case "validate":
                case "v":
                    Mode = WorkingMode.Validate;
                    break;
                case "transform":
                case "t":
                    Mode = WorkingMode.Transform;
                    break;
                case "reinstate":
                case "r":
                    Mode = WorkingMode.Reinstate;
                    break;
                default:
                    Program.WriteWarning(Resources.Warning_InvalidMode);
                    ArgumentError = true;
                    break;
            }
        }

        private void SetStorageType(string value)
        {
            switch (value.ToLower())
            {
                case "directory":
                case "dir":
                case "d":
                    StorageType = StorageType.Directory;
                    break;
                case "zipfile":
                case "zip":
                case "z":
                    StorageType = StorageType.ZipFile;
                    break;
                default:
                    Program.WriteWarning(Resources.Warning_InvalidStorageType);
                    ArgumentError = true;
                    break;
            }
        }

        private void AddEntityDescriptionFile(string value)
        {
            var entityFile = GetExistingFile(value);
            if (entityFile != null)
            {
                entityFiles.Add(entityFile);
            }
        }
    }
}
