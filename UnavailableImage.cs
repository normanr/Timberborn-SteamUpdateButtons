using Timberborn.CoreUI;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.ElementBuilders;
using TimberApi.UIBuilderSystem.StyleSheetSystem;
using TimberApi.UIBuilderSystem.StyleSheetSystem.Extensions;

namespace Mods.SteamUpdateButtons {
  public class UnavailableImage : BaseBuilder<NineSliceVisualElement> {
    protected override NineSliceVisualElement InitializeRoot() {
      return UIBuilder.Create<VisualElementBuilder>()
          .AddClass("unavailable-image")
          .Build();
    }

    protected override void InitializeStyleSheet(StyleSheetBuilder styleSheetBuilder) {
      styleSheetBuilder
          .AddBackgroundClass("unavailable-image", "sprites/statusicons/stranded")
          .AddClass("unavailable-image", builder => builder
              .Height(28)
              .Width(28)
              .MarginLeft(7));
    }
  }
}
