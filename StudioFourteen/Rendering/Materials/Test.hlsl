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

struct Constants
{
    float4x4 ViewProjection;
	float4 Color;
};

struct Vertex
{
	float4 Position:POSITION;
	float4 Color:COLOR;
};

struct VertexResult
{
    float4 Position:SV_POSITION;
    float4 Color:COLOR;
	float4 WorldPosition:WORLD;
};

Constants constants : register(c0);

VertexResult vert(in Vertex vertex)
{
	VertexResult result;
	result.Position = mul(vertex.Position, constants.ViewProjection);
	result.Color = vertex.Color;
	result.WorldPosition = vertex.Position;
	return result;
}

float4 pixel(VertexResult vertex) : SV_TARGET
{
	return vertex.Color;
}