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

namespace StudioFourteen.Services.Portraits;

using System;
using System.Collections.Generic;
using System.IO;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StudioFourteen.Services.Images;
using StudioFourteen.Services.Tick;

using Device = SharpDX.Direct3D11.Device;

public class PortraitService : IService
{
	private readonly Queue<PortraitGenerator> pending = new();
	private PortraitGenerator? current;

	public PortraitService()
	{
		Studio.Tick.Add(TickChannels.Game, this.OnGameTick);
	}

	public void Dispose()
	{
		Studio.Tick.Remove(TickChannels.Game, this.OnGameTick);
	}

	public PortraitGenerator Generate(int objectIndex)
	{
		PortraitGenerator generator = new(objectIndex);
		this.pending.Enqueue(generator);
		return generator;
	}

	private void OnGameTick()
	{
		if (this.current != null)
		{
			this.current.OnGameTick();

			if (this.current.IsDone)
				this.current = null;

			return;
		}

		if (this.pending.TryDequeue(out PortraitGenerator? generator) && generator != null)
		{
			this.current = generator;
		}
	}

	public unsafe class PortraitGenerator
	{
		private readonly int objectIndex = -1;

		private int framesToRender = 10;

		private CharaView* pView;
		private Texture* pTexture;
		private Texture2D? characterTexture;
		private SaveTexturePass? pass;

		public PortraitGenerator(int objectIndex)
		{
			this.objectIndex = objectIndex;
		}

		public bool IsDone { get; private set; }

		internal void OnGameTick()
		{
			if (Studio.IsDisposed)
				return;

			RenderTargetManagerEx* pRenderTargetManager = RenderTargetManagerEx.Instance();
			if (pRenderTargetManager == null)
				return;

			Device? device = Studio.Rendering.OverlayRenderer.Device;
			if (device == null)
				return;

			if (this.pView == null)
			{
				this.pView = CharaView.Create();

				// Set customization options to anything we want. =]
				this.pView->ModelData.CustomizeData.Race = 0;
				this.pView->ModelData.CustomizeData.Sex = 1;

				// Use object Id 1 as its guaranteed to be the current characters minion/mount/whatever,
				//  which wont ever have its own chara view, so we can safely use it for our purposes.
				this.pView->Initialize(null, 1, 0);
				this.pTexture = pRenderTargetManager->Base.GetCharaViewTexture(1);

				if (this.pTexture == null || (nint)this.pTexture->D3D11Texture2D == 0)
				{
					Studio.Log.Error(new Exception("Failed to create chara view texture"), "Error generating portrait");
					this.IsDone = true;
				}

				this.characterTexture = new((nint)pTexture->D3D11Texture2D);

				this.pass = new(this.characterTexture);
				Studio.Rendering.OverlayRenderer.AddAfterEffectsPass(this.pass);
			}

			this.pView->Render(1);
			this.framesToRender--;

			if (this.framesToRender > 0)
				return;

			if (this.characterTexture == null)
				return;

			Image? image = this.pass?.Image;
			if (image == null)
				return;

			if (this.pass == null)
				return;

			Studio.Log.Information("Success!");
			Studio.Rendering.OverlayRenderer.RemoveAfterEffectsPass(this.pass);

			PngEncoder encoder = new()
			{
				ColorType = PngColorType.RgbWithAlpha,
				TransparentColorMode = PngTransparentColorMode.Preserve,
				CompressionLevel = PngCompressionLevel.BestSpeed,
			};
			image.SaveAsPng("C:\\Users\\yukiw\\Desktop\\Test.png", encoder);

			this.IsDone = true;
		}
	}
}