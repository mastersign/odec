using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// An interface for objects providing a password.
    /// </summary>
    public interface IPasswordSource
    {
        /// <summary>
        /// Gets a password.
        /// </summary>
        /// <returns>The password.</returns>
        string GetPassword();
    }
}
