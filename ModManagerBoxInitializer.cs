using System;
using System.Linq;
using Timberborn.MainMenuModdingUI;
using Timberborn.Modding;
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
    private readonly ModLoader _modLoader;
    private readonly ModManagerBox _modManagerBox;
    private readonly SteamWorkshopModsProvider _steamWorkshopModsProvider;
    private readonly ITooltipRegistrar _tooltipRegistrar;

    public ModManagerBoxInitializer(UIBuilder uiBuilder,
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
      var button = _uiBuilder.Build<UpdateButton>("UpdateAllModsButton");
      _tooltipRegistrar.RegisterLocalizable(button, "SteamUpdateButtons.UpdateAllMods");
      button.RegisterCallback<ClickEvent>(UpdateAll);
      _modManagerBox.GetPanel().Q<Button>("BrowseButton").parent.Add(button);
    }

    private void UpdateAll(ClickEvent evt) {
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: Updating all mods");
      var updatableMods = _modManagerBox.GetModListView().GetModItems().Keys.Where((Mod mod) =>
        !mod.ModDirectory.IsUserMod &&
        _steamWorkshopModsProvider.IsUpdatable(mod.ModDirectory));

      foreach (var mod in updatableMods) {
        Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: Updating: " + mod.DisplayName);
        _steamWorkshopModsProvider.UpdateModDirectory(mod.ModDirectory);
      }
    }
  }
}
