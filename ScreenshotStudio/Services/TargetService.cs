// © XivTools.
// Licensed under the MIT license.

//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Ktisis.cs

//// Brio
//// https://github.com/AsgardXIV/Brio
//// https://github.com/AsgardXIV/Brio/blob/main/Brio/UI/Components/Actor/ActorTab.cs

namespace ScreenshotStudio.Services;

using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using NativeObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

public class TargetService : ServiceBase
{
	private const int GPoseActorCount = 39;
	private const int GPoseFirstActor = 201;

	private static readonly unsafe TargetSystem* Targets = TargetSystem.Instance();

	private readonly Dictionary<IntPtr, ActorViewModel> actorLookup = new();
	private ActorViewModel? currentTarget;

	public ObservableCollection<ActorViewModel> AllGPoseActors { get; init; } = new();

	public ActorViewModel? CurrentTarget
	{
		get => this.currentTarget;
		set
		{
			this.currentTarget = value;
			this.RaisePropertyChanged(nameof(TargetService.CurrentTarget));

			if (value != null)
			{
				this.TargetPtr = value.Address;
			}
		}
	}

	public bool IsInGPose => DalamudServices.PluginInterface.UiBuilder.GposeActive;
	public unsafe IntPtr TargetPtr
	{
		get => (IntPtr)Targets->GPoseTarget;
		set
		{
			if (this.IsValidGPoseActorAddress(value))
			{
				Targets->GPoseTarget = (NativeObject*)value;
			}
		}
	}

	public override async Task Start()
	{
		await base.Start();
		_ = Task.Run(this.TargetWatcher);
	}

	public bool IsValidGPoseActorAddress(IntPtr address)
	{
		for (int i = GPoseFirstActor; i < GPoseFirstActor + GPoseActorCount; ++i)
		{
			IntPtr objectAddress = DalamudServices.ObjectTable.GetObjectAddress(i);
			if (objectAddress == address)
			{
				return true;
			}
		}

		return false;
	}

	private async Task TargetWatcher()
	{
		IntPtr currentTarget = IntPtr.Zero;
		IntPtr lastTarget = IntPtr.Zero;

		while(this.IsAlive)
		{
			await Task.Delay(100);

			try
			{
				if (!this.IsInGPose)
				{
					this.CurrentTarget = null;
					this.AllGPoseActors.Clear();
					continue;
				}

				this.UpdateActorTable();

				// Update actor target
				currentTarget = this.TargetPtr;
				if (currentTarget != lastTarget)
				{
					lastTarget = currentTarget;

					this.actorLookup.TryGetValue(currentTarget, out ActorViewModel? targetActor);
					this.CurrentTarget = targetActor;
				}
			}
			catch(Exception ex)
			{
				this.Log.Error(ex, "Error in TargetService Target Watcher task");
			}
		}
	}

	private void UpdateActorTable()
	{
		HashSet<IntPtr> oldPointers = new(this.actorLookup.Keys);
		for (int i = GPoseFirstActor; i < GPoseFirstActor + GPoseActorCount; ++i)
		{
			IntPtr objectAddress = DalamudServices.ObjectTable.GetObjectAddress(i);

			if (objectAddress == IntPtr.Zero)
				continue;

			oldPointers.Remove(objectAddress);

			if (!this.actorLookup.ContainsKey(objectAddress))
			{
				ActorViewModel actor = new(objectAddress);

				this.actorLookup.Add(objectAddress, actor);
				this.AllGPoseActors.Add(actor);
				this.Log.Information("got actor " + actor);
			}
		}

		foreach (IntPtr oldPtr in oldPointers)
		{
			this.Log.Information("lost actor " + this.actorLookup[oldPtr]);
			this.AllGPoseActors.Remove(this.actorLookup[oldPtr]);
			this.actorLookup.Remove(oldPtr);
		}
	}
}