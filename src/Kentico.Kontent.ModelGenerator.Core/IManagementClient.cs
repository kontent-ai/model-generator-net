using System.Collections.Generic;
using System.Threading.Tasks;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Newtonsoft.Json.Linq;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public interface IManagementClient
    {
        Task<IList<JObject>> GetAllContentTypesAsync(CodeGeneratorOptions options);
    }
}
