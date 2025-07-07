using UrlShorter.Models;
using UrlShorter.Responses;

namespace UrlShorter.Responses.UrlResponses
{
    public class NewUrlResponse: BaseResponse
    {
        public URLModel UrlModel { get; set; }
    }
}
