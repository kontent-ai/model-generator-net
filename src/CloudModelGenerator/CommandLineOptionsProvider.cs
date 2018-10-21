using KenticoCloud.Delivery;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace CloudModelGenerator
{
    /// <summary>
    /// Configuration source for System.CommandLine.Argument
    /// </summary>
    public class CommandLineOptionsProvider : ConfigurationProvider, IConfigurationSource
    {
        public CommandLineOptionsProvider(IEnumerable<Argument> appOptions)
        {
            foreach (var commandOption in appOptions)
            {
                if (commandOption.Value != null)
                {
                    string value = commandOption.Value.ToString();

                    if (!string.IsNullOrEmpty(value))
                    {
                        var paramName = commandOption.Names.Last();

                        // Backward compatibility
                        if (paramName == "projectid")
                        {
                            paramName = $"{nameof(DeliveryOptions)}:ProjectId";
                        }

                        Data.Add(paramName, value);
                    }
                }
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}
