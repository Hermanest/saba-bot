using System.Net;

namespace SabaBot;

public class ApiResponseBuilder(HttpResponseMessage message) {
    public readonly HttpResponseMessage Message = message;
    public bool Succeed;
    public string? Reason;

    public ApiResponse<T> Build<T>(T? value) {
        return new(value, Succeed, Succeed ? null : Reason ?? Message.ReasonPhrase);
    }

    public ApiResponseBuilder WithSucceed(bool succeed) {
        Succeed = Message.IsSuccessStatusCode && succeed;
        return this;
    }

    public ApiResponseBuilder WithReason(string reason) {
        Reason = reason;
        return this;
    }

    public ApiResponseBuilder WithReason(HttpStatusCode code, string reason) {
        if (Message.StatusCode == code) {
            Reason = reason;
        }
        return this;
    }
}