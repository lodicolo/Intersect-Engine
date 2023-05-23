using System.CommandLine;

namespace Intersect.Update.Generator
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand("Generates update files for the Intersect Game Engine.");
            UpdateOptionsBinder.Configure(rootCommand);
            return rootCommand.Invoke(args);
        }
    }
}