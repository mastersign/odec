namespace de.mastersign.odec
{
    /// <summary>
    /// An enumeration with all classes of messages, which can occur while validating the container.
    /// </summary>
    public enum ValidationMessageClass
    {
        /// <summary>
        /// Messages of this class are send, while validating the container.
        /// </summary>
        Global,

        /// <summary>
        /// Messages of this class are send, while validating the file structure of the container.
        /// </summary>
        ContainerStructure,

        /// <summary>
        /// Messages of this class are send, while validating XML documents.
        /// </summary>
        XmlSchema,

        /// <summary>
        /// Messages of this class are send, while verifying signatures.
        /// </summary>
        Signature,

        /// <summary>
        /// Messages of this class are send, while validating certificates.
        /// </summary>
        Certificate,

        /// <summary>
        /// Messages of this class are send, while validating the type of an entity.
        /// </summary>
        Entity,

        /// <summary>
        /// Messages of this class are send, while validating the profile of an container.
        /// </summary>
        ContainerProfile,
    }
}