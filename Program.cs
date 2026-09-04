using FrameworkTest.Application;
using FrameworkTest.Configuration;

ApplicationRunResult result;

do
{
    GameConfig config = ConfigLoader.Load("config.json");
    var application = new GameApplication(config);
    result = application.Run();
}
while (result == ApplicationRunResult.Restart);
