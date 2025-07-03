namespace SabaBot.Database;

public class Resources(ApplicationConfig config) {
    private const string STATIC_RESOURCES_PATH = "Resources/Static";
    private readonly string _managedResourcesPath = config.ResourcesPath;

    public static Stream ReadStaticResource(string path) {
        return File.OpenRead($"{STATIC_RESOURCES_PATH}/{path}");
    }

    public Stream ReadManagedResource(string path) {
        EnsureManagedDirectoryExists(path);
        return File.OpenRead($"{_managedResourcesPath}/{path}");
    }

    public Stream ReadManagedResource(ulong guild, string path) {
        return ReadManagedResource($"{guild}/{path}");
    }

    public Task<string?> WriteManagedResourceAsync(ulong guild, string path, Stream stream) {
        return WriteManagedResourceAsync($"{guild}/{path}", stream);
    }

    public async Task<string?> WriteManagedResourceAsync(string path, Stream stream) {
        try {
            EnsureManagedDirectoryExists(path);
            using var wStream = File.Open($"{_managedResourcesPath}/{path}", FileMode.OpenOrCreate, FileAccess.Write);
            await stream.CopyToAsync(wStream);

            return null;
        } catch (Exception ex) {
            return $"An error occurred: {ex.Message}";
        }
    }

    private void EnsureManagedDirectoryExists(string file) {
        var path = $"{_managedResourcesPath}/{Path.GetDirectoryName(file)}";
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }
}