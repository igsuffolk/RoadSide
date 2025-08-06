using Microsoft.AspNetCore.Components.Authorization;
using RoadSide.Helpers;
using System.Security.Claims;

namespace RoadSide.Models
{
    public class User
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";

        public bool isAuthenticated { get; set; } = false;  

    }
}
