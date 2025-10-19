using System.Collections.Immutable;
using Aigamo.ResXGenerator.Models;

namespace Aigamo.ResXGenerator.Tools;

public interface IGenerator<in T>
{
	GeneratedOutput Generate(T options, CancellationToken cancellationToken = default);
}

public interface IResXGenerator : IGenerator<GenFileOptions>;
public interface IComboGenerator : IGenerator<CultureInfoCombo>;
public interface ILocalRegisterGenerator : IGenerator<GenFilesNamespace>;
public interface IGlobalRegisterGenerator : IGenerator<ImmutableArray<GenFilesNamespace>>;
