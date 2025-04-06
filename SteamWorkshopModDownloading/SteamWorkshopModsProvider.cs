using System;
using System.Collections.Generic;
using UnityEngine;
using Timberborn.Modding;
using Timberborn.SingletonSystem;
using Timberborn.SteamStoreSystem;
using Mods.SteamUpdateButtons.SteamWorkshop;
using Mods.SteamUpdateButtons.SteamWorkshopContent;

namespace Mods.SteamUpdateButtons.SteamWorkshopModDownloading {
  public class SteamWorkshopModsProvider : Timberborn.SteamWorkshopModDownloading.SteamWorkshopModsProvider, ILoadableSingleton {
    private readonly SteamManager _steamManager;
    private readonly SteamWorkshopContentProvider _steamWorkshopContentProvider;
    private readonly ModLoader _modLoader;
    private readonly Dictionary<string, Tuple<ContentDirectory, SteamWorkshopItem>> _items;
    public event EventHandler DownloadComplete;
    public event EventHandler RefreshComplete;

    public SteamWorkshopModsProvider(SteamManager steamManager, SteamWorkshopContentProvider steamWorkshopContentProvider, ModLoader modLoader) : base(steamWorkshopContentProvider, modLoader) {
      _steamManager = steamManager;
      _steamWorkshopContentProvider = steamWorkshopContentProvider;
      _modLoader = modLoader;
      _items = new Dictionary<string, Tuple<ContentDirectory, SteamWorkshopItem>>();
    }

    public void Load() {
      if (_steamManager.Initialized) {
        RefreshWorkshopItems(null);
        _steamWorkshopContentProvider.DownloadComplete += (sender, e) => {
          RefreshWorkshopItems(() => {
            DownloadComplete?.Invoke(this, EventArgs.Empty);
          });
        };
      }
    }

    public void RefreshWorkshopItems(Action callback) {
      var directoryDetails = _steamWorkshopContentProvider.GetContentDirectoryDetails();
      var detailsById = new Dictionary<ulong, ContentDirectory>();
      var subscribedFiles = new List<ulong>();
      foreach (ContentDirectory contentDirectory in directoryDetails) {
        detailsById[contentDirectory.ItemId] = contentDirectory;
        subscribedFiles.Add(contentDirectory.ItemId);
      }

      var request = new SteamWorkshopQueryRequest(subscribedFiles);
      var requestor = new SteamWorkshopQueryRequester();

      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: Refreshing workshop items");
      var start = DateTime.Now;
      requestor.Query(request, (response) => {
        var duration = DateTime.Now - start;
        Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: result = " + response.ResultMessage.Replace("k_EResult", "") + " in " + duration);
        if (response.Successful) {
          foreach (var item in response.Items) {
            var contentDirectory = detailsById[item.ItemId];

            if (item.TimeUpdated > contentDirectory.TimeUpdated) {
              Debug.Log("- [Newer] " + item.ItemId + "/" + item.Name + ", Local=" + contentDirectory.TimeUpdated.ToLocalTime().ToString("o") + ", Server=" + item.TimeUpdated.ToLocalTime().ToString("o"));
            } else if (item.TimeUpdated < contentDirectory.TimeUpdated) {
              Debug.Log("- [Older] " + item.ItemId + "/" + item.Name + ", Local=" + contentDirectory.TimeUpdated.ToLocalTime().ToString("o") + ", Server=" + item.TimeUpdated.ToLocalTime().ToString("o"));
            } else {
              Debug.Log("- [Equal] " + item.ItemId + "/" + item.Name + ", Local & Server=" + item.TimeUpdated.ToLocalTime().ToString("o"));
            }

            _items[contentDirectory.Folder] = new Tuple<ContentDirectory, SteamWorkshopItem>(contentDirectory, item);
          }
        }
        RefreshComplete?.Invoke(this, EventArgs.Empty);
        callback?.Invoke();
      });
    }

    private readonly DateTime UnixEpoch = DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime;
    public bool IsAvailable(ModDirectory directory) {
      if (_items.TryGetValue(directory.OriginPath, out var t)) {
        t.Deconstruct(out var contentDirectory, out var workshopItem);
        return workshopItem.TimeUpdated > UnixEpoch;
      }
      return false;
    }

    public bool IsUpdatable(ModDirectory directory) {
      if (_items.TryGetValue(directory.OriginPath, out var t)) {
        t.Deconstruct(out var contentDirectory, out var workshopItem);
        return contentDirectory.TimeUpdated < workshopItem.TimeUpdated;
      }
      return false;
    }

    public bool IsDownloadPending(ModDirectory directory) {
      if (_items.TryGetValue(directory.OriginPath, out var t)) {
        t.Deconstruct(out var contentDirectory, out var workshopItem);
        return contentDirectory.DownloadPending;
      }
      return false;
    }

    public bool TryLoadModManifest(ModDirectory directory, out ModManifest manifest) {
      if (_modLoader.TryLoadMod(directory, out var mod)) {
        manifest = mod.Manifest;
        return true;
      }
      manifest = null;
      return false;
    }

    public bool UpdateModDirectory(ModDirectory directory) {
      return _steamWorkshopContentProvider.UpdateContentDirectory(directory.OriginPath);
    }
  }
}
