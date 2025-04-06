using System;
using Timberborn.CoreUI;
using Timberborn.MainMenuModdingUI;
using Timberborn.SingletonSystem;
using Timberborn.TooltipSystem;
using UnityEngine;
using UnityEngine.UIElements;
using TimberApi.UIBuilderSystem;
using Mods.SteamUpdateButtons.MainMenuModdingUI;
using Mods.SteamUpdateButtons.ModdingUI;
using Mods.SteamUpdateButtons.SteamWorkshopModDownloading;

namespace Mods.SteamUpdateButtons {
  public class ModManagerBoxInitializer : ILoadableSingleton {
    private readonly UIBuilder _uiBuilder;
    private readonly ModManagerBox _modManagerBox;
    private readonly SteamWorkshopModsProvider _steamWorkshopModsProvider;
    private readonly ITooltipRegistrar _tooltipRegistrar;

    public ModManagerBoxInitializer(UIBuilder uiBuilder,
                                    ModManagerBox modManagerBox,
                                    SteamWorkshopModsProvider steamWorkshopModsProvider,
                                    ITooltipRegistrar tooltipRegistrar) {
      _uiBuilder = uiBuilder;
      _modManagerBox = modManagerBox;
      _steamWorkshopModsProvider = steamWorkshopModsProvider;
      _tooltipRegistrar = tooltipRegistrar;
    }

    public void Load() {
      var button = _uiBuilder.Build<UpdateButton>("UpdateAllModsButton");
      _tooltipRegistrar.RegisterLocalizable(button, "SteamUpdateButtons.UpdateAllMods");
      button.RegisterCallback<ClickEvent>(UpdateAll);
      _modManagerBox.GetPanel().Q<Button>("BrowseButton").parent.Add(button);
    }

    private void UpdateAll(ClickEvent evt) {
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: Updating all mods");
      foreach (var pair in _modManagerBox.GetModListView().GetModItems()) {
        // pair.Deconstruct(out var mod, out var modItem);
        var mod = pair.Key;
        var modItem = pair.Value;
        if (mod.ModDirectory.IsUserMod) continue;
        if (!_steamWorkshopModsProvider.IsUpdatable(mod.ModDirectory)) continue;

        Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: Updating: " + mod.DisplayName);
        var downloadPendingImage = modItem.Root.Q<VisualElement>("DownloadPendingImage");
        var button = modItem.Root.Q<VisualElement>("UpdateModButton");
        button.ToggleDisplayStyle(false);
        downloadPendingImage.ToggleDisplayStyle(true);
        _steamWorkshopModsProvider.UpdateModDirectory(mod.ModDirectory);
      }
    }
  }
}
