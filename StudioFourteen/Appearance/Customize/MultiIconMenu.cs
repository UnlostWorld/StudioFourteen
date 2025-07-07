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

namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.GameData;
using System;
using System.Collections.Generic;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public class MultiIconMenu : MakeMenuViewModel
{
	private readonly CharaMakeType makeType;
	private byte faceType = 255;
	private FacialFeatureIndex lastUiValue = FacialFeatureIndex.None;

	public MultiIconMenu(CharaMakeType makeType, CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex)
		: base(makeMenu, customizeIndex, ToggleModes.None)
	{
		this.makeType = makeType;
		this.Options = new();
	}

	[Flags]
	public enum FacialFeatureIndex
	{
		None = 0,
		First = 1,
		Second = 2,
		Third = 4,
		Fourth = 8,
		Fifth = 16,
		Sixth = 32,
		Seventh = 64,
		LegacyTattoo = 128,
	}

	public List<Option>? Options { get; set; }

	public override unsafe void OnGameTick(Character character)
	{
		base.OnGameTick(character);

		byte faceType = character.GetCustomizeValue(CustomizeIndex.FaceType);
		if (faceType != this.faceType)
		{
			this.faceType = faceType;
			this.OnFaceChanged(faceType);
		}

		if (this.Options != null)
		{
			FacialFeatureIndex currentUiValue = FacialFeatureIndex.None;
			foreach (Option option in this.Options)
			{
				if (option.IsChecked)
				{
					currentUiValue |= option.Index;
				}
			}

			if (currentUiValue != this.lastUiValue)
			{
				this.lastUiValue = currentUiValue;
				this.Value = (byte)currentUiValue;
			}

			FacialFeatureIndex value = (FacialFeatureIndex)this.RealValue;
			foreach (Option option in this.Options)
			{
				option.IsChecked = value.HasFlag(option.Index);
			}
		}
	}

	protected override void OnValueChanged(byte oldValue, byte newValue)
	{
		base.OnValueChanged(oldValue, newValue);
	}

	private void OnFaceChanged(byte faceType)
	{
		this.Options = null;
		this.RaisePropertyChanged(nameof(this.Options));

		this.Options = new();

		if (faceType < 0 || faceType > this.makeType.FacialFeatureOption.Count)
			return;

		CharaMakeType.FaceTypeOptions faceOptions = this.makeType.FacialFeatureOption[faceType];

		this.Options.Add(new(new ImageReference(faceOptions.Options[0]), FacialFeatureIndex.First));
		this.Options.Add(new(new ImageReference(faceOptions.Options[1]), FacialFeatureIndex.Second));
		this.Options.Add(new(new ImageReference(faceOptions.Options[2]), FacialFeatureIndex.Third));
		this.Options.Add(new(new ImageReference(faceOptions.Options[3]), FacialFeatureIndex.Fourth));
		this.Options.Add(new(new ImageReference(faceOptions.Options[4]), FacialFeatureIndex.Fifth));
		this.Options.Add(new(new ImageReference(faceOptions.Options[5]), FacialFeatureIndex.Sixth));
		this.Options.Add(new(new ImageReference(faceOptions.Options[6]), FacialFeatureIndex.Seventh));
		this.Options.Add(new(new ImageReference(0), FacialFeatureIndex.LegacyTattoo));

		FacialFeatureIndex value = (FacialFeatureIndex)this.RealValue;
		foreach (Option option in this.Options)
		{
			option.IsChecked = value.HasFlag(option.Index);
		}

		this.lastUiValue = value;

		this.RaisePropertyChanged(nameof(this.Options));
	}

	public class Option(ImageReference image, FacialFeatureIndex index)
	{
		public ImageReference Image => image;
		public FacialFeatureIndex Index => index;
		public bool IsChecked { get; set; }
	}
}
