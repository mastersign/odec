using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace de.mastersign.odec
{
    /// <summary>
    /// An exception thrown if an unexpected error occurs while processing a container.
    /// </summary>
    [Serializable]
    public class ContainerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerException"/> class.
        /// </summary>
        public ContainerException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ContainerException(string message) 
            : base(message) 
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ContainerException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
