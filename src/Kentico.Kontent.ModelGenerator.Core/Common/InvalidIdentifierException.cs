using System;

namespace Kentico.Kontent.ModelGenerator.Core.Common
{
    public class InvalidIdentifierException : Exception
    {
        public InvalidIdentifierException(string message) : base(message)
        {
        }
    }
}