using System;

namespace KenticoCloudDotNetGenerators
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