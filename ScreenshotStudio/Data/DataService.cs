// Brio
// https://github.com/Etheirys/Brio/blob/main/Brio/Resources/ResourceProvider.cs

namespace ScreenshotStudio.Data;

using ScreenshotStudio.Serialization;
using ScreenshotStudio.Services;
using ScreenshotStudio.Studio.Pose;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class DataService : ServiceBase
{
	public const string NpcNamesIdFormat = "D7";
	public static Dictionary<string, string>? NpcNames { get; set; }
	public static Dictionary<string, PoseViewDefinition>? PoseViews { get; set; }

	public override Task Initialize()
	{
		NpcNames = this.GetResourceDocument<Dictionary<string, string>>("NpcNames");
		PoseViews = this.GetResourceDocument<Dictionary<string, PoseViewDefinition>>("Bones");

		return base.Initialize();
	}

	private Stream? GetRawResourceStream(string name)
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		string resourceName = $"ScreenshotStudio.Data.Documents.{name}.json";
		Stream? stream = assembly.GetManifestResourceStream(resourceName);

		if (stream == null)
			this.Log.Warning($"Embedded data document: {resourceName} not found ");

		return stream;
	}

	private T? GetResourceDocument<T>(string name)
		where T : class
	{
		using Stream? stream = this.GetRawResourceStream(name);

		if (stream == null)
			return null;

		using StreamReader reader = new StreamReader(stream);
		string txt = reader.ReadToEnd();
		T? document = Serializer.Deserialize<T>(txt);
		return document;
	}
}
