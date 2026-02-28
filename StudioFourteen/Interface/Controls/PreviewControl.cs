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

namespace StudioFourteen.Interface.Controls;

using System;
using System.IO;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using global::Avalonia.Media.Imaging;
using Lumina.Excel.Sheets;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using StudioFourteen.Services.Avalonia;
using StudioFourteen.Services.Avalonia.Platform;
using StudioFourteen.Services.Library.GameData.Extensions;
using StudioFourteen.Services.Rendering;
using StudioFourteen.Services.Tick;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

using AvaloniaImage = Avalonia.Controls.Image;
using Device = SharpDX.Direct3D11.Device;
using Image = SixLabors.ImageSharp.Image;
using Math = StudioFourteen.Services.Numerics.Math;
using XivTexture = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Texture;

public class PreviewControl : Panel, IDisposable
{
	public static readonly StyledProperty<Item?> ItemProperty;

	private static bool isPreviewVisible = false;

	private Item? item;
	private unsafe CharaView* pView;
	private unsafe XivTexture* pTexture;
	private Texture2D? characterTexture;
	private EquipmentSlot? lookAtSlot = null;

	private Visual? root;
	private WindowRenderer? windowRenderer;

	static PreviewControl()
	{
		ItemProperty = AvaloniaProperty.Register<PreviewControl, Item?>(nameof(PreviewControl.Item));
	}

	public Item? Item
	{
		get => this.GetValue(ItemProperty);
		set => this.SetValue(ItemProperty, value);
	}

	public unsafe void Dispose()
	{
		this.pView = null;
		this.pTexture = null;
		this.characterTexture = null;
		this.windowRenderer?.Subwindow = null;

		Studio.Tick.Remove(Services.Tick.TickChannels.Game, this.OnGameTick);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == IsVisibleProperty)
		{
			if (this.IsVisible)
			{
				this.Show();
			}
			else
			{
				this.Hide();
			}
		}
		else if (change.Property == ItemProperty)
		{
			this.item = this.Item;
		}
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);

		if (this.IsVisible)
		{
			this.Show();
		}
	}

	protected override void OnUnloaded(RoutedEventArgs e)
	{
		base.OnUnloaded(e);

		this.Hide();
	}

	private void Show()
	{
		if (isPreviewVisible)
			throw new NotSupportedException("Attempt tp preview multiple items at the same time");

		isPreviewVisible = true;
		this.item = this.Item;

		Studio.Tick.Add(Services.Tick.TickChannels.Game, this.OnGameTick);
	}

	private void Hide()
	{
		isPreviewVisible = false;
		this.Dispose();
	}

	private unsafe void OnGameTick()
	{
		if (Studio.IsDisposed)
			return;

		RenderTargetManagerEx* pRenderTargetManager = RenderTargetManagerEx.Instance();
		if (pRenderTargetManager == null)
			return;

		Device? device = Studio.Rendering.OverlayRenderer.Device;
		if (device == null)
			return;

		if (this.item == null)
			return;

		if (this.pView == null)
		{
			this.pView = IMemorySpace.GetUISpace()->Create<CharaView>();

			////Character* pCharacter = (Character*)Studio.Scene.GetXivObject(0);
			////this.pView->ModelData.CopyFromCharacter(pCharacter);

			// Mannequin.
			this.pView->ModelData.CustomizeData.Race = 0;
			this.pView->ModelData.CustomizeData.Sex = 1;
			this.pView->ModelData.CustomizeData.BodyType = 0;
			this.pView->ModelData.CustomizeData.Height = 50;
			this.pView->ModelData.CustomizeData.Tribe = 1;
			this.pView->ModelData.CustomizeData.Face = 250;
			this.pView->ModelData.CustomizeData.Hairstyle = 1;
			this.pView->ModelData.CustomizeData.SkinColor = 248;
			this.pView->ModelData.CustomizeData.EyeColorRight = 0;
			this.pView->ModelData.CustomizeData.HairColor = 1;
			this.pView->ModelData.CustomizeData.HighlightsColor = 0;
			this.pView->ModelData.CustomizeData.TattooColor = 0;
			this.pView->ModelData.CustomizeData.Eyebrows = 0;
			this.pView->ModelData.CustomizeData.EyeColorLeft = 0;
			this.pView->ModelData.CustomizeData.Nose = 0;
			this.pView->ModelData.CustomizeData.Jaw = 0;
			this.pView->ModelData.CustomizeData.LipColorFurPattern = 0;
			this.pView->ModelData.CustomizeData.MuscleMass = 0;
			this.pView->ModelData.CustomizeData.TailShape = 0;
			this.pView->ModelData.CustomizeData.BustSize = 50;
			this.pView->ModelData.CustomizeData.FacePaintColor = 0;

			// Put the item in any slots it fits in.
			EquipmentModelId id = default;
			id.Id = (ushort)this.item.Value.ModelMain;
			id.Variant = (byte)(this.item.Value.ModelMain >> 16);

			EquipSlotCategory equipSlot = this.item.Value.EquipSlotCategory.Value;
			foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
			{
				if (equipSlot.Contains(slot))
				{
					this.lookAtSlot = slot;
					this.pView->ModelData.EquipmentModelIds[(int)slot] = id;
				}
			}

			// Use object Id 1 as its guaranteed to be the current characters minion/mount/whatever,
			//  which wont ever have its own chara view, so we can safely use it for our purposes.
			this.pView->Initialize(null, 1, 0);
			this.pTexture = pRenderTargetManager->Base.GetCharaViewTexture(1);

			if (this.pTexture == null || (nint)this.pTexture->D3D11Texture2D == 0)
			{
				Studio.Log.Error(new Exception("Failed to create chara view texture"), "Error generating portrait");
				return;
			}

			this.characterTexture = new((nint)pTexture->D3D11Texture2D);

			ILayoutRoot? root = this.FindAncestorOfType<ILayoutRoot>();
			if (root == null)
			{
				Studio.Log.Error("Error getting layout root from preview control!");
				return;
			}

			if (root is WindowBase wnd && wnd.PlatformImpl is WindowImpl windowImpl)
			{
				this.root = wnd;
				this.windowRenderer = windowImpl.WindowRenderer;
			}

			if (this.windowRenderer == null)
			{
				Studio.Log.Error("Error getting window renderer for preview control!");
				return;
			}

			if (this.windowRenderer.Subwindow != null)
			{
				Studio.Log.Error("Multiple previews in the same window. This is not supported.");
				return;
			}

			this.windowRenderer.Subwindow = this.characterTexture;
			this.windowRenderer.SubWindowPosition = new(0, 0, 1, 1);
		}

		if (this.lookAtSlot != null)
		{
			this.pView->ResetPositions();

			switch (this.lookAtSlot)
			{
				case EquipmentSlot.Head:
				{
					this.pView->SetCameraDistance(-10f);
					this.pView->SetCameraXAndY(0f, -320.0f);
					this.pView->SetCameraYawAndPitch(45, 45);
					break;
				}
			}
		}

		this.pView->Render(1);

		if (this.root != null && this.windowRenderer != null)
		{
			Dispatcher.UIThread.Invoke(() =>
			{
				Matrix? matrix = this.TransformToVisual(this.root);
				if (matrix != null)
				{
					Avalonia.Point p0 = matrix.Value.Transform(default);
					Avalonia.Point p1 = matrix.Value.Transform(new(this.Bounds.Width, this.Bounds.Height));

					Vector4 pos = this.windowRenderer.SubWindowPosition;
					pos.X = (float)(p0.X / this.root.Bounds.Width);
					pos.Y = (float)(p0.Y / this.root.Bounds.Height);
					pos.Z = (float)(p1.X / this.root.Bounds.Width);
					pos.W = (float)(p1.Y / this.root.Bounds.Height);

					this.windowRenderer.SubWindowPosition = pos;
				}
			});
		}
	}
}