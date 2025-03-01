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
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using StudioFourteen.Plugin;
using StudioFourteen.Scripting.Instance;
using StudioFourteen.Services;
using StudioFourteen.Studio;
using WpfUtils.Extensions;

public class ScriptingService : ServiceBase
{
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

		LongTaskWindow? ltw = await LongTaskWindow.Show();
		if (ltw == null)
			return;

		try
		{
			ltw.SetStatus($"Compiling Script: {script.Name}");
			Assembly assembly = this.CompileScript(script);

			ltw.SetStatus($"Running Script: {script.Name}");
			await this.RunScript(assembly);

			ltw.SetStatus($"Completed Script: {script.Name}");
			await Task.Delay(1000);
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, $"Error compiling script file: {ex.Message}");
		}

		this.isRunningScript = false;
		ltw.Close();
	}

	private Assembly CompileScript(ScriptFile file)
	{
		if (this.loadContext == null)
			throw new Exception("No assembly load context");

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

		CSharpCompilation compilation = CSharpCompilation.Create(this.Name, [parsedSyntaxTree], [], options);
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile("C:/Projects/StudioFourteen/StudioFourteen/bin/StudioFourteen.dll"));
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile($"{runtimePath}/System.Private.CoreLib.dll"));
		compilation = compilation.AddReferences(MetadataReference.CreateFromFile($"{runtimePath}/System.Runtime.dll"));

		using var peStream = new MemoryStream();
		using var pdbStream = new MemoryStream();
		EmitResult result = compilation.Emit(peStream, pdbStream);

		if (!result.Success)
		{
			foreach(Diagnostic diagnostic in result.Diagnostics)
			{
				this.Log.Information($"[{file.Name}][{diagnostic.Severity}] {diagnostic.GetMessage()}");
			}

			throw new Exception("Failed to compile script");
		}

		peStream.Seek(0, SeekOrigin.Begin);
		pdbStream.Seek(0, SeekOrigin.Begin);

		return this.loadContext.LoadFromStream(peStream, pdbStream);
	}

	private async Task RunScript(Assembly assembly)
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