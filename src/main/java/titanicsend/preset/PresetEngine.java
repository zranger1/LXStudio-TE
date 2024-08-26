package titanicsend.preset;

import com.google.gson.Gson;
import com.google.gson.JsonObject;
import heronarts.lx.LX;
import heronarts.lx.LXComponent;
import heronarts.lx.LXPresetComponent;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class PresetEngine extends LXComponent {

  private static PresetEngine current;
  public static PresetEngine get() {
    return current;
  }

  private final List<UserPresetLibrary> mutableLibraries = new ArrayList<UserPresetLibrary>();
  public final List<UserPresetLibrary> libraries = Collections.unmodifiableList(this.mutableLibraries);

  public UserPresetLibrary currentLibrary;

  public PresetEngine(LX lx) {
    super(lx, "PresetEngine");
    current = this;

    currentLibrary = new UserPresetLibrary(lx);
  }

  public static String getPresetName(LXPresetComponent component) {
    return component.getPresetClass().getCanonicalName();
  }

  public static String getPresetShortName(LXPresetComponent component) {
    if (!(component instanceof LXComponent)) {
      return null;
    }
    return LXComponent.getComponentName(((LXComponent)component).getClass());
  }

  /**
   * Import existing .lxd presets from Chromatik's Presets/ folder, for the current component (pattern) class.
   */
  public void importPresets(LXPresetComponent component) {
    File presetFolder = this.lx.getPresetFolder((LXComponent)component);
    File[] files = presetFolder.listFiles((dir, name) -> name.endsWith(".lxd"));
    if (files != null) {
      for (File file : files) {
        try (FileReader fr = new FileReader(file)) {
          JsonObject obj = new Gson().fromJson(fr, JsonObject.class);
          String name = nameWithoutExtension(file);
          UserPresetCollection collection = this.currentLibrary.get(component);
          collection.addPreset(component, obj).setLabel(name);
        } catch (IOException iox) {
          LX.error("Could not load preset file: " + iox.getLocalizedMessage());
          this.lx.pushError(iox, "Could not load preset file: " + iox.getLocalizedMessage());
        } catch (Exception x) {
          LX.error(x, "Exception loading preset file: " + x.getLocalizedMessage());
          this.lx.pushError(x, "Exception in importPresets: " + x.getLocalizedMessage());
        }
      }
    }
  }

  /**
   * Import all file system presets for all patterns
   */
  public void importAllPatternPresets() {
    // TODO
  }

  private String nameWithoutExtension(File file) {
    String name = file.getName();
    int d = name.lastIndexOf('.');
    if (d > 0 && d < name.length() - 1) {
      return name.substring(0, d);
    } else {
      return name;
    }
  }

  public PresetEngine openFile(String path) {
    return openFile(path);
  }

  public PresetEngine openFile(File file) {
    this.currentLibrary.load(file);
    return this;
  }
}
