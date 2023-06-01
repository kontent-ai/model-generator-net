namespace Kontent.Ai.ModelGenerator.Core.Contract;

public interface IOutputProvider
{
    void Output(string content, string fileName, bool overwriteExisting);
}
