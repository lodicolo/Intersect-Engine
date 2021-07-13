using Newtonsoft.Json;

namespace Intersect.Server.Framework.Entities
{
    public struct Label
    {
        [JsonProperty("Label")]
        public string Text { get; set; }

        public Color Color;

        public Label(string label, Color color)
        {
            Text = label;
            Color = color;
        }
    }
}