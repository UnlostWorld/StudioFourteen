namespace ScreenshotStudio.Files;

using ScreenshotStudio.Services;
using ScreenshotStudio.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class FileThumbnailService : ServiceBase
{
	private readonly ConcurrentQueue<ThumbnailRequest> requests = new();

	public override Task Start()
	{
		Thread generatorThread = new(new ThreadStart(this.ThumbnailGeneratorThread));
		generatorThread.Start();

		return base.Start();
	}

	public void GetThumbnail(FileInfo fileInfo, Action<string> callback)
	{
		string name = this.HashName(fileInfo);
		string dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/ScreenshotStudio/Thumbnails/";

		if (!Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		string path = $"{dir}{name}.png";

		if (File.Exists(path))
		{
			callback.Invoke(path);
		}
		else
		{
			this.requests.Enqueue(new(fileInfo, callback, path));
		}
	}

	private string HashName(FileInfo fileInfo) => HashUtility.GetHashString(fileInfo.FullName);

	private void ThumbnailGeneratorThread()
	{
		while(this.IsAlive)
		{
			if (this.requests.Count <= 0)
			{
				Thread.Sleep(100);
				continue;
			}

			if (!this.requests.TryDequeue(out ThumbnailRequest request))
				continue;

			try
			{
				this.ProcessRequest(request);
			}
			catch (Exception ex)
			{
				this.Log.Warning(ex, "Failed to generate thumbnail");
			}
		}
	}

	private void ProcessRequest(ThumbnailRequest request)
	{
		FileTypeInfoBase? typeInfo = FileTypes.GetTypeInfo(request.FileInfo);
		if (typeInfo == null)
			throw new Exception($"No File Type Info for file: {request.FileInfo}");

		FileBase? file = typeInfo.Load(request.FileInfo);
		if (file == null)
			throw new Exception($"Failed to load file: {request.FileInfo}");

		if (file.Base64Image == null)
			return;

		byte[] binaryData = Convert.FromBase64String(file.Base64Image);

		using (Image image = Image.Load(binaryData))
		{
			ResizeOptions op = new();
			op.Mode = ResizeMode.Max;
			op.Size = new(128, 128);
			image.Mutate(x => x.Resize(op));

			image.SaveAsPng(request.ThumbnailPath);
		}

		request.Callback.Invoke(request.ThumbnailPath);
	}

	public struct ThumbnailRequest(FileInfo fileInfo, Action<string> callback, string path)
	{
		public readonly FileInfo FileInfo = fileInfo;
		public readonly Action<string> Callback = callback;
		public readonly string ThumbnailPath = path;
	}
}
