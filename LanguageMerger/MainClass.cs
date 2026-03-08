using System.Diagnostics.CodeAnalysis;
using Common;

namespace LanguageMerger;

public class MainClass
{
	public static void Main(string[] args)
	{
		Console.WriteLine("Generating Localization Files!");

		if (!TryGetProgramArguments(args, out CommandLineOptions? opts, out string? inputDir))
		{
			ConsoleLogHelper.WriteLine("Failed to get Program's Arguments, Press any key to exit...", LogLevel.Error);
			Console.ReadKey();
			return;
		}

		var programInstance = new LanguageMergerProgram(opts!, inputDir!);

		var task = programInstance.Run();
		task.Wait();

		ConsoleLogHelper.WriteLine("Done", LogLevel.Info);

		Console.ReadKey();
	}

	private static bool TryGetProgramArguments(string[] args, out CommandLineOptions? options, out string? inputDir)
	{
		options = CommandLineOptions.Parse(args);
		if (options is null)
		{
			inputDir = null;
			return false;
		}

		inputDir = GetLanguageFilesFolder(options);
		if (inputDir is null)
		{
			return false;
		}

		return true;
	}

	private static string? GetLanguageFilesFolder(CommandLineOptions options)
	{
		var cwd = Directory.GetCurrentDirectory();

		var projectFolder = cwd.Substring(0, cwd.IndexOf("LanguageMerger") + "LanguageMerger".Length);

		string languageFilesFolder = Path.Combine(projectFolder, "LanguageFiles");
		if (!Directory.Exists(languageFilesFolder))
		{
			Console.Error.WriteLine($"The \"LanguageFiles\" folder was not found in {languageFilesFolder}");
			return null;
		}

		return languageFilesFolder;
	}
}