using CommandLine;

namespace DemoLocalization;

public class Options
{
	[Option('l', "locale", Required = false, HelpText = """
														Set the langage of the application values: 
														- 0 -> English
														- 1 -> Danish 
														- 2 -> French
														""")]
	public int Locale { get; set; }
}
