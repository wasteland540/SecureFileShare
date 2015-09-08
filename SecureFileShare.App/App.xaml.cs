using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using log4net.Config;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Services;
using SecureFileShare.App.Views;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.DataAccessLayer.NDatabase;

namespace SecureFileShare.App
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (App));

        public IUnityContainer Container;
        private IDataAccessLayer _dbContext;

        protected override void OnStartup(StartupEventArgs e)
        {
            XmlConfigurator.Configure();

            _logger.Info("[START]");
            _logger.Info("setup dependency injection container");
            Container = new UnityContainer();

            _logger.Info("setup database");
            _dbContext = new NDatabaseConnector();

            //database registration
            Container.RegisterInstance(typeof (IDataAccessLayer), _dbContext);

            //service registrations
            Container.RegisterType<ICryptographyService, CryptographyService>();

            //registraions utils
            //only one instance from messenger can exists! (recipient problems..)
            _logger.Info("register messenger instance");
            var messenger = new Messenger();
            Container.RegisterInstance(typeof (IMessenger), messenger);

            _logger.Info("show login view");
            var loginView = Container.Resolve<LoginView>();
            loginView.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.Info("close database");
            _dbContext.Close();

            _logger.Info("[Exit]");
            base.OnExit(e);
        }
    }
}