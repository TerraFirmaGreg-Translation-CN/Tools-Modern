using System.Text.Encodings.Web;
using System.Text.Json;
using Common;

namespace LanguageMerger
{
	public class ModLocale(string modID)
	{
		public readonly string r_modID = modID;

		private DirectoryInfo? m_modLocaleOutputFolder;
		private readonly List<Dictionary<string, string>> m_deserializedToolsDictionaries = [];
		private readonly Dictionary<string, string> m_mergedToolsDictionary = [];
		private readonly Dictionary<string, string> m_deserializedModpackDictionary = [];
		private Dictionary<string, string> m_mergedModpackDictionary = [];

		private readonly JsonSerializerOptions r_options = new()
		{
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			WriteIndented = true,
			IndentCharacter = '\t',
			IndentSize = 1,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		public async Task DeserializeToolsDictionaries(string inputFolder)
		{
			foreach (var file in Directory.GetFiles(Path.Combine(inputFolder, r_modID), "*.json", SearchOption.AllDirectories))
			{
				string json = await File.ReadAllTextAsync(file);

				//return an empty dict for empty json files.
				m_deserializedToolsDictionaries.Add(JsonSerializer.Deserialize<Dictionary<string, string>>(json, r_options) ?? []);
			}
		}

		public async Task DeserializeModpackDictionaries(string assetsFolder)
		{
			string path = Path.Combine(assetsFolder, r_modID, "lang", "en_us.json");
			if (Path.Exists(path))
			{
				string json = await File.ReadAllTextAsync(path);

				var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json, r_options) ?? [];
				foreach (var kvp in dict)
				{
					if (kvp.Key.StartsWith("__"))
						continue;

					m_deserializedModpackDictionary[kvp.Key] = kvp.Value;
				}
			}
		}

		public Task MergeDeserializedDictionaries()
		{
			Dictionary<string, string> mergedDictionary = [];

			foreach (var innerDict in m_deserializedToolsDictionaries)
			{
				foreach ((var innerKey, var innerValue) in innerDict)
				{
					if (innerKey.StartsWith("__"))
						continue;

					if (m_mergedToolsDictionary.TryGetValue(innerKey, out string? mergedValue))
					{
						ConsoleLogHelper.WriteLine($"Key \"{innerKey}\" already exists in the merged dictionary! Value will be overwritten. Original value: {mergedValue}", LogLevel.Warning);
					}
					m_mergedToolsDictionary[innerKey] = innerValue;
				}
			}
			return Task.CompletedTask;
		}

		public Task MergeWithModpack()
		{
			m_mergedModpackDictionary = new(m_deserializedModpackDictionary);

			foreach (var kvp in m_mergedToolsDictionary)
			{
				m_mergedModpackDictionary[kvp.Key] = kvp.Value;
			}

			return Task.CompletedTask;
		}

		public Task EnsureOutputDir(DirectoryInfo mainOutputDir)
		{
			var modIdDirectory = mainOutputDir.CreateSubdirectory(r_modID);
			m_modLocaleOutputFolder = modIdDirectory.CreateSubdirectory("lang");

			return Task.CompletedTask;
		}

		public async Task OverwriteModpackFile()
		{
			var fileOutput = Path.Combine(m_modLocaleOutputFolder!.FullName, "en_us.json");
			//delete the previous file.
			File.Delete(fileOutput);

			using StreamWriter writer = File.CreateText(fileOutput);
			var json = JsonSerializer.Serialize(m_mergedToolsDictionary, r_options);
			await writer.WriteAsync(json);
			writer.Close();
			writer.Dispose();
		}

		public async Task WriteMergedModpackFile()
		{
			var fileOutput = Path.Combine(m_modLocaleOutputFolder!.FullName, "en_us.json");
			//delete the previous file.
			File.Delete(fileOutput);

			using StreamWriter writer = File.CreateText(fileOutput);
			var json = JsonSerializer.Serialize(m_mergedModpackDictionary.OrderBy(kvp => kvp.Key).ToDictionary(), r_options);
			await writer.WriteAsync(json);
			writer.Close();
			writer.Dispose();
		}

		public Task MoveFilesToKJSAssetsFolder(DirectoryInfo kjsAssetsFolder)
		{
			var modIdDirectory = kjsAssetsFolder.CreateSubdirectory(r_modID);
			var langDirectory = modIdDirectory.CreateSubdirectory("lang");

			foreach (var file in m_modLocaleOutputFolder!.EnumerateFiles("*.json"))
			{
				file.CopyTo(Path.Combine(langDirectory.FullName, file.Name), true);
			}
			return Task.CompletedTask;
		}
	}
}
