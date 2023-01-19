using System.Security.Claims;


namespace TestHelper;
public class FakeClaimsPrincipal : ClaimsPrincipal
{
    public override IEnumerable<Claim> Claims { get; }
    public FakeClaimsPrincipal(string id, string role)
    {
        Claims = new List<Claim>(){
                new Claim(ClaimTypes.PrimarySid, id),
                new Claim(ClaimTypes.Role, role)
            }.AsEnumerable();
    }
}