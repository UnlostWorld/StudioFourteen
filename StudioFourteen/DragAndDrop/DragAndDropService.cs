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
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using WpfUtils.Extensions;

public interface IDraggable
{
	IDragSceneInstance? CreateSceneInstance();
}

public interface IDragSceneInstance
{
	Task EnterScene();
	void UpdatePosition(HitInfo hit);
	Task LeaveScene();
	void Drop();
}

public partial class DragAndDropService : ServiceBase
{
	[Notify] private bool isDragging;

	private IDraggable? currentDragObject;
	private IDragSceneInstance? currentSceneInstance;

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

	public void Drag(FrameworkElement owner, IDraggable obj)
	{
		this.IsDragging = true;
		this.currentDragObject = obj;

		DragDrop.DoDragDrop(owner, obj, DragDropEffects.All);
		this.IsDragging = false;
	}

	public void HandleDragEnterScene(DragEventArgs e)
	{
		if (this.currentDragObject == null)
			return;

		if (this.currentSceneInstance != null)
			return;

		this.currentSceneInstance = this.currentDragObject.CreateSceneInstance();
		if (this.currentSceneInstance != null)
		{
			e.Effects = DragDropEffects.Move;
			this.currentSceneInstance.EnterScene().Run();
		}
	}

	public void HandleDragOverScene(DragEventArgs e)
	{
		if (this.currentSceneInstance != null)
		{
			e.Effects = DragDropEffects.Move;
		}
	}

	public void HandleDragLeaveScene(DragEventArgs e)
	{
		if (this.currentSceneInstance != null)
		{
			this.currentSceneInstance.LeaveScene();
			this.currentSceneInstance = null;
		}
	}

	public void HandleDropScene(DragEventArgs e)
	{
		if (this.currentSceneInstance != null)
		{
			this.currentSceneInstance.Drop();
			this.currentSceneInstance = null;
		}
	}

	private void OnGameTick()
	{
		if (this.currentSceneInstance == null)
			return;

		HitInfo? hit = RayCast.CastFromCursor();
		if (hit == null)
			return;

		this.currentSceneInstance.UpdatePosition(hit);
	}
}