using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Aigamo.ResXGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Analyser : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
		DuplicateWarning,
		MemberSameAsClassWarning,
		MemberWithStaticError,
		LocalizerStaticError,
		LocalizerPartialError,
		LocalizationIncoherentNamespace,
		FatalError
	);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
		// We only use the DiagnosticDescriptor to report from the Source Generator
	}

	public static readonly DiagnosticDescriptor DuplicateWarning = new(
		id: "AigamoResXGenerator001",
		title: "Duplicate member",
		messageFormat: "Ignored added member '{0}'",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor MemberSameAsClassWarning = new(
		id: "AigamoResXGenerator002",
		title: "Member same name as class",
		messageFormat: "Ignored member '{0}' has same name as class",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor MemberWithStaticError = new(
		id: "AigamoResXGenerator003",
		title: "Incompatible settings",
		messageFormat: "Cannot have static members/class with an class instance",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor LocalizerStaticError = new(
		id: "AigamoResXGenerator004",
		title: "Incompatible settings",
		messageFormat: "When using StringLocalizer , the static option not available. Parameter ignored.",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor LocalizerPartialError = new(
		id: "AigamoResXGenerator005",
		title: "Incompatible settings",
		messageFormat: "When using StringLocalizer , the partial option not available. Parameter ignored.",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor LocalizationIncoherentNamespace = new(
		id: "AigamoResXGenerator006",
		title: "Incoherent namespace",
		messageFormat:
		"When using StringLocalizer, the namespace must be the same as the Resx file. Either remove the CustomToolNamespace or change the GenerationType to ResourceManager. Parameter ignored.",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true
	);

	public static DiagnosticDescriptor FatalError => new(
		id: "AigamoResXGenerator999",
		title: "Fatal Error generated",
		messageFormat: "An error occured on generation file {0} error {1}",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);
}
