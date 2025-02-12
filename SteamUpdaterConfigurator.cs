﻿using Bindito.Core;
using Mods.SteamUpdateButtons.SteamWorkshopContent;
using Mods.SteamUpdateButtons.SteamWorkshopModDownloading;

namespace Mods.SteamUpdateButtons {
  [Context("MainMenu")]
  internal class SteamUpdaterConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<ModManagerButtonIndicatorInitializer>().AsSingleton();
      containerDefinition.Bind<ModManagerBoxInitializer>().AsSingleton();
      containerDefinition.Bind<ModItemUpdateInitializer>().AsSingleton();
      containerDefinition.Bind<SteamWorkshopContentProvider>().AsSingleton();
      containerDefinition.Bind<SteamWorkshopModsProvider>().AsSingleton();
      containerDefinition.Bind<UpdateAvailableImage>().AsTransient();
      containerDefinition.Bind<UpdateButton>().AsTransient();
      containerDefinition.Bind<UnavailableImage>().AsTransient();
    }

  }
}
