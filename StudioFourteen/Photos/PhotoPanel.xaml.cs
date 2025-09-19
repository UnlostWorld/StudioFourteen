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

namespace StudioFourteen.Photos;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Passes;
using StudioFourteen.Settings;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using StudioFourteen.Animation;
using StudioFourteen.Extensions;

using Panel = StudioFourteen.Panels.Panel;

public partial class PhotoPanel : Panel
{
	private const float LerpTimeMs = 250;
	private readonly ScreenEffectPass<PhotoGuidesEffectMaterial> guidesPass = new();
	private readonly Stopwatch lerpTimer = new();
	private readonly EasingFunctionBase easing = new SineEase();
	private float fromLeftRight;
	private float fromTopBottom;
	private float toLeftRight;
	private float toTopBottom;

	public bool HideUI
	{
		get => this.Persistence.GetPersistence<bool>();
		set
		{
			this.Persistence.SetPersistence(value);
			this.Services.Photos.IsPhotoMode = this.HideUI;
		}
	}

	public PhotoGuidesEffectMaterial.GuideModes Guide
	{
		get => this.Persistence.GetPersistence<PhotoGuidesEffectMaterial.GuideModes>();
		set
		{
			this.Persistence.SetPersistence(value);
			this.guidesPass.Material.GuidesMode = (uint)value;
		}
	}

	public int GuideIndex
	{
		get => (int)this.Guide;
		set => this.Guide = (PhotoGuidesEffectMaterial.GuideModes)value;
	}

	public int AspectRatioIndex
	{
		get => this.Persistence.GetPersistence<int>();
		set => this.Persistence.SetPersistence(value);
	}

	public AspectRatioEntry SelectedAspectRatio
	{
		get => this.Services.Photos.AspectRatios[this.AspectRatioIndex];

		set
		{
			this.AspectRatioIndex = this.Services.Photos.AspectRatios.IndexOf(value);
			this.Services.Photos.AspectRatio = value.Aspect;
			this.Services.Photos.Width = this.SelectedAspectRatio.Width;
			this.Services.Photos.Height = this.SelectedAspectRatio.Height;

			this.SetAspectRatio(value.Aspect);

			this.NotifyPropertyChanged(nameof(this.SelectedAspectRatio));
			this.ResolutionToggle.IsChecked = false;
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.Photos.IsPhotoMode = this.HideUI;
		this.Services.Photos.AspectRatio = this.SelectedAspectRatio.Aspect;
		this.Services.Photos.Width = this.SelectedAspectRatio.Width;
		this.Services.Photos.Height = this.SelectedAspectRatio.Height;

		this.Services.Rendering.OverlayRenderer.AddAfterEffectsPass(this.guidesPass);

		this.SetAspectRatio(this.SelectedAspectRatio.Aspect);
		this.guidesPass.Material.GuidesMode = (uint)this.Guide;
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		this.Services.Photos.IsPhotoMode = false;
		this.Services.Photos.AspectRatio = 0;

		this.RemoveGuidesAsync().RunAsynchronously();
	}

	protected override void OnGameTick()
	{
		base.OnGameTick();

		float p = this.lerpTimer.ElapsedMilliseconds / LerpTimeMs;
		p = float.Clamp(p, 0.0f, 1.0f);
		p = this.easing.Ease(p, EasingFunctionBase.EasingModes.EaseOut);

		this.guidesPass.Material.LeftRight = float.Lerp(this.fromLeftRight, this.toLeftRight, p);
		this.guidesPass.Material.TopBottom = float.Lerp(this.fromTopBottom, this.toTopBottom, p);

		if (this.lerpTimer.ElapsedMilliseconds > LerpTimeMs)
		{
			this.lerpTimer.Stop();
		}
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Photos.Capture();
	}

	private void OnSettingsClicked(object sender, RoutedEventArgs e)
	{
		SettingsPanel.Show(this.GetContext(), "PhotosSection");
	}

	private void OnCloseClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private async Task RemoveGuidesAsync()
	{
		this.guidesPass.Material.GuidesMode = (uint)PhotoGuidesEffectMaterial.GuideModes.None;
		this.SetAspectRatio(0);
		await Task.Delay(250);

		this.Services.Rendering.OverlayRenderer.RemoveAfterEffectsPass(this.guidesPass);
	}

	private void OnAspectsExpanderExpanded(object sender, RoutedEventArgs e)
	{
		if (sender is Expander expander)
		{
			List<Expander> expanders = this.AspectsList.FindChildren<Expander>();
			foreach (Expander otherExpander in expanders)
			{
				if (otherExpander == expander)
					continue;

				otherExpander.IsExpanded = false;
			}
		}
	}

	private void SetAspectRatio(double aspect)
	{
		this.fromLeftRight = this.toLeftRight;
		this.fromTopBottom = this.toTopBottom;

		if (aspect == 0)
		{
			this.toTopBottom = 0;
			this.toLeftRight = 0;
			this.lerpTimer.Restart();
			return;
		}

		double height = ServiceManager.Instance.Rendering.OverlayRenderer.Width;
		double width = ServiceManager.Instance.Rendering.OverlayRenderer.Height;

		double currentAspect = height / width;

		this.toTopBottom = 0;
		this.toLeftRight = (1 - (float)(aspect / currentAspect)) / 2;

		if (this.toLeftRight < 0)
		{
			this.toTopBottom = -(float)(this.toLeftRight / currentAspect);
			this.toLeftRight = 0;
		}

		this.lerpTimer.Restart();
	}
}
