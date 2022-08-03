
const float PI = 3.14159265359;
const float glowScale = 80.;

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

vec4 seg1 = vec4(0.2,-0.5,0.35,0.5);
vec4 seg2 = vec4(-0.2,-0.5,0.25,0.5);
vec4 seg3 = vec4(-0.6,-0.5,0.25,0.5);


void mainImage( out vec4 O, vec2 U )
{
    U = U / iResolution.xy - .5;
       
    float bri = glowline2( U, seg1);
    bri += glowline1( U, seg2);    
    bri += glowline2( U, seg3);        
    
    O = vec4( bri);
}