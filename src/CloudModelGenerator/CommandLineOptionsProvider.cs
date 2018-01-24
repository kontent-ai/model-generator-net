using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
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
                if (commandOption.Value == null)
                {
                    continue;
                }

                string value = commandOption.Value.ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    Data.Add(commandOption.Names.Last(), value);
                }
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}
