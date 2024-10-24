using Timberborn.CoreUI;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.ElementBuilders;
using TimberApi.UIBuilderSystem.StyleSheetSystem;
using TimberApi.UIBuilderSystem.StyleSheetSystem.Extensions;
using TimberApi.UIBuilderSystem.StyleSheetSystem.PropertyEnums;

namespace Mods.SteamUpdateButtons {
  public class UpdateAvailableImage : BaseBuilder<NineSliceVisualElement> {
    protected override NineSliceVisualElement InitializeRoot() {
      return UIBuilder.Create<VisualElementBuilder>()
          .AddClass("update-available-image")
          .Build();
    }

    protected override void InitializeStyleSheet(StyleSheetBuilder styleSheetBuilder) {
      styleSheetBuilder
          .AddBackgroundClass("update-available-image", "ui/images/buttons/migration/allow-emigration")
          .AddClass("update-available-image", builder => builder
              .Height(24)
              .Width(24)
              .MarginTop(10)
              .AlignSelf(AlignSelf.FlexEnd));
    }
  }
}
