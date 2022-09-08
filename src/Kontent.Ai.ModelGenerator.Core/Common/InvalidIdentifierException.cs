using System;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class InvalidIdentifierException : Exception
{
    public InvalidIdentifierException(string message) : base(message)
    {
    }
}
