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

namespace StudioFourteen.Services.Library.Files;

using Lumina.Data.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StudioFourteen.Services.Images;
using StudioFourteen.Services.Library.Files;
using StudioFourteen.Services.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class FileThumbnails : IDisposable
{
	private readonly ConcurrentQueue<ThumbnailRequest> requests = new();
	private bool isDisposed = false;

	public FileThumbnails()
	{
		this.isDisposed = false;
		Thread generatorThread = new(new ThreadStart(this.ThumbnailGeneratorThread));
		generatorThread.Start();
	}

	public void Dispose()
	{
		this.isDisposed = true;
	}

	public void GetThumbnail(FileInfo fileInfo, Action<string> callback)
	{
		try
		{
			foreach (ThumbnailRequest otherRequest in this.requests)
			{
				if (otherRequest.FileInfo == fileInfo)
				{
					otherRequest.Callbacks.Add(callback);
					return;
				}
			}

			string name = this.HashName(fileInfo);
			string dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/StudioFourteen/Thumbnails/";

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			string path = $"{dir}{name}.png";

			if (File.Exists(path))
			{
				callback.Invoke(path);
			}
			else
			{
				ThumbnailRequest request = new();
				request.Type = ThumbnailRequest.RequestTypes.FileEmbeddedImage;
				request.FileInfo = fileInfo;
				request.AddCallback(callback);
				request.ThumbnailPath = path;
				this.requests.Enqueue(request);
			}
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Failed to generate thumbnail");
		}
	}

	public void GetThumbnailFromTexture(string texturePath, Action<string> callback)
	{
		try
		{
			foreach (ThumbnailRequest otherRequest in this.requests)
			{
				if (otherRequest.SourcePath == texturePath)
				{
					otherRequest.Callbacks.Add(callback);
					return;
				}
			}

			string name = HashUtility.GetHashString($"Tex:{texturePath}");
			string dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/StudioFourteen/Thumbnails/";

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			string path = $"{dir}{name}.png";

			if (File.Exists(path))
			{
				callback.Invoke(path);
			}
			else
			{
				ThumbnailRequest request = new();
				request.Type = ThumbnailRequest.RequestTypes.GameTexture;
				request.SourcePath = texturePath;
				request.AddCallback(callback);
				request.ThumbnailPath = path;
				this.requests.Enqueue(request);
			}
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Failed to generate thumbnail");
		}
	}

	private string HashName(FileInfo fileInfo) => HashUtility.GetHashString(fileInfo.FullName);

	private void ThumbnailGeneratorThread()
	{
		while (!this.isDisposed)
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
				Studio.Log.Warning(ex, "Failed to generate thumbnail");
			}
		}
	}

	private void ProcessRequest(ThumbnailRequest request)
	{
		if (request.ThumbnailPath == null)
			throw new Exception("No thumbnail path in request");

		Image? image = null;
		if (request.Type == ThumbnailRequest.RequestTypes.FileEmbeddedImage)
		{
			if (request.FileInfo == null)
				throw new Exception("No file info in thumbnail request");

			FileTypeInfoBase? typeInfo = Studio.Library.FileTypes.GetTypeInfo(request.FileInfo);
			if (typeInfo == null)
				throw new Exception($"No File Type Info for file: {request.FileInfo}");

			FileBase? file = typeInfo.Load(request.FileInfo);
			if (file == null)
				throw new Exception($"Failed to load file: {request.FileInfo}");

			if (file.Base64Image == null)
				return;

			byte[] binaryData = Convert.FromBase64String(file.Base64Image);
			image = Image.Load<Rgba32>(binaryData);
		}
		else if (request.Type == ThumbnailRequest.RequestTypes.GameTexture)
		{
			if (request.SourcePath == null)
				throw new Exception("No source file in thumbnail request");

			/*TexFile? tex = this.Services.GameData.GetFile<TexFile>(request.SourcePath);
			if (tex == null)
				throw new Exception("Failed to get source texture");

			image = Image.LoadPixelData<Bgra32>(tex.ImageData, tex.Header.Width, tex.Header.Height);*/
		}

		if (image == null)
			throw new Exception("failed to get source image");

		PngEncoder encoder = new()
		{
			ColorType = PngColorType.RgbWithAlpha,
			TransparentColorMode = PngTransparentColorMode.Preserve,
			CompressionLevel = PngCompressionLevel.BestSpeed,
		};

		ResizeOptions op = new();
		op.Mode = ResizeMode.Max;
		op.Size = new(128, 128);
		op.PremultiplyAlpha = false;
		image.Mutate(x => x.Resize(op));
		image.Mutate(x => x.ApplyRoundedCorners(8));
		image.SaveAsPng(request.ThumbnailPath, encoder);

		image.Dispose();

		foreach (Action<string> callback in request.Callbacks)
		{
			callback.Invoke(request.ThumbnailPath);
		}
	}

	public struct ThumbnailRequest()
	{
		public readonly List<Action<string>> Callbacks = new();
		public FileInfo? FileInfo;
		public string? SourcePath;
		public string? ThumbnailPath;
		public RequestTypes Type;

		public enum RequestTypes
		{
			FileEmbeddedImage,
			GameTexture,
		}

		public void AddCallback(Action<string> callback)
		{
			this.Callbacks.Add(callback);
		}
	}
}
