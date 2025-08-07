using Microsoft.AspNetCore.Identity;
using SharedProject1.Models.DTO;

namespace Api.Helpers
{
    public static class IdentityErrorHelper
    {
        public static string ReadErrors(IdentityResult identityResult)
        {
            string result = "";
            foreach(IdentityError error in identityResult.Errors)
                result += error.Description + Environment.NewLine;

            return result;  
        }
    }
}
