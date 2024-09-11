using System;
namespace Mods.SteamUpdateButtons.SteamWorkshopContent {
  public class ContentDirectory {
    public ulong ItemId { get; }
    public string Folder { get; }
    public DateTime TimeUpdated { get; set; }

    public ContentDirectory(ulong itemId, string folder, DateTime timeUpdated) {
      ItemId = itemId;
      Folder = folder;
      TimeUpdated = timeUpdated;
    }
  }
}
