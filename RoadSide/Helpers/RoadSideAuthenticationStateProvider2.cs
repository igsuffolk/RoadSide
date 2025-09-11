using Microsoft.AspNetCore.Components.Authorization;
using RoadSide.Models;
using System.Security.Claims;

namespace RoadSide.Helpers
{
    public class RoadSideAuthenticationStateProvider2 : AuthenticationStateProvider
    {
        private readonly UserService _userService;

        public RoadSideAuthenticationStateProvider2(UserService userService)
        {
            _userService = userService;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = new ClaimsPrincipal();
            User? user = await _userService.FetchUserFromBrowser();

            if (user is not null)
            {
                var authenticatedUser = await _userService.AuthenticateUserAsync(await _userService.GetToken());

                if (authenticatedUser is not null)
                {
                    var identity = new ClaimsIdentity(new Claim[]
                        {
                            new (ClaimTypes.Email, user.Email),
                            new (ClaimTypes.Hash, user.Password)},
                         "RoadSide");

                    principal = new ClaimsPrincipal(identity);
                }
            }

            return new(principal);
        }

        ///public async Task LoginAsync(string token)
        ///{
        ///    var principal = new ClaimsPrincipal();
        ///    var user = await _userService.AuthenticateUserAsync(token);

        ///    if (user is not null)
        ///    {
        ///        principal = user.ToClaimsPrincipal();
        ///    }

        ///    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        ///}

        public void Logout()
        {
            _userService.ClearBrowserUserData();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
        }
    }
}
