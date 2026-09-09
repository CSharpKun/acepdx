using System.Net.Http.Headers;

namespace Acepdx.Core.Models;

internal sealed record CachedData<T>(T Value, EntityTagHeaderValue? ETag, DateTimeOffset? LastModified);