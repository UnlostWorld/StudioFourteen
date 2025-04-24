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
};

struct Vertex
{
	float4 Position:POSITION;
	float4 Color:COLOR;
	float2 TexCoord:TEXCOORD;
};

struct Pixel
{
    float4 Position:SV_POSITION;
    float4 Color:COLOR;
	float2 TexCoord:TEXCOORD;
	float4 ScreenPosition:SCREENPOS;
};

Constants constants : register(c0);

Texture2D maskDepth_texture : register(t0);
SamplerState maskDepth_sampler : register(s0);

float2 GetMaskDepth(Pixel pixel)
{
	pixel.ScreenPosition.xyz = pixel.ScreenPosition.xyz / pixel.ScreenPosition.w;
	pixel.ScreenPosition.xy = 0.5f * float2(pixel.ScreenPosition.x, -pixel.ScreenPosition.y) + 0.5f;
	return maskDepth_texture.Sample(maskDepth_sampler, pixel.ScreenPosition.xy).rg;
}