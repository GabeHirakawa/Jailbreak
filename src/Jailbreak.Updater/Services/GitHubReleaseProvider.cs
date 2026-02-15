using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jailbreak.Updater.Services;

public record GitHubRelease(
  [property: JsonPropertyName("tag_name")] string TagName,
  [property: JsonPropertyName("prerelease")] bool Prerelease,
  [property: JsonPropertyName("assets")] GitHubAsset[] Assets,
  [property: JsonPropertyName("published_at")] DateTime PublishedAt
);

public record GitHubAsset(
  [property: JsonPropertyName("name")] string Name,
  [property: JsonPropertyName("browser_download_url")] string DownloadUrl
);

public class GitHubReleaseProvider {
  private static readonly HttpClient Http = new() {
    DefaultRequestHeaders = {
      { "User-Agent", "Jailbreak-Updater" },
      { "Accept", "application/vnd.github+json" }
    }
  };

  private readonly string _repository;

  public GitHubReleaseProvider(string repository) {
    _repository = repository;
  }

  public async Task<GitHubRelease?> GetLatestRelease(bool includePreRelease) {
    var url = $"https://api.github.com/repos/{_repository}/releases";
    var response = await Http.GetAsync(url);

    if (!response.IsSuccessStatusCode) return null;

    var json = await response.Content.ReadAsStringAsync();
    var releases = JsonSerializer.Deserialize<GitHubRelease[]>(json);

    if (releases is null || releases.Length == 0) return null;

    return includePreRelease
      ? releases[0]
      : releases.FirstOrDefault(r => !r.Prerelease);
  }

  public async Task<Stream?> DownloadAsset(GitHubRelease release, string assetName) {
    var asset = release.Assets.FirstOrDefault(a => a.Name == assetName);
    if (asset is null) return null;

    var response = await Http.GetAsync(asset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
    if (!response.IsSuccessStatusCode) return null;

    return await response.Content.ReadAsStreamAsync();
  }

  public static string ParseVersion(string tagName) {
    return tagName.TrimStart('v');
  }
}
