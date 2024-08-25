namespace ScreenshotStudio.Files;

using System;
using System.Collections.Generic;

public class SceneFileTypeInfo : JsonFileTypeInfoBase<SceneFile>
{
	public override string Extension => ".studio";
	public override string TypeName => "Studio Scene";
}

[Serializable]
public class SceneFile : FileBase
{
	public string Guid { get; set; } = System.Guid.NewGuid().ToString();

	// TODO
	////public string Location { get; set; }
	////public string TimeOfDay { get; set; }
	////public string Weather { get; set; }
	public List<Actor> Actors { get; set; } = new();

	public class Actor
	{
		public string? Name { get; set; }

		public PoseFile? Pose { get; set; }
		public CharacterFile? Character { get; set; }
	}
}
