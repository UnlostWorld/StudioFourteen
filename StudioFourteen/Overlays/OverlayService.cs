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

namespace StudioFourteen.Overlays;

using Dalamud.Plugin.Services;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class OverlayService
	: ServiceBase
{
	private readonly List<OverlayLayerBase> overlays = new();
	[Notify] private bool showOverlays = true;

	public delegate void OverlayEvent(OverlayLayerBase overlay);
	public delegate void OverlayStateEvent(bool state);

	public event OverlayEvent? LayerAdded;
	public event OverlayEvent? LayerRemoved;
	public event OverlayStateEvent? ShowOverlaysChanged;

	public override async Task Start()
	{
		await base.Start();
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.Services.Panels.PanelsRestarted += this.OnPanelsRestarted;
		this.OnGroupPoseStateChanged(this.Services.GroupPose.IsGroupPosing);
	}

	public override async Task Stop()
	{
		await base.Stop();
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChanged;
	}

	public void AddOverlay(OverlayLayerBase overlay)
	{
		lock (this.overlays)
		{
			this.overlays.Add(overlay);
		}

		this.LayerAdded?.Invoke(overlay);
	}

	public void RemoveOverlay(OverlayLayerBase overlay)
	{
		lock (this.overlays)
		{
			this.overlays.Remove(overlay);
		}

		this.LayerRemoved?.Invoke(overlay);
	}

	public List<OverlayLayerBase> GetOverlayLayers()
	{
		return this.overlays;
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		this.ShowOverlays = this.Services.GroupPose.IsGroupPosing
			&& this.Settings.ShowOverlays
			&& !this.Services.Photos.IsPhotoMode;

		if (this.ShowOverlays)
		{
			lock (this.overlays)
			{
				foreach (OverlayLayerBase overlay in this.overlays)
				{
					overlay.OnFrameworkUpdate();
				}
			}
		}
	}

	private void OnShowOverlaysChanged(bool oldValue, bool newValue)
	{
		this.ShowOverlaysChanged?.Invoke(newValue);
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.Services.Panels.GamePanels.SetIsOpen<OverlayControlPanel>(newState, false);
	}

	private void OnPanelsRestarted(PanelService self)
	{
		this.Services.Panels.GamePanels.SetIsOpen<OverlayControlPanel>(this.Services.GroupPose.IsGroupPosing, false);
	}
}
