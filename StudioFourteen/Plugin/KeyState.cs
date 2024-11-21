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

namespace StudioFourteen.Plugin;

using Dalamud.Game.ClientState.Keys;

using Dalamud.Utility;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public class KeyState
{
	protected readonly ILogger Log = Logging.ForContext<KeyState>();

	// The array is accessed in a way that this limit doesn't appear to exist
	// but there is other state data past this point, and keys beyond here aren't
	// generally valid for most things anyway
	private const int MaxKeyCode = 0xF0;
	private IntPtr bufferBase;
	private IntPtr indexBase;
	private VirtualKey[]? validVirtualKeyCache;

	public enum KeyValue
	{
		Up = 0,
		Unk = 1,
		Down = 2,
		Pressed = 3,
		Released = 4,
	}

	public KeyValue this[VirtualKey vkCode]
	{
		get => (KeyValue)this.GetValue(vkCode);
		set => this.SetValue(vkCode, value);
	}

	public int GetValue(VirtualKey key) => this.GetRefValue(key);
	public void SetValue(VirtualKey key, KeyValue value) => this.GetRefValue(key) = (int)value;

	public bool IsVirtualKeyValid(VirtualKey key) => this.ConvertVirtualKey(key) != 0;
	public IEnumerable<VirtualKey> GetValidVirtualKeys() => this.validVirtualKeyCache ??= Enum.GetValues<VirtualKey>().Where(this.IsVirtualKeyValid).ToArray();

	public void Attach()
	{
		if (DalamudServices.SigScanner == null)
			return;

		// These resolve to fixed offsets only, without the base address added in, so GetStaticAddressFromSig() can't be used.
		// lea   rcx, ds:1DB9F74h[rax*4]          KeyboardState
		// movzx edx, byte ptr [rbx+rsi+1D5E0E0h] KeyboardStateIndexArray
		nint moduleBaseAddress = DalamudServices.SigScanner.Module.BaseAddress;
		nint keyboardStateAddress = DalamudServices.SigScanner.ScanText("48 8D 0C 85 ?? ?? ?? ?? 8B 04 31 85 C2 0F 85") + 0x4;
		nint keyboardStateIndexArrayAddress = DalamudServices.SigScanner.ScanText("0F B6 94 33 ?? ?? ?? ?? 84 D2") + 0x4;

		this.bufferBase = moduleBaseAddress + Marshal.ReadInt32(keyboardStateAddress);
		this.indexBase = moduleBaseAddress + Marshal.ReadInt32(keyboardStateIndexArrayAddress);
	}

	public void Detach()
	{
		this.bufferBase = IntPtr.Zero;
		this.indexBase = IntPtr.Zero;
	}

	public void ClearAll()
	{
		foreach (var vk in this.GetValidVirtualKeys())
		{
			this[vk] = 0;
		}
	}

	/// <summary>
	/// Converts a virtual key into the equivalent value that the game uses.
	/// Valid values are non-zero.
	/// </summary>
	/// <param name="key">Virtual key.</param>
	/// <returns>Converted value.</returns>
	private unsafe byte ConvertVirtualKey(VirtualKey key)
	{
		if (this.indexBase == IntPtr.Zero)
			throw new Exception("Attempt to access key state while not attached");

		if (key <= 0 || (int)key >= MaxKeyCode)
			return 0;

		return *(byte*)(this.indexBase + (int)key);
	}

	/// <summary>
	/// Gets the raw value from the key state array.
	/// </summary>
	/// <param name="key">Virtual key code.</param>
	/// <returns>A reference to the indexed array.</returns>
	private unsafe ref int GetRefValue(VirtualKey key)
	{
		if (this.bufferBase == IntPtr.Zero)
			throw new Exception("Attempt to access key state while not attached");

		byte index = this.ConvertVirtualKey(key);
		if (index == 0)
			throw new ArgumentException($"Invalid Key: {key}");

		return ref *(int*)(this.bufferBase + (4 * index));
	}
}
