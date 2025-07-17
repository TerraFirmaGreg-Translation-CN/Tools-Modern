using System.IO;
using System.Text.Json;
using Common;

namespace MarsBiomes
{
	public class Program
	{
		// TODO: Write the noise generator file as well

		public static void Main(string[] args)
		{
			var modpackDir = CommonUtil.GetModpackDirectory();

			// Read inputs

			var cwd = Directory.GetCurrentDirectory();
			var inputPath = cwd.Substring(0, cwd.IndexOf("MarsBiomes") + "MarsBiomes".Length);

			var dim = JsonSerializer.Deserialize<DimensionFile>(File.ReadAllText(Path.Combine(inputPath, "unmodified_dimension.json")));
			var groups = JsonSerializer.Deserialize<BiomeGroupFile>(File.ReadAllText(Path.Combine(inputPath, "biome_groups.json")));


			// Swap vanilla biomes for tfg ones

			foreach (var biome in dim.Generator.BiomeSource.Biomes)
			{
				foreach (var group in groups.Groups)
				{
					if (group.VanillaBiomes.Contains(biome.Name) && group.MarsBiome != null)
					{
						biome.Name = group.MarsBiome.Name;
					}
				}
			}

			// Outputs

			var options = new JsonSerializerOptions
			{
				WriteIndented = true
			};

			File.WriteAllText(Path.Combine(CommonUtil.GetKJSDataFolder(modpackDir), "ad_astra/dimension/mars.json"), JsonSerializer.Serialize(dim, options));
		}
	}
}