namespace StudioFourteen.Appearance;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Tags;
using System;
using System.Windows;

public abstract class GearViewModelBase : AutoViewModel
{
	public unsafe Character* Target => this.Services.Target.Target;
	[AlwaysNotify] public string? CharacterName => this.Services.Target.CharacterName;
	[AlwaysNotify] public virtual bool HasValidTarget => this.Services.Target.HasValidTarget;
	[AlwaysNotify] public int TargetObjectIndex => this.Services.Target.TargetObjectIndex;

	public abstract void Clear();
	public abstract void Change(UIElement placementTarget);

	public override bool ShouldTickAutoProperties() => this.HasValidTarget;
}

public abstract class GearViewModelBase<T> : GearViewModelBase
	where T : struct, IExcelRow<T>
{
	[AutoNotify] public abstract T? Item { get; set; }

	public override void Clear()
	{
		this.Item = default;
	}

	public sealed override void Change(UIElement placementTarget)
	{
		TagCollection defaultTags = new();
		this.GetDefaultTags(defaultTags);

		string searchTitle = this.GetSearchTitle();

		MiniLibraryPopOut.Show<T>(
			placementTarget,
			searchTitle,
			defaultTags,
			this.Item,
			(item, isFinal) =>
			{
				this.Item = item;
			});
	}

	protected virtual string GetSearchTitle()
	{
		return string.Empty;
	}

	protected virtual void GetDefaultTags(TagCollection tags)
	{
	}
}