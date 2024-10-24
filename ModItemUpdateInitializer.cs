using System;
using Timberborn.CoreUI;
using Timberborn.MainMenuModdingUI;
using Timberborn.Modding;
using Timberborn.ModdingUI;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;
using TimberApi.UIBuilderSystem;
using Mods.SteamUpdateButtons.MainMenuModdingUI;
using Mods.SteamUpdateButtons.ModdingUI;
using Mods.SteamUpdateButtons.SteamWorkshopModDownloading;

namespace Mods.SteamUpdateButtons {
  internal class ModItemUpdateInitializer : ILoadableSingleton {

    private readonly UIBuilder _uiBuilder;
    private readonly ModLoader _modLoader;
    private readonly ModManagerBox _modManagerBox;
    private readonly SteamWorkshopModsProvider _steamWorkshopModsProvider;

    public ModItemUpdateInitializer(UIBuilder uiBuilder,
                                    ModLoader modLoader,
                                    ModManagerBox modManagerBox,
                                    SteamWorkshopModsProvider steamWorkshopModsProvider) {
      _uiBuilder = uiBuilder;
      _modLoader = modLoader;
      _modManagerBox = modManagerBox;
      _steamWorkshopModsProvider = steamWorkshopModsProvider;
      _steamWorkshopModsProvider.DownloadComplete += SteamWorkshopModsProvider_DownloadComplete;
    }

    public void Load() {
      foreach (var kv in _modManagerBox.GetModListView().GetModItems()) {
        if (kv.Key.ModDirectory.IsUserMod) {
          continue;
        }
        Initialize(kv.Value);
      }
    }

    private void Initialize(ModItem modItem) {
      var image = _uiBuilder.Build<UnavailableImage>("UnavailableImage");
      modItem.Root.Add(image);
      image.RegisterCallback<AttachToPanelEvent>(
          _ => image.ToggleDisplayStyle(
              !_steamWorkshopModsProvider.IsAvailable(modItem.Mod.ModDirectory)));
      var button = _uiBuilder.Build<UpdateButton>("UpdateModButton");
      modItem.Root.Add(button);
      button.RegisterCallback<AttachToPanelEvent>(
          _ => button.ToggleDisplayStyle(
              _steamWorkshopModsProvider.IsUpdatable(modItem.Mod.ModDirectory)));
      button.RegisterCallback<ClickEvent>(ce => {
        Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: Updating: " + modItem.Mod.DisplayName);
        _steamWorkshopModsProvider.UpdateModDirectory(modItem.Mod.ModDirectory);
      });
    }

    private void SteamWorkshopModsProvider_DownloadComplete(object sender, EventArgs e) {
      _modManagerBox.GetModListView().OnModToggled(this, EventArgs.Empty);  // show restartWarning
      foreach (var kv in _modManagerBox.GetModListView().GetModItems()) {
        if (kv.Key.ModDirectory.IsUserMod) {
          continue;
        }
        Update(kv.Value);
      }
    }

    private void Update(ModItem modItem) {
      if (_modLoader.TryLoadMod(modItem.Mod.ModDirectory, out var mod)) {
        var version = modItem.ModManifest.Version.AsFormattedString();
        if (version != mod.Manifest.Version.AsFormattedString()) {
          version += " → " + mod.Manifest.Version.AsFormattedString();
          modItem.Root.Q<Label>("ModVersion").text = version;
        }
      }
      var image = modItem.Root.Q<VisualElement>("UnavailableImage");
      image.ToggleDisplayStyle(
          !_steamWorkshopModsProvider.IsAvailable(modItem.Mod.ModDirectory));
      var button = modItem.Root.Q<VisualElement>("UpdateModButton");
      button.ToggleDisplayStyle(
          _steamWorkshopModsProvider.IsUpdatable(modItem.Mod.ModDirectory));
    }
  }
}
