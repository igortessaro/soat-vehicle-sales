using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Domain.Tests;

public class DocumentTests
{
    [TestCase("52998224725")]
    [TestCase("529.982.247-25")]
    [TestCase("12345678909")]
    [TestCase("11144477735")]
    public void IsValid_ShouldReturnTrueForValidCpf(string value)
    {
        Document document = value;

        Assert.That(document.IsValid(), Is.True);
    }

    [TestCase("11222333000181")]
    [TestCase("11.222.333/0001-81")]
    [TestCase("11444777000161")]
    [TestCase("45723174000110")]
    public void IsValid_ShouldReturnTrueForValidCnpj(string value)
    {
        Document document = value;

        Assert.That(document.IsValid(), Is.True);
    }

    [TestCase("52998224724")]
    [TestCase("52998224715")]
    [TestCase("11111111111")]
    [TestCase("11222333000182")]
    [TestCase("11222333000171")]
    [TestCase("00000000000000")]
    [TestCase("123")]
    [TestCase("abcdefghijk")]
    public void IsValid_ShouldReturnFalseForInvalidDocuments(string value)
    {
        Document document = value;

        Assert.That(document.IsValid(), Is.False);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void IsValid_ShouldReturnFalseForEmptyValues(string? value)
    {
        Document document = value!;

        Assert.That(document.IsValid(), Is.False);
    }

    [Test]
    public void ImplicitConversions_ShouldPreserveValue()
    {
        Document document = "529.982.247-25";
        string value = document;

        Assert.That(value, Is.EqualTo("529.982.247-25"));
    }

    [Test]
    public void Equality_ShouldCompareByValue()
    {
        Document first = "52998224725";
        Document second = "52998224725";
        Document third = "11144477735";

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(second));
            Assert.That(first, Is.Not.EqualTo(third));
        });
    }
}
