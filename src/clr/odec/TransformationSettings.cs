using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec
{
    /// <summary>
    /// A collection of configuration parameters for a container transformation phase.
    /// </summary>
    public class TransformationSettings : InitializationSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationSettings"/> class.
        /// </summary>
        public TransformationSettings()
        {
            PreventReinstatingFormerCurrentEdition = false;
        }

        /// <summary>
        /// Gets or sets a value controlling if the salt of the former current edition
        /// will be removed while pushing it on to the history stack.
        /// If set to <c>true</c>, the salt will be removed and reinstating the former current edition
        /// will be impossible; otherwise the salt will stay untouched and it will be possible to reinstate
        /// the edition as current edition later on.
        /// </summary>
        public bool PreventReinstatingFormerCurrentEdition { get; set; }
    }
}
