using System;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class InvalidIdException : Exception
    {
        public InvalidIdException(string message) : base(message)
        {
        }
    }
}