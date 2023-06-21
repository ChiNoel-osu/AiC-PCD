namespace AiC_PCD;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState activationState) =>
		new Window(new AppShell())
		{
			Width = 600,
			Height = 600,
		};
}