using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// This enumeration represents the XML type <c>VALUEAPPEARANCE</c> in the 
    /// namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    public enum ValueAppearance
    {
        /// <summary>
        /// If a value or parameter set is marked with <see cref="plain"/>, it is stored without change as octet stream.
        /// </summary>
        plain,

        /// <summary>
        /// If a value or parameter set is marked with <see cref="encrypted"/>, it is stored in an encrypted manner.
        /// </summary>
        encrypted,

        /// <summary>
        /// If a value or parameter set is marked with <see cref="suppressed"/>, 
        /// it is not included the container. This can be done for privacy reasons
        /// or to save storage space.
        /// </summary>
        suppressed,
    }
}