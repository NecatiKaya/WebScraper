using WebScraper.Api.V2.Data.Dto;

namespace WebScraper.Api.V2.Business.Parsing;

internal interface IHtmlParser
{
    ProductPriceInformation? Parse();
}