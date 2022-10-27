using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Dungeon.Generator;

using Newtonsoft.Json;

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
        private static GeneratorParams gp = GeneratorParams.Default;
        private static readonly Display display = new Display();
        private static readonly Dictionary<string, Action> _inputMap = new Dictionary<string, Action>
        {
            {"size", PrintSize },
            {"inc", IncreaseSize},
            {"dec", DecreaseSize},
            {"seed", ChangeSeed},
            {"rseed", RandomSeed },
            {"quit", Quit},
            {"exit", Quit},
            {"gen", Generate},
            {"exp", Export},
            //{"render", Render1},
            {"render", Render2},
            {"help", ShowHelp },
            {"clear", Clear },
            {"rc", SetRoomChance },
            {"lc", SetLootChance },
        };

        public static void Main()
        {
            PixelTemplate.LoadPixelTemplates();
            running = true;
            if (File.Exists("generatorparam.json"))
            {
                gp = JsonConvert.DeserializeObject<GeneratorParams>(File.ReadAllText("generatorparam.json"));
            }

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

        private static void SetRoomChance()
        {
            Console.WriteLine("old room chance: {0}", gp.RoomChance);
            Console.Write("new room chance: ");
            if (float.TryParse(Console.ReadLine(), out var roomChance) && roomChance >= 0f && roomChance <= 1f)
            {
                gp.RoomChance = roomChance;
            }
            SaveGeneratorParams();
        }

        private static void SetLootChance()
        {
            Console.WriteLine("old loot chance: {0}", gp.LootChance);
            Console.Write("new loot chance: ");
            if (float.TryParse(Console.ReadLine(), out var LootChance) && LootChance >= 0f && LootChance <= 1f)
            {
                gp.LootChance = LootChance;
            }
            SaveGeneratorParams();
        }

        private static void Generate()
        {
            var size = sizes[selectedSize];
            gp.Seed = Seed;
            gp.MobsInRoomsOnly = true;
            dungeon = CellBasedGeneratorCaller.Generate((int)size, gp);
            cells = CellBasedGeneratorCaller.cbg._cells;
        }

        private static void Quit()
        {
            running = false;
            SaveGeneratorParams();
        }

        private static void SaveGeneratorParams()
        {
            File.WriteAllText("generatorparam.json", JsonConvert.SerializeObject(gp, Formatting.Indented));
        }

        static void RandomSeed()
        {
            Console.WriteLine("old seed: {0}", Seed);
            Seed = (uint)new Random().Next();
            Console.WriteLine("new seed: {0}", Seed);
            SaveGeneratorParams();
        }

        static void ChangeSeed()
        {
            Console.WriteLine("old seed: {0}", Seed);
            Console.Write("new seed: ");
            if (uint.TryParse(Console.ReadLine(), out var seedValue))
            {
                Seed = seedValue;
            }
            SaveGeneratorParams();
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
            int renderWidth = 16 * dungeon.Width;
            int renderHeight = 16 * dungeon.Height;

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
                image.SaveAsPng($"map_{Seed}_{sizes[selectedSize]}.png");
            }
            Process.Start("explorer.exe", $"map_{Seed}_{sizes[selectedSize]}.png");

            void RenderTemplate(Image<Rgba32> image, int x, int y, Image<Rgba32> template)
            {
                for (int j = 0; j < 16; j++)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (template[i, j].A != 0)
                        {
                            image[x * 16 + i, y * 16 + j] = template[i, j];
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
}
