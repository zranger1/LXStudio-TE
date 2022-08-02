/*
  Xorcery

  An XOR in 3D space based on the 'block reflections' pattern.
*/

var scale = 5 //scale coordinates for xor
var xOffs,yOffs
var wrap
export function beforeRender(delta) {
  t1 = time(.1)
  t2 = sin(time(.1) * PI2)
  t3 = triangle(time(.5)) * 10
  t4 = sin(time(.34) * PI2) * 4

  k = wave(measure())
  xOffs = k * .1 + t1
  yOffs = k * .2
  wrap = .3 + triangle(t1) * .2
}

function xorf(v1, v2) {
  v1 *= 65536
  v2 *= 65536
  return (v1 ^ v2) / 65536
}

export function render3D(index, x, y, z) {
  y += yOffs //bounce / fall
  // y += time(.1) //blittery waterfalls!
  x += xOffs //breathing

  // Calculate hue:  xor coordinates, vary density/detail, and wrap into a variable range
  h = t2 + wave(xorf(scale * (x - .5), xorf(scale * (z - .5),scale * (y - .5)))
                / 50 * (t3 + t4) % wrap)
  v = (abs(h) + abs(wrap) + t1) % 1
  v = triangle(v * v)
  v = v * v

  //original hsv calculates teal through purple colors (.45 - .85):
  // h = triangle(h) * .2 + triangle(x + y + z) * .2 + .45
  // hsv(h, 1, v)

  //for paint(), don't downscale the range
  h = triangle(h) + triangle(x + y + z)
  paint(h) //color gradient based on edge/panel
  setAlpha(v) //cut out areas that would otherwise be dark
}
