using System;
using System.Threading;
using System.Windows.Forms;
using EveOPreview.Configuration;
using EveOPreview.Presenters;
using EveOPreview.Services;
using EveOPreview.View;
using MediatR;

namespace EveOPreview;

internal static class Program
{
	private static string MUTEX_NAME = "EVE-O Preview Single Instance Mutex";

	private static Mutex _singleInstanceMutex;

	[STAThread]
	private static void Main()
	{
		_singleInstanceMutex = GetInstanceToken();
		if (_singleInstanceMutex != null)
		{
			ExceptionHandler exceptionHandler = new ExceptionHandler();
			exceptionHandler.SetupExceptionHandlers();
			IApplicationController applicationController = InitializeApplicationController();
			InitializeWinForms();
			applicationController.Run<MainFormPresenter>();
		}
	}

	private static Mutex GetInstanceToken()
	{
		try
		{
			Mutex.OpenExisting(MUTEX_NAME);
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
		catch (Exception)
		{
			Mutex mutex = new Mutex(initiallyOwned: true, MUTEX_NAME, out var createdNew);
			return createdNew ? mutex : null;
		}
	}

	private static void InitializeWinForms()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
	}

	private static IApplicationController InitializeApplicationController()
	{
		IIocContainer container = new LightInjectContainer();
		container.Register<IWindowManager>();
		container.Register<IProcessMonitor>();
		container.Register<IMediator, MediatR.Mediator>();
		container.RegisterInstance<ServiceFactory>((Type t) => container.Resolve(t));
		container.Register(typeof(INotificationHandler<>), typeof(Program).Assembly);
		container.Register(typeof(IRequestHandler<>), typeof(Program).Assembly);
		container.Register(typeof(IRequestHandler<, >), typeof(Program).Assembly);
		container.Register<IConfigurationStorage>();
		container.Register<IAppConfig>();
		container.Register<IThumbnailConfiguration>();
		container.Register<IThumbnailManager>();
		container.Register<IThumbnailViewFactory>();
		container.Register<IThumbnailDescription>();
		IApplicationController applicationController = new ApplicationController(container);
		applicationController.RegisterView<StaticThumbnailView, StaticThumbnailView>();
		applicationController.RegisterView<LiveThumbnailView, LiveThumbnailView>();
		applicationController.RegisterView<IMainFormView, MainForm>();
		applicationController.RegisterInstance(new ApplicationContext());
		return applicationController;
	}
}
