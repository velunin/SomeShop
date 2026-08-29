using CqrsVibe.Commands;
using CqrsVibe.Commands.Pipeline;
using SomeShop.Common.Domain.Ids;
using SomeShop.StockManagement.Domain;

namespace SomeShop.StockManagement.App.ProductsReservation;

public class ReserveProductsHandler : ICommandHandler<ReserveProducts>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IStock _stock;

    public ReserveProductsHandler(IReservationRepository reservationRepository, IStock stock)
    {
        _reservationRepository = reservationRepository;
        _stock = stock;
    }

    public async Task HandleAsync(ICommandHandlingContext<ReserveProducts> context,
        CancellationToken cancellationToken)
    {
        // A reservation is keyed by the order, so a redelivered OrderCreated does not reserve twice.
        if (await _reservationRepository.Exists(context.Command.OrderId, cancellationToken))
        {
            return;
        }

        var reservation = await Domain.Reservation.Create(
            context.Command.OrderId,
            context.Command.Items,
            _stock,
            cancellationToken);

        await _reservationRepository.Add(reservation, cancellationToken);
    }
}

public class ReserveProducts : ICommand
{
    public ReserveProducts(OrderId orderId, IReadOnlyCollection<RequestedItem> items)
    {
        OrderId = orderId;
        Items = items;
    }

    public OrderId OrderId { get; }

    public IReadOnlyCollection<RequestedItem> Items { get; }
}
