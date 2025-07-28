// ReSharper disable once CheckNamespace

namespace FreeMediator;

public interface IMediatorConfiguration
{
	/// <summary>
	///     Adds a open behavior to the pipeline without specifying the generic type arguments.
	///     The behavior must implement either IPipelineBehavior&lt;TNotification&gt; or IPipelineBehavior&lt;TRequest,
	///     TResponse&gt;.
	///     Generic constraints can still be used to limit the types that can be used.
	///     The behavior must take the same number of generic type arguments as the IPipelineBehavior&lt;TNotification&gt; or
	///     IPipelineBehavior&lt;TRequest, TResponse&gt; interface implemented.
	/// </summary>
	IMediatorConfiguration AddOpenBehavior(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

	/// <summary>
	///     Adds a collection of open behaviors to the pipeline without specifying the generic type arguments.
	///     The behavior must implement either IPipelineBehavior&lt;TNotification&gt; or IPipelineBehavior&lt;TRequest,
	///     TResponse&gt;.
	///     Generic constraints can still be used to limit the types that can be used.
	///     The behavior must take the same number of generic type arguments as the IPipelineBehavior&lt;TNotification&gt; or
	///     IPipelineBehavior&lt;TRequest, TResponse&gt; interface implemented.
	/// </summary>
	IMediatorConfiguration AddOpenBehaviors(IEnumerable<Type> openBehaviorTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

	/// <summary>
	///     Adds a closed behavior to the pipeline, meaning it cannot have generic type arguments.
	///     The behavior must implement either IPipelineBehavior&lt;TNotification&gt; or IPipelineBehavior&lt;TRequest,
	///     TResponse&gt;.
	/// </summary>
	IMediatorConfiguration AddBehavior<TBehavior>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
		where TBehavior : IBasePipelineBehavior;

	/// <summary>
	///     Adds a closed behavior to the pipeline, meaning it cannot have generic type arguments.
	///     The behavior must implement either IPipelineBehavior&lt;TNotification&gt; or IPipelineBehavior&lt;TRequest,
	///     TResponse&gt;.
	/// </summary>
	IMediatorConfiguration AddBehavior(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

	/// <summary>
	///     Registers all request- and notification handlers from the assembly containing the specified type.
	/// </summary>
	IMediatorConfiguration RegisterServicesFromAssemblyContaining<T>();

	/// <summary>
	///     Registers all request- and notification handlers from the assembly containing the specified type.
	/// </summary>
	IMediatorConfiguration RegisterServicesFromAssemblyContaining(Type markerType);

	/// <summary>
	///     Registers all request- and notification handlers from the specified assemblies.
	/// </summary>
	IMediatorConfiguration RegisterServicesFromAssemblies(params IEnumerable<Assembly> assemblies);

	/// <summary>
	///     Registers all request- and notification handlers from the specified assembly.
	/// </summary>
	IMediatorConfiguration RegisterServicesFromAssembly(Assembly assembly);

	/// <summary>
	///     Manually register  IRequestHandlers or INotificationHandlers.
	///     Useful for registering closed versions of generic handlers that cannot be auto-registered (you'll notice them as
	///     they throw UnmappableHandlerException)
	/// </summary>
	IMediatorConfiguration RegisterServices(params IEnumerable<Type> types);

	/// <summary>
	///     Manually register an IRequestHandler or INotificationHandler.
	///     Useful for registering closed versions of generic handlers that cannot be auto-registered (you'll notice them as
	///     they throw UnmappableHandlerException)
	/// </summary>
	IMediatorConfiguration RegisterService<T>() where T : IBaseHandler;

	/// <summary>
	///     Explicitly ignore IRequestHandlers or INotificationHandlers from auto registration.
	///     Useful for dealing with partially closed generic types, like e.g. &lt;IRequestHandler&lt;MyGenericType&lt;T&gt;,
	///     bool&gt;&gt; that cannot be auto-mapped.
	///     Instead ignore them with this method, and do manual registration using <see cref="RegisterService&lt;T&gt;" /> or
	///     <see cref="RegisterServices" />
	/// </summary>
	/// <param name="types">OPEN type description of the handler type to ignore</param>
	IMediatorConfiguration IgnoreServices(params IEnumerable<Type> types);

	/// <summary>
	///     Explicitly ignore an IRequestHandler or INotificationHandler for auto registration.
	///     Useful for dealing with partially closed generic types, like e.g. &lt;IRequestHandler&lt;MyGenericType&lt;T&gt;,
	///     bool&gt;&gt; that cannot be auto-mapped.
	///     Instead ignore them with this method, and do manual registration using <see cref="RegisterService&lt;T&gt;" /> or
	///     <see cref="RegisterServices" />
	/// </summary>
	/// <param name="predicate">predicate used to evaluate types for ignorance</param>
	IMediatorConfiguration IgnoreServices(Predicate<Type> predicate);
}