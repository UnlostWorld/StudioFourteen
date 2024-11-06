// Mare
// https://github.com/Penumbra-Sync/client/blob/main/MareSynchronos/PlayerData/Export/MareCharaFileData.cs
// https://github.com/Penumbra-Sync/client/blob/main/MareSynchronos/PlayerData/Export/MareCharaFileHeader.cs

// Glamourer
// https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Designs/DesignConverter.cs
namespace StudioFourteen.Files;

using Dalamud.Game.ClientState.Objects.Types;
using LZ4;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudioFourteen.Appearance;
using StudioFourteen.Library.Sources;
using StudioFourteen.Mvm.Commands;
using StudioFourteen.Plugin;
using StudioFourteen.Serialization;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
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
		file.Name = fileInfo.Name;

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
	public MareFile()
	{
		this.ApplyCommand = new TargetCommand(this.Apply);
		this.RevertCommand = new RevertTargetAppearanceCommand();
	}

	[JsonIgnore] public string? FilePath { get; set; }
	[JsonIgnore] public GlamourerDesign? Design { get; set; }

	[JsonIgnore] public ICommand ApplyCommand { get; init; }
	[JsonIgnore] public ICommand RevertCommand { get; init; }

	public string GlamourerData { get; set; } = string.Empty;
	public string? Name { get; set; }

	public async Task Apply(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		if (this.FilePath == null || DalamudServices.ObjectTable == null)
			return;

		IGameObject? target = DalamudServices.ObjectTable[objectTableIndex];
		if (target == null)
			return;

		ServiceManager.Instance.IPC.MareSynchronosLoadMcdf(this.FilePath, target);
	}

	public override void GetAutoTags(TagCollection tags)
	{
		this.Design?.GetAutoTags(tags);
	}
}