namespace Kontent.Ai.ModelGenerator.Core
{
    public interface IOutputProvider
    {
        void Output(string content, string fileName, bool overwriteExisting);
    }
}
