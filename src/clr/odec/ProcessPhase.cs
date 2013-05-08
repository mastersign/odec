using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using de.mastersign.odec.crypto;

namespace de.mastersign.odec
{
    /// <summary>
    /// The process phases of a container.
    /// </summary>
    public enum ProcessPhase
    {
        /// <summary>
        /// The container is not initialized and the minimum structure is not build yet.
        /// </summary>
        /// <remarks>
        /// A container with an empty storage is in this phase.
        /// </remarks>
        Empty,

        /// <summary>
        /// The container is sealed and only read operations are allowed.
        /// </summary>
        /// <remarks>
        /// A container is in the <see cref="Sealed"/> phase after calling
        /// <see cref="Container.Open(container.storage.IStorage,container.ValidationHandler,container.CompatibilityFlags)"/>, <see cref="Container.FinishInitialization"/>
        /// or <see cref="Container.FinishTransformation"/>.
        /// </remarks>
        Sealed,

        /// <summary>
        /// The container is in the initialization phase. 
        /// In this phase entities can be added to the container.
        /// </summary>
        /// <remarks>
        /// A container is in the <see cref="Initialization"/> phase after calling
        /// <see cref="Container.Create(container.storage.IStorage,container.model.EditionElement,InitializationSettings,container.crypto.IRSAProvider,container.crypto.IRSAProvider,container.CompatibilityFlags)"/>.
        /// </remarks>
        Initialization,

        /// <summary>
        /// The container is in the transformation phase.
        /// In this phase entities can be added to and removed from the container.
        /// </summary>
        /// <remarks>
        /// A container is in the <see cref="Transformation"/> phase after calling
        /// <see cref="Container.StartTransformation(de.mastersign.odec.model.EditionElement,TransformationSettings,IRSAProvider)"/>
        /// or <see cref="Container.StartTransformation(de.mastersign.odec.model.EditionElement,TransformationSettings,IRSAProvider,IRSAProvider)"/>.
        /// </remarks>
        Transformation,
    }
}
