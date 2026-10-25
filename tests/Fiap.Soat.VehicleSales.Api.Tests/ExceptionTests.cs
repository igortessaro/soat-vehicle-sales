using Fiap.Soat.VehicleSales.Api.Shared.Exceptions;

namespace Fiap.Soat.VehicleSales.Api.Tests;

public class ExceptionTests
{
    [Test]
    public void DomainException_WithInnerException_PreservesDetails()
    {
        var inner = new InvalidOperationException("inner");
        var exception = new DomainException("domain", inner);

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo("domain"));
            Assert.That(exception.InnerException, Is.SameAs(inner));
        });
    }

    [Test]
    public void ResourceNotFoundException_WithMessage_StoresMessage()
    {
        var exception = new ResourceNotFoundException("sale not found");

        Assert.That(exception.Message, Is.EqualTo("sale not found"));
    }

    [Test]
    public void ResourceNotFoundException_WithInnerException_PreservesDetails()
    {
        var inner = new InvalidOperationException("inner");
        var exception = new ResourceNotFoundException("sale not found", inner);

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo("sale not found"));
            Assert.That(exception.InnerException, Is.SameAs(inner));
        });
    }
}
