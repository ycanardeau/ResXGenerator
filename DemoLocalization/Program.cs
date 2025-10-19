using System.Globalization;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using ResXGenerator.Registration;

namespace DemoLocalization;

public static class Program
{
	public static void Main(string[] args)
	{
		// Configurer les services
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddLocalization();
		services.UsingResXGenerator();
		services.AddTransient<IMainProcess, MainProcess>();
		var provider = services.BuildServiceProvider();

		Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
		{
			RunProgram(o, provider);
		});
	}

	private static void RunProgram(Options opt, IServiceProvider provider)
	{
		CultureInfo.CurrentUICulture = opt.Locale switch
		{
			1 => new CultureInfo("da"),
			2 => new CultureInfo("fr"),
			_ => new CultureInfo("en")
		};

		var process = provider.GetService<IMainProcess>();
		process?.Run();
	}
}
