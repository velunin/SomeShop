using Grpc.Core;
using Grpc.Core.Interceptors;
using SomeShop.Common.Domain;
using SomeShop.Common.Exceptions;

namespace SomeShop.Api;

public class ExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (ArgumentException ex)
        {
            ThrowRpcException(context,
                StatusCodes.Status400BadRequest,
                StatusCode.InvalidArgument,
                ex.Message);
        }
        catch (NotFoundException ex)
        {
            ThrowRpcException(context,
                StatusCodes.Status404NotFound,
                StatusCode.NotFound,
                ex.Message);
        }
        catch (DomainException ex)
        {
            ThrowRpcException(context,
                StatusCodes.Status422UnprocessableEntity,
                StatusCode.FailedPrecondition,
                ex.Message);
        }
        catch (Exception ex)
        {
            ThrowRpcException(context,
                StatusCodes.Status500InternalServerError,
                StatusCode.Internal,
                ex.Message);
        }

        return null!;
    }

    private static void ThrowRpcException(ServerCallContext context, int httpStatusCode, 
        StatusCode grpcStatusCode,
        string message)
    {
        context.GetHttpContext().Response.StatusCode = httpStatusCode;
        
        throw new RpcException(new Status(grpcStatusCode, message));
    }
}