using Minio;
using Minio.DataModel.Args;

namespace OnlineCinema.Backend.Services;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IConfiguration config, ILogger<MinioStorageService> logger)
    {
        _bucket = config["Minio:Bucket"] ?? "cinema";
        var endpoint = config["Minio:Endpoint"] ?? "localhost:9000";
        var accessKey = config["Minio:AccessKey"] ?? "minioadmin";
        var secretKey = config["Minio:SecretKey"] ?? "minioadmin";

        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false)
            .Build();

        _logger = logger;
        EnsureBucket().GetAwaiter().GetResult();
    }

    private async Task EnsureBucket()
    {
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));
            _logger.LogInformation("Создал бакет {Bucket}", _bucket);
        }
    }

    public async Task<string> SaveAsync(Stream stream, string objectKey, string contentType, CancellationToken ct = default)
    {
        // Стримим напрямую в MinIO, в память файл не грузим
        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await _client.PutObjectAsync(args, ct);
        return $"{_bucket}/{objectKey}";
    }

    public async Task<(Stream Stream, string ContentType, long Length)> GetAsync(string objectKey, CancellationToken ct = default)
    {
        var ms = new MemoryStream();
        var statArgs = new StatObjectArgs().WithBucket(_bucket).WithObject(objectKey);
        var stat = await _client.StatObjectAsync(statArgs, ct);

        var getArgs = new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithCallbackStream(s => s.CopyTo(ms));

        await _client.GetObjectAsync(getArgs, ct);
        ms.Position = 0;
        return (ms, stat.ContentType, stat.Size);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey), ct);
    }

    public string GetPresignedUrl(string objectKey, int expirySeconds = 3600)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithExpiry(expirySeconds);

        return _client.PresignedGetObjectAsync(args).GetAwaiter().GetResult();
    }

    public bool TryParseObjectKey(string? urlOrKey, out string objectKey)
    {
        objectKey = "";
        if (string.IsNullOrEmpty(urlOrKey)) return false;

        // Пытаемся достать bucket/key из presigned url или "bucket/key" формы
        if (urlOrKey.Contains("://"))
        {
            try
            {
                var uri = new Uri(urlOrKey);
                var path = uri.AbsolutePath.TrimStart('/');
                var parts = path.Split('/', 2);
                if (parts.Length == 2 && parts[0] == _bucket)
                {
                    objectKey = Uri.UnescapeDataString(parts[1]);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // форма bucket/key
        if (urlOrKey.StartsWith($"{_bucket}/"))
        {
            objectKey = urlOrKey.Substring(_bucket.Length + 1);
            return true;
        }

        // голый ключ
        objectKey = urlOrKey;
        return true;
    }
}
