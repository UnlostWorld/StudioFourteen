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

namespace StudioFourteen.Services;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Component.GUI;
using StudioFourteen.Input;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using global::Windows.Win32;
using global::Windows.Win32.Foundation;
using global::Windows.Win32.UI.WindowsAndMessaging;
using FFXIVClientStructs.FFXIV.Client.System.Input;

[Service]
public partial class WindowService : ServiceBase
{
	private readonly Guid propertyGuid = Guid.NewGuid();
	private readonly HashSet<string> atkUnitBlacklist = new()
	{
		"GroupPoseStampImage",
		"CursorAddon",
		"_ActionDoubleCrossL",
		"_ActionDoubleCrossR",
		"_ActionContents",
		"_LimitBreak",
		"_ContentGauge",
	};

	private unsafe AtkUnitBase* atkUnitUnderCursor;

	private delegate long WndProcDelegate(IntPtr hWnd, uint msg, ulong wParam, long lParam);

	[Bind] public partial bool IsCursorOverAtkUnit { get; private set; }
	[Bind] public partial bool IsCursorOverImGui { get; private set; }
	[Bind] public partial bool IsCursorOverXiv { get; private set; }
	[Bind] public partial bool IsCursorOverStudio { get; private set; }

	public Process? XivProcess { get; set; }
	public nint? XivWindowHwnd => this.XivProcess?.MainWindowHandle;

	public override Task Initialize()
	{
		this.XivProcess = Process.GetCurrentProcess();

		if (!this.XivProcess.ProcessName.Contains("ffxiv"))
		{
			this.XivProcess = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
			this.Log.Warning($"Failed to get local XIV Process. This should never happen while running via Dalamud. Searching for process: {this.XivProcess}");
		}

		return base.Initialize();
	}

	public override void Attach()
	{
		base.Attach();

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Tick.Add(TickService.Channels.StudioTick, this.OnTick);
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Tick.Remove(TickService.Channels.StudioTick, this.OnTick);

		base.Detach();
	}

	public Vector4 GetXivWindowClientSize()
	{
		throw new NotImplementedException();
	}

	public bool IsAnyStudioWindowActive()
	{
		throw new NotImplementedException();
	}

	protected unsafe void OnGameTick()
	{
		AtkUnitBase* pAtkUnit = this.GetAtkUnitUnderCursor();

		this.atkUnitUnderCursor = pAtkUnit;
		this.IsCursorOverAtkUnit = pAtkUnit != null;
		this.IsCursorOverImGui = this.GetIsCursorOverImGui();
	}

	protected unsafe void OnTick()
	{
		this.IsCursorOverXiv = this.GetIsCursorOverXiv();
		this.IsCursorOverStudio = this.GetIsCursorOverStudio();
	}

	private bool GetIsCursorOverXiv()
	{
		throw new NotImplementedException();
		////CursorUtility.GetWindowUnderCursor() == this.XivWindowHwnd;
	}

	private bool GetIsCursorOverStudio()
	{
		throw new NotImplementedException();
	}

	private bool GetIsCursorOverImGui()
	{
		return ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) || ImGui.IsAnyItemHovered();
	}

	private unsafe AtkUnitBase* GetAtkUnitUnderCursor()
	{
		Point? cursorPos = null; ////this.GetCursorPosition();
		if (cursorPos == null)
			return null;

		AtkUnitList? loadedUnits = AtkManager.GetAllLoadedUnits();
		if (loadedUnits == null)
			return null;

		AtkStage* atkStage = AtkStage.Instance();
		if (atkStage == null)
			return null;

		for (int i = 0; i < loadedUnits.Value.Count; i++)
		{
			AtkUnitBase* unit = loadedUnits.Value.Entries[i];

			if (!unit->IsVisible)
				continue;

			// HACK: yes/no box is always considered under the mouse, while changing resolution
			// the X/Y pos of the box is wrong, so we can't test it.
			if (unit->NameString == "SelectYesno")
				return unit;

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

			if (this.atkUnitBlacklist.Contains(unit->NameString))
				continue;

			if (cursorPos.Value.X > unit->X
				&& cursorPos.Value.X < unit->X + unit->GetScaledWidth(true)
				&& cursorPos.Value.Y > unit->Y
				&& cursorPos.Value.Y < unit->Y + unit->GetScaledHeight(true))
			{
				////this.Log.Information($">> {unit->NameString}");
				return unit;
			}
		}

		return null;
	}
}