using RaylibGameFramework.Configuration;
using RaylibGameFramework.Input;
using Raylib_cs;

namespace FrameworkTest.Application;

public enum ApplicationRunResult
{
    Exit,
    Restart
}

public sealed class GameApplication
{
    private const int PrimaryMonitor = 0;

    private readonly GameConfig _config;
    private readonly InputController _inputController;
    private ApplicationRunResult _runResult = ApplicationRunResult.Exit;
    private bool _stopRequested;
    private bool _windowInitialized;

    public GameApplication(GameConfig config, InputConfig inputConfig)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _inputController = new InputController(inputConfig);
    }

    public ApplicationRunResult Run()
    {
        try
        {
            Initialize();
            RunMainLoop();
            return _runResult;
        }
        finally
        {
            Shutdown();
        }
    }

    public void RequestExit()
    {
        _runResult = ApplicationRunResult.Exit;
        _stopRequested = true;
    }

    public void RequestRestart()
    {
        _runResult = ApplicationRunResult.Restart;
        _stopRequested = true;
    }

    private void Initialize()
    {
        ConfigFlags flags = 0;

        if (_config.VSync)
        {
            flags |= ConfigFlags.VSyncHint;
        }

        if (_config.Fullscreen)
        {
            flags |= ConfigFlags.FullscreenMode;
        }

        if (flags != 0)
        {
            Raylib.SetConfigFlags(flags);
        }

        Raylib.InitWindow(_config.WindowWidth, _config.WindowHeight, _config.WindowTitle);
        _windowInitialized = true;

        Raylib.SetWindowMonitor(PrimaryMonitor);

        if (!_config.Fullscreen)
        {
            FitWindowToPrimaryMonitor();
        }

        Raylib.SetTargetFPS(_config.TargetFps);
    }

    private void FitWindowToPrimaryMonitor()
    {
        int monitorWidth = Raylib.GetMonitorWidth(PrimaryMonitor);
        int monitorHeight = Raylib.GetMonitorHeight(PrimaryMonitor);

        int windowWidth = Math.Clamp(_config.WindowWidth, 1, monitorWidth);
        int windowHeight = Math.Clamp(_config.WindowHeight, 1, monitorHeight);

        if (Raylib.GetScreenWidth() != windowWidth || Raylib.GetScreenHeight() != windowHeight)
        {
            Raylib.SetWindowSize(windowWidth, windowHeight);
        }

        System.Numerics.Vector2 monitorPosition = Raylib.GetMonitorPosition(PrimaryMonitor);
        int windowX = (int)monitorPosition.X + ((monitorWidth - windowWidth) / 2);
        int windowY = (int)monitorPosition.Y + ((monitorHeight - windowHeight) / 2);

        Raylib.SetWindowPosition(windowX, windowY);
    }

    private void RunMainLoop()
    {
        while (!_stopRequested && !Raylib.WindowShouldClose())
        {
            _inputController.Update();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);
            Raylib.DrawText("Framework Test", 20, 20, 20, Color.DarkGray);
            Raylib.DrawText($"Physical: {_inputController.LastPhysicalInputName}", 20, 60, 30, Color.Black);
            Raylib.DrawText($"Action: {_inputController.LastActionName}", 20, 100, 30, Color.Black);
            Raylib.EndDrawing();
        }
    }

    private void Shutdown()
    {
        if (!_windowInitialized)
        {
            return;
        }

        Raylib.CloseWindow();
        _windowInitialized = false;
    }
}
