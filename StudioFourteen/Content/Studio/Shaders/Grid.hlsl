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

// Thanks @Javier Salcedo!
// https://dev.to/javiersalcedopuyo/simple-infinite-grid-shader-5fah

#include "Geometry.hlsl"

static const float quadScale = 50.0f;

cbuffer MaterialInstanceData : register(MaterialDataRegister)
{
	float4 Color;
	float4 XColor;
	float4 ZColor;
	float GridSize;
	float LineThickness;
	float DepthOffset;
	float Unused2;
};

Fragment vert(in Vertex vertex)
{
	Fragment frag = DefaultVert(vertex);
	frag.Position.z += DepthOffset;
	frag.ScreenPosition.z += DepthOffset;
	return frag;
}

float4 pixel(Fragment frag) : SV_TARGET
{
	float largeGridSize = GridSize * 10;
	float2 cell_coords = mod((frag.WorldPosition.xz * 1) + (largeGridSize * 0.5f), largeGridSize);
	float2 distance_to_cell = abs(cell_coords - (largeGridSize * 0.5f));

	float2 subcell_coords = mod((frag.WorldPosition.xz * 1) + (GridSize * 0.5f), GridSize);
	float2 distance_to_subcell = abs(subcell_coords - (GridSize * 0.5f));

	float lineThickness = LineThickness * ViewportScale;

	float4 color = Color;
	color.a = 0;
	if (any(distance_to_subcell < (lineThickness / 100) * (frag.Position.w * 2)))
	{
		color.a = 0.5;
	}

	if(distance_to_cell.y < (lineThickness / 100) * (frag.Position.w * 2))
	{
		color = XColor;
		color.a = 1;
	}
	if(distance_to_cell.x < (lineThickness / 100) * (frag.Position.w * 2))
	{
		color = ZColor;
		color.a = 1;
	}

	color.a *= Color.a;
	color.a *= GetClippingAlpha(frag, 0.0f);

	// Fade out towards the ends
	color.a *= lerp(1, 0, (length(frag.TexCoord - float2(0.5, 0.5)) * 2));

	return color;
}