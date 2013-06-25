using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class AuthHandler : DelegatingHandler
    {
        public AuthHandler(string appAccessKey, string appAccessKeySecret, string appToken)
            : base(new HttpClientHandler())
        {
            _appAccessKey = appAccessKey;
            _appAccessKeySecret = appAccessKeySecret;
            _appToken = appToken;
        }

        private readonly string _appAccessKey;
        private readonly string _appAccessKeySecret;
        private readonly string _appToken;


        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Date = DateTimeOffset.UtcNow;
            request.Headers.Add("Instanext-AccessKey", _appAccessKey);
            request.Headers.Add("Instanext-Signature", ComputeSignature(request));

            if (_appToken != null) request.Headers.Add("Instanext-AuthToken", _appToken);

            return base.SendAsync(request, cancellationToken);
        }

        private string ComputeSignature(HttpRequestMessage request)
        {
            var sb = new StringBuilder();

            sb.AppendLine(_appAccessKey);
            sb.AppendLine(request.Method.ToString());
            sb.AppendLine(request.RequestUri.AbsolutePath);
            sb.AppendLine(request.Headers.Date.GetValueOrDefault().ToString("r"));

            using (var hmac = new HMACSHA256(Convert.FromBase64String(_appAccessKeySecret)))
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
        }
    }
}
