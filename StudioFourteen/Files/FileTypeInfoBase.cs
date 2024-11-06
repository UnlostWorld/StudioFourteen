namespace StudioFourteen.Files;

using Serilog;
using System;
using System.IO;

public abstract class FileTypeInfoBase
{
	public ILogger Log => Logging.ForContext(this.GetType());

	public abstract string Extension { get; }
	public abstract string TypeName { get; }
	public abstract Type LoadsType { get; }

	public abstract FileBase? Load(FileInfo fileInfo);
}

public abstract class JsonFileTypeInfoBase<T> : FileTypeInfoBase
	where T : FileBase, new()
{
	public override Type LoadsType => typeof(T);

	public override FileBase? Load(FileInfo fileInfo)
	{
		if (!fileInfo.Exists)
			return null;

		string json = File.ReadAllText(fileInfo.FullName);
		return Serialization.Serializer.Deserialize<T>(json);
	}
}