// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Scripting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Serilog.Events;
using StudioFourteen.Panels;
using StudioFourteen.Plugin;
using StudioFourteen.Scripting.Instance;
using StudioFourteen.Services;
using StudioFourteen.Utils;
using WpfUtils.Extensions;

public class ScriptingService : ServiceBase
{
	private readonly HashSet<string> allowedNamespaces = new()
	{
		"StudioFourteen.Scripting.Instance",
		"StudioFourteen.GameData.Library",
		"StudioFourteen.Library.Sources",
		"System.Threading.Tasks",
		"System.Collections.Generic",
	};

	private readonly HashSet<string> allowedTypes = new()
	{
		"?",
		"bool",
		"byte",
		"float",
		"int",
		"string",
		"sbyte",
		"StudioFourteen.GameData.Genders",
		"System.Runtime.CompilerServices.YieldAwaitable",
		"uint",
		"void",
	};

	private bool isRunningScript = false;
	private AssemblyLoadContext? loadContext;

	public override Task Start()
	{
		this.loadContext = PluginManager.GetLocalPlugin().LoadContext;
		return base.Start();
	}

	public void RunScript(ScriptFile script)
	{
		if (this.isRunningScript)
			return;

		this.RunScriptAsync(script).Run();
	}

	public bool GetIsScriptTrusted(ScriptFile file)
	{
		if (this.Settings.TrustedScripts.TryGetValue(file.Info.FullName, out string? hash))
		{
			if (hash == "Always")
				return true;

			return file.Hash == hash;
		}

		return false;
	}

	public void SetScriptTrusted(ScriptFile file, bool always)
	{
		this.Settings.TrustedScripts[file.Info.FullName] = file.Hash;

		if (always)
		{
			this.Settings.TrustedScripts[file.Info.FullName] = "Always";
		}
	}

	public async Task RunScriptAsync(ScriptFile script)
	{
		this.isRunningScript = true;

		ScriptPanel? panel = await ScriptPanel.Show(script);
		if (panel == null)
			return;

		try
		{
			panel.SetTitle(script.Title ?? string.Empty);

			if (script.HasErrors)
				throw new Exception("Script parsing failed");

			Assembly? assembly = script.Assembly;
			if (assembly == null)
			{
				panel.SetStatus($"Compiling Script: {script.Title}");
				await this.CompileScript(script);
				assembly = script.Assembly;
			}

			if (script.HasErrors)
				throw new Exception("Script failed to compile");

			bool trust = this.GetIsScriptTrusted(script);
			if (!trust)
			{
				trust = await panel.CheckTrust();
				this.SetScriptTrusted(script, panel.AlwaysTrust);
			}

			if (trust)
			{
				panel.SetStatus($"Configure Script: {script.Title}");
				Dictionary<string, object>? options = await panel.Configure();
				if (options != null)
				{
					panel.SetStatus($"Running Script: {script.Title}");
					await this.RunScript(script, panel, options);

					panel.SetProgress(1.0);
					panel.SetStatus($"Completed Script: {script.Title}");
				}
			}
		}
		catch (OperationCanceledException)
		{
			panel.SetProgress(0);
			panel.SetStatus($"Canceled");
			panel.IsRunning = false;
		}
		catch (Exception ex)
		{
			panel.SetStatus($"Error in script");
			panel.AppendLog(LogEventLevel.Error, ex.Message);

#if DEBUG
			if (ex.StackTrace != null)
				panel.AppendLog(LogEventLevel.Debug, ex.StackTrace);
#endif

			foreach (DiagnosticEntry diagnostic in script.Diagnostics)
			{
				panel.AppendLog(diagnostic.Level, diagnostic.Message, diagnostic.Location);
			}

			panel.SetProgress(0.0);
		}

		this.isRunningScript = false;
	}

	private async Task CompileScript(ScriptFile file)
	{
		if (this.loadContext == null)
			throw new Exception("No assembly load context");

		StringBuilder textBuilder = new();
		textBuilder.Append("using StudioFourteen.Scripting.Instance;");
		textBuilder.Append("public class ScriptMain : ScriptBase");
		textBuilder.Append("{");
		textBuilder.Append($"public async System.Threading.Tasks.Task _InternalScriptRun()");
		textBuilder.Append("{");
		textBuilder.AppendLine("await System.Threading.Tasks.Task.Yield();");

		textBuilder.AppendLine(file.Code);

		textBuilder.Append("}");
		textBuilder.Append("}");

		SourceText codeString = SourceText.From(textBuilder.ToString());
		CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12);
		SyntaxTree parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, parseOptions);

		CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary);
		options = options.WithAllowUnsafe(false);
		options = options.WithOverflowChecks(true);
		options = options.WithDeterministic(true);

		string? runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
		if (runtimePath == null)
			throw new Exception("Unable to get system runtime directory");

		CSharpCompilation compilation = CSharpCompilation.Create($"{file.Info.Name}_{file.Hash}", [parsedSyntaxTree], [], options);
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile("C:/Projects/StudioFourteen/StudioFourteen/bin/StudioFourteen.dll"));
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile($"{runtimePath}/System.Private.CoreLib.dll"));
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile($"{runtimePath}/System.Runtime.dll"));
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile($"{runtimePath}/System.Collections.dll"));

		SemanticModel semanticModel = compilation.GetSemanticModel(parsedSyntaxTree);
		SyntaxNode root = await parsedSyntaxTree.GetRootAsync();
		IEnumerable<SyntaxNode> nodes = root.DescendantNodes(descendIntoChildren => true);
		foreach (SyntaxNode node in nodes)
		{
			// Check Types
			if (node is IdentifierNameSyntax nameSyntax)
			{
				ISymbol? symbol = semanticModel.GetSymbolInfo(nameSyntax).Symbol;
				if (!this.IsSymbolAllowed(symbol))
				{
					throw new Exception($"Illegal symbol: {symbol} in name: \"{nameSyntax}\"");
				}
			}
			else if (node is ExpressionSyntax expressionSyntax)
			{
				ITypeSymbol? symbol = semanticModel.GetTypeInfo(expressionSyntax).Type;
				if (!this.IsSymbolAllowed(symbol))
				{
					throw new Exception($"Illegal symbol: {symbol} in expression: \"{expressionSyntax}\"");
				}
			}
		}

		using var peStream = new MemoryStream();
		using var pdbStream = new MemoryStream();
		EmitResult result = compilation.Emit(peStream, pdbStream);

		foreach (Diagnostic diagnostic in result.Diagnostics)
		{
			if (diagnostic.Severity == DiagnosticSeverity.Hidden)
				continue;

			string? location = null;
			int line = diagnostic.Location.GetLineSpan().StartLinePosition.Line;
			if (line > 0)
				location = $"Line {line - 1}";

			LogEventLevel level = diagnostic.Severity switch
			{
				DiagnosticSeverity.Info => LogEventLevel.Information,
				DiagnosticSeverity.Warning => LogEventLevel.Warning,
				DiagnosticSeverity.Error => LogEventLevel.Error,
				_ => throw new InvalidOperationException(),
			};

			file.Diagnostics.Add(new(level, diagnostic.GetMessage(), location));
		}

		if (!result.Success)
		{
			file.HasErrors = true;
			return;
		}

		peStream.Seek(0, SeekOrigin.Begin);
		pdbStream.Seek(0, SeekOrigin.Begin);

		file.Assembly = this.loadContext.LoadFromStream(peStream, pdbStream);
	}

	private bool IsSymbolAllowed(ISymbol? symbol)
	{
		if (symbol == null)
			return true;

		if (symbol is ITypeSymbol typeSymbol)
		{
			string? symbolName = typeSymbol.ToString();
			if (symbolName == null)
				return false;

			if (symbolName.EndsWith("[]"))
				symbolName = symbolName.Substring(0, symbolName.Length - 2);

			if (this.allowedTypes.Contains(symbolName))
				return true;

			string? namespaceName = symbol?.ContainingNamespace?.ToString();
			if (namespaceName == null)
				return false;

			if (this.allowedNamespaces.Contains(namespaceName))
				return true;

			return false;
		}

		return true;
	}

	private async Task RunScript(ScriptFile file, ScriptPanel panel, Dictionary<string, object> options)
	{
		if (file.Assembly == null)
			throw new InvalidOperationException("Attempt to run a script that is not compiled");

		Type[] types = file.Assembly.GetTypes();
		Type? scriptType = null;
		foreach (Type type in types)
		{
			if (type.BaseType == typeof(ScriptBase))
			{
				scriptType = type;
			}
		}

		if (scriptType == null)
			throw new Exception("Failed to find ScriptBase type in script assembly");

		ScriptBase? script = Activator.CreateInstance(scriptType, []) as ScriptBase;
		if (script == null)
			throw new Exception("Failed to create instance of script");

		script.CancellationToken = file.GetCancellationToken();

		// Initialize script services
		PropertyInfo[] properties = typeof(ScriptBase).GetProperties(BindingFlags.Public | BindingFlags.Instance);
		foreach (PropertyInfo property in properties)
		{
			if (property.PropertyType.BaseType == typeof(ScriptServiceBase))
			{
				ScriptServiceBase? service = property.GetValue(script) as ScriptServiceBase;
				if (service == null)
					continue;

				service.File = file;
				service.Panel = panel;
				service.CancellationToken = file.GetCancellationToken();
			}
		}

		script.Options.Options = options;

		MethodInfo? runMethodInfo = scriptType.GetMethod("_InternalScriptRun");
		if (runMethodInfo == null)
			throw new Exception("Failed to locate entry point in script");

		panel.IsRunning = true;

		object? entryReturn = runMethodInfo.Invoke(script, []);
		Task? task = entryReturn as Task;
		if (task != null)
			await task;

		panel.IsRunning = false;
	}
}