using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.storage
{
    /// <summary>
    /// Represents exceptions in the context of the container storage.
    /// </summary>
    public class StorageException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageException"/> class.
        /// </summary>
        /// <param name="message">A message, describing the error.</param>
        public StorageException(string message) 
            : base(message) 
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageException"/> class.
        /// </summary>
        /// <param name="message">A message, describing the cause of the error.</param>
        /// <param name="innerException">The exception causing the error.</param>
        public StorageException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}
