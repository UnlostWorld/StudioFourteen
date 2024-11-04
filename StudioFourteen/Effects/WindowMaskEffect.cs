namespace StudioFourteen.Effects;

using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using StudioFourteen.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media.Media3D;

public class WindowMaskEffect
	: ShaderEffectBase
{
	public static readonly DependencyProperty OpacityProperty =
		DependencyProperty.Register(
			nameof(Opacity),
			typeof(float),
			typeof(WindowMaskEffect),
			new UIPropertyMetadata(1.0f, PixelShaderConstantCallback(0)));

	public static readonly DependencyProperty RegionProperty =
		DependencyProperty.Register(
			nameof(Region),
			typeof(Point4D),
			typeof(WindowMaskEffect),
			new UIPropertyMetadata(new Point4D(0, 0, 0, 0), PixelShaderConstantCallback(2)));

	private const int RegionCount = 32;

	private static readonly List<PropertyChangedCallback> RegionCallbacks = new();

	private readonly HashSet<Vector4> imGuiClipBounds = new();

	static WindowMaskEffect()
	{
		for (int i = 0; i < 32; i++)
		{
			RegionCallbacks.Add(PixelShaderConstantCallback(1 + i));
		}
	}

	public WindowMaskEffect()
		: base("pack://application:,,,/StudioFourteen;component/Effects/Compiled/WindowMask.ps")
	{
		this.UpdateShaderValue(OpacityProperty);
		this.UpdateShaderValue(RegionProperty);

		for (int i = 0; i < RegionCount; i++)
			this.SetRegion(i, Rect.Empty);

		this.Opacity = 0.0f;

		if (DalamudServices.Framework == null)
			return;

		DalamudServices.Framework.Update += this.OnFrameworkUpdate;
	}

	public float Opacity
	{
		get => (float)this.GetValue(OpacityProperty);
		set => this.SetValue(OpacityProperty, value);
	}

	public Point4D Region
	{
		get => (Point4D)this.GetValue(RegionProperty);
		set => this.SetValue(RegionProperty, value);
	}

	private void SetRegion(int index, Rect rect)
	{
		if (index < 0 || index > RegionCount - 1)
		{
			this.Log.Error($"Attempt to set mask region with an invalid index: {index}");
			return;
		}

		DependencyPropertyChangedEventArgs args = new(RegionProperty, new Point4D(0, 0, 0, 0), new Point4D(rect.X, rect.Y, rect.Width, rect.Height));
		RegionCallbacks[index].Invoke(this, args);
	}

	private unsafe void OnFrameworkUpdate(IFramework framework)
	{
		if (!this.Services.Studio.IsOpen)
			return;

		Rect[] bounds = new Rect[RegionCount];

		for (int i = 0; i < RegionCount; i++)
			bounds[i] = Rect.Empty;

		AtkUnitList? loadedUnits = AtkManager.GetAllLoadedUnits();
		if (loadedUnits == null)
			return;

		var device = Device.Instance();
		float width = device->Width;
		float height = device->Height;

		int index = 0;
		for (int i = 0; i < loadedUnits.Value.Count; i++)
		{
			AtkUnitBase* unit = loadedUnits.Value.Entries[i];

			if (!unit->IsVisible)
				continue;

			if (unit->Alpha < 32)
				continue;

			if (unit->DepthLayer < 4)
				continue;

			if (unit->VisibilityFlags != 0)
				continue;

			if (unit->WindowCollisionNode == null)
				continue;

			if (unit->WindowCollisionNode->Alpha_2 < 32)
				continue;

			Rect rect = new(unit->X / width, unit->Y / height, unit->GetScaledWidth(true) / width, unit->GetScaledHeight(true) / height);

			if (rect.Width == 0 || rect.Height == 0)
				continue;

			if (rect.Width == 1 && rect.Height == 1)
				continue;

			bounds[index] = rect;
			index++;
		}

		this.imGuiClipBounds.Clear();
		ImDrawDataPtr drawData = ImGui.GetDrawData();
		var clipOff = drawData.DisplayPos;
		for (int n = 0; n < drawData.CmdListsCount; n++)
		{
			var cmdList = drawData.CmdListsRange[n];
			for (int cmd = 0; cmd < cmdList.CmdBuffer.Size; cmd++)
			{
				this.imGuiClipBounds.Add(cmdList.CmdBuffer[cmd].ClipRect);
			}
		}

		foreach (Vector4 clipRect in this.imGuiClipBounds)
		{
			float x = (clipRect.X - clipOff.X) - (ImGui.GetStyle().WindowBorderSize + ImGui.GetStyle().WindowPadding.X);
			float y = ((clipRect.Y - clipOff.Y) - ImGui.GetTextLineHeight()) - (ImGui.GetStyle().WindowBorderSize + ImGui.GetStyle().WindowPadding.Y);
			float w = ((clipRect.Z - clipOff.X) - x) + ImGui.GetStyle().WindowBorderSize;
			float h = ((clipRect.W - clipOff.Y) - y) + ImGui.GetStyle().WindowBorderSize;

			Rect rect = new(x / width, y / height, w / width, h / height);

			if (rect.Width <= 0 || rect.Height <= 0)
				continue;

			if (rect.Width >= 1.0f && rect.Height >= 1.0f)
				continue;

			bounds[index] = rect;
			index++;
		}

		this.Dispatcher.Invoke(() =>
		{
			for (int i = 0; i < RegionCount; i++)
			{
				Rect rect = bounds[i];
				DependencyPropertyChangedEventArgs args = new(RegionProperty, new Point4D(0, 0, 0, 0), new Point4D(rect.X, rect.Y, rect.Width, rect.Height));
				RegionCallbacks[i].Invoke(this, args);
			}
		});
	}
}
