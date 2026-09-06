using FrameworkTest.Application;
using FrameworkTest.Assets;
using RaylibGameFramework.Menus;
using RaylibGameFramework.Configuration;
using RaylibGameFramework.Input;

ApplicationRunResult result;

do
{
    GameConfig config = ConfigLoader.Load("config.json");
    InputConfig inputConfig = InputConfigLoader.Load("input.json");
    MenuConfig menuConfig = MenuConfigLoader.Load("menu.json");
    AssetConfig assetConfig = AssetConfigLoader.Load("assets.json");
    var application = new GameApplication(config, inputConfig, menuConfig, assetConfig);
    result = application.Run();
}
while (result == ApplicationRunResult.Restart);
