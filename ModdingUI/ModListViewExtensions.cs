using System;
using System.Collections.Generic;
using System.Reflection;
using Timberborn.Modding;
using Timberborn.ModdingUI;

namespace Mods.SteamUpdateButtons.ModdingUI {
  internal static class ModListViewExtensions {

    internal static Dictionary<Mod, ModItem> GetModItems(this ModListView modListView) {
      var modItemsField = modListView.GetType()
          .GetField("_modItems", BindingFlags.Instance | BindingFlags.NonPublic);
      if (modItemsField == null) {
        throw new Exception($"Mod items field named _modItems "
                   + $"wasn't found in {modListView.GetType().Name}");
      }
      return (Dictionary<Mod, ModItem>)modItemsField.GetValue(modListView);
    }

    internal static void OnModToggled(this ModListView modListView, object sender, EventArgs e) {
      var onModToggledMethod = modListView.GetType()
          .GetMethod("OnModToggled", BindingFlags.Instance | BindingFlags.NonPublic);
      if (onModToggledMethod == null) {
        throw new Exception($"Method named OnModToggled "
                   + $"wasn't found in {modListView.GetType().Name}");
      }
      onModToggledMethod.Invoke(modListView, new object[] { sender, e });
    }

  }
}
