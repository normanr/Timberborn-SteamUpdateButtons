using System;
using System.Reflection;
using Timberborn.Modding;

namespace Mods.SteamUpdateButtons.Modding {
  internal static class ModRepositoryExtensions {

    internal static bool TryGetModDirectory(this ModRepository modRepository, ModDirectory modDirectory, out ModDirectory versionedModDirectory) {
      var tryGetModDirectoryMethod = modRepository.GetType()
          .GetMethod("TryGetModDirectory", BindingFlags.Static | BindingFlags.NonPublic);
      if (tryGetModDirectoryMethod == null) {
        throw new Exception($"Method named TryGetModDirectory "
                   + $"wasn't found in {modRepository.GetType().Name}");
      }
      var parameters = new object[] { modDirectory, null };
      var result = (bool)tryGetModDirectoryMethod.Invoke(modRepository, parameters);
      versionedModDirectory = (ModDirectory)parameters[1];
      return result;
    }

  }
}
