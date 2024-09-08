using System;
using UnityEngine.UIElements;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.ElementBuilders;
using TimberApi.UIBuilderSystem.StyleSheetSystem;
using TimberApi.UIBuilderSystem.StyleSheetSystem.Extensions;

namespace Mods.SteamInfo {
  public class UpdateButton : BaseBuilder<Button> {
    protected override Button InitializeRoot() {
      return UIBuilder.Create<ButtonBuilder>()
          .AddClass("update-button")
          .Build();
    }

    protected override void InitializeStyleSheet(StyleSheetBuilder styleSheetBuilder) {
      styleSheetBuilder
          .AddBackgroundHoverClass("update-button", "ui/images/buttons/migration/allow-emigration", "ui/images/buttons/migration/allow-emigration-hover")
          .AddClass("update-button", builder => builder
              .ClickSound("UI.Click")
              .Height(28)
              .Width(28));
    }
  }
}
