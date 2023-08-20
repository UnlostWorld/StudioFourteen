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

using NativeObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

public class TargetService : ServiceBase
{
	private const int GPoseActorCount = 39;
	private const int GPoseFirstActor = 201;

	private static readonly unsafe TargetSystem* Targets = TargetSystem.Instance();

	private readonly Dictionary<IntPtr, Actor> actorLookup = new();
	private Actor? currentTarget;

	public ObservableCollection<Actor> AllGPoseActors { get; init; } = new();

	public Actor? CurrentTarget
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

					this.actorLookup.TryGetValue(currentTarget, out Actor? targetActor);
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
			oldPointers.Remove(objectAddress);

			if (!this.actorLookup.ContainsKey(objectAddress))
			{
				GameObject? obj = DalamudServices.ObjectTable.CreateObjectReference(objectAddress);
				if (obj != null)
				{
					this.actorLookup.Add(objectAddress, new(obj));
					this.AllGPoseActors.Add(this.actorLookup[objectAddress]);
					this.Log.Information("got actor " + this.actorLookup[objectAddress]);
				}
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

public class Actor
{
	public Actor(GameObject obj)
	{
		this.Object = obj;
	}

	public GameObject Object { get; init; }
	public string DisplayName => this.Object.Name.ToString();
	public IntPtr Address => this.Object.Address;
}