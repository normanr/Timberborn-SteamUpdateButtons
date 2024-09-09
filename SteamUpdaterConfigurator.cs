using Bindito.Core;
using Mods.SteamInfo.SteamWorkshopContent;
using Mods.SteamInfo.SteamWorkshopModDownloading;

namespace Mods.SteamInfo {
  [Context("MainMenu")]
  internal class SteamUpdaterConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<ModItemUpdateInitializer>().AsSingleton();
      containerDefinition.Bind<SteamWorkshopContentProvider>().AsSingleton();
      containerDefinition.Bind<SteamWorkshopModsProvider>().AsSingleton();
      containerDefinition.Bind<UpdateButton>().AsTransient();
    }

  }
}
