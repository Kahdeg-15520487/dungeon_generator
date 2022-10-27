
using Dungeon.Generator;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Demo
{
    public class ExportTile
    {
        public ExportTile(Tile tile)
        {
            MaterialType = tile.MaterialType;
            AttributeType = tile.Attributes;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public MaterialType MaterialType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public AttributeType AttributeType { get; set; }
    }
}
