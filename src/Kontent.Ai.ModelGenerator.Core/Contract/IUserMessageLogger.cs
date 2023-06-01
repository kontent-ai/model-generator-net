using System.Threading.Tasks;

namespace Kontent.Ai.ModelGenerator.Core.Contract;

public interface IUserMessageLogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    Task LogErrorAsync(string message);
}
