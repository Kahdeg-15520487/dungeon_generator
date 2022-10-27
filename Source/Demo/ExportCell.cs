
using Dungeon.Generator;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Demo
{
    public class ExportCell
    {
        public ExportCell(Cell cell)
        {
            this.CellType = cell.Type;
            this.CellAttributes = cell.Attributes;
            this.CellOpenings = cell.Openings;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public CellType CellType { get; private set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public AttributeType CellAttributes { get; private set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Direction CellOpenings { get; private set; }
    }
}
