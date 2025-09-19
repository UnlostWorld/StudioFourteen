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

namespace StudioFourteen.ResourcePacks;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Services;
using WpfUtils.Extensions;

public partial class ResourcePackService : ServiceBase
{
	public List<ResourcePackReference> Packs = new();

	public override Task Start()
	{
		List<string> resourcePackPaths = this.Services.Content.GetContents("ResourcePacks");
		foreach (string path in resourcePackPaths)
		{
			if (!path.EndsWith(".xaml"))
				continue;

			ResourceDictionary pack = new();
			pack.Source = new(path);

			string? fileName = Path.GetFileName(path);
			string? packName = pack["PackName"] as string;
			string? packAuthor = pack["PackAuthor"] as string;
			string? packIcon = pack["PackIcon"] as string;

			if (fileName == null || (packName == null && packAuthor == null))
				continue;

			string iconPath = $"{Path.GetDirectoryName(path)}/{packIcon}";

			this.Packs.Add(new(path, fileName, packName ?? fileName, packAuthor, iconPath));
		}

		foreach (ResourcePackReference pack in this.Packs)
		{
			if (pack.Enabled)
			{
				Resources.MergeDictionary(new(pack.Path));
			}
		}

		return base.Start();
	}

	public async Task Apply()
	{
		try
		{
			foreach (ResourcePackReference pack in this.Packs)
			{
				Resources.UnMergeDictionary(new(pack.Path));

				if (pack.Enabled)
				{
					Resources.MergeDictionary(new(pack.Path));
				}
			}
		}
		catch (TaskCanceledException)
		{
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error applying resource packs");
		}

		await this.Services.Panels.RestartPanels();
	}
}

public class ResourcePackReference(string path, string fileName, string name, string? author, string? iconPath)
{
	public string Path { get; init; } = path;
	public string FileName { get; init; } = fileName;
	public string Name { get; init; } = name;
	public string? Author { get; init; } = author;
	public string? IconPath { get; init; } = iconPath;

	public bool Enabled
	{
		get => ServiceManager.Instance.Settings.Current.ResourcePacks.Contains(this.FileName);
		set
		{
			if (value == ServiceManager.Instance.Settings.Current.ResourcePacks.Contains(this.FileName))
				return;

			if (value)
			{
				ServiceManager.Instance.Settings.Current.ResourcePacks.Add(this.FileName);
			}
			else
			{
				ServiceManager.Instance.Settings.Current.ResourcePacks.Remove(this.FileName);
			}
		}
	}
}