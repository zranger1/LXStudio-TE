precision mediump float;

/*
  This comment is here to make sure the missing controls script
  picks up controls we use in this pattern's Java code.
  The controls are:
  iWowTrigger
  iWow1
  levelReact
  iSpeed
*/

#define LINE_COUNT 52
uniform vec4[LINE_COUNT] lines;

const float PI = 3.14159265359;
const float lineNoiseAmplitude = 0.00575;

// line drawing algorithm from Fabrice Neyret: Makes interesting, pointy-ended lines.
// For TE, we also perturb the line slightly as we draw so it's never totally static
float glowline(vec2 U, vec4 seg, float glow) {
    seg.xy -= U; seg.zw -= U;
    float a = mod ( ( atan(seg.y,seg.x) - atan(seg.w,seg.z) ) / PI, 2.);

    a += lineNoiseAmplitude * sin(60. * U.y - iTime * 3.0);
    return pow(min( a, 2.-a ),glow/6.);
}

void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
    // normalize coordinates
    vec2 uv = -1. + 2. * fragCoord / iResolution.xy;

    // draw line segments using animated endpoints from Java
    vec3 color = vec3(0.0);
    float bri = 0.0;

    for (int i = 0; i < LINE_COUNT; i++) {
       bri = clamp(1.5 * glowline( uv, lines[i],iScale),0.0,1.0);
       color += bri * mix(iColorRGB,mix(iColorRGB,iColor2RGB,bri),iWow2);
    }

    fragColor = vec4(color,1.0);
}