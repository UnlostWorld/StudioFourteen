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

namespace StudioFourteen.IPC;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using System.Threading.Tasks;

public class IPCService
	: ServiceBase
{
	public Task<bool> MareSynchronosLoadMcdfAsync(string fileName, IGameObject target)
	{
		Task<bool>? b = this.Invoke<Task<bool>, string, IGameObject>("MareSynchronos.LoadMcdfAsync", fileName, target);
		if (b == null)
			return Task.FromResult(false);

		return b;
	}

	private TReturn? Invoke<TReturn>(string name)
	{
		if (DalamudServices.PluginInterface == null)
			return default;

		ICallGateSubscriber<TReturn> subscriber = DalamudServices.PluginInterface.GetIpcSubscriber<TReturn>(name);
		return subscriber.InvokeFunc();
	}

	private TReturn? Invoke<TReturn, TArg1>(string name, TArg1 arg1)
	{
		if (DalamudServices.PluginInterface == null)
			return default;

		ICallGateSubscriber<TArg1, TReturn> subscriber = DalamudServices.PluginInterface.GetIpcSubscriber<TArg1, TReturn>(name);
		return subscriber.InvokeFunc(arg1);
	}

	private TReturn? Invoke<TReturn, TArg1, TArg2>(string name, TArg1 arg1, TArg2 arg2)
	{
		if (DalamudServices.PluginInterface == null)
			return default;

		ICallGateSubscriber<TArg1, TArg2, TReturn> subscriber = DalamudServices.PluginInterface.GetIpcSubscriber<TArg1, TArg2, TReturn>(name);
		return subscriber.InvokeFunc(arg1, arg2);
	}

	private TReturn? Invoke<TReturn, TArg1, TArg2, TArg3>(string name, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		if (DalamudServices.PluginInterface == null)
			return default;

		ICallGateSubscriber<TArg1, TArg2, TArg3, TReturn> subscriber = DalamudServices.PluginInterface.GetIpcSubscriber<TArg1, TArg2, TArg3, TReturn>(name);
		return subscriber.InvokeFunc(arg1, arg2, arg3);
	}

	private bool IsPluginInstalled(string pluginName)
	{
		if (DalamudServices.PluginInterface == null)
			return false;

		foreach (IExposedPlugin plugin in DalamudServices.PluginInterface.InstalledPlugins)
		{
			if (plugin.Name == pluginName && plugin.IsLoaded)
			{
				return true;
			}
		}

		return false;
	}
}