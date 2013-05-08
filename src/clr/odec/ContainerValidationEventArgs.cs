using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec
{
    /// <summary>
    /// Arguments for container validation handlers.
    /// </summary>
    public class ContainerValidationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the severity level of the message.
        /// </summary>
        /// <value>The severity.</value>
        public ValidationSeverity Severity { get; private set; }

        /// <summary>
        /// Gets the class of the message.
        /// </summary>
        /// <value>The message class.</value>
        public ValidationMessageClass MessageClass { get; private set; }

        /// <summary>
        /// Gets the message text.
        /// </summary>
        /// <value>The message text.</value>
        public string Message { get; private set; }

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="T:System.EventArgs"/>-Klasse.
        /// </summary>
        /// <param name="severity">The severity level of the message.</param>
        /// <param name="messageClass">The message class.</param>
        /// <param name="message">The message text.</param>
        public ContainerValidationEventArgs(ValidationSeverity severity, ValidationMessageClass messageClass, string message)
        {
            Severity = severity;
            MessageClass = messageClass;
            Message = message;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[{0}] {1}: {2}",
                                 Enum.GetName(typeof (ValidationSeverity), Severity),
                                 Enum.GetName(typeof (ValidationMessageClass), MessageClass),
                                 Message);
        }
    }
}
