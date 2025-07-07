using UrlShorter.Responses;

namespace UrlShorter.Responses.LoginResponses
{
    public class LoginResponse: BaseResponse
    {
        public string? TokenString { get; set; }
        public DateTime Expires { get; set; }
    }
}
