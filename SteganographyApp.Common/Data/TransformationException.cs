namespace SteganographyApp.Common.Data;

using System;

/// <summary>
/// A case class inheriting from Exception that specifies an error occured
/// when an IFileCoder instance attempted to transform input data.
/// </summary>
public class TransformationException : Exception
{
    /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessage/*' />
    public TransformationException(string message)
    : base(message) { }

    /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessageInner/*' />
    public TransformationException(string message, Exception inner)
    : base(message, inner) { }
}
