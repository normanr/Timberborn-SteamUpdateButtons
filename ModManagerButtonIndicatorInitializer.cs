using System;
using System.Linq;
using Timberborn.Localization;
using Timberborn.MainMenuPanels;
using Timberborn.MainMenuModdingUI;
using Timberborn.Modding;
using Timberborn.SingletonSystem;
using Timberborn.TooltipSystem;
using UnityEngine;
using UnityEngine.UIElements;
using TimberApi.UIBuilderSystem;
using Mods.SteamUpdateButtons.MainMenuModdingUI;
using Mods.SteamUpdateButtons.SteamWorkshopModDownloading;

namespace Mods.SteamUpdateButtons {
  public class ModManagerButtonIndicatorInitializer : ILoadableSingleton {

    private readonly UIBuilder _uiBuilder;
    private readonly ILoc _loc;
    private readonly MainMenuPanel _mainMenuPanel;
    private readonly ModManagerBox _modManagerBox;
    private readonly ModRepository _modRepository;
    private readonly SteamWorkshopModsProvider _steamWorkshopModsProvider;
    private readonly ITooltipRegistrar _tooltipRegistrar;

    private VisualElement _updateAvailableImage;
    private int _updatesAvailable;

    internal ModManagerButtonIndicatorInitializer(UIBuilder uiBuilder,
                                                ILoc loc,
                                                MainMenuPanel mainMenuPanel,
                                                ModRepository modRepository,
                                                ModManagerBox modManagerBox,
                                                SteamWorkshopModsProvider steamWorkshopModsProvider,
                                                ITooltipRegistrar tooltipRegistrar) {
      _uiBuilder = uiBuilder;
      _loc = loc;
      _mainMenuPanel = mainMenuPanel;
      _modRepository = modRepository;
      _modManagerBox = modManagerBox;
      _modManagerBox.GetModListView().ListChanged += ModListView_ListChanged;
      _steamWorkshopModsProvider = steamWorkshopModsProvider;
      _steamWorkshopModsProvider.RefreshComplete += SteamWorkshopModsProvider_RefreshComplete;
      _tooltipRegistrar = tooltipRegistrar;
    }

    public void Load() {
      _updateAvailableImage = _uiBuilder.Build<UpdateAvailableImage>("UpdateAvailableImage");
      _tooltipRegistrar.Register(_updateAvailableImage, () => _loc.T("SteamUpdateButtons.UpdatesAvailable", _updatesAvailable));
      _updatesAvailable = GetUpdatesAvailable();
      _updateAvailableImage.visible = _updatesAvailable > 0;
      var button = _mainMenuPanel.GetPanel().Q<Button>("ModManagerButton");
      button.Add(_updateAvailableImage);
    }

    private int GetUpdatesAvailable() {
      return _modRepository.Mods.Count((Mod mod) =>
        !mod.ModDirectory.IsUserMod &&
        ModPlayerPrefsHelper.IsModEnabled(mod) &&
        _steamWorkshopModsProvider.IsUpdatable(mod.ModDirectory));
    }

    private void ModListView_ListChanged(object sender, EventArgs e) {
      _updatesAvailable = GetUpdatesAvailable();
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: ListChanged, UpdatesAvailable = " + _updatesAvailable);
      if (_updateAvailableImage != null) {
        _updateAvailableImage.visible = _updatesAvailable > 0;
      }
    }

    private void SteamWorkshopModsProvider_RefreshComplete(object sender, EventArgs e) {
      _updatesAvailable = GetUpdatesAvailable();
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: RefreshComplete, UpdatesAvailable = " + _updatesAvailable);
      if (_updateAvailableImage != null) {
        _updateAvailableImage.visible = _updatesAvailable > 0;
      }
    }
  }
}
