using System;

namespace CloudModelGenerator
{
    public class InvalidIdentifierException : Exception
    {
        public InvalidIdentifierException()
        {
        }

        public InvalidIdentifierException(string message) : base(message)
        {
        }
    }
}