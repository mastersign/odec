using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.OpenSsl;

namespace de.mastersign.odec.crypto
{
    internal class BouncyCastlePasswordFinder : IPasswordFinder
    {
        private IPasswordSource PasswordSource { get; set; }

        private BouncyCastlePasswordFinder(IPasswordSource passwordSource)
        {
            PasswordSource = passwordSource;
        }

        public char[] GetPassword()
        {
            return PasswordSource.GetPassword().ToCharArray();
        }

        public static IPasswordFinder FromPasswordSource(IPasswordSource passwordSrc)
        {
            return passwordSrc != null ? new BouncyCastlePasswordFinder(passwordSrc) : null;
        }
    }
}
