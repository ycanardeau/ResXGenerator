using System.Collections.Immutable;
using Aigamo.ResXGenerator.Extensions;
using Aigamo.ResXGenerator.Generators;
using Aigamo.ResXGenerator.Models;
using Aigamo.ResXGenerator.Tools;
using Microsoft.CodeAnalysis;

namespace Aigamo.ResXGenerator;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var globalOptions = context.AnalyzerConfigOptionsProvider.Select(GlobalOptions.Select);

		// Note: Each Resx file will get a hash (random guid) so we can easily differentiate in the pipeline when the file changed or just some options
		var allResxFiles = context.AdditionalTextsProvider.Where(static af => af.Path.EndsWith(".resx"))
			.Select(static (f, _) => new AdditionalTextWithHash(f, Guid.NewGuid()));

		var monitor = allResxFiles
			.Collect()
			.SelectMany(static (x, _) => GroupResxFiles.Group(x))
			.Combine(globalOptions)
			.Combine(context.AnalyzerConfigOptionsProvider)
			.Select(static (x, _) => GenFileOptions.Select(x.Left.Left, x.Right, x.Left.Right))
			.Where(static x => x is { IsValid: true, SkipFile: false });

		//--------------------------------
		// Classic ResxManager
		//--------------------------------
		var inputsResXManager = monitor
			.Where(static x => x is { GenerationType: GenerationType.ResourceManager, GenerateCode: false });
		GenerateResXFiles(context, inputsResXManager);

		//--------------------------
		//Code generation for strongly typed access to resources
		//--------------------------
		var inputsResXCodeGen = monitor
			.Where(static x => x is { GenerationType: GenerationType.CodeGeneration } or { GenerationType: GenerationType.ResourceManager, GenerateCode: true });
		GenerateCodeFiles(context, inputsResXCodeGen);

		//--------------------------
		//IStringLocalizer service injection
		//Need nugets
		//	Microsoft.Extensions.DependencyInjection
		//	Microsoft.Extensions.Localization
		//	Microsoft.Extensions.Logging
		//--------------------------
		var inputResXLocalizer = monitor
			.Where(static x => x is { GenerationType: GenerationType.StringLocalizer })
			.Collect()
			.SelectMany(static (x, _) => x.GroupBy(f => f.LocalNamespace))
			.Select(static (x, _) => new GenFilesNamespace(x.Key, x.ToImmutableArray()));
		GenerateLocalizerRegister(context, inputResXLocalizer); // Generate one localizer per namespace+class combo
	}

	private static void GenerateResXFiles(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GenFileOptions> inputs)
	{
		var generator = new ResourceManagerGenerator();
		context.RegisterSourceOutput(inputs, (ctx, file) =>
		{
			try
			{
				var output = generator.Generate(file, ctx.CancellationToken);
				output.ErrorsAndWarnings.ForEach(ctx.ReportDiagnostic);
				ctx.AddSource(output.FileName, output.SourceCode);
			}
			catch (Exception e)
			{
				ctx.ReportDiagnostic(Diagnostic.Create(Analyser.FatalError, Location.None, $"ResXManager({file.ClassName})", e.Message));
			}
		});
	}

	private static void GenerateCodeFiles(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GenFileOptions> inputs)
	{
		var generator = new CodeGenerator();
		context.RegisterSourceOutput(inputs, (ctx, file) =>
		{
			try
			{
				var output = generator.Generate(file, ctx.CancellationToken);
				output.ErrorsAndWarnings.ForEach(ctx.ReportDiagnostic);
				ctx.AddSource(output.FileName, output.SourceCode);
			}
			catch (Exception e)
			{
				ctx.ReportDiagnostic(Diagnostic.Create(Analyser.FatalError, Location.None, $"ResXManager({file.ClassName})", e.Message));
			}
		});

		var inputsCombos = inputs.Select(static (x, _) => x.GroupedFile);
		GenerateResXCombos(context, inputsCombos);
	}

	private static void GenerateResXCombos(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GroupedAdditionalFile> monitor)
	{
		var generator = new ComboGenerator();
		var detectAllCombosOfResx = monitor
			.Collect()
			.SelectMany((x, _) => GroupResxFiles.DetectChildCombos(x));
		context.RegisterSourceOutput(detectAllCombosOfResx, (ctx, combo) =>
		{
			try
			{
				var output = generator.Generate(combo, ctx.CancellationToken);
				output.ErrorsAndWarnings.ForEach(ctx.ReportDiagnostic);
				ctx.AddSource(output.FileName, output.SourceCode);
			}
			catch (Exception e)
			{
				ctx.ReportDiagnostic(Diagnostic.Create(Analyser.FatalError, Location.None, generator.GeneratedFileName, e.Message));
			}
		});
	}

	private static void GenerateLocalizerRegister(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GenFilesNamespace> inputs)
	{
		var generator = new LocalizerRegisterGenerator();

		context.RegisterSourceOutput(inputs, (ctx, ns) =>
		{
			try
			{
				var output = generator.Generate(ns, ctx.CancellationToken);
				output.ErrorsAndWarnings.ForEach(ctx.ReportDiagnostic);
				ctx.AddSource(output.FileName, output.SourceCode);
			}
			catch (Exception e)
			{
				ctx.ReportDiagnostic(Diagnostic.Create(Analyser.FatalError, Location.None, "Registration of localizers", e.Message));
			}
		});

		GenerateLocalizerResXClasses(context, inputs);
		GenerateLocalizerGlobalRegister(context, inputs);
	}

	private static void GenerateLocalizerResXClasses(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GenFilesNamespace> inputs)
	{
		var generator = new LocalizerGenerator();

		var resxFiles = inputs
			.SelectMany(static (x, _) => x.Files.GroupBy(f => f.ClassName))
			.Select(static (x, _) => x.First());

		context.RegisterSourceOutput(resxFiles, (ctx, file) =>
		{
			try
			{
				var output = generator.Generate(file, ctx.CancellationToken);
				output.ErrorsAndWarnings.ForEach(ctx.ReportDiagnostic);
				ctx.AddSource(output.FileName, output.SourceCode);
			}
			catch (Exception e)
			{
				ctx.ReportDiagnostic(Diagnostic.Create(Analyser.FatalError, Location.None, $"Error while generating class for {file.ClassName}", e.Message));
			}
		});
	}

	private static void GenerateLocalizerGlobalRegister(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GenFilesNamespace> inputs)
	{
		var globalGenerator = new LocalizerGlobalRegisterGenerator();
		var global = inputs.Collect();

		context.RegisterSourceOutput(global, (ctx, gns) =>
		{
			if (gns.Length == 0) return;
			try
			{
				var output = globalGenerator.Generate(gns, ctx.CancellationToken);
				ctx.AddSource(output.FileName, output.SourceCode);
			}
			catch (Exception e)
			{
				ctx.ReportDiagnostic(Diagnostic.Create(Analyser.FatalError, Location.None, " Global localizer registration", e.Message));
			}
		});
	}
}
