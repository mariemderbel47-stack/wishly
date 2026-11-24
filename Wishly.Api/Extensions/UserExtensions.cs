using System.Security.Claims;

namespace Wishly.Api;

public static class UserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal u) => Guid.Parse(u.FindFirst("uid")!.Value);
    public static string GetEmail(this ClaimsPrincipal u) => u.FindFirst("email")!.Value;
    public static string GetNom(this ClaimsPrincipal u) => u.FindFirst("nom")!.Value;
}