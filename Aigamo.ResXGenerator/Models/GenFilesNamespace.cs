using System.Collections.Immutable;
using Aigamo.ResXGenerator.Tools;

namespace Aigamo.ResXGenerator.Models;

public record GenFilesNamespace(string Namespace, ImmutableArray<GenFileOptions> Files)
{
	public string SafeNamespaceName { get; } = Namespace.NamespaceNameCompliant();

	public bool NullForgivingOperator => Files.All(f => f.NullForgivingOperators);

	public string NameOfUsingMethodRegistration => $"Using{SafeNamespaceName}ResX";
}
