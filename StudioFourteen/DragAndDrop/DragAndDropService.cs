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

namespace StudioFourteen.DragAndDrop;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using StudioFourteen.Xaml;

public interface IDraggable
{
	IDragSceneInstance? CreateSceneInstance();
	object? GetDragPreviewContent() => null;
}

public interface IDragSceneInstance
{
	Task EnterScene();
	void UpdatePosition(HitInfo hit);
	Task LeaveScene();
	Task Drop(HitInfo hit);

	object? GetOperationIcon();
}

[Service]
public partial class DragAndDropService : ServiceBase
{
	private DragAndDropOperation? currentOperation;

	[Bind] public partial bool IsDragging { get; set; }
	[Bind] public partial IDraggable? CurrentDragObject { get; set; }

	public override Task Start()
	{
		return base.Start();
	}

	public override Task Shutdown()
	{
		return base.Shutdown();
	}

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Detach();
	}

	public void HandleDragEnterScene()
	{
		if (this.CurrentDragObject == null)
			return;

		if (this.currentOperation != null)
			return;

		IDragSceneInstance? instance = this.CurrentDragObject.CreateSceneInstance();
		if (instance != null)
		{
			this.currentOperation = new(instance);
			this.currentOperation.EnterScene().RunAsynchronously();
		}
		else
		{
		}
	}

	public void HandleDragOverScene()
	{
		if (this.currentOperation != null)
		{
		}
		else
		{
		}
	}

	public void HandleDragLeaveScene()
	{
		if (this.currentOperation != null)
		{
			this.currentOperation.LeaveScene().RunAsynchronously();
			this.currentOperation = null;
		}
	}

	public void HandleDropScene()
	{
		if (this.currentOperation != null)
		{
			this.currentOperation.Drop().RunAsynchronously();
			this.currentOperation = null;
		}
	}

	private void OnGameTick()
	{
		if (this.currentOperation == null)
			return;

		HitInfo? hit = RayCast.CastFromCursor();
		if (hit == null)
			return;

		this.currentOperation.UpdatePosition(hit);
	}
}

public class DragAndDropOperation(IDragSceneInstance instance)
{
	private bool isEntering;
	private bool isLeaving;
	private bool isDropping;
	private HitInfo? lastHit;

	public async Task EnterScene()
	{
		while(this.isLeaving || this.isDropping)
			await Task.Delay(100);

		this.isEntering = true;
		await instance.EnterScene();
		this.isEntering = false;
	}

	public async Task LeaveScene()
	{
		while(this.isEntering || this.isDropping)
			await Task.Delay(100);

		this.isLeaving = true;
		await instance.LeaveScene();
		this.isLeaving = false;
	}

	public async Task Drop()
	{
		while(this.isEntering || this.isLeaving)
			await Task.Delay(100);

		if (this.lastHit == null)
			throw new Exception("Last Hit is null");

		this.isDropping = true;
		await instance.Drop(this.lastHit);
		this.isDropping = false;
	}

	public void UpdatePosition(HitInfo hit)
	{
		this.lastHit = hit;
		instance.UpdatePosition(hit);
	}

	public object? GetOperationIcon() => instance.GetOperationIcon();
}