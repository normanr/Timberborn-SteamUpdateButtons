using System;
using Steamworks;

namespace Mods.SteamUpdateButtons.SteamWorkshop {
  public class SteamWorkshopQueryRequester {

    public SteamWorkshopQueryHandle Query(SteamWorkshopQueryRequest request, Action<SteamWorkshopQueryResponse> queryCallback) {
      PublishedFileId_t[] files = new PublishedFileId_t[request.Files.Length];
      request.Files.CopyTo(files);
      UGCQueryHandle_t uGCQueryHandle_t = SteamUGC.CreateQueryUGCDetailsRequest(files, (uint)files.Length);
      SetUpdateContent(request, uGCQueryHandle_t);
      SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(uGCQueryHandle_t);
      if (hAPICall == SteamAPICall_t.Invalid) return null;
      CallResult<SteamUGCQueryCompleted_t>.Create().Set(hAPICall, delegate (SteamUGCQueryCompleted_t t, bool failure) {
        OnQueryCompleted(uGCQueryHandle_t, t, failure, queryCallback, request);
      });
      return new SteamWorkshopQueryHandle(uGCQueryHandle_t);
    }

    private static void SetUpdateContent(SteamWorkshopQueryRequest queryRequest, UGCQueryHandle_t query) {
      // SteamUGC.SetReturnMetadata(query, true);
    }

#if TEST
    static int testCounter;
#endif

    private static void OnQueryCompleted(UGCQueryHandle_t query, SteamUGCQueryCompleted_t result, bool ioFailure, Action<SteamWorkshopQueryResponse> queryCallback, SteamWorkshopQueryRequest request) {
      SteamWorkshopQueryResponse response = new SteamWorkshopQueryResponse(request, ioFailure ? EResult.k_EResultIOFailure : result.m_eResult);
      if (!ioFailure && result.m_eResult == EResult.k_EResultOK) {
        for (uint i = 0; i < result.m_unNumResultsReturned; ++i) {
          var r = SteamUGC.GetQueryUGCResult(query, i, out SteamUGCDetails_t details);
#if TEST
          if (testCounter < 3) {
            testCounter++;
            details.m_rtimeUpdated *= i;
          }
#endif
          SteamWorkshopItem item = new SteamWorkshopItem(
            (ulong)details.m_nPublishedFileId,
            details.m_rgchTitle,
            DateTimeOffset.FromUnixTimeSeconds(details.m_rtimeUpdated).UtcDateTime
          );
          response.AddItem(item);
        }
      }
      queryCallback(response);
    }
  }
}
