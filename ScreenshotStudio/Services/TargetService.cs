// © XivTools.
// Licensed under the MIT license.

//// Special thanks to Ktisis, @chirpxiv
//// https://github.com/ktisis-tools/Ktisis/

//// Special thanks to Brio, @AsgardXIV
//// https://github.com/AsgardXIV/Brio

namespace ScreenshotStudio.Services;

using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ScreenshotStudio.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class TargetService : ServiceBase
{
	private const int GPoseActorCount = 39;
	private const int GPoseFirstActor = 201;

	private static readonly unsafe TargetSystem* Targets = TargetSystem.Instance();

	private readonly Dictionary<IntPtr, Actor> actorTable = new();
	private Actor? currentTarget;

	public ObservableCollection<Actor> Actors { get; init; } = new();

	public Actor? CurrentTarget
	{
		get => this.currentTarget;
		set
		{
			this.currentTarget = value;
			this.RaisePropertyChanged(nameof(TargetService.CurrentTarget));

			// TODO: set target in game!
		}
	}

	public bool IsInGPose => DalamudServices.PluginInterface.UiBuilder.GposeActive;
	public unsafe IntPtr TargetPtr => (IntPtr)Targets->GPoseTarget;

	public override async Task Start()
	{
		await base.Start();
		_ = Task.Run(this.TargetWatcher);
	}

	private async Task TargetWatcher()
	{
		IntPtr currentTarget = IntPtr.Zero;
		IntPtr lastTarget = IntPtr.Zero;

		while(this.IsAlive)
		{
			await Task.Delay(100);

			if (!this.IsInGPose)
			{
				this.CurrentTarget = null;
				this.Actors.Clear();
				continue;
			}

			this.UpdateActorTable();

			// Update actor target
			currentTarget = this.TargetPtr;
			if (currentTarget != lastTarget)
			{
				lastTarget = currentTarget;

				this.actorTable.TryGetValue(currentTarget, out Actor? targetActor);
				this.CurrentTarget = targetActor;
			}
		}
	}

	private void UpdateActorTable()
	{
		HashSet<IntPtr> oldPointers = new(this.actorTable.Keys);
		for (int i = GPoseFirstActor; i < GPoseFirstActor + GPoseActorCount; ++i)
		{
			IntPtr objectAddress = DalamudServices.ObjectTable.GetObjectAddress(i);
			oldPointers.Remove(objectAddress);

			if (!this.actorTable.ContainsKey(objectAddress))
			{
				GameObject? obj = DalamudServices.ObjectTable.CreateObjectReference(objectAddress);
				if (obj != null)
				{
					this.actorTable.Add(objectAddress, new(obj));
					this.Actors.Add(this.actorTable[objectAddress]);
				}
			}
		}

		foreach (IntPtr oldPtr in oldPointers)
		{
			this.Actors.Remove(this.actorTable[oldPtr]);
			this.actorTable.Remove(oldPtr);
		}
	}
}

public class Actor
{
	public Actor(GameObject obj)
	{
		this.Object = obj;
	}

	public GameObject Object { get; init; }
	public string DisplayName => this.Object.Name.ToString();
}