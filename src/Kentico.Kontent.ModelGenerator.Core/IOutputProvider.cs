using System.Collections.Generic;
using System.Text;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public interface IOutputProvider
    {
        void Output(string content, string fileName, bool overwriteExisting);
    }
}
