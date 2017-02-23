namespace KenticoCloudDotNetGenerators
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                new CodeGenerator(options).Run();
            }
        }
    }
}
