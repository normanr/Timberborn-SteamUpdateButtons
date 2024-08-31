using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;
using Timberborn.ModManagerScene;
using Timberborn.Modding;
using Timberborn.SerializationSystem;

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

      Debug.Log("Steam mods:");
      var merger = new JsonMerger();
      var obrw = new ObjectSaveReaderWriter(merger);
      var modLoader = new ModLoader(obrw);
      foreach (var tuple in GetMods(modLoader)) {
        tuple.Deconstruct(out var state, out var mod);
        Debug.Log("- " + state.ToString().Replace("k_EItemState", "") + ": " + mod.ModDirectory.Directory.Name + "/" + mod.Manifest.Name + " (" + mod.Manifest.Version.AsFormattedString() + ")");
      }
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
