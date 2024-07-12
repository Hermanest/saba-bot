using Newtonsoft.Json;

namespace SabaBot.Utils;

internal static class ApiResponseExtensions {
    public static async Task<ApiResponse<byte[]>> ToRawAsync(this ApiResponseBuilder builder) {
        return await ToObject(builder, static x => ParseRaw(x));
    }

    public static async Task<ApiResponse<T>> ToJsonAsync<T>(this ApiResponseBuilder builder) {
        return await ToObject(builder, static x => ParseJson<T>(x));
    }

    public static async Task<ApiResponse<T>> ToObject<T>(this ApiResponseBuilder builder, Func<HttpContent, Task<T?>> parseFunc) {
        try {
            var content = await parseFunc(builder.Message.Content);
            return builder.WithSucceed(content != null).Build(content);
        } catch (Exception ex) {
            return builder.WithSucceed(false).WithReason(ex.ToString()).Build<T>(default);
        }
    }

    public static ApiResponseBuilder ToResponse(this HttpResponseMessage message) {
        return new ApiResponseBuilder(message);
    }

    private static async Task<byte[]?> ParseRaw(HttpContent content) {
        return await content.ReadAsByteArrayAsync();
    }

    private static async Task<T?> ParseJson<T>(HttpContent content) {
        var str = await content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(str);
    }
}