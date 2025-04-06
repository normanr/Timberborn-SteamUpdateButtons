using Bindito.Core;
using Mods.SteamUpdateButtons.SteamWorkshopContent;
using Mods.SteamUpdateButtons.SteamWorkshopModDownloading;

namespace Mods.SteamUpdateButtons {
  [Context("Bootstrapper")]
  internal class SteamUpdaterBootstrapperConfigurator : Configurator {

    protected override void Configure() {
      Bind<SteamWorkshopContentProvider>().AsSingleton().AsExported();
      Bind<SteamWorkshopModsProvider>().AsSingleton().AsExported();
    }

  }

  [Context("MainMenu")]
  internal class SteamUpdaterConfigurator : Configurator {

    protected override void Configure() {
      Bind<ModManagerButtonIndicatorInitializer>().AsSingleton();
      Bind<ModManagerBoxInitializer>().AsSingleton();
      Bind<ModItemUpdateInitializer>().AsSingleton();
      Bind<UpdateAvailableImage>().AsTransient();
      Bind<UpdateButton>().AsTransient();
      Bind<UnavailableImage>().AsTransient();
      Bind<DownloadPendingImage>().AsTransient();
    }

  }
}
