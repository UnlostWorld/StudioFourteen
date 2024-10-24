// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Input/KeyBind.cs

namespace StudioFourteen.Input;

using Dalamud.Game.ClientState.Keys;
using System.Windows.Input;

public class KeyBind
{
	public KeyBind()
	{
	}

	public KeyBind(VirtualKey key, ModifierKeys modifiers = ModifierKeys.None)
	{
		this.Key = key;
		this.Modifiers = modifiers;
	}

	public VirtualKey Key { get; set; }
	public ModifierKeys Modifiers { get; set; }

	public bool Control => this.Modifiers.HasFlag(ModifierKeys.Control);
	public bool Alt => this.Modifiers.HasFlag(ModifierKeys.Alt);
	public bool Shift => this.Modifiers.HasFlag(ModifierKeys.Shift);

	public bool GetIsEmpty()
	{
		return this.Key == VirtualKey.NO_KEY;
	}

	public override string ToString()
	{
		if (!this.Control && !this.Alt && !this.Shift)
			return this.Key.GetFancyName();

		string str = string.Empty;

		if (this.Control)
			str += "Ctrl, ";

		if (this.Alt)
			str += "Alt, ";

		if (this.Shift)
			str += "Shift, ";

		str += this.Key.GetFancyName();
		return str;
	}
}
