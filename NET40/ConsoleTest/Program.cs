using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

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
            var json = JsonConvert.SerializeObject(new {Username = username, Password = password});
            var result = client.PostAsync("api/account/token", new StringContent(json, Encoding.UTF8, "application/json")).Result;
            var sTokens = result.Content.ReadAsStringAsync().Result;

            //Parse the Token to get logged in user info
            var tokens = new TokenHandler();
            JsonConvert.PopulateObject(sTokens, tokens);

            var token = tokens.Token;
            var parts = token.Split('.');
            var jwtBody = parts[1];

            var body = Encoding.UTF8.GetString(FromBase64UrlString(jwtBody));
            dynamic moreInfo = JsonConvert.DeserializeObject(body);

            ////get the logged in userInfo from Json Web token
            Console.WriteLine("Your User ID is " + moreInfo["http://schemas.instanext.services.api/identity/claims/userid"]);
            Console.WriteLine("Your App name is " + moreInfo["http://schemas.instanext.services.api/identity/claims/appname"]);
            Console.WriteLine("Your License Type is " + moreInfo["http://schemas.instanext.services.api/identity/claims/userlicensetype"]);

            Console.WriteLine("Your Username is " + moreInfo["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);
            Console.WriteLine("Your First Name is " + moreInfo["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"]);
            Console.WriteLine("Your Last Name is " + moreInfo["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"]);
            Console.WriteLine("Your Email is " + moreInfo["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"]);
            Console.WriteLine("Your Role is " + moreInfo["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);

            client.Dispose();

            //Create Authenticated Httpclient with ApiToken 
            var verifiedclient = new HttpClient(new AuthHandler(appKey, appSecret, tokens.ApiToken)) {BaseAddress = new Uri("https://api.instanexdev.com")};
            verifiedclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            ///////////////////////////////Authenticated API Calls code//////////////////////////////////////


            //GetCompanies
            var newtask = verifiedclient.GetAsync("api/companies");
            var response = newtask.Result.Content.ReadAsStringAsync().Result;

            var companies = new List<Company>();
            JsonConvert.PopulateObject(response, companies);

            foreach (var cpny in companies)
                Console.WriteLine(cpny.Name);

            Console.Write("Press <Enter> to continue...");
            Console.ReadLine();
        }

        private static byte[] FromBase64UrlString(string input)
        {
            var output = input;

            output = output.Replace('-', '+');
            output = output.Replace('_', '/');

            switch (output.Length % 4)
            {
                case 0:
                    break;
                case 2:
                    output += "==";
                    break;
                case 3:
                    output += "=";
                    break;
                default:
                    throw new Exception("Illegal base64url string");
            }

            return Convert.FromBase64String(output);
        }
    }
}
