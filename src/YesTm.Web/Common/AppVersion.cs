using System.Reflection;

namespace YesTm.Web.Common;

/// <summary>
/// Version shown in the app: release (VersionPrefix in the .csproj, bumped per release) + build number
/// stamped at every build/publish (yyMMdd.HHmm) + git commit — so the deployed build is always identifiable.
/// </summary>
public static class AppVersion
{
    private static readonly Assembly Asm = typeof(AppVersion).Assembly;

    public static string Release { get; } =
        (Asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0").Split('+')[0];

    public static string Build { get; } = Meta("BuildStamp") ?? "dev";
    public static string BuildTime { get; } = Meta("BuildTime") ?? "";
    public static string? Commit { get; } = Meta("GitCommit");

    /// <summary>"v1.2.0 · build 261024.1532"</summary>
    public static string Short => $"v{Release} · build {Build}";

    /// <summary>Tooltip / log text with build time and commit.</summary>
    public static string Full => $"v{Release} build {Build}" +
        (string.IsNullOrEmpty(BuildTime) ? "" : $", built {BuildTime}") +
        (string.IsNullOrEmpty(Commit) ? "" : $", commit {Commit}");

    private static string? Meta(string key) =>
        Asm.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == key)?.Value;
}
