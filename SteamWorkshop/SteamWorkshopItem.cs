using System;

namespace Mods.SteamUpdateButtons.SteamWorkshop {
  public class SteamWorkshopItem { // : Timberborn.SteamWorkshop.SteamWorkshopItem {
    public ulong ItemId { get; }
    public string Name { get; }
    public DateTime TimeUpdated { get; set; }

    public SteamWorkshopItem(ulong itemId, string name, DateTime timeUpdated) {
      ItemId = itemId;
      Name = name;
      TimeUpdated = timeUpdated;
    }
  }
}
