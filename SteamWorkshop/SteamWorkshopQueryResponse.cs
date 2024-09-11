using System.Collections.Generic;
using Steamworks;

namespace Mods.SteamUpdateButtons.SteamWorkshop {
  public class SteamWorkshopQueryResponse {
    public SteamWorkshopQueryRequest Request { get; }

    public EResult Result { get; }
    public List<SteamWorkshopItem> Items { get; }

    public bool Successful => Result == EResult.k_EResultOK;

    public string ResultMessage => $"{Result.ToString()} ({(int)Result})";

    public SteamWorkshopQueryResponse(SteamWorkshopQueryRequest request, EResult result) {
      Request = request;
      Result = result;
      Items = new List<SteamWorkshopItem>();
    }

    public void AddItem(SteamWorkshopItem item) {
      Items.Add(item);
    }
  }
}
