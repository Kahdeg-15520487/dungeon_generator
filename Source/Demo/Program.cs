using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Dungeon.Generator;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Demo
{
    public class Program
    {
        private static readonly TileMapSize[] sizes = Enum.GetValues(typeof(TileMapSize)).Cast<TileMapSize>().ToArray();
        private static int selectedSize;
        private static uint Seed = 1032u;
        private static ITileMap dungeon;
        private static Cell[,] cells;
        private static bool running;

        private static readonly Display display = new Display();
        private static readonly Dictionary<string, Action> _inputMap = new Dictionary<string, Action>
        {
            {"size", PrintSize },
            {"inc", IncreaseSize},
            {"dec", DecreaseSize},
            {"seed", ChangeSeed},
            {"quit", Quit},
            {"exit", Quit},
            {"gen", Generate},
            {"exp", Export},
            {"render", Render1},
            {"render2", Render2},
            {"help", ShowHelp },
            {"clear", Clear },
        };

        public static void Main()
        {
            PixelTemplate.LoadPixelTemplates();
            running = true;

            while (running)
            {
                //display.ShowInstructions(Seed, sizes[selectedSize]);

                Action result;
                string input;
                do
                {
                    Console.Write("> ");
                    input = Console.ReadLine();
                } while (!_inputMap.TryGetValue(input, out result));
                result();
            }
        }

        private static void Generate()
        {
            var size = sizes[selectedSize];
            GeneratorParams gp = GeneratorParams.Default;
            gp.RoomChance = 1f;
            gp.Seed = Seed;
            gp.MobsInRoomsOnly = true;
            dungeon = CellBasedGeneratorCaller.Generate((int)size, gp);
            cells = CellBasedGeneratorCaller.cbg._cells;
        }

        private static void Quit()
        {
            running = false;
        }

        static void ChangeSeed()
        {
            Console.WriteLine("old seed: {0}", Seed);
            Console.Write("new seed: ");
            if (uint.TryParse(Console.ReadLine(), out var seedValue))
            {
                Seed = seedValue;
            }
        }

        static void PrintSize()
        {
            Console.WriteLine("{0}: {1}", sizes[selectedSize], (int)sizes[selectedSize]);
        }

        static void IncreaseSize()
        {
            selectedSize = selectedSize == sizes.Length - 1 ? selectedSize : selectedSize + 1;
            PrintSize();
        }

        static void DecreaseSize()
        {
            selectedSize = selectedSize == 0 ? selectedSize : selectedSize - 1;
            PrintSize();
        }

        static void Export()
        {
            List<List<ExportTile>> tiles = new List<List<ExportTile>>();
            for (int y = 0; y < dungeon.Height; y++)
            {
                var row = new List<ExportTile>();
                for (int x = 0; x < dungeon.Width; x++)
                {
                    row.Add(new ExportTile(dungeon[x, y]));
                }
                tiles.Add(row);
            }
            File.WriteAllText($"map_{Seed}.json", JsonConvert.SerializeObject(tiles, Formatting.Indented));


            List<List<ExportCell>> cs = new List<List<ExportCell>>();
            for (int y = 0; y < dungeon.Height / CellBasedGenerator.CellSize; y++)
            {
                var row = new List<ExportCell>();
                for (int x = 0; x < dungeon.Width / CellBasedGenerator.CellSize; x++)
                {
                    row.Add(new ExportCell(cells[x, y]));
                }
                cs.Add(row);
            }
            File.WriteAllText($"cell_{Seed}.json", JsonConvert.SerializeObject(cs, Formatting.Indented));
        }

        static void Render1()
        {
            display.ShowDungeon(dungeon);
        }

        static void Render2()
        {
            int renderWidth = 8 * dungeon.Width;
            int renderHeight = 8 * dungeon.Height;

            // Creates a new image with empty pixel data. 
            using (Image<Rgba32> image = new(renderWidth, renderHeight))
            {
                for (int y = 0; y < dungeon.Height; y++)
                {
                    for (int x = 0; x < dungeon.Width; x++)
                    {
                        var tile = dungeon[x, y];
                        switch (tile.MaterialType)
                        {
                            case MaterialType.Air:
                                RenderTemplate(image, x, y, PixelTemplate.PixelTemplates["air"]);
                                break;
                            case MaterialType.Floor:
                                RenderTemplate(image, x, y, PixelTemplate.PixelTemplates["floor"]);
                                if (tile.Attributes.HasFlag(AttributeType.Entry))
                                {
                                    RenderTemplate(image, x, y, PixelTemplate.PixelTemplates["entrance"]);
                                }
                                else if (tile.Attributes.HasFlag(AttributeType.Exit))
                                {
                                    RenderTemplate(image, x, y, PixelTemplate.PixelTemplates["exit"]);
                                }
                                else if (tile.Attributes.HasFlag(AttributeType.Loot))
                                {
                                    RenderTemplate(image, x, y, PixelTemplate.PixelTemplates["loot"]);
                                }
                                else if (tile.Attributes.HasFlag(AttributeType.MobSpawn))
                                {
                                    RenderTemplate(image, x, y, PixelTemplate.PixelTemplates["mobspawn"]);

                                }
                                else if (tile.Attributes.HasFlag(AttributeType.Doors))
                                {
                                }
                                break;
                            case MaterialType.Wall:
                                RenderTemplate(image, x, y, PixelTemplate.PixelTemplates["wall"]);
                                break;
                            case MaterialType.BreakableWall:
                                break;
                        }
                    }
                }
                image.SaveAsPng($"map_{Seed}.png");
            }
            Process.Start("explorer.exe", $"map_{Seed}.png");

            void RenderTemplate(Image<Rgba32> image, int x, int y, Image<Rgba32> template)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (template[i, j].A != 0)
                        {
                            image[x * 8 + i, y * 8 + j] = template[i, j];
                        }
                        else
                        {
                            image[x * 8 + i, y * 8 + j] = template[i, j];
                        }
                    }
                }
            }
        }

        static void ShowHelp()
        {
            foreach (var item in _inputMap)
            {
                Console.WriteLine("{0}: {1}", item.Key, item.Value.Method.Name);
            }
        }

        static void Clear()
        {
            Console.Clear();
        }
    }

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
