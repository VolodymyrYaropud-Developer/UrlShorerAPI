using UrlShorter.Models;
using UrlShorter.Responses;

namespace UrlShorter.Responses.UrlResponses
{
    public class AllUrlsResponse: BaseResponse
    {
        public List<URLModel> URLModels { get; set; }
    }
}
