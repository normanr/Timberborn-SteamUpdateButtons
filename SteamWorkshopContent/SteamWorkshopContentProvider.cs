using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Steamworks;
using Timberborn.SingletonSystem;
using Timberborn.SteamStoreSystem;

namespace Mods.SteamUpdateButtons.SteamWorkshopContent {
  public class SteamWorkshopContentProvider : Timberborn.SteamWorkshopContent.SteamWorkshopContentProvider, ILoadableSingleton {
    private static readonly uint PathBufferSize = 1024u;

    private readonly SteamManager _steamManager;

    private Callback<DownloadItemResult_t> m_DownloadItemResult;
    private readonly Dictionary<PublishedFileId_t, Action<bool>> _pendingDownloadCallbacks = [];

    public SteamWorkshopContentProvider(SteamManager steamManager) : base(steamManager) {
      _steamManager = steamManager;
    }

    public void Load() {
      if (_steamManager.Initialized) {
        m_DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnDownloadItemResult);
      }
    }

    public IEnumerable<ContentDirectory> GetContentDirectoryDetails() {
      if (!_steamManager.Initialized) {
        yield break;
      }
      foreach (PublishedFileId_t subscribedItem in GetSubscribedItems()) {
        if (SteamUGC.GetItemInstallInfo(subscribedItem, out var _, out var pchFolder, PathBufferSize, out var punTimeStamp)) {
          var state = (EItemState)SteamUGC.GetItemState(subscribedItem);
          yield return new ContentDirectory(
            (ulong)subscribedItem,
            pchFolder,
            DateTimeOffset.FromUnixTimeSeconds(punTimeStamp).UtcDateTime,
            (state & EItemState.k_EItemStateDownloadPending) == EItemState.k_EItemStateDownloadPending
          );
        }
      }
    }

    private static IEnumerable<PublishedFileId_t> GetSubscribedItems() {
      uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
      PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
      SteamUGC.GetSubscribedItems(array, numSubscribedItems);
      return array;
    }

    public bool UpdateContentDirectory(string contentDirectory, Action<bool> callback) {
      // TODO, HACK: Match this with ItemInstallInfo.Folder instead
      if (!ulong.TryParse(Path.GetFileName(contentDirectory), out ulong id)) {
        throw new Exception("Failed to parse as uint: " + Path.GetFileName(contentDirectory));
      }
      var r = SteamUGC.DownloadItem((PublishedFileId_t)id, true);
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: SteamUGC.DownloadItem(" + id + ") == " + r);
      if (r) {
        _pendingDownloadCallbacks[(PublishedFileId_t)id] = callback;
      }
      return r;
    }

    public bool GetDownloadProgress(string contentDirectory, out ulong downloaded, out ulong total) {
      // TODO, HACK: Match this with ItemInstallInfo.Folder instead
      if (!ulong.TryParse(Path.GetFileName(contentDirectory), out ulong id)) {
        throw new Exception("Failed to parse as uint: " + Path.GetFileName(contentDirectory));
      }
      return SteamUGC.GetItemDownloadInfo((PublishedFileId_t)id, out downloaded, out total);
    }

    private void OnDownloadItemResult(DownloadItemResult_t pCallback) {
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: OnDownloadItemResult(" + pCallback.m_unAppID + ", " + pCallback.m_nPublishedFileId + ") = " + pCallback.m_eResult);
      if (_pendingDownloadCallbacks.TryGetValue(pCallback.m_nPublishedFileId, out var action)) {
        _pendingDownloadCallbacks.Remove(pCallback.m_nPublishedFileId);
        action(pCallback.m_eResult == EResult.k_EResultOK);
      }
    }
  }
}
