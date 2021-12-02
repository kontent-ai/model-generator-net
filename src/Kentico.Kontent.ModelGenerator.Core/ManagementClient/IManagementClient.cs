using System.Collections.Generic;
using System.Threading.Tasks;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core.Configuration;

namespace Kentico.Kontent.ModelGenerator.Core.ManagementClient
{
    public interface IManagementClient
    {
        Task<IEnumerable<ContentTypeModel>> GetAllContentTypesAsync(CodeGeneratorOptions options);
        Task<IEnumerable<SnippetModel>> GetAllSnippetsAsync(CodeGeneratorOptions options);
    }
}
