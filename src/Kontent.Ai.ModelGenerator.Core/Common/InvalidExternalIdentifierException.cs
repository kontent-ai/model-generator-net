using System;

namespace Kontent.Ai.ModelGenerator.Core.Common;
public class InvalidExternalIdentifierException : Exception
{
    public InvalidExternalIdentifierException(string message) : base(message)
    {
    }
}
