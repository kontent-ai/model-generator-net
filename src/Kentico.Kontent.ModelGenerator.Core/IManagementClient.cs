using System.Collections.Generic;
using System.Threading.Tasks;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core.Configuration;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public interface IManagementClient
    {
        Task<IList<ContentTypeModel>> GetAllContentTypesAsync(CodeGeneratorOptions options);
        Task<IList<SnippetModel>> GetAllSnippetsAsync(CodeGeneratorOptions options);
    }
}
