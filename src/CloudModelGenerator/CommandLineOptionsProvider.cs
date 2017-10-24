using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace CloudModelGenerator
{
    public class CommandLineOptionsProvider : ConfigurationProvider, IConfigurationSource
    {
        public CommandLineOptionsProvider(List<CommandOption> appOptions)
        {
            foreach (var commandOption in appOptions)
            {
                string value = null;
                switch (commandOption.OptionType)
                {
                    case CommandOptionType.MultipleValue:
                    case CommandOptionType.SingleValue:
                        value = commandOption.Value();
                        break;

                    case CommandOptionType.NoValue:
                        value = commandOption.HasValue().ToString();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Data.Add(commandOption.LongName, value);
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}
