using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using de.mastersign.odec.crypto;

namespace de.mastersign.odec.cli
{
    internal class PasswordSource : IPasswordSource
    {
        public string GetPassword()
        {
            Console.Out.Write(Resources.PasswordSource_GetPassword_Prompt);
            Console.Out.Flush();
            var sb = new StringBuilder();
            do
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    Console.Out.Write("\b \b");
                    sb.Remove(sb.Length - 1, 1);
                }
                else
                {
                    Console.Out.Write('*');
                    Console.Out.Flush();
                    sb.Append(key.KeyChar);
                }
            } while (true);
            Console.WriteLine();
            Console.Out.Flush();
            return sb.ToString();
        }
    }
}
