using Bindito.Core;

namespace Mods.SteamInfo {
  [Context("MainMenu")]
  internal class SteamUpdaterConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<ModItemUpdateInitializer>().AsSingleton();
      containerDefinition.Bind<SteamWorkshopContentProvider>().AsSingleton();
      containerDefinition.Bind<UpdateButton>().AsTransient();
    }

  }
}
