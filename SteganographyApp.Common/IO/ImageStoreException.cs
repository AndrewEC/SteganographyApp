namespace SteganographyApp.Common.IO;

using System;

/// <summary>
/// A general exception to represent a specific error occured
/// while reading or writing data to the images.
/// </summary>
public sealed class ImageStoreException : Exception
{
    /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessage/*' />
    public ImageStoreException(string message)
    : base(message) { }

    /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessageInner/*' />
    public ImageStoreException(string message, Exception inner)
    : base(message, inner) { }
}