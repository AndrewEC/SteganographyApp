namespace SteganographyApp.Common.Arguments
{
    using System;

#pragma warning disable SA1402

    /// <summary>
    /// Specifies that an exception occured while trying to read and parse the command line arguments
    /// or that certain required arguments were not present.
    /// </summary>
    public class ArgumentParseException : Exception
    {
        /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessage/*' />
        public ArgumentParseException(string message) : base(message) { }

        /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessageInner/*' />
        public ArgumentParseException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Specifies that a value provided for a particular argument was not valid or could not be properly
    /// parsed into the required data type.
    /// </summary>
    public class ArgumentValueException : Exception
    {
        /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessage/*' />
        public ArgumentValueException(string message) : base(message) { }

        /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessageInner/*' />
        public ArgumentValueException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// An exception type that is thrown when an error occurs while invoking the
    /// post validation delegate from the argument parser.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessageInner/*' />
        public ValidationException(string message, Exception inner) : base(message, inner) { }
    }
#pragma warning restore SA1402
}