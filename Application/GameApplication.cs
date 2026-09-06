using RaylibGameFramework.Menus;
using RaylibGameFramework.Configuration;
using RaylibGameFramework.Input;
using Raylib_cs;
using FrameworkTest.Assets;

namespace FrameworkTest.Application;

public enum ApplicationRunResult
{
    Exit,
    Restart
}

public sealed class GameApplication
{
    private const int PrimaryMonitor = 0;

    private readonly AssetManager _assetManager;
    private readonly GameConfig _config;
    private readonly InputController _inputController;
    private readonly MenuManager _menuManager;
    private ApplicationRunResult _runResult = ApplicationRunResult.Exit;
    private ApplicationState _state = ApplicationState.Loading;
    private bool _stopRequested;
    private bool _windowInitialized;
    private string? _lastMenuAction;
    private int _preferredWindowedWidth;
    private int _preferredWindowedHeight;

    public GameApplication(GameConfig config, InputConfig inputConfig, MenuConfig menuConfig, AssetConfig assetConfig)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _assetManager = new AssetManager(assetConfig);
        _inputController = new InputController(inputConfig);
        _menuManager = new MenuManager(menuConfig, _inputController, inputConfig);
        _preferredWindowedWidth = config.WindowWidth;
        _preferredWindowedHeight = config.WindowHeight;
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
        int windowWidth = Math.Clamp(_preferredWindowedWidth, 1, monitorWidth);
        int windowHeight = Math.Clamp(_preferredWindowedHeight, 1, monitorHeight);

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

            if (_state == ApplicationState.Menu)
            {
                HandleMenuAction(_menuManager.Update());
            }
            else if (_state == ApplicationState.GameStarted && _inputController.WasPressed("MenuBack"))
            {
                _state = ApplicationState.Menu;
                _menuManager.ReturnToStartMenu();
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);

            if (_state == ApplicationState.Menu)
            {
                _menuManager.Draw();
                DrawLastMenuAction();
                // FrameworkTest-only proof that the texture survives the state transition.
                Raylib.DrawTextureEx(_assetManager.GetTexture("TestTexture"),
                    new System.Numerics.Vector2(16, Raylib.GetScreenHeight() - 80), 0, 3, Color.White);
            }
            else if (_state == ApplicationState.Loading)
            {
                DrawLoading();
            }
            else
            {
                DrawGameStarted();
            }

            Raylib.EndDrawing();

            // Present a loading frame before doing one synchronous native load.
            if (_state == ApplicationState.Loading && _assetManager.LoadNext())
            {
                _state = ApplicationState.Menu;
            }
        }
    }

    private void DrawLoading()
    {
        string text = $"Loading {_assetManager.LoadedCount} / {_assetManager.TotalCount}";
        int x = (Raylib.GetScreenWidth() - Raylib.MeasureText(text, 28)) / 2;
        Raylib.DrawText(text, x, Raylib.GetScreenHeight() / 2, 28, Color.DarkGray);
    }

    private void HandleMenuAction(MenuAction? action)
    {
        if (action is null)
        {
            return;
        }

        _lastMenuAction = action.Value is null
            ? action.Function
            : $"{action.Function} = {action.Value}";

        switch (action.Function)
        {
            case "StartGame":
                _state = ApplicationState.GameStarted;
                break;
            case "ExitGame":
                RequestExit();
                break;
            case "SetResolution" when action.Value is string resolution:
                SetResolution(resolution);
                break;
        }
    }

    private void SetResolution(string value)
    {
        string[] dimensions = value.Split('x', StringSplitOptions.TrimEntries);
        if (dimensions.Length != 2 ||
            !int.TryParse(dimensions[0], out int width) ||
            !int.TryParse(dimensions[1], out int height) ||
            width <= 0 || height <= 0)
        {
            return;
        }

        _preferredWindowedWidth = width;
        _preferredWindowedHeight = height;

        if (Raylib.IsWindowFullscreen())
        {
            return;
        }

        Raylib.SetWindowMonitor(PrimaryMonitor);
        FitWindowToPrimaryMonitor();
    }

    private void DrawLastMenuAction()
    {
        if (string.IsNullOrEmpty(_lastMenuAction))
        {
            return;
        }

        string text = $"Last action: {_lastMenuAction}";
        int x = Raylib.GetScreenWidth() - Raylib.MeasureText(text, 18) - 16;
        Raylib.DrawText(text, Math.Max(16, x), 10, 18, Color.DarkGray);
    }

    private static void DrawGameStarted()
    {
        const string title = "Game Started";
        const string hint = "Press Escape or controller B to return to the main menu";
        int centreY = Raylib.GetScreenHeight() / 2;
        int titleX = (Raylib.GetScreenWidth() - Raylib.MeasureText(title, 48)) / 2;
        int hintX = (Raylib.GetScreenWidth() - Raylib.MeasureText(hint, 20)) / 2;

        Raylib.DrawText(title, titleX, centreY - 40, 48, Color.DarkBlue);
        Raylib.DrawText(hint, hintX, centreY + 30, 20, Color.DarkGray);
    }

    private void Shutdown()
    {
        if (!_windowInitialized)
        {
            return;
        }

        _assetManager.UnloadAll();
        Raylib.CloseWindow();
        _windowInitialized = false;
    }
}

internal enum ApplicationState
{
    Loading,
    Menu,
    GameStarted
}
