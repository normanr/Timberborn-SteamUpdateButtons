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
        tuple.Deconstruct(out var state, out var mod);
        Debug.Log("- " + state.ToString().Replace("k_EItemState", "") + ": " + mod.ModDirectory.Directory.Name + "/" + mod.Manifest.Name + " (" + mod.Manifest.Version.AsFormattedString() + ")");
      }

      Debug.Log("Triggering query for installed workshop items");
      QueryInstalledWorkshopItems((items, param, ioFailure) => {
        Debug.Log("Steam mods (subscribed at server):");
        if (ioFailure) {
          Debug.Log("- ioFailure");
        };
        if (items == null) {
          Debug.Log("- null items");
        };
        foreach (var item in items) {
          Debug.Log("- " + item.Title + "/" + item.Description + ": Created=" + item.UgcDetails.m_rtimeCreated + ", Updated=" + item.UgcDetails.m_rtimeUpdated);
        };
      });
    }

    // From https://gist.github.com/GMMan/d9305ffde52372d926662522ed9259b1

    class SteamWorkshopItem {
      public PublishedFileId_t FileId { get; set; }
      public string Title { get; set; }
      public string Description { get; set; }
      public string Metadata { get; set; }
      public ERemoteStoragePublishedFileVisibility Visibility { get; set; }
      public List<string> Tags { get; set; }
      public Dictionary<string, string> KeyValues { get; set; }
      public string UpdateLanguage { get; set; }
      public string UpdateContentPath { get; set; }
      public string UpdatePreviewPath { get; set; }
      public string UpdateChangeNotes { get; set; }
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
      SteamUGC.SetReturnLongDescription(query, true);
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

    private IEnumerable<Tuple<EItemState, Mod>> GetMods(ModLoader modLoader) {
      foreach (var t in GetModDirectories(modLoader)) {
        t.Deconstruct(out var state, out var modDirectory);
        if (modLoader.TryLoadMod(modDirectory, out var mod)) {
          yield return new Tuple<EItemState, Mod>(state, mod);
        }
      }
    }

    // From Timberborn.SteamWorkshopModDownloading.SteamWorkshopModsProvider

    public IEnumerable<Tuple<EItemState, ModDirectory>> GetModDirectories(ModLoader modLoader) {
      foreach (var tuple in GetContentDirectories()) {
        tuple.Deconstruct(out var state, out var contentDirectory);
        if (modLoader.IsModDirectory(new DirectoryInfo(contentDirectory))) {
          yield return new Tuple<EItemState, ModDirectory>(state, new ModDirectory(new DirectoryInfo(contentDirectory), isUserMod: false, "Steam Workshop"));
        }
      }
    }

    // From Timberborn.SteamWorkshopContent.SteamWorkshopContentProvider

    public IEnumerable<Tuple<EItemState, string>> GetContentDirectories() {
      foreach (PublishedFileId_t subscribedItem in GetSubscribedItems()) {
        if (SteamUGC.GetItemInstallInfo(subscribedItem, out var _, out var pchFolder, PathBufferSize, out var _)) {
          yield return new Tuple<EItemState, string>((EItemState)SteamUGC.GetItemState(subscribedItem), pchFolder);
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
