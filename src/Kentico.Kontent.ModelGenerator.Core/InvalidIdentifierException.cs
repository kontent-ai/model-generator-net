using System;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class InvalidIdentifierException : Exception
    {
        public InvalidIdentifierException(string message) : base(message)
        {
        }
    }
}