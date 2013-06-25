using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.ServiceModel.Security.Tokens;
using Microsoft.IdentityModel.Tokens.JWT;

namespace ConsoleTest
{
    internal static class Program
    {
        private static void Main()
        {
            const string appKey = "-- replace with your App Key --";
            const string appSecret = "-- replace with your App Secret --";

            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            //Pre-Authenticated client supplies appKey and Secret
            var client = new HttpClient(new AuthHandler(appKey, appSecret, null)) {BaseAddress = new Uri("https://api.instanexdev.com")};
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Get the Tokens supplied by authentication
            var result = client.PostAsJsonAsync("api/account/token", new {Username = username, Password = password}).Result;
            var token = result.Content.ReadAsAsync<TokenHandler>().Result;

            //Parse JSON token to get user info
            var handler = new JWTSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = "https://api.instanexdev.com",
                AllowedAudience = "urn:instanexserviceapps",
                SigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(appSecret))
            };

            var principal = handler.ValidateToken(token.Token, parameters);
            principal.Identities.First().AddClaim(new Claim("_apiToken", token.ApiToken));

            //get the logged in userInfo from Json Web token
            Console.WriteLine("Your userId is " + principal.FindFirst("http://schemas.instanext.services.api/identity/claims/userid").Value);
            Console.WriteLine("Your Username is " + principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value);
            Console.WriteLine("Your First Name is " + principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").Value);
            Console.WriteLine("Your Last Name is " + principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").Value);

            Console.Write("Press <Enter> to continue...");
            Console.ReadLine();

            //Create Authenticated Httpclient that supplies ApiToken with call
            var verifiedclient = new HttpClient(new AuthHandler(appKey, appSecret, token.ApiToken)) {BaseAddress = new Uri("https://api.instanexdev.com")};
            verifiedclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            ///////////////////////////////Authenticated API Calls code//////////////////////////////////////


            //GetCompanies
            var cpnytask = verifiedclient.GetAsync("api/companies");
            var response = cpnytask.Result.Content.ReadAsAsync<Company[]>().Result;

            Console.WriteLine(response.Count());

            foreach (var cpny in response)
                Console.WriteLine(cpny.Name);

            Console.Write("Press <Enter> to continue...");
            Console.ReadLine();
        }
    }
}
