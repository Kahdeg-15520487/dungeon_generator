
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Demo
{
    static class PixelTemplate
    {
        public static Dictionary<string, Image<Rgba32>> PixelTemplates = new Dictionary<string, Image<Rgba32>>();
        public static void LoadPixelTemplates()
        {
            foreach (var template in Directory.EnumerateFiles("templates", "*.png"))
            {
                var image = Image.Load<Rgba32>(template);
                PixelTemplates.Add(Path.GetFileNameWithoutExtension(template), image);
            }
        }
    }
}
