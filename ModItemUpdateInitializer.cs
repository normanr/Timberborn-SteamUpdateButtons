using System;
using System.Collections.Generic;
using Timberborn.CoreUI;
using Timberborn.MainMenuModdingUI;
using Timberborn.Modding;
using Timberborn.ModdingUI;
using Timberborn.SingletonSystem;
using Timberborn.TooltipSystem;
using UnityEngine;
using UnityEngine.UIElements;
using TimberApi.UIBuilderSystem;
using Mods.SteamUpdateButtons.MainMenuModdingUI;
using Mods.SteamUpdateButtons.ModdingUI;
using Mods.SteamUpdateButtons.SteamWorkshopModDownloading;

namespace Mods.SteamUpdateButtons {
  internal class ModItemUpdateInitializer : ILoadableSingleton, IUpdatableSingleton {

    private readonly UIBuilder _uiBuilder;
    private readonly ModLoader _modLoader;
    private readonly ModManagerBox _modManagerBox;
    private readonly SteamWorkshopModsProvider _steamWorkshopModsProvider;
    private readonly ITooltipRegistrar _tooltipRegistrar;

    private readonly List<ModItem> _updatingMods = [];

    public ModItemUpdateInitializer(UIBuilder uiBuilder,
                                    ModLoader modLoader,
                                    ModManagerBox modManagerBox,
                                    SteamWorkshopModsProvider steamWorkshopModsProvider,
                                    ITooltipRegistrar tooltipRegistrar) {
      _uiBuilder = uiBuilder;
      _modLoader = modLoader;
      _modManagerBox = modManagerBox;
      _steamWorkshopModsProvider = steamWorkshopModsProvider;
      _tooltipRegistrar = tooltipRegistrar;
    }

    public void Load() {
      foreach (var kv in _modManagerBox.GetModListView().GetModItems()) {
        if (kv.Key.ModDirectory.IsUserMod) {
          continue;
        }
        Initialize(kv.Value);
      }
    }

    public void UpdateSingleton() {
      foreach (var modItem in _updatingMods) {
        if (_steamWorkshopModsProvider.GetDownloadProgress(modItem.Mod.ModDirectory, out var downloaded, out var total) && total > 0) {
          Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: progress " + modItem.Mod.DisplayName + $" {downloaded}/{total}");
          var version = modItem.ModManifest.Version.AsFormattedString();
          version += $" → {(float)downloaded / total:0%}";
          modItem.Root.Q<Label>("ModVersion").text = version;
        } else {
          Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: no progress " + modItem.Mod.DisplayName);
        }
      }
    }

    private void Initialize(ModItem modItem) {
      var unavailableImage = _uiBuilder.Build<UnavailableImage>("UnavailableImage");
      modItem.Root.Add(unavailableImage);
      unavailableImage.RegisterCallback<AttachToPanelEvent>(
          _ => unavailableImage.ToggleDisplayStyle(
              !_steamWorkshopModsProvider.IsAvailable(modItem.Mod.ModDirectory)));
      var downloadPendingImage = _uiBuilder.Build<DownloadPendingImage>("DownloadPendingImage");
      modItem.Root.Add(downloadPendingImage);
      downloadPendingImage.RegisterCallback<AttachToPanelEvent>(
          _ => downloadPendingImage.ToggleDisplayStyle(
              _steamWorkshopModsProvider.IsDownloadPending(modItem.Mod.ModDirectory)));
      var button = _uiBuilder.Build<UpdateButton>("UpdateModButton");
      _tooltipRegistrar.RegisterLocalizable(button, "SteamUpdateButtons.UpdateMod");
      modItem.Root.Add(button);
      button.RegisterCallback<AttachToPanelEvent>(
          _ => button.ToggleDisplayStyle(
              !_steamWorkshopModsProvider.IsDownloadPending(modItem.Mod.ModDirectory) &&
              _steamWorkshopModsProvider.IsUpdatable(modItem.Mod.ModDirectory)));
      button.RegisterCallback<ClickEvent>(ce => {
        UpdateMod(modItem);
      });
    }

    public void UpdateMod(ModItem modItem) {
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: Updating: " + modItem.Mod.DisplayName);
      if (_steamWorkshopModsProvider.UpdateModDirectory(modItem.Mod.ModDirectory, r => {
        Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: No longer updating: " + modItem.Mod.DisplayName);
        _updatingMods.Remove(modItem);
        _modManagerBox.GetModListView().OnModToggled(this, EventArgs.Empty);  // show restartWarning
        Update(modItem);
      })) {
        var downloadPendingImage = modItem.Root.Q<VisualElement>("DownloadPendingImage");
        var button = modItem.Root.Q<VisualElement>("UpdateModButton");
        button.ToggleDisplayStyle(false);
        downloadPendingImage.ToggleDisplayStyle(true);
        _updatingMods.Add(modItem);
      }
    }

    private void Update(ModItem modItem) {
      if (_modLoader.TryLoadMod(modItem.Mod.ModDirectory, out var mod)) {
        var version = modItem.ModManifest.Version.AsFormattedString();
        version += " → " + mod.Manifest.Version.AsFormattedString();
        modItem.Root.Q<Label>("ModVersion").text = version;
      }
      var unavailableImage = modItem.Root.Q<VisualElement>("UnavailableImage");
      unavailableImage.ToggleDisplayStyle(
          !_steamWorkshopModsProvider.IsAvailable(modItem.Mod.ModDirectory));
      var downloadPendingImage = modItem.Root.Q<VisualElement>("DownloadPendingImage");
      bool downloadPending = _steamWorkshopModsProvider.IsDownloadPending(modItem.Mod.ModDirectory);
      downloadPendingImage.ToggleDisplayStyle(
          downloadPending);
      var button = modItem.Root.Q<VisualElement>("UpdateModButton");
      button.ToggleDisplayStyle(
          !downloadPending &&
          _steamWorkshopModsProvider.IsUpdatable(modItem.Mod.ModDirectory));
    }
  }
}
