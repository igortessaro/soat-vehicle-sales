namespace Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway;

internal static partial class PaymentGatewayValidation
{
    public static string? Validate(string name, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required";
        if (string.IsNullOrWhiteSpace(baseUrl)) return "BaseUrl is required";
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https") ||
            uri.UserInfo.Length > 0)
            return "BaseUrl must be an absolute HTTP or HTTPS URL without credentials";
        return null;
    }
}
