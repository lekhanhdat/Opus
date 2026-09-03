using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGoogle.API
{
    public class AutoRefreshServiceCredential : ServiceCredential
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private string _accessToken;

        private long _expiryUtc;

        private readonly Func<Task<(string AccessToken, DateTimeOffset ExpiryUtc)>> _refreshFunc;

        public AutoRefreshServiceCredential(Func<Task<(string AccessToken, DateTimeOffset ExpiryUtc)>> refreshFunc)
            : base(new Initializer("https://oauth2.googleapis.com/token"))
        {
            _refreshFunc = refreshFunc;
        }

        public override async Task<bool> RequestAccessTokenAsync(CancellationToken taskCancellationToken)
        {
            if (!NeedRefresh())
            {
                return true;
            }

            await _lock.WaitAsync(taskCancellationToken);
            try
            {
                if (!NeedRefresh())
                {
                    return true;
                }

                (string, DateTimeOffset) tuple = await _refreshFunc();
                _accessToken = tuple.Item1;
                _expiryUtc = tuple.Item2.UtcTicks;
                base.Token = new TokenResponse
                {
                    AccessToken = tuple.Item1,
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresInSeconds = (long)(tuple.Item2 - DateTimeOffset.UtcNow).TotalSeconds
                };
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        private bool NeedRefresh()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                return DateTimeOffset.UtcNow.AddMinutes(5.0).UtcTicks >= _expiryUtc;
            }

            return true;
        }
    }
}
