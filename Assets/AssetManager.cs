using Raylib_cs;

namespace FrameworkTest.Assets;

/// <summary>Owns native assets. Use on the graphics thread and unload before closing the window.</summary>
public sealed class AssetManager
{
    private readonly Queue<AssetDefinition> _pending;
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Font> _fonts = new(StringComparer.Ordinal);

    public AssetManager(AssetConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.Validate();
        _pending = new Queue<AssetDefinition>(config.Assets);
        TotalCount = _pending.Count;
    }

    public int LoadedCount => _textures.Count + _fonts.Count;

    /// <summary>The original configured count; unloading does not change it.</summary>
    public int TotalCount { get; }

    /// <summary>Loads at most one asset. Returns true when no pending assets remain.
    /// A failed asset stays pending and throws with its key and resolved path.</summary>
    public bool LoadNext()
    {
        if (!_pending.TryPeek(out var asset))
            return true;

        string path = System.IO.Path.IsPathRooted(asset.Path)
            ? asset.Path
            : System.IO.Path.Combine(AppContext.BaseDirectory, asset.Path);
        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Asset file does not exist.", path);

            if (asset.Type == "Texture")
                _textures.Add(asset.Key, LoadTexture(path));
            else
                _fonts.Add(asset.Key, LoadFont(path, asset.Size!.Value));

            _pending.Dequeue();
            return _pending.Count == 0;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load {asset.Type} asset '{asset.Key}' from '{path}': {ex.Message}", ex);
        }
    }

    private static Texture2D LoadTexture(string path)
    {
        Image image = Raylib.LoadImage(path);
        if (!Raylib.IsImageValid(image))
            throw new InvalidDataException("Raylib could not decode the image.");
        try
        {
            Texture2D texture = Raylib.LoadTextureFromImage(image);
            if (!Raylib.IsTextureValid(texture))
                throw new InvalidDataException("Raylib could not create the texture.");
            return texture;
        }
        finally
        {
            Raylib.UnloadImage(image);
        }
    }

    private static Font LoadFont(string path, int size)
    {
        Font font = Raylib.LoadFontEx(path, size, Array.Empty<int>(), 0);
        // Raylib can return its borrowed default font on failure. Never cache or unload it.
        if (font.Texture.Id == Raylib.GetFontDefault().Texture.Id)
            throw new InvalidDataException("Raylib returned the default font instead of the configured font.");
        if (!Raylib.IsFontValid(font))
        {
            Raylib.UnloadFont(font);
            throw new InvalidDataException("Raylib could not load the font.");
        }
        return font;
    }

    /// <summary>Returned handles are borrowed; only this manager should unload them.</summary>
    public Texture2D GetTexture(string key) => _textures.TryGetValue(key, out var texture)
        ? texture : throw new KeyNotFoundException($"Texture '{key}' is not loaded.");

    /// <summary>Returned handles are borrowed; only this manager should unload them.</summary>
    public Font GetFont(string key) => _fonts.TryGetValue(key, out var font)
        ? font : throw new KeyNotFoundException($"Font '{key}' is not loaded.");

    public bool IsLoaded(string key) => _textures.ContainsKey(key) || _fonts.ContainsKey(key);

    /// <summary>Unloads a loaded key. Unknown/pending keys are unchanged; assets are not requeued.</summary>
    public void Unload(string key)
    {
        if (_textures.Remove(key, out var texture))
            Raylib.UnloadTexture(texture);
        if (_fonts.Remove(key, out var font))
            Raylib.UnloadFont(font);
    }

    /// <summary>Unloads owned resources and cancels remaining queued loads. Safe to call again.</summary>
    public void UnloadAll()
    {
        foreach (var texture in _textures.Values)
            Raylib.UnloadTexture(texture);
        foreach (var font in _fonts.Values)
            Raylib.UnloadFont(font);
        _textures.Clear();
        _fonts.Clear();
        _pending.Clear();
    }
}
