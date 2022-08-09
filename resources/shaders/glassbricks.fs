// credits: Dave_Hoskins Hash functions: https://www.shadertoy.com/view/4djSRW

const float layers = 3.;
const float layerDrift = 0.3;

const float PI = 3.141592654;

mat2 rot2D(float r){
    float c = cos(r), s = sin(r);
    return mat2(c, s, -s, c);
}
float nsin(float x) {return sin(x)*.5+.5; }
vec2 nsin(vec2 x) {return sin(x)*.5+.5; }
vec3 hash32(vec2 p){
	vec3 p3 = fract(vec3(p.xyx) * vec3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yxz+19.19);
    return fract((p3.xxy+p3.yzz)*p3.zyx);
}
// returns { RGB, dist to edge (0 = edge, 1 = center) }
vec4 disco(vec2 uv) {
    float v = abs(cos(uv.x * PI * 2.) + cos(uv.y *PI * 2.)) * .5;
    uv.x -= .5;
    vec3 cid2 = hash32(vec2(floor(uv.x - uv.y), floor(uv.x + uv.y))); // generate a color
    return vec4(cid2, v);
}

void mainImage( out vec4 o, in vec2 fragCoord)
{
    vec2 uv = fragCoord / iResolution.xy;

    float t = (iTime+200.) * .6; //t = 0.;
    uv *= 81.;
    uv *= rot2D(sin(t*2.)*.03);
    uv.x += t*3.;

    // start with tile pattern
    vec2 uv2 = uv;
    uv2 += t*layerDrift;// sync with below; pattern drift
    uv2 = sin(uv2-PI*.5)*.5+.5;// sync with below. actually i have no idea why -PI/2
    o.r = min(uv2.x,uv2.y);// grid pattern
    o = vec4(pow(o.r, .4));
    o = clamp(o, 0.,.6)/.6;// plateau

    // layer in bricks
    for(float i = 0.; i <=layers; ++i) {
        uv += sin(uv+t*layerDrift)*(1.+sin(t)*.3);
        vec4 d = disco(uv);
        d.a = pow(d.a, .2);//sin(t*1.2+i)*.5+.5
        o *= clamp(d*d.a,.25, 1.);
    }

    // post
    o = clamp(o,.0,1.);
    vec2 N = (fragCoord / iResolution.xy )- .5;// norm coords
    o = 1.-pow(1.-o, vec4((layers - .5) * 12.));// curve
    o.rgb += hash32(fragCoord + iTime).r*.07;//noise
    o *= 1.1-step(.4,abs(N.y));
    o *= 1.0-dot(N,N*2.);// vingette
    o.a = 1.;
}
