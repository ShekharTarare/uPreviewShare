using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Scoping;
using uPreviewShare.Models;
using uPreviewShare.Models.DTOs;

namespace uPreviewShare.Services;

public partial class BrandingService : IBrandingService
{
    private const string CacheKey = "ups:branding";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);
    private const int MaxLogoSizeBytes = 512_000;
    private const int MaxLogoWidth = 1000;
    private const int MaxLogoHeight = 500;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".png", ".svg" };

    private readonly IScopeProvider _scopeProvider;
    private readonly IMemoryCache _cache;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<BrandingService> _logger;

    public BrandingService(IScopeProvider scopeProvider, IMemoryCache cache, IHostEnvironment hostEnvironment, ILogger<BrandingService> logger)
    {
        _scopeProvider = scopeProvider;
        _cache = cache;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task<BrandingConfigDto> GetBrandingAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out BrandingConfigDto? cached) && cached is not null)
            return cached;

        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var results = await database.FetchAsync<uPreviewShareBrandingConfig>("SELECT * FROM uPreviewShare_Branding");
        var brandingConfig = results.FirstOrDefault();
        scope.Complete();

        var dto = MapToDto(brandingConfig);
        _cache.Set(CacheKey, dto, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl });
        return dto;
    }

    public async Task SaveBrandingAsync(string? primaryColor, string? backgroundColor, string? textColor, CancellationToken ct = default)
    {
        if (primaryColor is not null) ValidateHexColor(primaryColor, nameof(primaryColor));
        if (backgroundColor is not null) ValidateHexColor(backgroundColor, nameof(backgroundColor));
        if (textColor is not null) ValidateHexColor(textColor, nameof(textColor));

        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var results = await database.FetchAsync<uPreviewShareBrandingConfig>("SELECT * FROM uPreviewShare_Branding");
        var existing = results.FirstOrDefault();

        if (existing is not null)
        {
            existing.PrimaryColor = primaryColor;
            existing.BackgroundColor = backgroundColor;
            existing.TextColor = textColor;
            existing.UpdatedAt = DateTime.UtcNow;
            await database.UpdateAsync(existing);
        }
        else
        {
            var newConfig = new uPreviewShareBrandingConfig
            {
                Id = Guid.NewGuid(),
                PrimaryColor = primaryColor,
                BackgroundColor = backgroundColor,
                TextColor = textColor,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.Empty
            };
            await database.InsertAsync(newConfig);
        }

        scope.Complete();
        _cache.Remove(CacheKey);
        _logger.LogInformation("Branding colors saved. Primary: {PrimaryColor}, Background: {BackgroundColor}, Text: {TextColor}",
            primaryColor ?? "(default)", backgroundColor ?? "(default)", textColor ?? "(default)");
    }

    public async Task<string> SaveLogoAsync(Stream fileStream, string fileName, long fileSize, CancellationToken ct = default)
    {
        if (fileSize > MaxLogoSizeBytes)
            throw new ArgumentException($"Logo file size ({fileSize} bytes) exceeds the maximum allowed size of {MaxLogoSizeBytes} bytes (500 KB).");

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            throw new ArgumentException($"Logo file format '{extension}' is not supported. Only PNG and SVG files are accepted.");

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;
        ValidateLogoDimensions(memoryStream, extension);
        memoryStream.Position = 0;

        var logoDirectory = GetLogoDirectory();
        Directory.CreateDirectory(logoDirectory);
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(logoDirectory, uniqueFileName);
        var relativePath = $"upreviewshare/logo/{uniqueFileName}";

        await using (var fileOutput = new FileStream(absolutePath, FileMode.Create, FileAccess.Write))
            await memoryStream.CopyToAsync(fileOutput, ct);

        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var results = await database.FetchAsync<uPreviewShareBrandingConfig>("SELECT * FROM uPreviewShare_Branding");
        var existing = results.FirstOrDefault();

        if (existing is not null)
        {
            DeleteLogoFile(existing.LogoPath);
            existing.LogoPath = relativePath;
            existing.UpdatedAt = DateTime.UtcNow;
            await database.UpdateAsync(existing);
        }
        else
        {
            var newConfig = new uPreviewShareBrandingConfig
            {
                Id = Guid.NewGuid(),
                LogoPath = relativePath,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.Empty
            };
            await database.InsertAsync(newConfig);
        }

        scope.Complete();
        _cache.Remove(CacheKey);
        _logger.LogInformation("Logo saved: {LogoPath}", relativePath);
        return relativePath;
    }

    public async Task ResetBrandingAsync(CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var results = await database.FetchAsync<uPreviewShareBrandingConfig>("SELECT * FROM uPreviewShare_Branding");
        var existing = results.FirstOrDefault();
        if (existing is not null)
        {
            DeleteLogoFile(existing.LogoPath);
            await database.DeleteAsync(existing);
        }
        scope.Complete();
        _cache.Remove(CacheKey);
        _logger.LogInformation("Branding reset to defaults");
    }

    #region Private Helpers

    private static BrandingConfigDto MapToDto(uPreviewShareBrandingConfig? config)
    {
        if (config is null)
            return new BrandingConfigDto { LogoPath = null, PrimaryColor = null, BackgroundColor = null, TextColor = null, UpdatedAt = null, IsCustom = false };

        return new BrandingConfigDto
        {
            LogoPath = config.LogoPath,
            PrimaryColor = config.PrimaryColor,
            BackgroundColor = config.BackgroundColor,
            TextColor = config.TextColor,
            UpdatedAt = config.UpdatedAt,
            IsCustom = true
        };
    }

    private static void ValidateHexColor(string color, string parameterName)
    {
        if (!HexColorRegex().IsMatch(color))
            throw new ArgumentException($"Color value '{color}' is not a valid hex color. Expected format: #RRGGBB (e.g., #8B5CF6).", parameterName);
    }

    private void ValidateLogoDimensions(MemoryStream stream, string extension)
    {
        if (extension.Equals(".svg", StringComparison.OrdinalIgnoreCase)) ValidateSvgDimensions(stream);
        else ValidatePngDimensions(stream);
    }

    private void ValidatePngDimensions(MemoryStream stream)
    {
        if (stream.Length < 24) throw new ArgumentException("Invalid PNG file: file is too small to contain valid header data.");
        var buffer = stream.GetBuffer();
        byte[] pngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
        for (var i = 0; i < 8; i++)
            if (buffer[i] != pngSignature[i]) throw new ArgumentException("Invalid PNG file: file does not have a valid PNG signature.");
        var width = (buffer[16] << 24) | (buffer[17] << 16) | (buffer[18] << 8) | buffer[19];
        var height = (buffer[20] << 24) | (buffer[21] << 16) | (buffer[22] << 8) | buffer[23];
        if (width > MaxLogoWidth || height > MaxLogoHeight)
            throw new ArgumentException($"Logo dimensions ({width}x{height}px) exceed the maximum allowed dimensions of {MaxLogoWidth}x{MaxLogoHeight}px.");
    }

    private static void ValidateSvgDimensions(MemoryStream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, leaveOpen: true);
        var content = reader.ReadToEnd();
        var widthMatch = SvgWidthRegex().Match(content);
        var heightMatch = SvgHeightRegex().Match(content);
        if (widthMatch.Success && heightMatch.Success)
        {
            if (double.TryParse(widthMatch.Groups[1].Value, out var width) && double.TryParse(heightMatch.Groups[1].Value, out var height))
                if (width > MaxLogoWidth || height > MaxLogoHeight)
                    throw new ArgumentException($"SVG logo dimensions ({width}x{height}px) exceed the maximum allowed dimensions of {MaxLogoWidth}x{MaxLogoHeight}px.");
        }
    }

    private string GetLogoDirectory() => Path.Combine(_hostEnvironment.ContentRootPath, "App_Data", "uPreviewShare", "logos");

    private void DeleteLogoFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        try
        {
            var filename = Path.GetFileName(relativePath);
            var absolutePath = Path.Combine(GetLogoDirectory(), filename);
            if (File.Exists(absolutePath)) { File.Delete(absolutePath); _logger.LogInformation("Deleted logo file: {LogoPath}", relativePath); }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete logo file: {LogoPath}", relativePath); }
    }

    [GeneratedRegex(@"^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColorRegex();

    [GeneratedRegex(@"<svg[^>]*\swidth=[""'](\d+(?:\.\d+)?)", RegexOptions.IgnoreCase)]
    private static partial Regex SvgWidthRegex();

    [GeneratedRegex(@"<svg[^>]*\sheight=[""'](\d+(?:\.\d+)?)", RegexOptions.IgnoreCase)]
    private static partial Regex SvgHeightRegex();

    #endregion
}
