// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using System.Collections.Specialized;
using XivToolsWpf;
using XivToolsWpf.Extensions;

public partial class TargetPanel : DockPanel
{
	public TargetPanel()
	{
		this.Services.Targets.AllGPoseActors.CollectionChanged += this.OnGPoseActorsChanged;

		lock (this.Services.Targets)
		{
			this.Actors.AddRange(this.Services.Targets.AllGPoseActors);
		}
	}

	public FastObservableCollection<ActorViewModel> Actors { get; init; } = new();

	private async void OnGPoseActorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		await this.Dispatcher.MainThread();
		this.Actors.Synchronize(e);
	}
}
