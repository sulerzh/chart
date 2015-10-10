/// <class>GloomEffect</class>

/// <description>An effect that intensifies dark regions.</description>

//-----------------------------------------------------------------------------------------
// Shader constant register mappings (scalars - float, double, Point, Color, Point3D, etc.)
//-----------------------------------------------------------------------------------------

/// <summary>Gets or sets the intensity of the gloom image.</summary>
/// <minValue>0</minValue>
/// <maxValue>1</maxValue>
/// <defaultValue>1</defaultValue>
float GloomIntensity : register(C0);

/// <summary>Gets or sets the intensity of the base image.</summary>
/// <minValue>0</minValue>
/// <maxValue>1</maxValue>
/// <defaultValue>0.5</defaultValue>
float BaseIntensity : register(C1);

/// <summary>Gets or sets the saturation of the gloom image.</summary>
/// <minValue>0</minValue>
/// <maxValue>1</maxValue>
/// <defaultValue>0.2</defaultValue>
float GloomSaturation : register(C2);

/// <summary>Gets or sets the saturation of the base image.</summary>
/// <minValue>0</minValue>
/// <maxValue>1</maxValue>
/// <defaultValue>1</defaultValue>
float BaseSaturation : register(C3);

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float3 AdjustSaturation(float3 color, float saturation)
{
    float grey = dot(color, float3(0.3, 0.59, 0.11));
    return lerp(grey, color, saturation);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float GloomThreshold = 0.25;

	float4 color = tex2D(implicitInputSampler, uv);
	if (color.a == 0)
		discard;
	
    float3 base = 1 - color.rgb / color.a;
    float3 gloom = saturate((base - GloomThreshold) / (1 - GloomThreshold));
    
    // Adjust color saturation and intensity.
    gloom = AdjustSaturation(gloom, GloomSaturation) * GloomIntensity;
    base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
    
    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    base *= (1 - saturate(gloom));
    
    // Combine the two images.
    return float4((1 - (base + gloom)) * color.a, color.a);
}
