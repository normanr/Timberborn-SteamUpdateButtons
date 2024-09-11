using System.Collections.Generic;
using System.Collections.Immutable;
using Steamworks;

namespace Mods.SteamUpdateButtons.SteamWorkshop {
  public class SteamWorkshopQueryRequest {
    public ImmutableArray<PublishedFileId_t> Files { get; }

    public SteamWorkshopQueryRequest(IEnumerable<ulong> files) {
      var aULong = files.ToImmutableArray();
      var aPublishedFileId_t = new PublishedFileId_t[aULong.Length];
      for (var i = 0; i < aULong.Length; i++)
        aPublishedFileId_t[i] = (PublishedFileId_t)aULong[i];
      Files = aPublishedFileId_t.ToImmutableArray();
    }
  }
}
