using Fiap.Soat.VehicleSales.Application.Adapters.Presenters;
using Fiap.Soat.VehicleSales.Application.Mappers;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using System.Net;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class PresentationTests
{
    [Test]
    public void SalePresenter_MapsSaleToDetail()
    {
        var sale = ApplicationTestData.CreateSale(SaleStatus.Paid);

        var detail = SalePresenter.ToDto(sale);

        Assert.Multiple(() =>
        {
            Assert.That(detail.Id, Is.EqualTo(sale.Id));
            Assert.That(detail.VehicleId, Is.EqualTo(sale.VehicleId));
            Assert.That(detail.BuyerDocument, Is.EqualTo(ApplicationTestData.ValidDocument));
            Assert.That(detail.Status, Is.EqualTo(SaleStatus.Paid));
            Assert.That(detail.PaymentCode, Is.EqualTo(sale.PaymentCode));
            Assert.That(detail.PaidAt, Is.EqualTo(sale.PaidAt));
            Assert.That(detail.CanceledAt, Is.Null);
            Assert.That(detail.CreatedAt, Is.EqualTo(sale.CreatedAt));
            Assert.That(detail.UpdatedAt, Is.EqualTo(sale.UpdatedAt));
        });
    }

    [Test]
    public void SalePresenter_MapsSuccessfulResponse()
    {
        var sale = ApplicationTestData.CreateSale();

        var response = SalePresenter.ToDto(ResponseFactory.Ok(sale));

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data.PaymentCode, Is.EqualTo(sale.PaymentCode));
        });
    }

    [Test]
    public void SalePresenter_MapsFailedResponse()
    {
        var response = SalePresenter.ToDto(ResponseFactory.Fail<Sale>("Sale not found", HttpStatusCode.NotFound));

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Sale not found"));
        });
    }

    [Test]
    public void SalePresenter_MapsSaleList()
    {
        IReadOnlyList<Sale> sales = [ApplicationTestData.CreateSale(), ApplicationTestData.CreateSale(SaleStatus.Canceled)];

        var result = SalePresenter.ToDto(sales);

        Assert.That(result.Select(x => x.Status), Is.EqualTo(new[] { SaleStatus.PendingPayment, SaleStatus.Canceled }));
    }

    [Test]
    public void SalePresenter_MapsSuccessfulSaleListResponse()
    {
        IReadOnlyList<Sale> sales = [ApplicationTestData.CreateSale()];

        var response = SalePresenter.ToDto(ResponseFactory.Ok(sales));

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void SalePresenter_MapsFailedSaleListResponse()
    {
        var response = SalePresenter.ToDto(
            ResponseFactory.Fail<IReadOnlyList<Sale>>("Invalid filter", HttpStatusCode.BadRequest));

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Invalid filter"));
        });
    }

    [Test]
    public void PaymentGatewayPresenter_MapsGatewayToDetail()
    {
        var gateway = ApplicationTestData.CreatePaymentGateway();

        var detail = PaymentGatewayPresenter.ToDto(gateway);

        Assert.Multiple(() =>
        {
            Assert.That(detail.Id, Is.EqualTo(gateway.Id));
            Assert.That(detail.Name, Is.EqualTo(gateway.Name));
            Assert.That(detail.BaseUrl, Is.EqualTo(gateway.BaseUrl));
            Assert.That(detail.CreatedAt, Is.EqualTo(gateway.CreatedAt));
            Assert.That(detail.UpdatedAt, Is.EqualTo(gateway.UpdatedAt));
        });
    }

    [Test]
    public void SaleMapper_MapsCommandToDomainEntity()
    {
        var vehicleId = Guid.NewGuid();

        var sale = new CreateSaleCommand(vehicleId, ApplicationTestData.ValidDocument).ToDomainEntity();

        Assert.Multiple(() =>
        {
            Assert.That(sale.VehicleId, Is.EqualTo(vehicleId));
            Assert.That(sale.BuyerDocument.Value, Is.EqualTo(ApplicationTestData.ValidDocument));
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.PendingPayment));
        });
    }

    [Test]
    public void ResponseMapper_MapsSuccessfulResponseAndPreservesStatusCode()
    {
        var response = ResponseFactory.Ok("sale", HttpStatusCode.Created);

        var mapped = ResponseMapper.Map(response, value => value.Length);

        Assert.Multiple(() =>
        {
            Assert.That(mapped.IsSuccess, Is.True);
            Assert.That(mapped.Data, Is.EqualTo(4));
            Assert.That(mapped.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        });
    }

    [Test]
    public void ResponseMapper_MapsFailedResponse()
    {
        var response = ResponseFactory.Fail<string>("Invalid sale", HttpStatusCode.UnprocessableEntity);

        var mapped = ResponseMapper.Map(response, value => value.Length);

        Assert.Multiple(() =>
        {
            Assert.That(mapped.IsSuccess, Is.False);
            Assert.That(mapped.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
            Assert.That(mapped.Reasons[0].Message, Is.EqualTo("Invalid sale"));
        });
    }

    [Test]
    public void ResponseMapper_MapsPaginatedResponse()
    {
        var page = new Paginate<string>(new[] { "one", "two" }, 10, 2, 1, 5);

        var mapped = ResponseMapper.Map(ResponseFactory.Ok(page), value => value.Length);

        Assert.Multiple(() =>
        {
            Assert.That(mapped.IsSuccess, Is.True);
            Assert.That(mapped.Data.Items, Is.EqualTo(new[] { 3, 3 }));
            Assert.That(mapped.Data.TotalCount, Is.EqualTo(10));
            Assert.That(mapped.Data.PageSize, Is.EqualTo(2));
            Assert.That(mapped.Data.CurrentPage, Is.EqualTo(1));
            Assert.That(mapped.Data.TotalPages, Is.EqualTo(5));
        });
    }

    [Test]
    public void ResponseMapper_MapsFailedPaginatedResponse()
    {
        var response = ResponseFactory.Fail<Paginate<string>>("Invalid page", HttpStatusCode.BadRequest);

        var mapped = ResponseMapper.Map(response, value => value.Length);

        Assert.Multiple(() =>
        {
            Assert.That(mapped.IsSuccess, Is.False);
            Assert.That(mapped.Reasons[0].Message, Is.EqualTo("Invalid page"));
        });
    }
}
