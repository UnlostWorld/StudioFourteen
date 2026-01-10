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

namespace StudioFourteen.Services.Rendering.Meshes.Polygon;

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SharpDX.Direct3D;

public class PolygonMesh : Mesh
{
	public string? Comment;

	private readonly List<VertexProperties> vertexPropertyOrder = new();
	private int vertexCount;
	private int faceCount;
	private Elements processingElement;

	public enum Elements
	{
		None,
		Vertex,
		Face,
	}

	public enum VertexProperties
	{
		X,
		Y,
		Z,
		Nx,
		Ny,
		Nz,
		S,
		T,
		Red,
		Green,
		Blue,
		Alpha,
	}

	public void Load(Stream stream)
	{
		using StreamReader reader = new StreamReader(stream);

		this.Topology = PrimitiveTopology.TriangleList;

		bool isHeader = true;
		while (isHeader && !reader.EndOfStream)
		{
			string? line = reader.ReadLine();
			if (line == null)
				continue;

			isHeader = this.ParseHeaderLine(line);
		}

		if (isHeader)
			throw new Exception("Failed to find end of phy header region");

		for (int i = 0; i < this.vertexCount; i++)
		{
			string? line = reader.ReadLine();
			if (line == null)
				throw new Exception();

			this.ParseVertex(line);
		}

		for (int i = 0; i < this.faceCount; i++)
		{
			string? line = reader.ReadLine();
			if (line == null)
				throw new Exception();

			this.ParseFace(line);
		}
	}

	private bool ParseHeaderLine(string line)
	{
		string[] parts = line.Split(' ');
		if (parts.Length <= 0)
			return true;

		switch (parts[0])
		{
			case "ply":
				break;

			case "format":
			{
				if (parts[1] != "ascii" || parts[2] != "1.0")
					throw new Exception("Unsupported format, only ascii 1.0 format is supported");
				break;
			}

			case "comment":
			{
				this.Comment = line.Substring(8);
				break;
			}

			case "element":
			{
				switch (parts[1])
				{
					case "vertex":
					{
						this.vertexCount = int.Parse(parts[2]);
						this.processingElement = Elements.Vertex;
						break;
					}

					case "face":
					{
						this.faceCount = int.Parse(parts[2]);
						this.processingElement = Elements.Face;
						break;
					}

					default:
					{
						throw new Exception($"Unsupported element type: {parts[1]}");
					}
				}

				break;
			}

			case "property":
			{
				if (this.processingElement == Elements.Vertex)
				{
					if (parts[1] != "float")
						throw new Exception("Unsupported property format. only float property format is supported for vertex values.");

					this.vertexPropertyOrder.Add(Enum.Parse<VertexProperties>(parts[2], true));
				}
				else if (this.processingElement == Elements.Face)
				{
					if (parts[1] != "list")
						throw new Exception("Unsupported property format. only list property format is supported for face order");
				}

				break;
			}

			case "end_header": return false;
		}

		return true;
	}

	private void ParseVertex(string line)
	{
		string[] parts = line.Split(' ');

		if (parts.Length != this.vertexPropertyOrder.Count)
			throw new Exception("Vertex line incorrect length");

		Vertex v = new();
		Vector4 pos = default;
		pos.W = 1;
		Vector4 normal = default;
		normal.W = 1;
		Vector2 tex = default;
		Color col = Color.White;

		for (int i = 0; i < parts.Length; i++)
		{
			switch (this.vertexPropertyOrder[i])
			{
				case VertexProperties.X:
				{
					pos.X = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.Y:
				{
					pos.Y = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.Z:
				{
					pos.Z = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.Nx:
				{
					normal.X = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.Ny:
				{
					normal.Y = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.Nz:
				{
					normal.Z = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.S:
				{
					tex.X = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.T:
				{
					tex.Y = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.Red:
				{
					col.R = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.Green:
				{
					col.G = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.Blue:
				{
					col.B = float.Parse(parts[i]);
					break;
				}

				case VertexProperties.Alpha:
				{
					col.A = float.Parse(parts[i]);
					break;
				}
			}
		}

		v.Position = pos;
		////v.Normal = normal;
		v.Color = Color.White;
		v.TexCoord = tex;

		this.Vertices.Add(v);
	}

	private void ParseFace(string line)
	{
		string[] parts = line.Split(' ');

		if (parts.Length != 4)
			throw new Exception($"Invalid face element count : {parts.Length}");

		if (parts[0] != "3")
			throw new Exception("Only triangulated faces are supported");

		if (this.Indices == null)
			this.Indices = new();

		this.Indices.Add(ushort.Parse(parts[3]));
		this.Indices.Add(ushort.Parse(parts[2]));
		this.Indices.Add(ushort.Parse(parts[1]));
	}
}