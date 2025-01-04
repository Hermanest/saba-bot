namespace SabaBot.Database;

internal class Resources {
    private const string MANAGED_RESOURCES_PATH = "Resources/Managed";
    private const string STATIC_RESOURCES_PATH = "Resources/Static";

    public static Stream ReadStaticResource(string path) {
        return File.OpenRead($"{STATIC_RESOURCES_PATH}/{path}");
    }

    public static Stream ReadManagedResource(string path) {
        EnsureManagedDirectoryExists(path);
        return File.OpenRead($"{MANAGED_RESOURCES_PATH}/{path}");
    }

    public static Stream ReadManagedResource(ulong guild, string path) {
        return ReadManagedResource($"{guild}/{path}");
    }

    public static Task<string?> WriteManagedResourceAsync(ulong guild, string path, Stream stream) {
        return WriteManagedResourceAsync($"{guild}/{path}", stream);
    }

    public static async Task<string?> WriteManagedResourceAsync(string path, Stream stream) {
        try {
            EnsureManagedDirectoryExists(path);
            using var wStream = File.Open($"{MANAGED_RESOURCES_PATH}/{path}", FileMode.OpenOrCreate, FileAccess.Write);
            await stream.CopyToAsync(wStream);

            return null;
        } catch (Exception ex) {
            return $"An error occurred: {ex.Message}";
        }
    }

    private static void EnsureManagedDirectoryExists(string file) {
        var path = $"{MANAGED_RESOURCES_PATH}/{Path.GetDirectoryName(file)}";
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }
}