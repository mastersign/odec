using System;
using System.IO;
using System.Reflection;

namespace de.mastersign.odec.cli
{
    internal partial class Program
    {
        public static StartupInfo StartupInfo { get; private set; }

        static int Main(string[] args)
        {
            StartupInfo = new StartupInfo(args);

            if (StartupInfo.ArgumentError)
            {
                WriteHelpHint();
                return ERR_ARGUMENT_INVALID;
            }
            if (StartupInfo.ShowHelp)
            {
                return ShowHelp();
            }
            if (StartupInfo.ShowInfo)
            {
                return ShowInfo();
            }
            switch (StartupInfo.Mode)
            {
                case WorkingMode.Create:
                    return Create();
                case WorkingMode.Extent:
                    DisplayValidationErrorsOnly = true;
                    return Extend();
                case WorkingMode.Inspect:
                    DisplayValidationErrorsOnly = true;
                    return Inspect();
                case WorkingMode.Validate:
                    DisplayValidationErrorsOnly = false;
                    return Validate();
                case WorkingMode.Transform:
                    return Transform();
                case WorkingMode.Reinstate:
                    DisplayValidationErrorsOnly = true;
                    return Reinstate();
            }
            return OK;
        }

        private static string GetBaseDir()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var codeBaseUri = new Uri(codeBase);
            return Path.GetDirectoryName(codeBaseUri.LocalPath);
        }

        private static void PrintFileContent(string file)
        {
            var an = Assembly.GetExecutingAssembly().GetName().Name;
            var fileName = an + "." + file;
            var path = Path.Combine(GetBaseDir(), fileName);
            if (File.Exists(path))
            {
                Console.WriteLine(File.ReadAllText(path));
            }
            else
            {
                WriteWarning(string.Format(Resources.Warning_FileNotFound, fileName));
            }
        }

        private static int ShowHelp()
        {
            PrintFileContent("help.md");
            return OK;
        }

        private static int ShowInfo()
        {
            var clrV = Environment.Version;
            var libV = typeof(Container).Assembly.GetName().Version;
            Console.WriteLine();
            Console.WriteLine("ODEC");
            Console.WriteLine("====");
            Console.WriteLine();
            Console.WriteLine("Version Information");
            Console.WriteLine("-------------------");
            Console.WriteLine("Runtime: {0}", clrV);
            Console.WriteLine("ODEC:    {0}", libV);
            Console.WriteLine();
            Console.WriteLine("License Information");
            Console.WriteLine("-------------------");
            Console.WriteLine();
            PrintFileContent("license.md");
            return OK;
        }

        public static void WriteError(string messageFormat, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("[Error] " + string.Format(messageFormat, args));
            Console.ResetColor();
        }

        public static void WriteWarning(string messageFormat, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("[Warning] " + string.Format(messageFormat, args));
            Console.ResetColor();
        }

        public static void WriteInfo(string messageFormat, params object[] args)
        {
            Console.WriteLine("[Info] " + string.Format(messageFormat, args));
        }

        public static void WriteHelpHint()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("Use -? for help.");
            Console.ResetColor();
        }

        #region Error Codes

        public const int OK = 0;
        public const int ERR_ARGUMENT_INVALID = 2;
        public const int ERR_ARGUMENT_MISSING = 3;
        public const int ERR_CONTAINER_EXISTS = 4;
        public const int ERR_XML = 5;
        public const int ERR_INITIALIZATION = 6;
        public const int ERR_SEALING = 7;
        public const int ERR_ENTITY_CREATION = 8;
        public const int ERR_ENTITY_STORAGE = 9;
        public const int ERR_DESCRIPTION_INVALID = 10;
        public const int ERR_VALUE_SOURCE_FILE_MISSING = 11;
        public const int ERR_CONTAINER_MISSING = 12;
        public const int ERR_PROFILE_MISMATCH = 13;
        public const int ERR_CONTAINER_OPENING = 14;
        public const int ERR_COPY_STORAGE_FILE = 15;
        public const int ERR_CERTIFICATE_MISSING = 16;
        public const int ERR_PRIVATE_KEY_MISSING = 17;
        public const int ERR_REINSTATE_EDITION_IMPOSSIBLE = 18;
        public const int ERR_REINSTATE_EDITION_FAILED = 19;

        public const int ERR_VALIDATION_STEP_1 = 101;
        public const int ERR_VALIDATION_STEP_2 = 102;
        public const int ERR_VALIDATION_STEP_3 = 103;
        public const int ERR_VALIDATION_STEP_4 = 104;

        #endregion
    }
}
