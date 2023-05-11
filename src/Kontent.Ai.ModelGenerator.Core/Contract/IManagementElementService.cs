using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.TypeSnippets;
using System.Collections.Generic;

namespace Kontent.Ai.ModelGenerator.Core.Contract;

public interface IManagementElementService
{
    IEnumerable<ElementMetadataBase> GetManagementContentTypeSnippetElements(
        ElementMetadataBase element,
        IEnumerable<ContentTypeSnippetModel> managementSnippets);
}
