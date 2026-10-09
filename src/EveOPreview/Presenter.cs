namespace EveOPreview;

public abstract class Presenter<TView> : IPresenter where TView : IView
{
	protected TView View { get; private set; }

	protected IApplicationController Controller { get; private set; }

	protected Presenter(IApplicationController controller, TView view)
	{
		Controller = controller;
		View = view;
	}

	public void Run()
	{
		View.Show();
	}
}
public abstract class Presenter<TView, TArgument> : IPresenter<TArgument> where TView : IView
{
	protected TView View { get; private set; }

	protected IApplicationController Controller { get; private set; }

	protected Presenter(IApplicationController controller, TView view)
	{
		Controller = controller;
		View = view;
	}

	public abstract void Run(TArgument args);
}
