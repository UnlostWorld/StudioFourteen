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
using PropertyChanged.SourceGenerator;
using StudioFourteen.Cursors;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using StudioFourteen.Extensions;

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

public partial class DragAndDropService : ServiceBase
{
	private DragAndDropOperation? currentOperation;
	private DragObjectVisual? currentVisual;

	private object? dragOperationIconNo;
	private object? dragOperationIconAssign;

	[Bind] public partial bool IsDragging { get; set; }
	[Bind] public partial IDraggable? CurrentDragObject { get; set; }

	public override Task Start()
	{
		this.dragOperationIconNo = Resources.Find("ICON_Drag_No");
		this.dragOperationIconAssign = Resources.Find("ICON_Drag_Assign");
		return base.Start();
	}

	public override Task Shutdown()
	{
		this.currentVisual?.Close();
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

	public void Drag(FrameworkElement owner, IDraggable obj, IDraggable? previewParent = null)
	{
		this.currentVisual?.Close();

		this.IsDragging = true;
		this.CurrentDragObject = obj;

		object? dragPreview = obj.GetDragPreviewContent();
		if (dragPreview == null)
			dragPreview = previewParent?.GetDragPreviewContent();

		if (dragPreview != null)
		{
			this.currentVisual = new(dragPreview);
			this.currentVisual.Show();
		}

		owner.GiveFeedback += this.GiveFeedback;
		owner.QueryContinueDrag += this.QueryContinueDrag;

		string msg = "Hey, dont drag that over here.";
		DragDrop.DoDragDrop(owner, msg, DragDropEffects.All);

		this.currentVisual?.Close();
		this.currentVisual = null;
		owner.GiveFeedback -= this.GiveFeedback;
		owner.QueryContinueDrag -= this.QueryContinueDrag;
		this.IsDragging = false;
	}

	public void HandleDragEnterScene(DragEventArgs e)
	{
		if (this.CurrentDragObject == null)
			return;

		if (this.currentOperation != null)
			return;

		IDragSceneInstance? instance = this.CurrentDragObject.CreateSceneInstance();
		if (instance != null)
		{
			this.currentOperation = new(instance);
			e.Effects = DragDropEffects.Copy;
			this.currentOperation.EnterScene().RunAsynchronously();
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}

		e.Handled = true;
	}

	public void HandleDragOverScene(DragEventArgs e)
	{
		if (this.currentOperation != null)
		{
			e.Effects = DragDropEffects.Copy;
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}

		e.Handled = true;
	}

	public void HandleDragLeaveScene(DragEventArgs e)
	{
		if (this.currentOperation != null)
		{
			this.currentOperation.LeaveScene().RunAsynchronously();
			this.currentOperation = null;
		}
	}

	public void HandleDropScene(DragEventArgs e)
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

	private void GiveFeedback(object sender, GiveFeedbackEventArgs e)
	{
		if (this.currentVisual != null)
		{
			object? opIcon = this.currentOperation?.GetOperationIcon();

			if (opIcon == null)
			{
				if (e.Effects == DragDropEffects.Move)
				{
					opIcon = this.dragOperationIconAssign;
				}
				else
				{
					opIcon = this.dragOperationIconNo;
				}
			}

			this.currentVisual.SetOperation(opIcon);
		}

		Mouse.SetCursor(this.Services.Cursor.GetCursor(CursorService.CursorType.Grab));
		e.Handled = true;
	}

	private void QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
	{
		if (this.currentVisual == null)
			return;

		Point cursorPos = CursorUtility.GetPosition();
		this.currentVisual.Left = cursorPos.X - (this.currentVisual.Width / 2);
		this.currentVisual.Top = cursorPos.Y - (this.currentVisual.Height / 2);
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