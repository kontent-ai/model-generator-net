using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.Management;
using Moq;
using Kontent.Ai.ModelGenerator.Core.Tests.Fixtures;

namespace Kontent.Ai.ModelGenerator.Core.Tests;

public abstract class CodeGeneratorTestsBase
{
    protected abstract string TempDir { get; }
    protected const string ProjectId = "975bf280-fd91-488c-994c-2f04416e5ee3";

    protected CodeGeneratorTestsBase()
    {
        // Cleanup
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
        Directory.CreateDirectory(TempDir);
    }

    protected static IManagementClient CreateManagementClient()
    {
        var managementModelsProvider = new ManagementModelsProvider();
        var managementClientMock = new Mock<IManagementClient>();

        var contentTypeListingResponseModel = new Mock<IListingResponseModel<ContentTypeModel>>();
        contentTypeListingResponseModel.As<IEnumerable<ContentTypeModel>>()
            .Setup(c => c.GetEnumerator())
            .Returns(managementModelsProvider.ManagementContentTypeModels.GetEnumerator);

        var contentTypeSnippetListingResponseModel = new Mock<IListingResponseModel<ContentTypeSnippetModel>>();
        contentTypeSnippetListingResponseModel.As<IEnumerable<ContentTypeSnippetModel>>()
            .Setup(c => c.GetEnumerator())
            .Returns(managementModelsProvider.ManagementContentTypeSnippetModels.GetEnumerator);

        managementClientMock.Setup(client => client.ListContentTypeSnippetsAsync())
            .ReturnsAsync(() => contentTypeSnippetListingResponseModel.Object);
        managementClientMock.Setup(client => client.ListContentTypesAsync())
            .ReturnsAsync(() => contentTypeListingResponseModel.Object);

        return managementClientMock.Object;
    }
}
