using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec
{
    /// <summary>
    /// This enumeration contains the level of severity for validation messages.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// A success message. It can be used to trace the validation process.
        /// </summary>
        Success,

        /// <summary>
        /// An error message. It indicates, that a validation step has failed.
        /// </summary>
        Error,
    }
}
