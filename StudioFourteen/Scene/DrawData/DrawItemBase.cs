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

namespace StudioFourteen.Scene.DrawData;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using StudioFourteen.Services.Library.GameData.Library;

public abstract class DrawItemBase<TLibraryType> : ObservableObject
	where TLibraryType : ExcelLibraryEntry
{
	protected byte? nextWriteStain0;
	protected byte lastReadStain0 = 255;
	protected byte? nextWriteStain1;
	protected byte lastReadStain1 = 255;

	protected StainLibraryEntry? stain0;
	protected StainLibraryEntry? stain1;
	protected TLibraryType? item;

	public virtual TLibraryType? Item
	{
		get => this.item;
		set
		{
			this.item = value;
			this.OnPropertyChanged(nameof(this.Item));
			this.OnPropertyChanged(nameof(this.IsNone));
			this.OnItemChanged(value);
		}
	}

	public bool IsNone
	{
		get => this.Item == null;
		set => this.Item = null;
	}

	public Type LibraryType => typeof(TLibraryType);

	public StainLibraryEntry? Stain0
	{
		get => this.stain0;
		set
		{
			if (this.Stain0Id == value?.RowId)
				return;

			this.stain0 = value;

			if (value == null)
			{
				this.nextWriteStain0 = 0;
			}
			else
			{
				this.nextWriteStain0 = (byte)value.RowId;
			}

			this.OnPropertyChanged(nameof(this.Stain0));
		}
	}

	public bool IsStain0None
	{
		get => this.Stain0Id == 0;
		set => this.Stain0Id = 0;
	}

	public byte Stain0Id
	{
		get => this.nextWriteStain0 ?? this.lastReadStain0;
		set
		{
			if (value == this.Stain0Id)
				return;

			this.nextWriteStain0 = value;
			this.OnPropertyChanged(nameof(this.Stain0Id));

			this.stain0 = Studio.Library.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain0Id);
			this.OnPropertyChanged(nameof(this.Stain0));
		}
	}

	public StainLibraryEntry? Stain1
	{
		get => this.stain1;
		set
		{
			if (this.Stain1Id == value?.RowId)
				return;

			this.stain1 = value;

			if (value == null)
			{
				this.nextWriteStain1 = 0;
			}
			else
			{
				this.nextWriteStain1 = (byte)value.RowId;
			}

			this.OnPropertyChanged(nameof(this.Stain1));
		}
	}

	public bool IsStain1None
	{
		get => this.Stain1Id == 0;
		set => this.Stain1Id = 0;
	}

	public byte Stain1Id
	{
		get => this.nextWriteStain1 ?? this.lastReadStain1;
		set
		{
			if (value == this.Stain1Id)
				return;

			this.nextWriteStain1 = value;
			this.OnPropertyChanged(nameof(this.Stain1Id));

			this.stain1 = Studio.Library.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain1Id);
			this.OnPropertyChanged(nameof(this.Stain1));
		}
	}

	public void Clear()
	{
		this.Item = null;
		this.Stain0 = null;
		this.Stain1 = null;
	}

	protected abstract void OnItemChanged(TLibraryType? item);
}
