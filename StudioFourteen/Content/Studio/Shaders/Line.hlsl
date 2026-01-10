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
static float fThickness = 0.0025f;

Fragment vert(in Vertex vertex)
{
	return DefaultVert(vertex);
}

void addHalfCircle(inout TriangleStream<Fragment> triangleStream, int nCountTriangles, float4 linePointToConnect, float fPointWComponent, float fAngle, float4 color)
{
    Fragment output = (Fragment)0;
	output.Color = color;
    for (int nI = 0; nI < nCountTriangles; ++nI)
    {
        output.Position.x = cos(fAngle + (PI / nCountTriangles * nI)) * fThickness / fRatio;
        output.Position.y = sin(fAngle + (PI / nCountTriangles * nI)) * fThickness;
        output.Position.z = 0.0f;
        output.Position.w = 0.0f;
        output.Position += linePointToConnect;
        output.Position *= fPointWComponent;
		output.ScreenPosition = output.Position;
        triangleStream.Append(output);

        output.Position = linePointToConnect * fPointWComponent;
		output.ScreenPosition = output.Position;
        triangleStream.Append(output);

        output.Position.x = cos(fAngle + (PI / nCountTriangles * (nI + 1))) * fThickness / fRatio;
        output.Position.y = sin(fAngle + (PI / nCountTriangles * (nI + 1))) * fThickness;
        output.Position.z = 0.0f;
        output.Position.w = 0.0f;
        output.Position += linePointToConnect;
        output.Position *= fPointWComponent;
		output.ScreenPosition = output.Position;
        triangleStream.Append(output);

        triangleStream.RestartStrip();
    }
}

[maxvertexcount(42)]
void geometry(line Fragment input[2], inout TriangleStream<Fragment> triangleStream)
{
    Fragment output= (Fragment)0;

    int nCountTriangles = 6;

    float4 positionPoint0Transformed = input[0].Position;
    float4 positionPoint1Transformed = input[1].Position;

    float fPoint0w = positionPoint0Transformed.w;
    float fPoint1w = positionPoint1Transformed.w;

    //calculate out the W parameter, because of usage of perspective rendering
    positionPoint0Transformed.xyz = positionPoint0Transformed.xyz / positionPoint0Transformed.w;
    positionPoint0Transformed.w = 1.0f;
    positionPoint1Transformed.xyz = positionPoint1Transformed.xyz / positionPoint1Transformed.w;
    positionPoint1Transformed.w = 1.0f;

    //calculate the angle between the 2 points on the screen
    float3 positionDifference = positionPoint0Transformed.xyz - positionPoint1Transformed.xyz;
    float3 coordinateSysten = float3(1.0f, 0.0f, 0.0f);

    positionDifference.z = 0.0f;
    coordinateSysten.z = 0.0f;

    float fAngle = acos(dot(positionDifference.xy, coordinateSysten.xy) / (length(positionDifference.xy) * length(coordinateSysten.xy)));

    if (cross(positionDifference, coordinateSysten).z < 0.0f)
    {
        fAngle = 2.0f * PI - fAngle;
    }

    fAngle *= -1.0f;
    fAngle -= PI * 0.5f;

    //first half circle of the line
    addHalfCircle(triangleStream, nCountTriangles, positionPoint0Transformed, fPoint0w, fAngle, input[0].Color);
    addHalfCircle(triangleStream, nCountTriangles, positionPoint1Transformed, fPoint1w, fAngle + PI, input[1].Color);

    //connection between the two circles
    //triangle1
	output.Color = input[0].Color;
    output.Position.x = cos(fAngle) * fThickness / fRatio;
    output.Position.y = sin(fAngle) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint0Transformed;
    output.Position *= fPoint0w; //undo calculate out the W parameter, because of usage of perspective rendering
	output.ScreenPosition = output.Position;
	triangleStream.Append(output);

    output.Position.x = cos(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness / fRatio;
    output.Position.y = sin(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint0Transformed;
    output.Position *= fPoint0w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

    output.Position.x = cos(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness / fRatio;
    output.Position.y = sin(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint1Transformed;
    output.Position *= fPoint1w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

    //triangle2
	output.Color = input[1].Color;
    output.Position.x = cos(fAngle) * fThickness / fRatio;
    output.Position.y = sin(fAngle) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint0Transformed;
    output.Position *= fPoint0w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

    output.Position.x = cos(fAngle) * fThickness / fRatio;
    output.Position.y = sin(fAngle) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint1Transformed;
    output.Position *= fPoint1w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

    output.Position.x = cos(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness / fRatio;
    output.Position.y = sin(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint1Transformed;
    output.Position *= fPoint1w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);
}

float4 pixel(Fragment pixel) : SV_TARGET
{
	float4 color = pixel.Color * ObjectColor;
	color.a *= GetClippingAlpha(pixel, 0.1);
	return color;
}