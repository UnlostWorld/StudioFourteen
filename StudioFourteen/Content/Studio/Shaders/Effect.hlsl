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

#include "Common.hlsl"

Texture2D mask_texture : register(t0);
SamplerState mask_sampler : register(s0);

Texture2D depth_texture : register(t1);
SamplerState depth_sampler : register(s1);

Texture2D buffer_texture : register(t2);
SamplerState buffer_sampler : register(s2);

cbuffer EffectPassData : register(b0)
{
    float2 ScreenSize;
	float2 RenderScale;
};

struct Pixel
{
	float4 Position:SV_POSITION;
	float2 TexCoord:TEXCOORD;
};

Pixel vert(in Vertex vertex)
{
	Pixel result;
	result.TexCoord = vertex.TexCoord;
	result.Position = vertex.Position;
	return result;
}