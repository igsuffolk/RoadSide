using RoadSide.Models;
using RoadSide.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoadSide.Helpers
{
    public class UserService
    {       
        private readonly IStorageService _storageService;
        public UserService(IStorageService storageService)
        {
           
            _storageService = storageService;
        }

        //public async Task<User?> AuthenticateUserAsync(string token)
        //{
        //    var claimPrincipal = CreateClaimsPrincipalFromToken(token);
          
        //    var user = FromClaimsPrincipal(claimPrincipal);

        //    await PersistUserToBrowser(token);

        //    return user;
        //}

        //private ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var identity = new ClaimsIdentity();

        //    if (tokenHandler.CanReadToken(token))
        //    {
        //        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
        //        identity = new(jwtSecurityToken.Claims, "RoadSide");
        //    }

        //    return new(identity);
        //}

        //public async Task<User?> FetchUserFromBrowser()
        //{
        //    var claimsPrincipal = CreateClaimsPrincipalFromToken(await _storageService.GetStringAsync("Token"));

        //    if (claimsPrincipal.Identity.IsAuthenticated)
        //    {
        //        var user = FromClaimsPrincipal(claimsPrincipal);

        //        user.isAuthenticated = claimsPrincipal.Identity.IsAuthenticated;
        //        return user;
        //    }
        //    return null;
        //}

        //public ClaimsPrincipal ToClaimsPrincipal() => new(new ClaimsIdentity(new Claim[]
        //{
        //    new (ClaimTypes.Email, Email),
        //    new (ClaimTypes.Hash, Password)},
        // "RoadSide"));

        //private static User FromClaimsPrincipal(ClaimsPrincipal principal)
        //{
        //    User user = new()
        //    {
        //        Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "",
        //        Password = principal.FindFirst(ClaimTypes.Hash)?.Value ?? ""
        //    };
           
        //    return user;
        //}

        public async Task PersistUserToBrowser(string token) => await _storageService.SetStringAsync(token,"Token");

        public async Task<string> GetToken() => await _storageService.GetStringAsync("Token");

        public async Task ClearBrowserUserData() => await _storageService.SetStringAsync("","Token");
    }
}
