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

// Thanks Thomas!
// https://stackoverflow.com/questions/42510542/render-thick-lines-with-instanced-rendering-in-directx-11

#include "Geometry.hlsl"

cbuffer MaterialInstanceData : register(MaterialDataRegister)
{
	float4 Color;
};

static const float PI = 3.1415926f;
static const float fRatio = 2.0f;
static float fThickness = 0.01f;
static float zOffset = 0.0001f;
static float fShadowSize = 0.5f;

[maxvertexcount(42)]
void geometry(line Fragment input[2], inout TriangleStream<Fragment> triangleStream)
{
    Fragment output= (Fragment)0;

    int nCountTriangles = 12;

    float4 positionPoint0Transformed = input[0].Position;

    float fPoint0w = positionPoint0Transformed.w;
    //calculate out the W parameter, because of usage of perspective rendering
    positionPoint0Transformed.xyz = positionPoint0Transformed.xyz / positionPoint0Transformed.w;
    positionPoint0Transformed.w = 1.0f;

	output.Color = input[0].Color;
    for (int nI = 0; nI < nCountTriangles; ++nI)
    {
		output.TexCoord = float2(-1,0);
        output.Position.x = cos((PI * 2 / nCountTriangles * nI)) * fThickness / fRatio;
        output.Position.y = sin((PI * 2 / nCountTriangles * nI)) * fThickness;
        output.Position.z = zOffset;
        output.Position.w = 0.0f;
        output.Position += positionPoint0Transformed;
        output.Position *= fPoint0w;
		output.ScreenPosition = output.Position;
        triangleStream.Append(output);

		output.TexCoord = float2(-1,1);
        output.Position = positionPoint0Transformed;
		output.Position.z += zOffset;
		output.Position *= fPoint0w;
		output.ScreenPosition = output.Position;
        triangleStream.Append(output);

		output.TexCoord = float2(-1,0);
        output.Position.x = cos((PI * 2 / nCountTriangles * (nI + 1))) * fThickness / fRatio;
        output.Position.y = sin((PI * 2 / nCountTriangles * (nI + 1))) * fThickness;
        output.Position.z = zOffset;
        output.Position.w = 0.0f;
        output.Position += positionPoint0Transformed;
        output.Position *= fPoint0w;
		output.ScreenPosition = output.Position;
        triangleStream.Append(output);

        triangleStream.RestartStrip();
    }
}

float4 pixel(Fragment frag) : SV_TARGET
{
	float4 color = frag.Color * ObjectColor;

	// End Cap
	if (frag.TexCoord.x < 0)
	{
		if (frag.TexCoord.y < fShadowSize)
		{
			color.rgb = 0;
		}
	}

	color.a *= GetUiClippingAlpha(frag);
	return color;
}