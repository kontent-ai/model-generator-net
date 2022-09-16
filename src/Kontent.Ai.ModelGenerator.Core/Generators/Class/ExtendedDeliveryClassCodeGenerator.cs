using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class ExtendedDeliveryClassCodeGenerator : DeliveryClassCodeGenerator
{
    public ExtendedDeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
        : base(classDefinition, classFilename, @namespace)
    {
    }
}
