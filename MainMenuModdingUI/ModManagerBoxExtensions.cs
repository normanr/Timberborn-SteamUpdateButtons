using System;
using System.Reflection;
using Timberborn.MainMenuModdingUI;
using Timberborn.ModdingUI;

namespace Mods.SteamUpdateButtons.MainMenuModdingUI {
  internal static class ModManagerBoxExtensions {

    internal static ModListView GetModListView(this ModManagerBox modManagerBox) {
      var modListViewField = modManagerBox.GetType()
          .GetField("_modListView", BindingFlags.Instance | BindingFlags.NonPublic);
      if (modListViewField == null) {
        throw new Exception($"{nameof(GetModListView)} field named _modListView "
                   + $"wasn't found in {modManagerBox.GetType().Name}");
      }
      return (ModListView)modListViewField.GetValue(modManagerBox);
    }

  }
}
