// based on https://www.shadertoy.com/view/XdByD1

vec2 csqr( vec2 a )  { return vec2( a.x*a.x - a.y*a.y, 2.*a.x*a.y  ); }

vec2 iSphere( in vec3 ro, in vec3 rd, in vec4 sph )//from iq
{
	vec3 oc = ro;// - sph.xyz;
	float b = dot( oc, rd );
	float c = dot( oc, oc ) - sph.w*sph.w;
	float h = 1.0; //b*b - c;
	return vec2(-b-h, -b+h );
}

float map(in vec3 p, vec2 sctime) {

	float res = 0.;

    vec3 c = p;
    c.xy = c.xy * sctime.x + vec2(c.y, c.x) * sctime.y;
	for (int i = 0; i < 4; ++i)  // 10
    {
        p =.7*abs(p)/dot(p,p) -.7;
        p.yz= csqr(p.yz);
        p=p.zxy;
        res += exp(-19. * abs(dot(p,c)));
	}
	return res * 1.5;
}

vec3 raymarch( in vec3 ro, vec3 rd, vec2 tminmax , vec2 sctime)
{
    //tminmax += vec2(1.,1.) * sin( iTime * 1.3)*3.0;
   	vec3 one3 = vec3(1.,1.,1.);
    vec3 t = one3 * tminmax.x;

    vec3 dt = vec3(.07, 0.02, 0.05);
    vec3 col= vec3(0.);
    vec3 c = one3 * 0.;
    for( int i=0; i<32; i++ )
	{
     	vec3 s = vec3(2.0, 3.0, 4.0);
        t+=dt*exp(-s*c);
        vec3 a = step(t,one3*tminmax.y);
        vec3 pos = ro+t*rd;

        c.x = map(ro+t.x*rd, sctime);
        c.y = map(ro+t.y*rd, sctime);
        c.z = map(ro+t.z*rd, sctime);

        col = mix(col, .99*col+ .08*c*c*c, a);
    }

    vec3 c0 = vec3(0.4,0.3,0.99);
    vec3 c1 = vec3(0.9,0.7,0.0);
    vec3 c2 = vec3(0.9,0.1,0.2);
    return c0 * col.x + c1 * col.y + c2 * col.z;
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
	float time = iTime;
    vec2 q = fragCoord.xy / iResolution.xy;
    vec2 p = -1.0 + 2.0 * q;
    p.x *= iResolution.x/iResolution.y;
    p -= vec2(0.,-0.43);
    vec2 m = vec2(0.);

    vec3 ro = vec3(6.);
    vec3 ta = vec3( 0.0 , 0.0, 0.0 );
    vec3 ww = normalize( ta - ro );
    vec3 uu = normalize( cross(ww,vec3(0.0,1.0,0.0) ) );
    vec3 vv = normalize( cross(uu,ww));
    vec3 rd = normalize( p.x*uu + p.y*vv + 4.0*ww );


    vec2 tmm = iSphere( ro, rd, vec4(0.,0.,0.,2.) );

	// raymarch
    vec3 col = raymarch(ro,rd,tmm, vec2(sin(iTime), cos(iTime)));

	// shade
    col =  .5 *(log(1.+col));
    col = clamp(col,0.,1.);
    fragColor = vec4( col, 1.0 );
}
