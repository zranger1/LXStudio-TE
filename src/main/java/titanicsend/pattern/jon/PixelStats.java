package titanicsend.pattern.jon;

import heronarts.lx.LX;
import heronarts.lx.LXCategory;
import heronarts.lx.color.LXColor;
import heronarts.lx.color.LinkedColorParameter;
import heronarts.lx.model.LXPoint;
import heronarts.lx.parameter.*;
import titanicsend.pattern.TEAudioPattern;
import titanicsend.util.TE;
import titanicsend.util.TEMath;

@LXCategory("Combo FG")
public class PixelStats extends TEAudioPattern {
    int pixelCount = 0;
    boolean isFirstPass = true;

    public PixelStats(LX lx) {
        super(lx);
     }

    public void runTEAudioPattern(double deltaMs) {
        pixelCount = 0;
        int r,g,b;

        // per pixel calculations
        for (LXPoint point : model.points) {
            pixelCount++;

            // translate and rescale normalized coords
            double x = (point.xn);  // x axis on vehicle
            double z = (point.zn - 0.5);  // z axis on vehicle
            double y = (point.yn - 0.5);

            if (Math.abs(z) < 0.3) {
                r = g = b = 0;
                if (x < 0) {
                    r = 255;
                } else {
                    g = 255;
                }
                colors[point.index] = LXColor.rgb(255, 0, 0);
            }

        }
        if (isFirstPass) {
            TE.log("Total model.points count is: %d",pixelCount - 1);
            isFirstPass = false;
        }
    }

}
