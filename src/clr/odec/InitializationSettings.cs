using de.mastersign.odec.model;

namespace de.mastersign.odec
{
    /// <summary>
    /// A collection of configuration parameters for the container initialization phase.
    /// </summary>
    public class InitializationSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationSettings"/> class.
        /// </summary>
        public InitializationSettings()
        {
            CreateSaltForNewEdition = true;
        }

        /// <summary>
        /// Gets or sets a value controlling if the new edition will be equiped
        /// with a cryptographic random salt.
        /// </summary>
        /// <seealso cref="EditionElement.Salt"/>
        public bool CreateSaltForNewEdition { get; set; }
    }
}