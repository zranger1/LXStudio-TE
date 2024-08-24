package titanicsend.preset;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.stream.JsonWriter;
import heronarts.lx.LX;
import heronarts.lx.LXPresetComponent;
import heronarts.lx.LXSerializable;
import heronarts.lx.color.ColorParameter;
import heronarts.lx.color.LXColor;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.*;

/**
 * Contains presets for multiple components
 */
public class UserPresetLibrary implements LXSerializable {
  private final LX lx;

  private static final String defaultFileName = "newFile.userPresets";
  private File file;

  public final ColorParameter color = new ColorParameter("Color", LXColor.RED);

  private final List<UserPresetCollection> mutableCollections = new ArrayList<UserPresetCollection>();
  public final List<UserPresetCollection> collections = Collections.unmodifiableList(this.mutableCollections);

  private final Map<String, UserPresetCollection> collectionDict = new HashMap<String, UserPresetCollection>();

  public UserPresetLibrary(LX lx) {
    this.lx = lx;
    this.file = lx.getMediaFile(defaultFileName);
  }

  public File getFile() {
    return this.file;
  }

  /**
   * Retrieve a collection of presets for a given component (pattern, etc)
   */
  public UserPresetCollection get(LXPresetComponent component) {
    return get(PresetEngine.getPresetName(component));
  }

  public UserPresetCollection get(String clazz) {
    UserPresetCollection c = collectionDict.get(clazz);
    if (c == null) {
      c = new UserPresetCollection(this.lx, clazz);
      this.mutableCollections.add(c);
      collectionDict.put(clazz, c);
    }
    return c;
  }

  /**
   * Save/Load
   */

  public void save(File file) {
    JsonObject obj = new JsonObject();
    save(this.lx, obj);
    try (JsonWriter writer = new JsonWriter(new FileWriter(file))) {
      writer.setIndent("  ");
      new GsonBuilder().create().toJson(obj, writer);
      this.file = file;
    } catch (IOException iox) {
      LX.error(iox, "Exception writing the user preset library file: " + this.file);
    }
  }

  public void load(File file) {
    try (FileReader fr = new FileReader(file)) {
      load(this.lx, new Gson().fromJson(fr, JsonObject.class));
    } catch (IOException iox) {
      LX.error(iox, "Could not import views from file: " + file.toString());
    }
  }

  private static final String KEY_COLLECTIONS = "collections";

  @Override
  public void save(LX lx, JsonObject obj) {
    obj.add(KEY_COLLECTIONS, LXSerializable.Utils.toArray(lx, this.collections));
  }

  @Override
  public void load(LX lx, JsonObject obj) {
    // Remove presets in all collections
    for (UserPresetCollection c : this.collections) {
      for (int j = c.presets.size() - 1; j >= 0; --j) {
        c.removePreset(c.presets.get(j));
      }
    }

    // Load collections
    JsonArray collectionsArray = obj.getAsJsonArray(KEY_COLLECTIONS);
    for (JsonElement patternElement : collectionsArray) {
      JsonObject patternObj = (JsonObject) patternElement;
      loadCollection(patternObj, -1);
    }
  }

  private void loadCollection(JsonObject patternObj, int index) {
    String clazz = patternObj.get(UserPresetCollection.KEY_CLASS).getAsString();
    // Find existing or create new
    // Existing are referenced by UI elements so we won't throw them away and recreate them.
    UserPresetCollection c = get(clazz);
    c.load(this.lx, patternObj);
  }
}
