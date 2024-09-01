using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;
using Timberborn.ModManagerScene;
using Timberborn.Modding;
using Timberborn.SerializationSystem;
using Timberborn.SteamStoreSystem;

namespace Mods.SteamInfo {
  internal class ModStarter : IModStarter {
    private static readonly uint PathBufferSize = 1024u;

    public void StartMod() {
      StartMod(null);
    }

    public void StartMod(IModEnvironment modEnvironment) {
      if (modEnvironment != null) {
        Debug.Log("SteamInfo.ModPath = " + modEnvironment.ModPath);
      }

      Debug.Log("Steam mods (local cache):");
      var merger = new JsonMerger();
      var obrw = new ObjectSaveReaderWriter(merger);
      var modLoader = new ModLoader(obrw);
      foreach (var tuple in GetMods(modLoader)) {
        tuple.Deconstruct(out var state, out var lastUpdate, out var mod);
        Debug.Log("- " + state.ToString().Replace("k_EItemState", "") + ": " + mod.ModDirectory.Directory.Name + "/" + mod.Manifest.Name + " (" + mod.Manifest.Version.AsFormattedString() + "), Updated=" + lastUpdate.ToString("o"));
      }

      Debug.Log("Triggering query for installed workshop items");
      QueryInstalledWorkshopItems((items, param, ioFailure) => {
        Debug.Log("Steam mods (subscribed at server):");
        if (ioFailure) {
          Debug.Log("- ioFailure");
        };
        if (items == null) {
          Debug.Log("- null items");
        } else {
          foreach (var item in items) {
            Debug.Log("- " + item.FileId + "/" + item.Title + ", Updated=" + item.TimeUpdated.ToString("o"));
          };
        };
      });
    }

    // From https://gist.github.com/GMMan/d9305ffde52372d926662522ed9259b1

    class SteamWorkshopItem {
      public PublishedFileId_t FileId { get; set; }
      public string Title { get; set; }
      public string Description { get; set; }
      public string Metadata { get; set; }
      public DateTime TimeUpdated { get; set; }
      public ERemoteStoragePublishedFileVisibility Visibility { get; set; }
      public List<string> Tags { get; set; }
      public Dictionary<string, string> KeyValues { get; set; }
      public SteamUGCDetails_t UgcDetails { get; internal set; }

      public SteamWorkshopItem() {
        Tags = new List<string>();
        KeyValues = new Dictionary<string, string>();
      }
    }

    List<CallResult<SteamUGCQueryCompleted_t>> ugcQueryCompletedCallResults = new List<CallResult<SteamUGCQueryCompleted_t>>();
    delegate void ugcQueryCompletedHandler(List<SteamWorkshopItem> items, SteamUGCQueryCompleted_t param, bool ioFailure);

    bool QueryInstalledWorkshopItems(ugcQueryCompletedHandler handler) {
      uint subscribedCount = SteamUGC.GetNumSubscribedItems();
      PublishedFileId_t[] subscribedFiles = new PublishedFileId_t[subscribedCount];
      if (SteamUGC.GetSubscribedItems(subscribedFiles, (uint)subscribedFiles.Length) != subscribedCount)
        return false;

      UGCQueryHandle_t query = SteamUGC.CreateQueryUGCDetailsRequest(subscribedFiles, (uint)subscribedFiles.Length);
      SteamUGC.SetReturnMetadata(query, true);
      SteamAPICall_t apiCall = SteamUGC.SendQueryUGCRequest(query);
      var callResult = CallResult<SteamUGCQueryCompleted_t>.Create();
      callResult.Set(apiCall, (param, ioFailure) =>
      {
        List<SteamWorkshopItem> items = null;
        if (!ioFailure && param.m_eResult == EResult.k_EResultOK) {
          items = new List<SteamWorkshopItem>();
          for (uint i = 0; i < param.m_unNumResultsReturned; ++i) {
            SteamWorkshopItem item = new SteamWorkshopItem();
            SteamUGC.GetQueryUGCResult(query, i, out SteamUGCDetails_t details);
            item.FileId = details.m_nPublishedFileId;
            item.Title = details.m_rgchTitle;
            item.Description = details.m_rgchDescription;
            SteamUGC.GetQueryUGCMetadata(query, i, out string metadata, Constants.k_cchDeveloperMetadataMax);
            item.Metadata = metadata;
            item.TimeUpdated = DateTimeOffset.FromUnixTimeSeconds(details.m_rtimeUpdated).LocalDateTime;
            item.Visibility = details.m_eVisibility;
            item.Tags.AddRange(details.m_rgchTags.Split(','));
            // KeyValues should be converted to a List<KeyValuePair<string, string>>, so ignore for now
            item.UgcDetails = details;
            items.Add(item);
          }
        }
        ugcQueryCompletedCallResults.Remove(callResult);
        SteamUGC.ReleaseQueryUGCRequest(query);
        handler(items, param, ioFailure);
      });
      ugcQueryCompletedCallResults.Add(callResult);
      return true;
    }

    // From Timberborn.Modding.ModRepository

    private IEnumerable<Tuple<EItemState, DateTime, Mod>> GetMods(ModLoader modLoader) {
      foreach (var t in GetModDirectories(modLoader)) {
        t.Deconstruct(out var state, out var lastUpdate, out var modDirectory);
        if (modLoader.TryLoadMod(modDirectory, out var mod)) {
          yield return new Tuple<EItemState, DateTime, Mod>(state, lastUpdate, mod);
        }
      }
    }

    // From Timberborn.SteamWorkshopModDownloading.SteamWorkshopModsProvider

    public IEnumerable<Tuple<EItemState, DateTime, ModDirectory>> GetModDirectories(ModLoader modLoader) {
      foreach (var tuple in GetContentDirectories()) {
        tuple.Deconstruct(out var state, out var contentDirectory, out var timeStamp);
        if (modLoader.IsModDirectory(new DirectoryInfo(contentDirectory))) {
          var lastUpdate = DateTimeOffset.FromUnixTimeSeconds(timeStamp).LocalDateTime;
          var modDirectory = new ModDirectory(new DirectoryInfo(contentDirectory), isUserMod: false, "Steam Workshop");
          yield return new Tuple<EItemState, DateTime, ModDirectory>(state, lastUpdate, modDirectory);
        }
      }
    }

    // From Timberborn.SteamWorkshopContent.SteamWorkshopContentProvider

    public IEnumerable<Tuple<EItemState, string, uint>> GetContentDirectories() {
      foreach (PublishedFileId_t subscribedItem in GetSubscribedItems()) {
        if (SteamUGC.GetItemInstallInfo(subscribedItem, out var _, out var pchFolder, PathBufferSize, out var punTimeStamp)) {
          yield return new Tuple<EItemState, string, uint>((EItemState)SteamUGC.GetItemState(subscribedItem), pchFolder, punTimeStamp);
        }
      }
    }

    private static IEnumerable<PublishedFileId_t> GetSubscribedItems() {
      uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
      PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
      SteamUGC.GetSubscribedItems(array, numSubscribedItems);
      return array;
    }
  }
}
