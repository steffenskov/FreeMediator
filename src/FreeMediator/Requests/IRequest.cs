namespace FreeMediator.Requests;

public interface IRequest<out TResponse> : IBaseRequest
{
}

public interface IRequest : IBaseRequest
{
}

public interface IBaseRequest
{
}