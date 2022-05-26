using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kentico.Kontent.Management;
using Kentico.Kontent.Management.Models.Shared;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Tests.Fixtures;
using Moq;

namespace Kentico.Kontent.ModelGenerator.Tests
{
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
                .Returns(() => managementModelsProvider.ManagementContentTypeModels);

            var contentTypeSnippetListingResponseModel = new Mock<IListingResponseModel<ContentTypeSnippetModel>>();
            contentTypeSnippetListingResponseModel.As<IEnumerable<ContentTypeSnippetModel>>()
                .Setup(c => c.GetEnumerator())
                .Returns(() => managementModelsProvider.ManagementContentTypeSnippetModels);

            managementClientMock.Setup(client => client.ListContentTypeSnippetsAsync())
                .Returns(Task.FromResult(contentTypeSnippetListingResponseModel.Object));
            managementClientMock.Setup(client => client.ListContentTypesAsync())
                .Returns(Task.FromResult(contentTypeListingResponseModel.Object));

            return managementClientMock.Object;
        }
    }
}
