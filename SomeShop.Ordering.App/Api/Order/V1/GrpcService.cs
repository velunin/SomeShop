using CqrsVibe.Commands;
using CqrsVibe.Queries;
using Grpc.Core;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.Proto;
using SomeShop.Ordering.App.Order;
using SomeShop.Ordering.Order.V1;

namespace SomeShop.Ordering.App.Api.Order.V1;

public class GrpcService : Service.ServiceBase
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryService _queryService;

    public GrpcService(ICommandProcessor commandProcessor, IQueryService queryService)
    {
        _commandProcessor = commandProcessor;
        _queryService = queryService;
    }

    public override async Task<CreateOrderResponse> Create(CreateOrderRequest request, ServerCallContext context)
    {
        var orderId = await _commandProcessor.ProcessAsync(new CreateOrder(new CartId(Guid.Parse(request.CartId))),
            context.CancellationToken);

        return new CreateOrderResponse
        {
            OrderId = orderId.Value.ToString("D")
        };
    }

    public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
    {
        var order = await _queryService.QueryAsync(new GetOrder(
                new OrderId(Guid.Parse(request.Id))),
            context.CancellationToken);

        var response = new GetResponse
        {
            Order = new Ordering.Order.V1.Order
            {
                Id = order.Id.Value.ToString("D"),
                TotalSum = new Money(order.TotalSumAmount, order.TotalSumCurrency),
            }
        };

        foreach (var orderItemModel in order.Items)
        {
            response.Order.Items.Add(new Ordering.Order.V1.Order.Types.OrderItem
            {
                Id = orderItemModel.Id.Value.ToString("D"),
                ProductId = orderItemModel.ProductId.Value.ToString("D"),
                Quantity = orderItemModel.Quantity,
                Price = new Money(orderItemModel.PriceAmount, orderItemModel.PriceCurrency)
            });
        }

        return response;
    }

    public override async Task<CheckoutOrderResponse> Checkout(CheckoutOrderRequest request, ServerCallContext context)
    {
        await _commandProcessor.ProcessAsync(new CheckoutOrder(new OrderId(Guid.Parse(request.Id))),
            context.CancellationToken);
        return new CheckoutOrderResponse();
    }
}