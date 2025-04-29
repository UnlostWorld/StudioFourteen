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

// https://dev.to/javiersalcedopuyo/simple-infinite-grid-shader-5fah

#include "Common.hlsl"

#define mod(x,y) ((x) - (y) * floor((x)/(y)))

struct Constants
{
	float4 ClippingPlanes;
    float4x4 ViewProjection;
	float4x4 ObjectTransform;
	float4 ObjectColor;
};

struct Fragment
{
    float4 Position:SV_POSITION;
    float4 Color:COLOR;
	float2 TexCoord:TEXCOORD;
	float4 ScreenPosition:SCREENPOS;
};

static const float grid_size = 100.0f;
static const float cell_size = 1.0f;

static const float cell_line_thickness = 0.0002f;
static const float half_cell_size = cell_size * 0.5f;
static const float subcell_size = 0.1f;
static const float half_subcell_size = subcell_size * 0.5f;
static const float subcell_line_thickness = 0.0001f;

Constants constants : register(c0);

Texture2D mask_texture : register(t0);
SamplerState mask_sampler : register(s0);

Texture2D depth_texture : register(t1);
SamplerState depth_sampler : register(s1);

float GetDepth(Fragment pixel)
{
	float3 pos = pixel.ScreenPosition.xyz / pixel.ScreenPosition.w;
	return pos.z;
}

float2 GetScreenPosition(Fragment pixel)
{
	float3 pos = pixel.ScreenPosition.xyz / pixel.ScreenPosition.w;
	return 0.5f * float2(pos.x, -pos.y) + 0.5f;
}

float GetClippingAlpha(Fragment pixel, float depthClipAlpha = 0)
{
	float2 screenPos = GetScreenPosition(pixel);
	float mask = mask_texture.Sample(mask_sampler, screenPos).r;
	float depth = depth_texture.Sample(depth_sampler, screenPos).r;
	float thisDepth = GetDepth(pixel);

	if (thisDepth < depth)
		mask = mask * depthClipAlpha;

	return mask;
}

Fragment vert(in Vertex vertex)
{
	float4 position = vertex.Position;
	position.xyz *= grid_size;
	position = mul(position, constants.ObjectTransform);
	position = mul(position, constants.ViewProjection);

	Fragment result;
	result.Position = position;
	result.Color = vertex.Color;
	result.TexCoord = vertex.TexCoord;
	result.ScreenPosition = position;
	return result;
}

float4 pixel(Fragment frag) : SV_TARGET
{
	float2 cell_coords = mod((frag.TexCoord * (grid_size / 5)) + half_cell_size, cell_size);
	float2 distance_to_cell = abs(cell_coords - half_cell_size);

	float2 subcell_coords = mod((frag.TexCoord * (grid_size / 5)) + half_subcell_size, subcell_size);
	float2 distance_to_subcell = abs(subcell_coords - half_subcell_size);

	float r = smoothstep(0, grid_size, frag.Position.w);
	r = 1 - r;

	float4 color = constants.ObjectColor;
	color.a = 0;
	if (any(distance_to_subcell < subcell_line_thickness * frag.Position.w))
	{
		color.a = 0.2f;
	}

	if(any(distance_to_cell < cell_line_thickness * frag.Position.w))
	{
		color.a = 0.5f;
	}

	color.a *= GetClippingAlpha(frag, 0.0f);
	color.a *= r;
	return color;
}