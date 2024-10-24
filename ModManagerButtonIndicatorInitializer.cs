using System;
using System.Linq;
using Timberborn.MainMenuPanels;
using Timberborn.MainMenuModdingUI;
using Timberborn.Modding;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;
using TimberApi.UIBuilderSystem;
using Mods.SteamUpdateButtons.MainMenuModdingUI;
using Mods.SteamUpdateButtons.SteamWorkshopModDownloading;

namespace Mods.SteamUpdateButtons {
  public class ModManagerButtonIndicatorInitializer : ILoadableSingleton {

    private readonly UIBuilder _uiBuilder;
    private readonly MainMenuPanel _mainMenuPanel;
    private readonly ModManagerBox _modManagerBox;
    private readonly ModRepository _modRepository;
    private readonly SteamWorkshopModsProvider _steamWorkshopModsProvider;

    private VisualElement _updateAvailableImage;

    internal ModManagerButtonIndicatorInitializer(UIBuilder uiBuilder,
                                                MainMenuPanel mainMenuPanel,
                                                ModRepository modRepository,
                                                ModManagerBox modManagerBox,
                                                SteamWorkshopModsProvider steamWorkshopModsProvider) {
      _uiBuilder = uiBuilder;
      _mainMenuPanel = mainMenuPanel;
      _modRepository = modRepository;
      _modManagerBox = modManagerBox;
      _modManagerBox.GetModListView().ListChanged += ModListView_ListChanged;
      _steamWorkshopModsProvider = steamWorkshopModsProvider;
      _steamWorkshopModsProvider.RefreshComplete += SteamWorkshopModsProvider_RefreshComplete;
    }

    public void Load() {
      _updateAvailableImage = _uiBuilder.Build<UpdateAvailableImage>("UpdateAvailableImage");
      _updateAvailableImage.visible = GetUpdatesAvailable();
      var button = _mainMenuPanel.GetPanel().Q<Button>("ModManagerButton");
      button.Add(_updateAvailableImage);
    }

    private bool GetUpdatesAvailable() {
      return _modRepository.Mods.Any((Mod mod) =>
        !mod.ModDirectory.IsUserMod &&
        ModPlayerPrefsHelper.IsModEnabled(mod) &&
        _steamWorkshopModsProvider.IsUpdatable(mod.ModDirectory));
    }

    private void ModListView_ListChanged(object sender, EventArgs e) {
      var updatesAvailable = GetUpdatesAvailable();
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: ListChanged, UpdatesAvailable = " + updatesAvailable);
      if (_updateAvailableImage != null) {
        _updateAvailableImage.visible = updatesAvailable;
      }
    }

    private void SteamWorkshopModsProvider_RefreshComplete(object sender, EventArgs e) {
      var updatesAvailable = GetUpdatesAvailable();
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: RefreshComplete, UpdatesAvailable = " + updatesAvailable);
      if (_updateAvailableImage != null) {
        _updateAvailableImage.visible = updatesAvailable;
      }
    }
  }
}
