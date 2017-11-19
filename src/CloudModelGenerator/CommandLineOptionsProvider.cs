using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace CloudModelGenerator
{
    /// <summary>
    /// Configuration source for Microsoft.Extensions.CommandLineUtils.Option
    /// </summary>
    public class CommandLineOptionsProvider : ConfigurationProvider, IConfigurationSource
    {
        public CommandLineOptionsProvider(List<CommandOption> appOptions)
        {
            foreach (var commandOption in appOptions)
            {
                switch (commandOption.OptionType)
                {
                    case CommandOptionType.MultipleValue:
                    case CommandOptionType.SingleValue:
                        string value = commandOption.Value();
                        if (!string.IsNullOrEmpty(value))
                        {
                            Data.Add(commandOption.LongName, value);
                        }
                        break;

                    case CommandOptionType.NoValue:
                        if (commandOption.HasValue())
                        {
                            Data.Add(commandOption.LongName, true.ToString());
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}
