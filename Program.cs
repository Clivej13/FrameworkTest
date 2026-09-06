using FrameworkTest.Application;
using RaylibGameFramework.Menus;
using RaylibGameFramework.Configuration;
using RaylibGameFramework.Input;

ApplicationRunResult result;

do
{
    GameConfig config = ConfigLoader.Load("config.json");
    InputConfig inputConfig = InputConfigLoader.Load("input.json");
    MenuConfig menuConfig = MenuConfigLoader.Load("menu.json");
    var application = new GameApplication(config, inputConfig, menuConfig);
    result = application.Run();
}
while (result == ApplicationRunResult.Restart);
