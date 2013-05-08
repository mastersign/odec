using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec
{
    /// <summary>
    /// This delegate decribes a handler for the validation process.
    /// </summary>
    public delegate void ValidationHandler(ContainerValidationEventArgs args);

    internal static class ValidationHelper
    {
        public static void Error(this ValidationHandler handler, ValidationMessageClass messageClass,
            string messageFormat, params object[] args)
        {
            if (handler == null) return;
            handler(new ContainerValidationEventArgs(
                ValidationSeverity.Error, messageClass, string.Format(messageFormat, args)));
        }

        public static void Success(this ValidationHandler handler, ValidationMessageClass messageClass,
            string messageFormat, params object[] args)
        {
            if (handler == null) return;
            handler(new ContainerValidationEventArgs(
                ValidationSeverity.Success, messageClass, string.Format(messageFormat, args)));
        }
    }
}
