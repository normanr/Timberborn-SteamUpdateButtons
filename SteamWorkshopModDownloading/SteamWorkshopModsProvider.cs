using System;
using System.Collections.Generic;
using UnityEngine;
using Timberborn.Modding;
using Timberborn.SingletonSystem;
using Mods.SteamInfo.SteamWorkshop;
using Mods.SteamInfo.SteamWorkshopContent;

namespace Mods.SteamInfo.SteamWorkshopModDownloading {
  public class SteamWorkshopModsProvider : Timberborn.SteamWorkshopModDownloading.SteamWorkshopModsProvider, ILoadableSingleton {
    private readonly SteamWorkshopContentProvider _steamWorkshopContentProvider;
    private readonly ModLoader _modLoader;
    private readonly Dictionary<string, Tuple<ContentDirectory, SteamWorkshopItem>> _items;
    public event EventHandler DownloadComplete;

    public SteamWorkshopModsProvider(SteamWorkshopContentProvider steamWorkshopContentProvider, ModLoader modLoader) : base(steamWorkshopContentProvider, modLoader) {
      _steamWorkshopContentProvider = steamWorkshopContentProvider;
      _modLoader = modLoader;
      _items = new Dictionary<string, Tuple<ContentDirectory, SteamWorkshopItem>>();
    }

    public void Load() {
      RefreshWorkshopItems(null);
      _steamWorkshopContentProvider.DownloadComplete += (sender, e) => {
        RefreshWorkshopItems(() => {
          DownloadComplete?.Invoke(this, EventArgs.Empty);
        });
      };
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

      Debug.Log(DateTime.Now.ToString("HH:mm:ss.fff") + " Steam workshop items:");
      requestor.Query(request, (response) => {
        Debug.Log(DateTime.Now.ToString("HH:mm:ss.fff") + " result = " + response.ResultMessage.Replace("k_EResult", ""));
        if (response.Successful) {
          foreach (var item in response.Items) {
            var contentDirectory = detailsById[item.ItemId];

            string state;
            if (item.TimeUpdated > contentDirectory.TimeUpdated) {
              state = "Newer";
            } else if (item.TimeUpdated < contentDirectory.TimeUpdated) {
              state = "Older";
            } else {
              state = "Equal";
            }
            Debug.Log(DateTime.Now.ToString("HH:mm:ss.fff") + " - [" + state + "] " + item.ItemId + "/" + item.Name + ", Server=" + item.TimeUpdated.ToLocalTime().ToString("o") + ", Local=" + contentDirectory.TimeUpdated.ToLocalTime().ToString("o"));

            _items[contentDirectory.Folder] = new Tuple<ContentDirectory, SteamWorkshopItem>(contentDirectory, item);
          }
        }
        callback?.Invoke();
      });
    }

    public bool IsUpdatable(ModDirectory directory) {
      if (_items.TryGetValue(directory.Path, out var t)) {
        t.Deconstruct(out var contentDirectory, out var workshopItem);
        return contentDirectory.TimeUpdated < workshopItem.TimeUpdated;
      }
      return false;
    }

    public void UpdateModDirectory(ModDirectory directory) {
      _steamWorkshopContentProvider.UpdateContentDirectory(directory.Path);
    }
  }
}
