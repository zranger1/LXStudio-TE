package titanicsend.pattern.jon;

import heronarts.lx.LX;
import heronarts.lx.LXCategory;
import heronarts.lx.color.LXColor;
import heronarts.lx.model.LXPoint;
import heronarts.lx.parameter.CompoundParameter;
import heronarts.lx.parameter.LXParameter;
import titanicsend.model.TEEdgeModel;
import titanicsend.model.TEPanelModel;
import titanicsend.pattern.TEAudioPattern;
import titanicsend.pattern.yoffa.shader_engine.ShaderPainter;
import titanicsend.util.TEMath;

import java.util.ArrayList;
import java.util.Arrays;

@LXCategory("AAAAardvark")
public class VertexSunTest extends TEAudioPattern {

    // pattern variables
    double time = 0;
    ShaderPainter painter;
    float MINRADIUS = 0.05f;

    int[][] frameBuffer;

    static class VertexCoord {
        float x,y,z;
        int index;
    }
    ArrayList<VertexCoord> vertices;

    // Controls
    public final CompoundParameter energy =
            new CompoundParameter("Energy", 0, 0, 1)
                    .setDescription("Music Reactivity");

    float dotf(float x,float y,float z) { return x * x + y * y + z * z; }
    float distancef(float x,float y,float z) {
        return (float) Math.sqrt(x * x + y * y + z * z);
    }

    float chebyshev(float x,float y,float z) {
        return Math.max(Math.abs(z),Math.abs(y));
    }

    void addPoint(LXPoint p) {
        VertexCoord uv = new VertexCoord();
        uv.x = p.xn; uv.y = p.yn; uv.z = p.zn;
        uv.index = p.index;
        vertices.add(uv);
    }

    public VertexSunTest(LX lx) {
        super(lx);
        addParameter("energy", energy);            // beat reactivity

        // build a list of points at edge vertices
        vertices = new ArrayList<>();
        for (TEEdgeModel edge : model.getAllEdges()) {
            for (TEEdgeModel.Point point : edge.points) {
                if (point.frac == 0f) {
                    addPoint(point);
                }
            }
        }
        System.out.println(String.format("%d edge vertices added",vertices.size()));
    }

    public void runTEAudioPattern(double deltaMs) {
        time = time + deltaMs / 1000;
        float t1 = (float) (time / 6);

        float t = (float) lx.engine.tempo.basis();
        int color = getSwatchColor(ColorType.PANEL);
        for (LXPoint p: model.getPoints()) {
            for (VertexCoord v : vertices) {
                float dist = chebyshev(v.x-p.xn,v.y-p.yn,v.z-p.zn);

                if (dist <= (0.01+MINRADIUS * t)) {
                    colors[p.index] = color;
                    break;
                } else {
                    colors[p.index] = LXColor.BLACK;
                }
            }
        }
    }
}
