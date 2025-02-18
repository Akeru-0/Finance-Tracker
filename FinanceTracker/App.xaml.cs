using System.Windows;

namespace FinanceTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize database and perform any required migrations
            using (var context = new Data.FinanceDbContext())
            {
                context.Database.EnsureCreated();
            }
        }
    }
}