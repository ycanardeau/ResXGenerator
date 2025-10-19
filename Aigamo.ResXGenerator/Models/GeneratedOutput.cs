using Microsoft.CodeAnalysis;

namespace Aigamo.ResXGenerator.Models;

public record GeneratedOutput(string FileName, string SourceCode, IEnumerable<Diagnostic> ErrorsAndWarnings);
