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
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Serilog.Events;
using StudioFourteen.Plugin;
using StudioFourteen.Scripting.Instance;
using StudioFourteen.Services;
using StudioFourteen.Studio;
using StudioFourteen.Utils;
using WpfUtils.Extensions;

public class ScriptingService : ServiceBase
{
	private readonly Dictionary<string, Assembly> assemblyCache = new();
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

	public async Task RunScriptAsync(ScriptFile script)
	{
		this.isRunningScript = true;

		ScriptPanel? panel = await ScriptPanel.Show();
		if (panel == null)
			return;

		try
		{
			panel.SetTitle(script.Name);

			panel.SetStatus($"Compiling Script: {script.Name}");
			Assembly assembly = this.CompileScript(script, panel);

			panel.SetStatus($"Running Script: {script.Name}");
			await this.RunScript(script, assembly, panel);

			panel.SetProgress(1.0);
			panel.SetStatus($"Completed Script: {script.Name}");
		}
		catch(Exception ex)
		{
			panel.SetStatus($"Error in script");
			panel.AppendLog(LogEventLevel.Error, ex.Message);
			panel.SetProgress(0.0);
		}

		this.isRunningScript = false;
	}

	private Assembly CompileScript(ScriptFile file, ScriptPanel panel)
	{
		if (this.loadContext == null)
			throw new Exception("No assembly load context");

		string hash = HashUtility.GetHashString(file.Text);
		if (this.assemblyCache.TryGetValue(hash, out var assembly))
			return assembly;

		string preText =
		@"using StudioFourteen.Scripting.Instance;
		using System.Threading.Tasks;
		public class MyScript : ScriptBase{public async Task _InternalScriptRun(){await Task.Yield();";

		string postText = "}}";

		SourceText codeString = SourceText.From(preText + file.Text + postText);
		CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12);
		SyntaxTree parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, parseOptions);
		CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary);
		options = options.WithAllowUnsafe(false);

		string? runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
		if (runtimePath == null)
			throw new Exception("Unable to get system runtime directory");

		CSharpCompilation compilation = CSharpCompilation.Create($"{this.Name}_{hash}", [parsedSyntaxTree], [], options);
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile("C:/Projects/StudioFourteen/StudioFourteen/bin/StudioFourteen.dll"));
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile($"{runtimePath}/System.Private.CoreLib.dll"));
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile($"{runtimePath}/System.Runtime.dll"));
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile($"{runtimePath}/System.Collections.dll"));

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

			panel.AppendLog(level, diagnostic.GetMessage(), location);
		}

		if (!result.Success)
			throw new Exception("Failed to compile script");

		peStream.Seek(0, SeekOrigin.Begin);
		pdbStream.Seek(0, SeekOrigin.Begin);

		Assembly newAssembly = this.loadContext.LoadFromStream(peStream, pdbStream);
		this.assemblyCache.Add(hash, newAssembly);
		return newAssembly;
	}

	private async Task RunScript(ScriptFile file, Assembly assembly, ScriptPanel panel)
	{
		Type[] types = assembly.GetTypes();
		Type? scriptType = null;
		foreach(Type type in types)
		{
			if (type.BaseType == typeof(ScriptBase))
			{
				scriptType = type;
			}
		}

		if (scriptType == null)
			throw new Exception("Failed to find ScriptBase type in script assembly");

		ScriptBase? script = Activator.CreateInstance(scriptType) as ScriptBase;
		if (script == null)
			throw new Exception("Failed to create instance of script");

		// Initialize script services
		PropertyInfo[] properties = typeof(ScriptBase).GetProperties(BindingFlags.Public | BindingFlags.Instance);
		foreach(PropertyInfo property in properties)
		{
			if (property.PropertyType.BaseType == typeof(ScriptServiceBase))
			{
				ScriptServiceBase? service = property.GetValue(script) as ScriptServiceBase;
				if (service == null)
					continue;

				service.File = file;
				service.Panel = panel;
			}
		}

		MethodInfo? runMethodInfo = scriptType.GetMethod("_InternalScriptRun");
		if (runMethodInfo == null)
			throw new Exception("Failed to locate entry point in script");

		object? entryReturn = runMethodInfo.Invoke(script, []);
		Task? task = entryReturn as Task;
		if (task != null)
		{
			await task;
		}
	}
}