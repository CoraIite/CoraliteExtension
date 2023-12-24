sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

// Author: Rigel
// Shader: Mystic Flower
// licence: https://creativecommons.org/licenses/by/4.0/

#define PI 3.141592653589793
#define TWO_PI 6.283185307179586

// radom number in 2d
float hash(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.2333))) * 43758.5453123);
}

        
float lerp2(float a, float b, float f)
{
    return (a * (1.0f - f)) + (b * f);
}


// noise in 2d
float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    return lerp2(lerp2(hash(i + float2(0.0, 0.0)), hash(i + float2(1.0, 0.0)), u.x),
                lerp2(hash(i + float2(0.0, 1.0)), hash(i + float2(1.0, 1.0)), u.x), u.y);
}

// fractal noise in 2d
float fbm(float2 p)
{
    const float2x2 m = float2x2(0.8, -0.6, 0.6, 0.8);
    float f = 0.0;
    f += 0.5000 * noise(p);
    //p *= m * 2.02;
    p = mul(p, m) * 2.02;
    f += 0.2500 * noise(p);
    //p *= m * 2.04; 
    p = mul(p, m) * 2.04;
    f += 0.1250 * noise(p);
    //p *= m * 2.03;
    p = mul(p, m) * 2.03;
    f += 0.0650 * noise(p);
    //p *= m * 2.01;
    p = mul(p, m) * 2.01;

    // normalize f;
    f /= 0.9375;
    return f;
}

// generates a palette with a cosine
// from https://www.shadertoy.com/view/ll2GD3
float3 pal(float domain, float3 frequency, float3 phase)
{
    return float3(0.5, 0.5, 0.5) + float3(0.5, 0.5, 0.5) * cos(TWO_PI * (frequency * domain + phase));
}

float4 ArmorMyShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
  // cordinate system from -2~2 将坐标拉伸一下
    float fx = (coords.x * uImageSize0.x - uSourceRect.x) / uSourceRect.z;
    float fy = (coords.y * uImageSize0.y - uSourceRect.y) / uSourceRect.w;
    float2 p = (float2(-0.5, 0.3) + float2(fx, fy)) * float2(0.5, 1.5);

  // angle and radius to center 0,0 极坐标
    float a = atan(p.y / abs(p.x));
    float r = length(p);

  // space distortion  空间失真
    float i = fbm(float2(a * 2. + uTime * .1, r * .2 - uTime * .1));
    p += float2(i, i) * 1.5;
  // divide the space into cells and get cell index to seed the palette 将空间划分为单元格并获取单元格索引以为调色板提供种子
    float cidx = (floor(p.x + uTime / 2.0) + (floor(p.y - uTime * 2.0) * 1.0)) / 16.0;
  // color is from palette with cell index
    float3 color = pal(fbm(p * .7), float3(0.25, 0.25, 0.25), float3(0.8 + cidx, 0.6 + cidx, 0.0));

  // draw a grid for the cells
    //color *= smoothstep(0.49, 0.44, abs(frac(p.x) - 0.5));
    //color *= smoothstep(0.49, 0.44, abs(frac(p.y) - 0.5));

  // angular distortion
    //a += fbm(p * 0.05);
  // flower white petals //中心花朵
    //float f = abs(cos(a * 9.) * sin(a * 6.)) * .7 + .1;
    //float ff = smoothstep(f, f + 0.25, r);
    //color = ff * color + (1.0 - ff) * float3(0.95, 0.7, 0.8) * (1.8 - r);

  // flower center 花朵中心，因为觉得不是太好看所以给它删了
  //color = mix(color,vec3(1.,1.-r*3.,1.0),smoothstep(0.26,0.1+fbm(vec2(r+iTime,a-iTime))*0.06 ,r));
    //和盔甲亮度混合
    float4 color2 = tex2D(uImage0, coords);
    float brightness =(color2.r + color2.g + color2.b) / 2;
    return float4(color.rgb * brightness, color2.a) * sampleColor;
}

technique Technique1
{
    pass ArmorMyShader
    {
        PixelShader = compile ps_3_0 ArmorMyShader();
    }
}