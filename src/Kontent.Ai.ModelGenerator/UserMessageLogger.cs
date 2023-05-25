using System;
using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator;

public class UserMessageLogger : IUserMessageLogger
{
    public void LogInfo(string message) => Log(message);

    public void LogWarning(string message) => Log(message, "Warning: ");

    private static void Log(string message, string messagePrefix = "")
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine($"{messagePrefix}{message}");
        }
    }
}
