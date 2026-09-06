namespace FrameworkTest.Assets;

public sealed class AssetConfig
{
    public List<AssetDefinition> Assets { get; init; } = [];

    internal void Validate()
    {
        if (Assets is null)
            throw new InvalidDataException("Assets must be an array.");

        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var asset in Assets)
        {
            if (asset is null || string.IsNullOrWhiteSpace(asset.Key))
                throw new InvalidDataException("Every asset must have a Key.");
            if (!keys.Add(asset.Key))
                throw new InvalidDataException($"Duplicate asset key '{asset.Key}'.");
            if (asset.Type is not ("Texture" or "Font"))
                throw new InvalidDataException($"Asset '{asset.Key}' requires Type 'Texture' or 'Font'.");
            if (string.IsNullOrWhiteSpace(asset.Path))
                throw new InvalidDataException($"Asset '{asset.Key}' requires a Path.");
            if (asset.Type == "Font" && asset.Size is not > 0)
                throw new InvalidDataException($"Font '{asset.Key}' requires a positive Size.");
        }
    }
}

public sealed record AssetDefinition
{
    public string Key { get; init; } = "";
    public string Type { get; init; } = "";
    public string Path { get; init; } = "";
    public int? Size { get; init; }
}
