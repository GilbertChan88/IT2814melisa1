using System.Net.Http.Json;
using System.Text.Json;

namespace ArcaneVault_Web.DAL
{
    /// <summary>
    /// Outcome of a write call. Several API endpoints reject requests with a
    /// plain-text explanation (out of stock, not your offer, and so on) that
    /// the page needs to show the user verbatim.
    /// </summary>
    public class ApiResult
    {
        public bool Success { get; init; }

        public string? ErrorMessage { get; init; }

        public static ApiResult Ok() => new ApiResult { Success = true };

        public static ApiResult Fail(string message) =>
            new ApiResult { Success = false, ErrorMessage = message };

        /// <summary>
        /// Builds a result from a response, extracting the most useful message
        /// available. Handles plain strings, ProblemDetails and ASP.NET
        /// validation payloads.
        /// </summary>
        public static async Task<ApiResult> FromResponseAsync(
            HttpResponseMessage response,
            string fallbackMessage = "The request could not be completed.")
        {
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }

            var body = string.Empty;

            try
            {
                body = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                // Fall through to the generic message.
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return Fail(fallbackMessage);
            }

            var trimmed = body.Trim();

            // Plain-text message (the common case for our BadRequest calls).
            if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
            {
                return Fail(trimmed.Trim('"'));
            }

            try
            {
                using var document = JsonDocument.Parse(trimmed);
                var root = document.RootElement;

                // Validation errors: surface the first one.
                if (root.TryGetProperty("errors", out var errors) &&
                    errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var field in errors.EnumerateObject())
                    {
                        if (field.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var message in field.Value.EnumerateArray())
                            {
                                var text = message.GetString();
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    return Fail(text);
                                }
                            }
                        }
                    }
                }

                if (root.TryGetProperty("message", out var messageProperty))
                {
                    var text = messageProperty.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return Fail(text);
                    }
                }

                if (root.TryGetProperty("title", out var titleProperty))
                {
                    var text = titleProperty.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return Fail(text);
                    }
                }
            }
            catch
            {
                // Unparseable JSON; fall back below.
            }

            return Fail(fallbackMessage);
        }
    }

    /// <summary>A result that also carries a deserialised payload.</summary>
    public class ApiResult<T> : ApiResult
    {
        public T? Data { get; init; }

        public static ApiResult<T> Ok(T data) =>
            new ApiResult<T> { Success = true, Data = data };

        public static new ApiResult<T> Fail(string message) =>
            new ApiResult<T> { Success = false, ErrorMessage = message };

        public static async Task<ApiResult<T>> FromResponseAsync(
            HttpResponseMessage response,
            string fallbackMessage = "The request could not be completed.")
        {
            if (!response.IsSuccessStatusCode)
            {
                var failure = await ApiResult.FromResponseAsync(response, fallbackMessage);
                return Fail(failure.ErrorMessage ?? fallbackMessage);
            }

            try
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                return data is null ? Fail(fallbackMessage) : Ok(data);
            }
            catch
            {
                return Fail(fallbackMessage);
            }
        }
    }
}
