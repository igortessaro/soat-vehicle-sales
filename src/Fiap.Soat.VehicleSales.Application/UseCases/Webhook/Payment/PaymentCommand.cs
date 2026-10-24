namespace Fiap.Soat.VehicleSales.Application.UseCases.Webhook.Payment;

public record PaymentCommand(string PaymentCode, PaymentStatus Status);

public enum PaymentStatus
{
    Paid,
    Canceled
}
