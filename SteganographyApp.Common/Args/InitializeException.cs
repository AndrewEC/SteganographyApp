namespace SteganographyApp.Common.Arguments
{
    using System;
    
    public sealed class InitializeException : Exception
    {
        public InitializeException(string message) : base(message) { }
    }
}