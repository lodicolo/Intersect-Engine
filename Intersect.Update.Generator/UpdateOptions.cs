namespace Intersect.Update.Generator
{
    public sealed class UpdateOptions
    {
        public string ClientExecutableName { get; set; }

        public bool Debug { get; set; }

        public string EditorExecutableName { get; set; }

        public PackingOptions PackingOptions { get; set; }

        public string SourceDirectory { get; set; }

        public string TargetDirectory { get; set; }
    }
}