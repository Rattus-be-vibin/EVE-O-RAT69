namespace EveOPreview;

public class ApplicationController : IApplicationController
{
	private readonly IIocContainer _container;

	public ApplicationController(IIocContainer container)
	{
		_container = container;
		_container.RegisterInstance((IApplicationController)this);
	}

	public IApplicationController RegisterView<TView, TImplementation>() where TView : IView where TImplementation : class, TView
	{
		_container.Register<TView, TImplementation>();
		return this;
	}

	public IApplicationController RegisterInstance<TArgument>(TArgument instance)
	{
		_container.RegisterInstance(instance);
		return this;
	}

	public IApplicationController RegisterService<TService, TImplementation>() where TImplementation : class, TService
	{
		_container.Register<TService, TImplementation>();
		return this;
	}

	public void Run<TPresenter>() where TPresenter : class, IPresenter
	{
		if (!_container.IsRegistered<TPresenter>())
		{
			_container.Register<TPresenter>();
		}
		TPresenter val = _container.Resolve<TPresenter>();
		val.Run();
	}

	public void Run<TPresenter, TParameter>(TParameter args) where TPresenter : class, IPresenter<TParameter>
	{
		if (!_container.IsRegistered<TPresenter>())
		{
			_container.Register<TPresenter>();
		}
		TPresenter val = _container.Resolve<TPresenter>();
		val.Run(args);
	}

	public TService Create<TService>() where TService : class
	{
		if (!_container.IsRegistered<TService>())
		{
			_container.Register<TService>();
		}
		return _container.Resolve<TService>();
	}
}
