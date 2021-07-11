namespace SteganographyApp.Common.Arguments
{
    /// <summary>
    /// Enum specifying whether the program is to attempt to proceed with encoding a file
    /// or decoding to an output file.
    /// </summary>
    public enum ActionEnum
    {
        Encode,
        Decode,
        Clean,
        CalculateStorageSpace,
        CalculateEncryptedSize,
        CSS, // shorthand alias for CalculateStorageSpace
        CES, // shorthand alias for CalculateEncryptedSize
    }
}