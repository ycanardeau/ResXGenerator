namespace DemoLocalization;

internal interface IMainProcess
{
	void Run();
}

internal class MainProcess(IResource resources) : IMainProcess
{
	public void Run()
	{
		// Afficher le message localisé
		Console.WriteLine(resources.HelloWorld);
		Console.ReadKey();
	}
}
