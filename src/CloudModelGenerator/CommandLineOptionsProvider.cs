using KenticoCloud.Delivery;
using Microsoft.Extensions.Configuration;
using System;
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
        public CommandLineOptionsProvider(ArgumentSyntax syntax)
        {
            try
            {
                IEnumerable<Argument> appOptions = syntax.GetOptions();
                foreach (var commandOption in appOptions)
                {
                    if (commandOption.Value != null)
                    {
                        string value = commandOption.Value.ToString();

                        if (!string.IsNullOrEmpty(value))
                        {
                            var paramName = commandOption.Names.Last();

                            /// Backward compatibility <see href="https://github.com/Kentico/cloud-generators-net/issues/69"/>
                            if (paramName == "projectid")
                            {
                                paramName = $"{nameof(DeliveryOptions)}:ProjectId";
                            }

                            Data.Add(paramName, value);
                        }
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                throw new Exception(exception.Message + "\n\n" + syntax.GetHelpText());
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}
