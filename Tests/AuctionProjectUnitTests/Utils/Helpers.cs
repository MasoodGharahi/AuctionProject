using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuctionProjectUnitTests.Utils
{
    public class Helpers
    {
        public static ClaimsPrincipal GetClaimsPrincipal()
        {
            var claims = new List<Claim> { new Claim("username","Test")};
            var identity = new ClaimsIdentity(claims, "testing","Test","Test");
  
            return new ClaimsPrincipal(identity);
        }
    }
}
