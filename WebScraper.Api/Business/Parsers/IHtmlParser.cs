using WebScraper.Api.Dto;

namespace WebScraper.Api.Business.Parsers
{
    internal interface IHtmlParser
    {
        ProductPriceInformation? Parse();
    }
}
