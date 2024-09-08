using System;
using System.Collections.Generic;
using System.Reflection;
using Timberborn.CoreUI;
using Timberborn.MainMenuModdingUI;
using Timberborn.Modding;
using Timberborn.ModdingUI;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;
using TimberApi.UIBuilderSystem;

namespace Mods.SteamInfo {
  internal class ModItemUpdateInitializer : ILoadableSingleton {

    private static readonly string ModListViewFieldName = "_modListView";
    private static readonly string ModItemsFieldName = "_modItems";
    private readonly UIBuilder _uiBuilder;
    private readonly ModManagerBox _modManagerBox;
    private readonly SteamWorkshopContentProvider _steamWorkshopContentProvider;

    public ModItemUpdateInitializer(UIBuilder uiBuilder,
                                    ModManagerBox modManagerBox,
                                    SteamWorkshopContentProvider steamWorkshopContentProvider) {
      _uiBuilder = uiBuilder;
      _modManagerBox = modManagerBox;
      _steamWorkshopContentProvider = steamWorkshopContentProvider;
      // Debug.Log("SteamInfo.ModItemUpdateInitializer()");
    }

    public void Load() {
      // Debug.Log("SteamInfo.ModItemUpdateInitializer.Load()");
      foreach (var createdModItem in GetCreatedModItems()) {
        if (createdModItem.Key.ModDirectory.IsUserMod) {
          continue;
        }
        Initialize(createdModItem.Key, createdModItem.Value);
      }
    }

    private Dictionary<Mod, ModItem> GetCreatedModItems() {
      var modListView = GetModListView();
      var modItemsField = modListView.GetType()
          .GetField(ModItemsFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
      if (modItemsField == null) {
        throw new Exception($"Mod items field named {ModItemsFieldName} "
                   + $"wasn't found in {modListView.GetType().Name}");
      }
      return (Dictionary<Mod, ModItem>)modItemsField.GetValue(modListView);
    }

    private ModListView GetModListView() {
      var modListViewField = _modManagerBox.GetType()
          .GetField(ModListViewFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
      if (modListViewField == null) {
        throw new Exception($"{nameof(ModListView)} field named {ModListViewFieldName} "
                   + $"wasn't found in {_modManagerBox.GetType().Name}");
      }
      return (ModListView)modListViewField.GetValue(_modManagerBox);
    }

    private void Initialize(Mod mod, ModItem modItem) {
      // Debug.Log(mod.ModDirectory.Path);

      var button = _uiBuilder.Build<UpdateButton>();
      modItem.Root.Add(button);
      //button.RegisterCallback<AttachToPanelEvent>(
          //_ => button.ToggleDisplayStyle(
              //_modSettingsOwnerRegistry.HasModSettings(modItem.Mod)));
      button.RegisterCallback<ClickEvent>(ce => {
        // _modSettingsBox.Open(modItem.Mod)
        Debug.Log(DateTime.Now.ToString("hh:mm:ss.fff") + ": SteamInfo.Update: " + modItem.Mod.DisplayName);
        _steamWorkshopContentProvider.UpdateItem(modItem.Mod.ModDirectory.Path);
      });
    }
  }
}
