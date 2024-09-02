namespace ScreenshotStudio.Files;

using ScreenshotStudio.Commands;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

public class SceneFileTypeInfo : JsonFileTypeInfoBase<SceneFile>
{
	public override string Extension => ".studio";
	public override string TypeName => "Studio Scene";
}

[Serializable]
public class SceneFile : FileBase
{
	public SceneFile()
	{
		this.ApplyCommand = new AutoCommand(this.Apply);
		this.RevertCommand = new AutoCommand(this.Revert);
	}

	[JsonIgnore] public ICommand ApplyCommand { get; init; }
	[JsonIgnore] public ICommand RevertCommand { get; init; }

	public string Guid { get; set; } = System.Guid.NewGuid().ToString();

	// TODO
	////public string Location { get; set; }
	////public string TimeOfDay { get; set; }
	////public string Weather { get; set; }
	public List<Actor> Actors { get; set; } = new();

	public Task Apply() => ServiceManager.Instance.Save.OpenAsync(this);
	public Task Revert() => ServiceManager.Instance.Save.Revert(this);

	public class Actor
	{
		public Actor()
		{
			this.ApplyCommand = new TargetCommand(this.Apply);
		}

		public string? Role { get; set; }
		public PoseFile? Pose { get; set; }
		public AppearanceFile? Appearance { get; set; }

		[JsonIgnore] public ICommand? ApplyCommand { get; init; }
		[JsonIgnore] public ICommand? ApplyPoseCommand => this.Pose?.ApplyCommand;
		[JsonIgnore] public ICommand? ApplyAppearanceCommand => this.Appearance?.ApplyCommand;

		public async Task Apply(int objectTableIndex)
		{
			if (this.Pose != null)
			{
				await this.Pose.Apply(objectTableIndex);
			}

			if (this.Appearance != null)
			{
				await this.Appearance.Apply(objectTableIndex);
			}
		}
	}
}
