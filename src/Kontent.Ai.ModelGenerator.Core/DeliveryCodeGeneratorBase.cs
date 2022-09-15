using Kontent.Ai.ModelGenerator.Core.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Kontent.Ai.ModelGenerator.Core;

public abstract class DeliveryCodeGeneratorBase : CodeGeneratorBase
{
    protected DeliveryCodeGeneratorBase(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider) : base(options, outputProvider)
    {
    }

    public new async Task<int> RunAsync()
    {
        throw new NotImplementedException();
    }
}
