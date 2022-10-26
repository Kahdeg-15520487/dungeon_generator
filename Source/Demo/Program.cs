using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Dungeon.Generator;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Demo
{
    public class Program
    {
        private static readonly MapSize[] sizes = Enum.GetValues(typeof(MapSize)).Cast<MapSize>().ToArray();
        private static int selectedSize;
        private static uint Seed = 1032u;
        private static ITileMap dungeon;
        private static bool running;

        private static readonly Display display = new Display();
        private static readonly Dictionary<string, Action> _inputMap = new Dictionary<string, Action>
        {
            {"inc", IncreaseSize},
            {"dec", DecreaseSize},
            {"seed", ChangeSeed},
            {"quit", Quit},
            {"gen", Generate},
            {"exp", Export},
            {"render", Render1},
            {"help", ShowHelp },
            {"clear", Clear },
        };

        public static void Main()
        {
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
            dungeon = Generator.Generate(size, Seed);
        }

        private static void Quit()
        {
            running = false;
        }

        static void ChangeSeed()
        {
            uint seedValue;
            if (display.PromptForInt("Enter a positive integer for the new seed or press \'q\' to cancel.", out seedValue))
                Seed = seedValue;
        }

        static void IncreaseSize()
        {
            selectedSize = (selectedSize + 1) % sizes.Length;
        }

        static void DecreaseSize()
        {
            selectedSize--;
            selectedSize = selectedSize < 0 ? sizes.Length - 1 : selectedSize;
        }

        static void Export()
        {
            List<List<Cell>> cells = new List<List<Cell>>();
            for (int y = 0; y < dungeon.Height; y++)
            {
                var row = new List<Cell>();
                for (int x = 0; x < dungeon.Width; x++)
                {
                    row.Add(new Cell(dungeon[x, y]));
                }
                cells.Add(row);
            }
            File.WriteAllText($"map_{Seed}.json", JsonConvert.SerializeObject(cells, Formatting.Indented));
        }

        static void Render1()
        {
            display.ShowDungeon(dungeon);
        }

        static void Render2()
        {
            List<List<Cell>> cells = new List<List<Cell>>();
            for (int y = 0; y < dungeon.Height; y++)
            {
                var roomCells = new List<Cell>();
                for (int x = 0; x < dungeon.Width; x++)
                {
                    //row.Add(new Cell(dungeon[x, y]));
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

    public class Cell
    {
        public Cell(Tile tile)
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
