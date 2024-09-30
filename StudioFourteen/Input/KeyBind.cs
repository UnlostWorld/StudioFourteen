// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Input/KeyBind.cs

namespace StudioFourteen.Input;

using Dalamud.Game.ClientState.Keys;

public class KeyBind
{
	public KeyBind()
	{
	}

	public KeyBind(VirtualKey key, bool control = false, bool alt = false, bool shift = false)
	{
		this.Key = key;
		this.Control = control;
		this.Alt = alt;
		this.Shift = shift;
	}

	public VirtualKey Key { get; set; }
	public bool Control { get; set; }
	public bool Alt { get; set; }
	public bool Shift { get; set; }

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
