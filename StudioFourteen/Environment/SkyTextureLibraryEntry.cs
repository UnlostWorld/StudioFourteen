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

namespace StudioFourteen.Environment;

using System;
using System.Threading.Tasks;
using StudioFourteen.GameData;
using StudioFourteen.Library;
using StudioFourteen.Library.Sources;

public class SkyTextureLibraryEntry(uint id, string path, SkyTextureSource source)
	: LibraryEntryBase(source)
{
	public uint SkyTexId => id;

	public override string? Name => $"Sky {id}";
	public override string? SubTitle => null;
	public override IComparable DefaultSortValue => id;
	public override object? Icon => new ImageReference(path);

	public override Task Execute()
	{
		this.Services.Environment.CurrentState.FreezeSkyTexture = true;
		this.Services.Environment.CurrentState.SkyTexture = this;
		return Task.CompletedTask;
	}

	protected override string GetInternalId() => $"SkyTex{id}";
}

public class SkyTextureSource : SourceBase
{
	public override string? Name => "Sky Textures";

	public SkyTextureLibraryEntry? Get(uint id)
	{
		foreach(SkyTextureLibraryEntry entry in this.AllEntries)
		{
			if (entry.SkyTexId == id)
			{
				return entry;
			}
		}

		return null;
	}

	protected override string GetInternalId() => "SkyTex";

	protected override void Scan()
	{
		for (uint i = 0; i < 1000; i++)
		{
			string path = $"bgcommon/nature/sky/texture/sky_{i:000}.tex";
			if (!ServiceManager.Instance.GameData.GetFileExists(path))
				continue;

			this.Add(new SkyTextureLibraryEntry(i, path, this));
		}
	}
}