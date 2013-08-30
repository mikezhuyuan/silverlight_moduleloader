using System.Windows;

namespace Core
{
    public abstract class Module
    {
        public abstract void Start(Application application, StartupEventArgs arguments);
    }
}
