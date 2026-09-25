using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace ChatHub.Web.Services;

public class UserTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTokenHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            var accessToken =
                await httpContext.GetTokenAsync(
                    "access_token");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        accessToken);
            }
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}