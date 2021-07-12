namespace SteganographyApp.Common.Arguments
{
    /// <summary>
    /// Enum specifying whether the program is to attempt to proceed with encoding a file
    /// or decoding to an output file.
    /// </summary>
    public enum ActionEnum
    {
        /// <summary>
        /// Used with the SteganographyApp to indicate that the user intends to encode a file
        /// and hide it within a series of images.
        /// </summary>
        Encode,

        /// <summary>
        /// Used with the SteganographyApp to indicate that the user intends to decode a file
        /// from a series of images.
        /// </summary>
        Decode,

        /// <summary>
        /// Used with the SteganographyApp to indicate that the user wants to ranmize the LSB
        /// of a series of images.
        /// </summary>
        Clean,

        /// <summary>
        /// Used with the Calculate to indicate that the user wants to calculate the amount of
        /// storage space that a series of images provides.
        /// </summary>
        CalculateStorageSpace,

        /// <summary>
        /// Used with the Calculator to indicate that the user wants to calculate the approximate size
        /// of a file after it has been encoded.
        /// </summary>
        CalculateEncryptedSize,

        /// <summary>
        /// A short hand representation CalculateStorageSpace.
        /// </summary>
        /// <see cref="ActionEnum.CalculateStorageSpace"></see>
        CSS,

        /// <summary>
        /// A short hand representation CalculateEncryptedSize.
        /// </summary>
        /// <see cref="ActionEnum.CalculateEncryptedSize"></see>
        CES,
    }
}