package titanicsend.pattern.jon;

import heronarts.lx.LX;
import heronarts.lx.LXCategory;
import heronarts.lx.color.LXColor;
import heronarts.lx.color.LinkedColorParameter;
import heronarts.lx.parameter.CompoundParameter;
import heronarts.lx.parameter.LXParameter;
import titanicsend.model.TEEdgeModel;
import titanicsend.pattern.TEAudioPattern;

import java.util.Random;

// All LED platforms, no matter how large, must have KITT!
@LXCategory("Edge FG")
public class EdgeGlitter extends TEAudioPattern {
    boolean[] zoneIsLit;
    long seed;
    Random prng;
    float cycleCount;
    double lastCycle;
    protected final CompoundParameter beatsPerCycle = (CompoundParameter)
            new CompoundParameter("Measures", 2, 1, 16)
                    .setUnits(LXParameter.Units.INTEGER)
                    .setDescription("Number of measures between light shifts");
    protected final CompoundParameter zonesPerEdge = (CompoundParameter)
            new CompoundParameter("Zones", 20, 1, 60)
                    .setUnits(LXParameter.Units.INTEGER)
                    .setDescription("Number of light zones per edge");

    public final CompoundParameter minBrightness =
            new CompoundParameter("BG Bri", 0.125, 0.0, 1)
                    .setDescription("Background Brightness");

    public final CompoundParameter minHeight =
            new CompoundParameter("Height", 0., 0.0, 1)
                    .setDescription("Min starting height for effect");

    protected final CompoundParameter minLit = (CompoundParameter)
            new CompoundParameter("MinLit", 3, 0, 60)
                    .setUnits(LXParameter.Units.INTEGER)
                    .setDescription("Min lit zones per edge");

    protected final CompoundParameter maxLit = (CompoundParameter)
            new CompoundParameter("MaxLit", 7, 1, 60)
                    .setUnits(LXParameter.Units.INTEGER)
                    .setDescription("Max lit zones per edge");

    public final CompoundParameter energy =
            new CompoundParameter("Energy", .75, 0, 1)
                    .setDescription("Depth of light pulse");

    public final LinkedColorParameter color =
            registerColor("Color", "color", ColorType.PRIMARY,
                    "Color");

    public EdgeGlitter(LX lx) {
        super(lx);
        addParameter("energy", energy);
        addParameter("beatsPerCycle", beatsPerCycle);
        addParameter("zoneCount", zonesPerEdge);
        addParameter("minLit", minLit);
        addParameter("maxLit", maxLit);
        addParameter("minBri", minBrightness);
        addParameter("height", minHeight);
        prng = new Random();
        lastCycle = 99f; // trigger immediate start;
        cycleCount = 0f;
        zoneIsLit = new boolean[100];  // should be plenty of room.
    }

    // choose between minLit and maxLit random segments of an edge to light
    void lightRandomSegments(int zoneCount, int minLit, int maxLit) {
        int nLit = Math.max(minLit,(int) (maxLit * prng.nextFloat()));

        for (int i = 0; i < (int) zoneCount; i++) {
            zoneIsLit[i] = false;
        }
        for (int i = 0; i < nLit; i++) {
            zoneIsLit[(int) (zoneCount * prng.nextFloat())] = true;
        }
    }

    public void runTEAudioPattern(double deltaMs) {
        updateGradients();

        // we sync lit segment changes to measures, and
        // light pulses to the beat.
        float currentCycle = (float) measure();
        float currentBeat = (float) getTempo().basis();

        if (currentCycle < lastCycle) {
            if (cycleCount >= beatsPerCycle.getValuef()) {
                seed = System.currentTimeMillis();
                cycleCount = 0;
            }
            cycleCount++;
        }
        lastCycle = currentCycle;
        prng.setSeed(seed);

        // pick up the current color
        int baseColor = this.color.calcColor();

        // spotlight brightness pulses with the beat
        float spotBrightness = (float) (1.0 - energy.getValue() * currentBeat);

        // get display parameter variables from control settings
        float yMin = minHeight.getValuef();
        float minBri = minBrightness.getValuef();
        int zoneCount = (int) zonesPerEdge.getValue();

        // get, and de-confuse min and max number of segments to light
        // (max must be greater than min, both must be <= current zone count)
        int minZones = (int) minLit.getValue();
        int maxZones = (int) maxLit.getValue();

        minZones = Math.min(zoneCount, minZones);
        minZones = (minZones > maxZones) ? maxZones : minZones;
        maxZones = Math.min(zoneCount,maxZones);

        for (TEEdgeModel edge : model.getAllEdges()) {
            lightRandomSegments(zoneCount,minZones,maxZones);
            for (TEEdgeModel.Point point : edge.points) {
                int zone = (int) Math.floor(point.frac * zoneCount); // which zone is pixel in
                float bri = (zoneIsLit[zone] && point.yn >= yMin) ? spotBrightness : minBri;

                // clear and reset alpha channel
                baseColor = baseColor & ~LXColor.ALPHA_MASK;
                baseColor = baseColor | ((int) (bri * 255) << LXColor.ALPHA_SHIFT);
                colors[point.index] = baseColor;
            }
        }
    }
}