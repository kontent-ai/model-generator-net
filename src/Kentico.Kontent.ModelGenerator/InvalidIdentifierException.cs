using System;

namespace Kentico.Kontent.ModelGenerator
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