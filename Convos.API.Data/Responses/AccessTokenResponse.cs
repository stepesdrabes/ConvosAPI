using Convos.API.Data.Entities;

namespace Convos.API.Data.Responses;

public struct AccessTokenResponse
{
    public string AccessToken { get; set; }
    public User User { get; set; }
    public DateTime ExpireDate { get; set; }
}