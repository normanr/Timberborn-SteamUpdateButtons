using System;
using System.IO;
using UnityEngine;
using Steamworks;
using Timberborn.SingletonSystem;
using Timberborn.SteamStoreSystem;

namespace Mods.SteamInfo {
  internal class SteamWorkshopContentProvider : ILoadableSingleton {
    protected Callback<DownloadItemResult_t> m_DownloadItemResult;

    private readonly SteamManager _steamManager;

    public SteamWorkshopContentProvider(SteamManager steamManager) {
      _steamManager = steamManager;
    }

    public void Load() {
      if (_steamManager.Initialized) {
        m_DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnDownloadItemResult);
      }
    }

    public void UpdateItem(string directory) {
      if (!uint.TryParse(Path.GetFileName(directory), out uint id)) {
        throw new Exception("Failed to parse as uint: " + Path.GetFileName(directory));
      }
      var r = SteamUGC.DownloadItem((PublishedFileId_t)id, true);
      Debug.Log(DateTime.Now.ToString("hh:mm:ss.fff") + ": SteamUGC.DownloadItem(" + id + ") == " + r);
    }

    private void OnDownloadItemResult(DownloadItemResult_t pCallback) {
      Debug.Log(DateTime.Now.ToString("hh:mm:ss.fff") + ": DownloadItemResult(" + pCallback.m_unAppID + ", " + pCallback.m_nPublishedFileId + ") = " + pCallback.m_eResult);
    }
  }
}