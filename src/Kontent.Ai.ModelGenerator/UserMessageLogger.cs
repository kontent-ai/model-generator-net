using System;
using System.Threading.Tasks;
using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator;

public class UserMessageLogger : IUserMessageLogger
{
    public void LogInfo(string message) => Log(message);

    public void LogWarning(string message) => Log(message, "Warning: ");

    public async Task LogErrorAsync(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            await Console.Error.WriteLineAsync(message);
        }
    }

    private static void Log(string message, string messagePrefix = "")
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine($"{messagePrefix}{message}");
        }
    }
}
