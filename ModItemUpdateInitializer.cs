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
using Mods.SteamUpdateButtons.SteamWorkshopModDownloading;

namespace Mods.SteamUpdateButtons {
  internal class ModItemUpdateInitializer : ILoadableSingleton {

    private static readonly string ModListViewFieldName = "_modListView";
    private static readonly string ModItemsFieldName = "_modItems";
    private readonly UIBuilder _uiBuilder;
    private readonly ModManagerBox _modManagerBox;
    private readonly SteamWorkshopModsProvider _steamWorkshopModsProvider;

    public ModItemUpdateInitializer(UIBuilder uiBuilder,
                                    ModManagerBox modManagerBox,
                                    SteamWorkshopModsProvider steamWorkshopModsProvider) {
      _uiBuilder = uiBuilder;
      _modManagerBox = modManagerBox;
      _steamWorkshopModsProvider = steamWorkshopModsProvider;
    }

    public void Load() {
      _steamWorkshopModsProvider.DownloadComplete += (sender, e) => {
        OnModToggled(this, EventArgs.Empty);
        foreach (var createdModItem in GetCreatedModItems()) {
          if (createdModItem.Key.ModDirectory.IsUserMod) {
            continue;
          }
          Update(createdModItem.Value);
        }
      };
      foreach (var createdModItem in GetCreatedModItems()) {
        if (createdModItem.Key.ModDirectory.IsUserMod) {
          continue;
        }
        Initialize(createdModItem.Value);
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

    private Dictionary<Mod, ModItem> OnModToggled(object sender, EventArgs e) {
      var modListView = GetModListView();
      var onModToggledMethod = modListView.GetType()
          .GetMethod("OnModToggled", BindingFlags.Instance | BindingFlags.NonPublic);
      if (onModToggledMethod == null) {
        throw new Exception($"Mod items field named {ModItemsFieldName} "
                   + $"wasn't found in {modListView.GetType().Name}");
      }
      return (Dictionary<Mod, ModItem>)onModToggledMethod.Invoke(modListView, new object[] { sender, e });
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

    private void Initialize(ModItem modItem) {
      var button = _uiBuilder.Build<UpdateButton>("UpdateModButton");
      modItem.Root.Add(button);
      button.RegisterCallback<AttachToPanelEvent>(
          _ => button.ToggleDisplayStyle(
              _steamWorkshopModsProvider.IsUpdatable(modItem.Mod.ModDirectory)));
      button.RegisterCallback<ClickEvent>(ce => {
        Debug.Log(DateTime.Now.ToString("HH:mm:ss.fff") + ": SteamUpdateButtons.Update: " + modItem.Mod.DisplayName);
        _steamWorkshopModsProvider.UpdateModDirectory(modItem.Mod.ModDirectory);
      });
    }

    private void Update(ModItem modItem) {
      var button = modItem.Root.Q<Button>("UpdateModButton");
      button.ToggleDisplayStyle(
          _steamWorkshopModsProvider.IsUpdatable(modItem.Mod.ModDirectory));
    }
  }
}
