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

namespace StudioFourteen.Rendering;

using System;
using System.IO;
using System.Reflection;
using StudioFourteen.Serialization;

public class EmbeddedGeometry(string file) : Geometry
{
	public override string ToString() => $"Geometry file {file}";

	protected override Mesh LoadMesh()
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		string resourceName = $"StudioFourteen.Rendering.Meshes.{file}";
		Stream? stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null)
			throw new Exception($"Mesh \"{file}\" not found in manifest resources");

		Mesh? mesh = Serializer.Deserialize<Mesh>(stream);
		if (mesh == null)
			throw new Exception($"Mesh \"{file}\" failed to deserialize");

		return mesh;
	}
}