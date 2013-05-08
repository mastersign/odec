using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// The enumeration with all possible states of the edition salt 
    /// used to prevent reinstating history editions.
    /// </summary>
    /// <seealso cref="EditionElement.Salt"/>
    /// <seealso cref="EditionElement.SaltState"/>
    public enum EditionSaltState
    {
        /// <summary>
        /// The edition has no salt, it is not protected against reinstating it as current edition.
        /// Thie edition can be the current edition or a history edition.
        /// </summary>
        None, 

        /// <summary>
        /// The salt of this history edition was removed while pushing it on to the history stack to prevent reinstating it as current edition.
        /// </summary>
        Removed, 

        /// <summary>
        /// The salt of the edition is present because the edition is the current edition.
        /// </summary>
        Present
    }
}
