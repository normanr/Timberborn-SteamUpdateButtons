using Steamworks;

namespace Mods.SteamInfo.SteamWorkshop {
  public class SteamWorkshopQueryHandle {
    private readonly UGCQueryHandle_t _queryHandle;

    public SteamWorkshopQueryHandle(UGCQueryHandle_t queryHandle) {
      _queryHandle = queryHandle;
    }

  }
}
