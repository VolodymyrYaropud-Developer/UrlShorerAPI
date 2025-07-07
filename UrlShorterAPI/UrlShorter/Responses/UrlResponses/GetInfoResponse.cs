using UrlShorter.Models;
using UrlShorter.Responses;

namespace UrlShorter.Responses.UrlResponses
{
    public class GetInfoResponse: BaseResponse
    {
        public URLModel URLModel { get; set; }
    }
}
