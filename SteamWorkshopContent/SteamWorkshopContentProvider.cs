﻿using System;
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
    public event EventHandler DownloadComplete;

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
          yield return new ContentDirectory(
            (ulong)subscribedItem,
            pchFolder,
            DateTimeOffset.FromUnixTimeSeconds(punTimeStamp).UtcDateTime
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

    public void UpdateContentDirectory(string contentDirectory) {
      // TODO, HACK: Match this with ItemInstallInfo.Folder instead
      if (!uint.TryParse(Path.GetFileName(contentDirectory), out uint id)) {
        throw new Exception("Failed to parse as uint: " + Path.GetFileName(contentDirectory));
      }
      var r = SteamUGC.DownloadItem((PublishedFileId_t)id, true);
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: SteamUGC.DownloadItem(" + id + ") == " + r);
#if TEST
      DownloadComplete?.Invoke(this, EventArgs.Empty);
#endif
    }

    private void OnDownloadItemResult(DownloadItemResult_t pCallback) {
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Steam Update Buttons: OnDownloadItemResult(" + pCallback.m_unAppID + ", " + pCallback.m_nPublishedFileId + ") = " + pCallback.m_eResult);
      DownloadComplete?.Invoke(this, EventArgs.Empty);
    }
  }
}
