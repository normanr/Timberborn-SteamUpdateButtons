using Timberborn.CoreUI;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.ElementBuilders;
using TimberApi.UIBuilderSystem.StyleSheetSystem;
using TimberApi.UIBuilderSystem.StyleSheetSystem.Extensions;

namespace Mods.SteamUpdateButtons {
  public class DownloadPendingImage : BaseBuilder<NineSliceVisualElement> {
    protected override NineSliceVisualElement InitializeRoot() {
      return UIBuilder.Create<VisualElementBuilder>()
          .AddClass("download-pending-image")
          .Build();
    }

    protected override void InitializeStyleSheet(StyleSheetBuilder styleSheetBuilder) {
      styleSheetBuilder
          .AddBackgroundClass("download-pending-image", "ui/images/game/ico-child-grow")
          .AddClass("download-pending-image", builder => builder
              .Height(28)
              .Width(28)
              .MarginLeft(7));
    }
  }
}
