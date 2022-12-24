﻿namespace WebScraper.Api.V2.Data.Dto;

public class ServerResponse<TResult>
{
    public IEnumerable<TResult>? Data { get; set; }

    public bool IsSuccess { get; set; } = true;

    public int TotalRowCount { get; set; }
}