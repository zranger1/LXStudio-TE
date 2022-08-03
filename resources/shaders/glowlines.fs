
const float PI = 3.14159265359;
const float glowScale = 80.;    // lower values == more glow

// from fabrice neyret: makes interesting, pointy-ended lines
float glowline1(vec2 U, vec4 seg) {
    seg.xy -= U; seg.zw -= U;
    float a = mod ( ( atan(seg.y,seg.x) - atan(seg.w,seg.z) ) / PI, 2.);
    return pow(min( a, 2.-a ),glowScale);
}

// normal 2D distance-from-line-segment function
float glowline2(vec2 p, vec4 seg) {
    vec2 ld = seg.xy - seg.zw;
    vec2 pd = p - seg.zw;

    float bri = 1. - length(pd - ld*clamp( dot(pd, ld)/dot(ld, ld), 0.0, 1.0) );
    return pow(bri,glowScale);
}

// line segment data. The vec4 contains (x1,y1,x2,y2)
vec4 seg1 = vec4(0.2,-0.5,0.35,0.5);
vec4 seg2 = vec4(-0.2,-0.5,0.25,0.5);
vec4 seg3 = vec4(-0.6,-0.5,0.25,0.5);


void mainImage( out vec4 fragColor, in vec2 fragCoord )
{   // normalize coords to range -0.5 to 0.5
    vec2 uv = fragCoord.xy / iResolution.xy - .5;

    vec3 color = vec3(0.);

    // draw some line segments
    color += glowline2( uv, seg1) * vec3(1.,0.,0.); // red
    color += glowline1( uv, seg2) * vec3(0.,1.,0.); // green
    color += glowline2( uv, seg3) * vec3(0.,0.,1.); // blue

    fragColor = vec4(color,1.0);
}