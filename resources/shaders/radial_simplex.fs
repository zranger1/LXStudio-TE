// Radial Simplex noise for Titanic's End 2023
//
// Uses the Ashima Arts 2D/3D/4D simplex noise generator
//
// Copyright (C) 2011 Ashima Arts. All rights reserved.
// Distributed under the MIT License. See LICENSE file.
// https://github.com/ashima/webgl-noise
//

const float PI = 3.1415924;
const float TAU = PI * 2.0;
const float QUARTER_WAVE = PI / 2.0;

vec3 mod289(vec3 x) {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod289(vec4 x) {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 permute(vec4 x) {
    return mod289(((x * 34.0) + 1.0) * x);
}

vec4 taylorInvSqrt(vec4 r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

float snoise(vec3 v)
{
    const vec2 C = vec2(1.0 / 6.0, 1.0 / 3.0);
    const vec4 D = vec4(0.0, 0.5, 1.0, 2.0);

    // First corner
    vec3 i = floor(v + dot(v, C.yyy));
    vec3 x0 = v - i + dot(i, C.xxx);

    // Other corners
    vec3 g = step(x0.yzx, x0.xyz);
    vec3 l = 1.0 - g;
    vec3 i1 = min(g.xyz, l.zxy);
    vec3 i2 = max(g.xyz, l.zxy);

    //   x0 = x0 - 0.0 + 0.0 * C.xxx;
    //   x1 = x0 - i1  + 1.0 * C.xxx;
    //   x2 = x0 - i2  + 2.0 * C.xxx;
    //   x3 = x0 - 1.0 + 3.0 * C.xxx;
    vec3 x1 = x0 - i1 + C.xxx;
    vec3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
    vec3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

    // Permutations
    i = mod289(i);
    vec4 p = permute(permute(permute(
                                 i.z + vec4(0.0, i1.z, i2.z, 1.0))
                             + i.y + vec4(0.0, i1.y, i2.y, 1.0))
                     + i.x + vec4(0.0, i1.x, i2.x, 1.0));

    // Gradients: 7x7 points over a square, mapped onto an octahedron.
    // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
    float n_ = 0.142857142857; // 1.0/7.0
    vec3 ns = n_ * D.wyz - D.xzx;

    vec4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

    vec4 x_ = floor(j * ns.z);
    vec4 y_ = floor(j - 7.0 * x_);    // mod(j,N)

    vec4 x = x_ * ns.x + ns.yyyy;
    vec4 y = y_ * ns.x + ns.yyyy;
    vec4 h = 1.0 - abs(x) - abs(y);

    vec4 b0 = vec4(x.xy, y.xy);
    vec4 b1 = vec4(x.zw, y.zw);

    //vec4 s0 = vec4(lessThan(b0,0.0))*2.0 - 1.0;
    //vec4 s1 = vec4(lessThan(b1,0.0))*2.0 - 1.0;
    vec4 s0 = floor(b0) * 2.0 + 1.0;
    vec4 s1 = floor(b1) * 2.0 + 1.0;
    vec4 sh = -step(h, vec4(0.0));

    vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    vec3 p0 = vec3(a0.xy, h.x);
    vec3 p1 = vec3(a0.zw, h.y);
    vec3 p2 = vec3(a1.xy, h.z);
    vec3 p3 = vec3(a1.zw, h.w);

    //Normalise gradients
    vec4 norm = taylorInvSqrt(vec4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;

    // Mix final noise value
    vec4 m = max(0.6 - vec4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;
    return 42.0 * dot(m * m, vec4(dot(p0, x0), dot(p1, x1),
                                  dot(p2, x2), dot(p3, x3)));
}

//END ASHIMA NOISE ///////////////////////////////////////////

const float levels = 5.0;
const float cutoff = 0.15;

// build 2D rotation matrix
mat2 rotate2D(float a) {
    return mat2(cos(a), -sin(a), sin(a), cos(a));
}

// generate 4 octave noise field
float noiseField(vec3 uv) {
    float noise = 0.0;
    float scale = 1.0;
    float div = 0.0;

    for (int i = 1; i <= 4; i++) {
        noise += snoise(vec3(uv * scale)) * 1. / scale;
        div += 1. / scale;
        scale *= 2.0;
    }

    noise = 0.5 + noise / div;
    return pow(noise, iQuantity);
}

float wave(float n) {
    return 0.5 + 0.5 * sin(-QUARTER_WAVE + TAU * fract(n));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    float noise;

    // normalize, scale, rotate
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.xy;
    uv *= iScale;

    // cheat - mirror across x axis to get rid of the tiling
    // discontinuity.
    uv.x = abs(uv.x);

    float dist = length(uv);
    uv.x = atan(uv.x, uv.y) + iRotationAngle;
    uv.y = dist;

    float k = iTime * 0.5;

    // Wow1 selects output mode by changing how we parameterize the
    // 3D noise field.
    if (iWow1 >= 2.0) {
        // fountains o' noise
        noise = noiseField(vec3(iQuantity * sin(uv.x - uv.y + k),0.0, uv.y-k));
    }
    else {
        if (iWow1 >= 1.0) {
            // concentric noise bands
            noise = noiseField(vec3(k,uv.y - k,uv.y));
        }
        else {
            // smoky noise rings
            noise = noiseField(vec3(uv.x, iQuantity * sin(uv.y - k), uv.y - k));
        }
    }

    // bump up the gain a little, and adjust gamma
    noise *= 1.15;
    noise = noise * noise;

    // Wow2 controls the mix of foreground color vs. gradient
    vec3 col = noise * mix(iColorRGB, mix(iColorRGB, iColor2RGB, wave(noise)), iWow2);

    fragColor = vec4(col, 1.0);
}