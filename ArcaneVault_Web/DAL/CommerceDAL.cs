using ArcaneVault_Web.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArcaneVault_Web.DAL
{
    public class CartApiClient
    {
        private readonly HttpClient _client;

        public CartApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<CartSummary> GetCart(string username)
        {
            try
            {
                var cart = await _client.GetFromJsonAsync<CartSummary>(
                    $"api/Cart/User/{Uri.EscapeDataString(username)}");

                return cart ?? new CartSummary();
            }
            catch
            {
                return new CartSummary();
            }
        }

        /// <summary>
        /// Cart badge count. Failures are swallowed because this is called on
        /// every page render from the layout.
        /// </summary>
        public async Task<int> GetCartCount(string username)
        {
            try
            {
                var json = await _client.GetFromJsonAsync<Dictionary<string, int>>(
                    $"api/Cart/User/{Uri.EscapeDataString(username)}/count");

                return json != null && json.TryGetValue("itemCount", out var count)
                    ? count
                    : 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<ApiResult> AddToCart(string username, int catalogItemId, int quantity = 1)
        {
            var response = await _client.PostAsJsonAsync("api/Cart", new
            {
                userName = username,
                catalogItemId,
                quantity
            });

            return await ApiResult.FromResponseAsync(
                response, "Could not add this item to your cart.");
        }

        /// <summary>Sets an absolute quantity. Zero removes the line.</summary>
        public async Task<ApiResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var response = await _client.PutAsJsonAsync(
                $"api/Cart/{cartItemId}", new { quantity });

            return await ApiResult.FromResponseAsync(
                response, "Could not update the quantity.");
        }

        public async Task<ApiResult> RemoveFromCart(int cartItemId)
        {
            var response = await _client.DeleteAsync($"api/Cart/{cartItemId}");

            return await ApiResult.FromResponseAsync(
                response, "Could not remove this item.");
        }

        public async Task<ApiResult> ClearCart(string username)
        {
            var response = await _client.DeleteAsync(
                $"api/Cart/User/{Uri.EscapeDataString(username)}");

            return await ApiResult.FromResponseAsync(response, "Could not clear the cart.");
        }
    }

    /// <summary>Result of a successful checkout.</summary>
    public class CheckoutResult
    {
        public int OrderId { get; set; }

        public decimal Total { get; set; }
    }

    public class OrderApiClient
    {
        private readonly HttpClient _client;

        public OrderApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<Order>> GetUserOrders(string username)
        {
            try
            {
                var orders = await _client.GetFromJsonAsync<List<Order>>(
                    $"api/Orders/User/{Uri.EscapeDataString(username)}");

                return orders ?? new List<Order>();
            }
            catch
            {
                return new List<Order>();
            }
        }

        /// <summary>All orders, for the admin order management screen.</summary>
        public async Task<List<Order>> GetAllOrders(int? status = null)
        {
            try
            {
                var url = status.HasValue
                    ? $"api/Orders?status={status.Value}"
                    : "api/Orders";

                var orders = await _client.GetFromJsonAsync<List<Order>>(url);

                return orders ?? new List<Order>();
            }
            catch
            {
                return new List<Order>();
            }
        }

        public async Task<Order?> GetOrder(int orderId)
        {
            try
            {
                var response = await _client.GetAsync($"api/Orders/{orderId}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<Order>();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Turns the user's cart into an order. The API validates stock and
        /// clears the cart transactionally.
        /// </summary>
        public async Task<ApiResult<CheckoutResult>> Checkout(
            string username, CheckoutForm form)
        {
            var response = await _client.PostAsJsonAsync("api/Orders/checkout", new
            {
                userName = username,
                shippingName = form.ShippingName,
                shippingAddress = form.ShippingAddress,
                shippingCity = form.ShippingCity,
                shippingPostalCode = form.ShippingPostalCode,
                shippingCountry = form.ShippingCountry
            });

            if (!response.IsSuccessStatusCode)
            {
                var failure = await ApiResult.FromResponseAsync(
                    response, "Checkout could not be completed.");

                return ApiResult<CheckoutResult>.Fail(
                    failure.ErrorMessage ?? "Checkout could not be completed.");
            }

            try
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                return ApiResult<CheckoutResult>.Ok(new CheckoutResult
                {
                    OrderId = json.TryGetProperty("orderId", out var id)
                        ? id.GetInt32() : 0,
                    Total = json.TryGetProperty("total", out var total)
                        ? total.GetDecimal() : 0m
                });
            }
            catch
            {
                // The order was placed; we just could not read the confirmation.
                return ApiResult<CheckoutResult>.Ok(new CheckoutResult());
            }
        }

        public async Task<ApiResult> UpdateStatus(int orderId, int status)
        {
            var response = await _client.PutAsJsonAsync(
                $"api/Orders/{orderId}/status", new { status });

            return await ApiResult.FromResponseAsync(
                response, "Could not update the order status.");
        }
    }
}
