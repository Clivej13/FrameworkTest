using FrameworkTest.Application;
using RaylibGameFramework.Configuration;
using RaylibGameFramework.Input;

ApplicationRunResult result;

do
{
    GameConfig config = ConfigLoader.Load("config.json");
    InputConfig inputConfig = InputConfigLoader.Load("input.json");
    var application = new GameApplication(config, inputConfig);
    result = application.Run();
}
while (result == ApplicationRunResult.Restart);
