using System.Collections.Generic;
using System.Linq;

namespace Kontent.Ai.ModelGenerator.Options;

public class ArgValidationResult
{
    public bool HasUnsupportedParams => UnsupportedArgs.Any();

    public IEnumerable<string> UnsupportedArgs { get; }

    public ArgValidationResult(IEnumerable<string> unsupportedArgs)
    {
        UnsupportedArgs = unsupportedArgs;
    }
}
