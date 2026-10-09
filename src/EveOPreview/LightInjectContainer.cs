using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using LightInject;

namespace EveOPreview;

internal sealed class LightInjectContainer : IIocContainer
{
	private readonly ServiceContainer _container;

	public LightInjectContainer()
	{
		_container = new ServiceContainer(ContainerOptions.Default);
	}

	public bool IsRegistered<TService>()
	{
		return _container.CanGetInstance(typeof(TService), "");
	}

	public void Register(Type serviceType, Assembly container)
	{
		if (!serviceType.IsInterface)
		{
			_container.Register(serviceType, new PerContainerLifetime());
			return;
		}
		if (serviceType.IsInterface && serviceType.IsGenericType)
		{
			_container.RegisterAssembly(container, (Type st, Type it) => st.IsConstructedGenericType && st.GetGenericTypeDefinition() == serviceType);
			return;
		}
		foreach (TypeInfo definedType in container.DefinedTypes)
		{
			if (definedType.IsClass && !definedType.IsAbstract && serviceType.IsAssignableFrom(definedType))
			{
				_container.Register(serviceType, definedType, new PerContainerLifetime());
			}
		}
	}

	public void Register<TService>()
	{
		Register(typeof(TService), typeof(TService).Assembly);
	}

	public void Register<TService, TImplementation>() where TImplementation : TService
	{
		_container.Register<TService, TImplementation>();
	}

	public void Register<TService>(Expression<Func<TService>> factory)
	{
		_container.Register((IServiceFactory f) => factory);
	}

	public void Register<TService, TArgument>(Expression<Func<TArgument, TService>> factory)
	{
		_container.Register((IServiceFactory f) => factory);
	}

	public void RegisterInstance<TService>(TService instance)
	{
		_container.RegisterInstance(instance);
	}

	public TService Resolve<TService>()
	{
		return _container.GetInstance<TService>();
	}

	public IEnumerable<TService> ResolveAll<TService>()
	{
		return _container.GetAllInstances<TService>();
	}

	public object Resolve(Type serviceType)
	{
		return _container.GetInstance(serviceType);
	}

	public IEnumerable<object> ResolveAll(Type serviceType)
	{
		return _container.GetAllInstances(serviceType);
	}
}
