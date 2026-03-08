using Common;

namespace LanguageMerger
{
    internal class LanguageMergerProgram(CommandLineOptions options, string inputDir)
	{
		private readonly List<ModLocale> r_modLocales = [];
        private readonly CommandLineOptions r_options = options;
		private readonly string r_inputDir = inputDir;

		public async Task<bool> Run()
        {
            ConsoleLogHelper.WriteLine("Language Merger Program has Started!", LogLevel.Message);

			ConsoleLogHelper.WriteLine("Generating Mod Locales", LogLevel.Message);
			await GenerateModLocales();
			ConsoleLogHelper.WriteLine("Creating Output Directories", LogLevel.Message);
            await CreateOutputDirectories();
            ConsoleLogHelper.WriteLine("Deserializing JSON Files", LogLevel.Message);
            await ReadLanguageFiles();
            ConsoleLogHelper.WriteLine("Merging Tools JSON Files", LogLevel.Message);
            await MergeToolsFiles();
            ConsoleLogHelper.WriteLine("Overwriting TFG Modpack JSON File", LogLevel.Message);
            await OverwriteTFGModpackFile();
            ConsoleLogHelper.WriteLine("Merging Tools JSON files with Modpack files", LogLevel.Message);
            await MergeWithModpackFiles();
            ConsoleLogHelper.WriteLine("Writing Files", LogLevel.Message);
            await WriteFiles();
            ConsoleLogHelper.WriteLine("Overwriting Files", LogLevel.Message);
            await MoveFilesToKJSAssetsFolder();

            return true;
        }

		private Task GenerateModLocales()
		{
			var enumeratedDirectories = new DirectoryInfo(r_inputDir).EnumerateDirectories();
			foreach (var modDirectory in enumeratedDirectories)
			{
				ModLocale locale = new ModLocale(modDirectory.Name);
				r_modLocales.Add(locale);
			}
			return Task.CompletedTask;
		}

		private async Task ReadLanguageFiles()
        {
            List<Task> tasks = [];
            foreach (ModLocale locale in r_modLocales)
            {
                ConsoleLogHelper.WriteLine($"Deserializing Tools JSON files for {locale.r_modID}", LogLevel.Info);
                tasks.Add(locale.DeserializeToolsDictionaries(r_inputDir));
				ConsoleLogHelper.WriteLine($"Deserializing Modpack JSON files for {locale.r_modID}", LogLevel.Info);
				tasks.Add(locale.DeserializeModpackDictionaries(r_options.AssetsDir!));
			}
            await Task.WhenAll(tasks);
        }

        private async Task MergeToolsFiles()
        {
            List<Task> tasks = [];
            foreach (ModLocale locale in r_modLocales)
            {
                ConsoleLogHelper.WriteLine($"Merging Tools JSON for {locale.r_modID}", LogLevel.Info);
                tasks.Add(locale.MergeDeserializedDictionaries());
            }
            await Task.WhenAll(tasks);
        }

        private async Task CreateOutputDirectories()
        {
            var workingDirectory = Directory.GetCurrentDirectory();
            var outputDirectory = Path.Combine(workingDirectory, "OUTPUT");

            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
            }
            var dirInfo = Directory.CreateDirectory(outputDirectory);

            List<Task> tasks = [];
            foreach (ModLocale locale in r_modLocales)
            {
                tasks.Add(locale.EnsureOutputDir(dirInfo));
            }
            await Task.WhenAll(tasks);
        }

        private Task OverwriteTFGModpackFile()
        {
            var tfg = r_modLocales.Single(l => l.r_modID == "tfg");
            return tfg.OverwriteModpackFile();
        }

        private async Task MergeWithModpackFiles()
        {
            List<Task> tasks = [];
            foreach (ModLocale locale in r_modLocales)
            {
                if (locale.r_modID == "tfg")
                    continue;

                tasks.Add(locale.MergeWithModpack());
            }
            await Task.WhenAll(tasks);
        }

        private async Task WriteFiles()
        {
            List<Task> tasks = [];
            foreach (ModLocale locale in r_modLocales)
            {
				if (locale.r_modID == "tfg")
					continue;

				ConsoleLogHelper.WriteLine($"Writing merged JSON for {locale.r_modID}.", LogLevel.Info);
                tasks.Add(locale.WriteMergedModpackFile());
            }
            await Task.WhenAll(tasks);
        }

        private async Task MoveFilesToKJSAssetsFolder()
        {
            List<Task> tasks = [];
            foreach (ModLocale locale in r_modLocales)
            {
                ConsoleLogHelper.WriteLine($"Moving merged JSON Files for {locale.r_modID} to the kjs assets folder.", LogLevel.Info);
                tasks.Add(locale.MoveFilesToKJSAssetsFolder(new DirectoryInfo(r_options.AssetsDir!)));
            }
            await Task.WhenAll(tasks);
        }
    }
}
