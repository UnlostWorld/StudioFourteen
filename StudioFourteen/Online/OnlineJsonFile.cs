namespace StudioFourteen.Online;

using StudioFourteen.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;

public class OnlineJsonFile<T> : OnlineFile
{
	private T? data;

	public OnlineJsonFile(string url, TimeSpan updateFrequency)
		: base(url, updateFrequency)
	{
	}

	public OnlineJsonFile(string url, int version = 1)
		: base(url, version)
	{
	}

	public async Task<T> GetAsync()
	{
		if (this.CurrentState == States.None || this.data == null)
		{
			using FileStream file = await this.GetFileAsync();
			using StreamReader streamReader = new(file);
			string json = await streamReader.ReadToEndAsync();
			T? obj = Serializer.Deserialize<T>(json);

			if (obj == null)
				throw new Exception($"Failed to deserialize online file {this.Url} to type {typeof(T)}");

			this.data = obj;
		}

		return this.data;
	}
}
