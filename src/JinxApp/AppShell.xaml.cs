using JinxApp.Views;

namespace JinxApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(ScoreBoardPage), typeof(ScoreBoardPage));
        Routing.RegisterRoute(nameof(GamePage), typeof(Views.GamePage));
        Routing.RegisterRoute(nameof(PlayPage), typeof(PlayPage));
        Routing.RegisterRoute(nameof(RulesPage), typeof(RulesPage));
    }
}
