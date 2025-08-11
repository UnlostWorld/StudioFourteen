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

namespace StudioFourteen.Content;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using StudioFourteen.Input;
using StudioFourteen.Plugin;
using StudioFourteen.Posing;
using StudioFourteen.Services;

public class ContentService : ServiceBase
{
	private readonly JsonContentReference<Dictionary<string, SimpleViewLayout>> simplePoseLayoutsContent = new("SimplePoseLayouts.jsonc");
	private readonly JsonContentReference<Dictionary<string, BlendTarget>> expressionBlends = new("ExpressionBlends.jsonc");
	private readonly JsonContentReference<HashSet<string>> genitalBones = new("GenitalBones.jsonc");
	private readonly Dictionary<string, HashSet<ContentReference>> references = new();

	#if DEBUG
	private bool isRunningFromProject = false;
	private FileSystemWatcher? watcher;
	#endif

	public Dictionary<string, SimpleViewLayout>? SimplePoseLayouts => this.simplePoseLayoutsContent.Get();
	public Dictionary<string, BlendTarget>? ExpressionBlends => this.expressionBlends.Get();
	public HashSet<string>? GenitalBones => this.genitalBones.Get();

	public override Task Initialize()
	{
		#if DEBUG
		{
			FileInfo? assembly = DalamudServices.PluginInterface?.AssemblyLocation;
			if (assembly != null)
			{
				string dir = Path.GetFullPath($"{assembly.DirectoryName}/../Content/Base/");
				this.isRunningFromProject = Directory.Exists(dir);

				if (this.isRunningFromProject)
				{
					this.watcher = new(dir);
					this.watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Attributes;
					this.watcher.IncludeSubdirectories = true;
					this.watcher.Changed += this.OnDirectoryChanged;
					this.watcher.EnableRaisingEvents = true;
				}
			}
		}
		#endif

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		#if DEBUG
		if (this.watcher != null)
		{
			this.watcher.Changed -= this.OnDirectoryChanged;
			this.watcher.EnableRaisingEvents = false;
			this.watcher.Dispose();
		}
		#endif

		return base.Shutdown();
	}

	public List<string> GetContents(string directoryPath)
	{
		string resolvedPath = this.ResolvePath(directoryPath);
		string[] paths = Directory.GetFiles(resolvedPath, "*.*", SearchOption.AllDirectories);
		return new List<string>(paths);
	}

	public Stream GetContent(ContentReference reference)
	{
		#if DEBUG
		lock (this.references)
		{
			string resolvedPath = this.ResolvePath(reference.Path);

			if (!this.references.ContainsKey(resolvedPath))
				this.references.Add(resolvedPath, new());

			this.references[resolvedPath].Add(reference);
		}
		#endif

		return this.GetContent(reference.Path);
	}

	public Stream GetContent(string path)
	{
		string resolvedPath = this.ResolvePath(path);

		FileStream? stream = null;
		for (int i = 0; i < 10; i++)
		{
			try
			{
				stream = new(resolvedPath, FileMode.Open, FileAccess.Read);
			}
			catch (IOException)
			{
				Thread.Sleep(10);
			}
		}

		if (stream == null)
			throw new Exception($"Content \"{resolvedPath}\" not found");

		return stream;
	}

	public string ResolvePath(string path)
	{
		FileInfo? assembly = DalamudServices.PluginInterface?.AssemblyLocation;
		if (assembly == null)
			return path;

		#if DEBUG
		if (this.isRunningFromProject && assembly.DirectoryName != null)
		{
			return Path.GetFullPath($"{assembly.DirectoryName}/../Content/Base/{path}");
		}
		#endif

		return Path.GetFullPath($"{assembly.DirectoryName}/Content/Base/{path}");
	}

	private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
	{
		lock(this.references)
		{
			if (this.references.TryGetValue(e.FullPath, out var references))
			{
				this.Log.Information($"Reloading file: {e.FullPath}");

				foreach(ContentReference reference in references)
				{
					reference.Reload();
				}
			}
		}
	}
}
