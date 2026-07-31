namespace ArcaneVault_Web.DAL
{
    /// <summary>
    /// Shared API address and HttpClient for the static DAL classes.
    ///
    /// The typed clients get theirs from IHttpClientFactory, but these older
    /// static helpers cannot. Routing them through here means the API address
    /// is configured in one place, and a single HttpClient is reused instead of
    /// allocating one per call (which exhausts sockets under load).
    /// </summary>
    public static class ApiConfig
    {
        private const string DefaultBaseUrl = "https://localhost:7297/";

        private static string _baseUrl = DefaultBaseUrl;
        private static HttpClient? _client;
        private static readonly object _lock = new object();

        /// <summary>
        /// Called once during startup, before any request is served.
        /// </summary>
        public static void Configure(string? baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return;
            }

            lock (_lock)
            {
                _baseUrl = baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";

                // Drop any client built against the previous address.
                _client?.Dispose();
                _client = null;
            }
        }

        public static string BaseUrl => _baseUrl;

        public static HttpClient Client
        {
            get
            {
                if (_client != null)
                {
                    return _client;
                }

                lock (_lock)
                {
                    _client ??= new HttpClient { BaseAddress = new Uri(_baseUrl) };
                }

                return _client;
            }
        }
    }
}
