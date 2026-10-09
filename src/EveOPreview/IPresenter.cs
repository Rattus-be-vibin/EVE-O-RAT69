namespace EveOPreview;

public interface IPresenter<in TArgument>
{
	void Run(TArgument args);
}
public interface IPresenter
{
	void Run();
}
