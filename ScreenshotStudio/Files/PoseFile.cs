namespace ScreenshotStudio.Files;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Posing;
using System;

public class PoseFileTypeInfo : JsonFileTypeInfoBase<PoseFile>
{
	public override string Extension => ".pose";
	public override string TypeName => "Pose";
}

[Serializable]
public class PoseFile : FileBase, IPose
{
	public unsafe void Apply(Character* character)
	{
	}
}