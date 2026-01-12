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

namespace StudioFourteen.Services.Xivalonia.Platform;

using System;
using System.Reflection;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Win32;

public static class GlManager
{
	public static IPlatformGraphics? Initialize()
	{
		IPlatformGraphics? gl = InitializeCore();
		if (gl == null)
			throw new Exception("Failed to create platform graphics");

		AvaloniaLocator.CurrentMutable.Bind<IPlatformGraphics>().ToConstant(gl);
		return gl;
	}

	private static IPlatformGraphics? InitializeCore()
	{
		AngleOptions options = AvaloniaLocator.Current.GetService<AngleOptions>() ?? new();
		options.AllowedPlatformApis = [AngleOptions.PlatformApi.DirectX11];

		Type? angleWin32PlatformGraphicsFactoryType = typeof(AngleOptions).Assembly.GetType("Avalonia.Win32.OpenGl.Angle.AngleWin32PlatformGraphicsFactory");
		if (angleWin32PlatformGraphicsFactoryType == null)
			throw new Exception("Failed to get Win32 ANGLE graphics factory");

		MethodInfo? method = angleWin32PlatformGraphicsFactoryType.GetMethod("TryCreate");
		if (method == null)
			throw new Exception("Failed to get Win32 ANGLE graphics factory create method");

		IPlatformGraphics? egl = method.Invoke(null, [options]) as IPlatformGraphics;
		if (method == null)
			throw new Exception("Failed to create platform graphics");

		if (egl != null && egl.GetType().Name == "D3D11AngleWin32PlatformGraphics")
		{
			Studio.Log.Information("Initialized D3D11 ANGLE platform graphics");
		}

		return egl;
	}
}
