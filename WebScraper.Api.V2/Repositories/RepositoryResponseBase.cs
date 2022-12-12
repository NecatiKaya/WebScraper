using System.Collections;

namespace WebScraper.Api.V2.Repositories;

public class RepositoryResponseBase<T> where T : class
{
    public T[]? Data { get; set; }

    public int Count { get; set; }
}