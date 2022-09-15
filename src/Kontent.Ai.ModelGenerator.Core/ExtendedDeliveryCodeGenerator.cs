using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator.Core;

public class ExtendedDeliveryCodeGenerator : DeliveryCodeGeneratorBase
{
    public ExtendedDeliveryCodeGenerator(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider, IManagementClient managementClient) : base(options, outputProvider)
    {
    }

    protected override Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators()
    {
        throw new NotImplementedException();
    }

    internal IEnumerable<ClassCodeGenerator> GetClassCodeGenerator(ContentTypeModel contentType, List<ContentTypeSnippetModel> managementSnippets, List<ContentTypeModel> contentTypes)
    {
        throw new NotImplementedException();
    }
}
