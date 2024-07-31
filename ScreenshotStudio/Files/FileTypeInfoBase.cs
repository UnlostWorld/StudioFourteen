namespace ScreenshotStudio.Files;

using System.IO;

public abstract class FileTypeInfoBase
{
	public abstract string Extension { get; }
	public abstract string TypeName { get; }

	public abstract FileBase? Load(FileInfo fileInfo);
}

public abstract class JsonFileTypeInfoBase<T> : FileTypeInfoBase
	where T : FileBase, new()
{
	public override FileBase? Load(FileInfo fileInfo)
	{
		if (!fileInfo.Exists)
			return null;

		string json = File.ReadAllText(fileInfo.FullName);
		return Serialization.Serializer.Deserialize<T>(json);
	}
}