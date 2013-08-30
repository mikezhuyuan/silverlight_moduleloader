using System.Windows;
using Core;

namespace ModuleA
{
    public class StartModule : Module
    {
        public override void Start(Application application, StartupEventArgs arguments)
        {
            application.RootVisual = new MainPage();
        }
    }
}
