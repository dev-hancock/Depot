namespace Depot.Storage;

using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public interface IStorage
{
    Task<string> SaveAsync(Guid id, string location, string ext, Stream content, CancellationToken token);

    Task<Stream> OpenAsync(string location, CancellationToken token);

    Task DeleteAsync(string location, CancellationToken token);
}

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string Root { get; set; }
}

public class

public class FileSystemStorage : IStorage
{
    private readonly StorageOptions _options;

    public FileSystemStorage(IOptions<StorageOptions> options)
    {
        _options = options.Value;

        Directory.CreateDirectory(Path.GetFullPath(_options.Root));
    }

    public async Task<Stream> OpenAsync(string location, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string location, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<string> SaveAsync(Guid id, string location, string ext, Stream content, CancellationToken token)
    {
        var filepath = Path.Combine(_options.Root, location, $"{id}{ext}");

        var directory = Path.GetDirectoryName(filepath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None);

        await content.CopyToAsync(stream, token);

        return filepath;
    }
}

public interface IStorageHasher
{
    Task<string> Hash(Stream stream, CancellationToken token);
}

public class StorageHasher : IStorageHasher
{
    public async Task<string> Hash(Stream stream, CancellationToken token)
    {
        stream.Position = 0;

        using var sha = SHA256.Create();

        var digest = await sha.ComputeHashAsync(stream, token);

        stream.Position = 0;

        return Convert.ToHexString(digest);
    }
}

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName));

        services.AddSingleton<IStorage, FileSystemStorage>();
        services.AddSingleton<IStorageHasher, StorageHasher>();

        return services;
    }
}