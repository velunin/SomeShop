using CqrsVibe.Commands;
using CqrsVibe.Queries;
using Grpc.Core;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.Proto;
using SomeShop.Ordering.App.Cart;
using SomeShop.Ordering.App.Cart.RemoveProduct;
using SomeShop.Ordering.Cart.V1;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.App.Api.Cart.V1;

public class GrpcService : Service.ServiceBase
{
    private readonly IQueryService _queryService;
    private readonly ICommandProcessor _commandProcessor;

    public GrpcService(IQueryService queryService, ICommandProcessor commandProcessor)
    {
        _queryService = queryService;
        _commandProcessor = commandProcessor;
    }

    public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
    {
        var cart = await _queryService.QueryAsync(new GetCart(
            new CartId(Guid.Parse(request.CartId))), 
            context.CancellationToken);

        var response = new GetResponse
        {
            Cart = new Ordering.Cart.V1.Cart
            {
                Id = cart.Id.Value.ToString("D"),
                TotalSum = new Money(cart.TotalSumAmount, cart.TotalSumCurrency),
                TotalProductsPositions = cart.TotalProductsPositions,
            }
        };
      
        foreach (var cartItemModel in cart.Items)
        {
            response.Cart.Items.Add(new Ordering.Cart.V1.Cart.Types.CartItem
            {   
                Id = cartItemModel.Id.Value.ToString("D"),
                ProductId = cartItemModel.ProductId.Value.ToString("D"),
                Quantity = cartItemModel.Quantity,
                Price = new Money(cartItemModel.PriceAmount, cartItemModel.PriceCurrency)
            });
        }
       
        return response;
    }

    public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
    {
        var cartId = await _commandProcessor.ProcessAsync(new CreateCart(), context.CancellationToken);

        return new CreateResponse
        {
            CartId = cartId.Value.ToString("D")
        };
    }

    public override async Task<AddProductResponse> AddProduct(AddProductRequest request, ServerCallContext context)
    {
        await _commandProcessor.ProcessAsync(
            new AddProduct(
                new CartId(Guid.Parse(request.CartId)),
                new ProductId(Guid.Parse(request.ProductId)),
                new Quantity(request.Quantity)),
            context.CancellationToken);

        return new AddProductResponse();
    }

    public override async Task<ChangeProductQuantityResponse> ChangeProductQuantity(ChangeProductQuantityRequest request, ServerCallContext context)
    {
        await _commandProcessor.ProcessAsync(
            new ChangeQuantity(
                new CartId(Guid.Parse(request.CartId)),
                new ProductId(Guid.Parse(request.ProductId)),
                new Quantity(request.NewQuantity)), 
            context.CancellationToken);

        return new ChangeProductQuantityResponse();
    }

    public override async Task<RemoveProductResponse> RemoveProduct(RemoveProductRequest request, ServerCallContext context)
    {
        await _commandProcessor.ProcessAsync(
            new RemoveProduct(
                new CartId(Guid.Parse(request.CartId)),
                new ProductId(Guid.Parse(request.ProductId))), 
            context.CancellationToken);

        return new RemoveProductResponse();
    }

    public override async Task<ClearResponse> Clear(ClearRequest request, ServerCallContext context)
    {
        await _commandProcessor.ProcessAsync(
            new Clear(
                new CartId(Guid.Parse(request.CartId))), 
            context.CancellationToken);

        return new ClearResponse();
    }
}
