using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.report
{
    /// <summary>
    /// Collects validation messages.
    /// </summary>
    public class ValidationMessageCollection : List<ContainerValidationEventArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessageCollection"/> class.
        /// </summary>
        /// <param name="name">The name of the collection.</param>
        public ValidationMessageCollection(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the message collection.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets fired if a new message is added to the Collection.
        /// </summary>
        public event EventHandler<ContainerValidationEventArgs> MessageAdded;

        /// <summary>
        /// The handler for validation processes.
        /// If this method gets called it adds the given message to the collection
        /// and fires the <see cref="MessageAdded"/> event.
        /// </summary>
        /// <param name="ea">The validation message.</param>
        public void MessageHandler(ContainerValidationEventArgs ea)
        {
            Add(ea);
            if (MessageAdded != null) MessageAdded(this, ea);
        }

        /// <summary>
        /// Gets a value indicating if the collection contains an error message.
        /// </summary>
        public bool ContainsError { get { return this.Any(m => m.Severity == ValidationSeverity.Error); } }
    }
}
