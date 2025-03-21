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

// Glamourer
// https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Designs/DesignConverter.cs
namespace StudioFourteen.Files;

using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FontAwesome.Sharp;
using LZ4;
using Newtonsoft.Json;
using StudioFourteen.Appearance;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Plugin;
using StudioFourteen.Serialization;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

public class MareFileTypeInfo : FileTypeInfoBase
{
	public override string Extension => ".mcdf";
	public override string TypeName => "Mare Character Data File";

	public override Type LoadsType => typeof(MareFile);

	public override FileBase? Load(FileInfo fileInfo)
	{
		using FileStream unwrapped = File.OpenRead(fileInfo.FullName);
		using LZ4Stream lz4Stream = new LZ4Stream(unwrapped, LZ4StreamMode.Decompress, LZ4StreamFlags.HighCompression);
		using BinaryReader reader = new BinaryReader(lz4Stream);

		string chars = new string(reader.ReadChars(4));
		if (!string.Equals(chars, "MCDF", StringComparison.Ordinal))
			throw new InvalidDataException("Not a Mare Chara File");

		byte version = reader.ReadByte();
		if (version != 1)
			throw new Exception();

		int dataLength = reader.ReadInt32();
		byte[] data = reader.ReadBytes(dataLength);
		string json = Encoding.UTF8.GetString(data);

		MareFile? file = Serializer.Deserialize<MareFile>(json);
		if (file == null)
			return null;

		file.FilePath = fileInfo.FullName;
		file.Name = Path.GetFileNameWithoutExtension(fileInfo.Name);

		try
		{
			byte[] compressed = Convert.FromBase64String(file.GlamourerData);
			byte glamourerVersion = compressed[0];

			if (glamourerVersion == 5)
			{
				const int Base64SizeV4 = 95;
				compressed = compressed[Base64SizeV4..];
				glamourerVersion = 6;
			}

			if (glamourerVersion == 3 || glamourerVersion == 6)
			{
				using MemoryStream compressedStream = new MemoryStream(compressed, 1, compressed.Length - 1);
				using GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
				using MemoryStream resultStream = new MemoryStream();
				zipStream.CopyTo(resultStream);
				byte[] decompressed = resultStream.ToArray();

				string glamourerJson = Encoding.UTF8.GetString(decompressed);
				file.Design = Serializer.Deserialize<GlamourerDesign>(glamourerJson);
			}
		}
		catch (Exception ex)
		{
			this.Log.Warning(ex, $"Failed to read glamourer data from mcdf: {fileInfo.Name}");
		}

		return file;
	}
}

public class MareFile
	: FileBase, ICharacterAppearance
{
	[JsonIgnore] public string? FilePath { get; set; }
	[JsonIgnore] public GlamourerDesign? Design { get; set; }

	public string GlamourerData { get; set; } = string.Empty;
	public string? Name { get; set; }

	[LibraryMenu(IconChar.Plus, "LOC_AppearanceCreateCharacter")]
	public Task Spawn()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	[LibraryMenuTarget(IconChar.UserShield, "LOC_AppearanceApplyTo")]
	public async Task Apply(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		if (this.FilePath == null || DalamudServices.ObjectTable == null)
			return;

		IGameObject? target = DalamudServices.ObjectTable[objectTableIndex];
		if (target == null)
			return;

		if (this.Name != null)
		{
			unsafe
			{
				Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
				character->SetDisplayName(this.Name);
			}
		}

		// The async version of LoadMcdf has some issues, but if we try to load two mcdf's
		// too close together it doesn't work, so just delay for a while.
		bool success = ServiceManager.Instance.IPC.MareSynchronosLoadMcdf(this.FilePath, target);
		await Task.Delay(3000);
	}

	public override void GetAutoTags(TagCollection tags)
	{
		tags.Add("MCDF").WithAlias("Mare Character");
		tags.Add("Named");
		this.Design?.GetAutoTags(tags);
	}
}