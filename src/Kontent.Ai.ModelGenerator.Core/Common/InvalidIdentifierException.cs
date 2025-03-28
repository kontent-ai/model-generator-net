using System;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class InvalidIdentifierException(string message) : Exception(message)
{
}
