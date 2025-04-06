using System;
namespace Mods.SteamUpdateButtons.SteamWorkshopContent {
  public class ContentDirectory {
    public ulong ItemId { get; }
    public string Folder { get; }
    public DateTime TimeUpdated { get; set; }
    public bool DownloadPending { get; set; }

    public ContentDirectory(ulong itemId, string folder, DateTime timeUpdated, bool downloadPending) {
      ItemId = itemId;
      Folder = folder;
      TimeUpdated = timeUpdated;
      DownloadPending = downloadPending;
    }
  }
}
