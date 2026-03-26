using Microsoft.UI.Xaml;
using Microsoft.Extensions.Configuration;
using MovieApp.Core.Repositories;
using MovieApp.Infrastructure;
using MovieApp.Core.Services;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using MovieApp.Ui.Views;

using System;

namespace MovieApp.Ui;

/// <summary>
/// WinUI application entry point responsible for configuration loading,
/// shared dependency construction, and main-window startup.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    private ICurrentUserService? _currentUserService;

    public static ICurrentUserService? CurrentUserService { get; private set; }
    
    public static IPriceWatcherRepository? PriceWatcherRepository { get; private set; }
    public static IEventRepository? EventRepository { get; private set; }
    public static ITriviaRepository? TriviaRepository { get; private set; }
    public static ITriviaRewardRepository? TriviaRewardRepository { get; private set; }
    public static IAmbassadorRepository? AmbassadorRepository { get; private set; }
    public static IReferralValidator? ReferralValidator { get; private set; }
    public static MainWindow? CurrentMainWindow { get; private set; }
    public static int CurrentUserId { get; private set; }
    public static IMovieRepository? MovieRepository { get; private set; }
    public static IUserSlotMachineStateRepository? SlotMachineStateRepository { get; private set; }
    public static IUserMovieDiscountRepository? UserMovieDiscountRepository { get; private set; }
    public static IScreeningRepository? ScreeningRepository { get; private set; }
    public static SlotMachineService? SlotMachineService { get; private set; }
    public static SlotMachineResultService? SlotMachineResultService { get; private set; }
    public static ReelAnimationService? ReelAnimationService { get; private set; }
    public static IConfigurationRoot? Configuration { get; private set; }
    public static IMarathonRepository? MarathonRepository { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainViewModel viewModel;
        EventRepository = UnavailableEventRepository.Instance;
        TriviaRepository = null;
        TriviaRewardRepository = null;
        CurrentUserId = 0;
        PriceWatcherRepository = null;

        try
        {
            var configuration = BuildConfiguration();
            Configuration = configuration;
            var databaseOptions = new DatabaseOptions
            {
                ConnectionString = configuration["Database:ConnectionString"]
                    ?? throw new InvalidOperationException("Missing configuration value 'Database:ConnectionString'."),
            };
            var bootstrapUserOptions = new BootstrapUserOptions
            {
                AuthProvider = configuration["Authentication:DummyUser:AuthProvider"]
                    ?? throw new InvalidOperationException("Missing configuration value 'Authentication:DummyUser:AuthProvider'."),
                AuthSubject = configuration["Authentication:DummyUser:AuthSubject"]
                    ?? throw new InvalidOperationException("Missing configuration value 'Authentication:DummyUser:AuthSubject'."),
            };

            var userRepository = new SqlUserRepository(databaseOptions);
            var eventRepository = new SqlEventRepository(databaseOptions);
            var triviaRepository = new SqlTriviaRepository(databaseOptions);
            var triviaRewardRepository = new SqlTriviaRewardRepository(databaseOptions);
            var ambassadorRepository = new SqlAmbassadorRepository(databaseOptions);
            var movieRepository = new SqlMovieRepository(databaseOptions);
            var slotMachineStateRepository = new SqlUserSlotMachineStateRepository(databaseOptions);
            var userMovieDiscountRepository = new SqlUserRewardRepository(databaseOptions);
            var screeningRepository = new SqlScreeningRepository(databaseOptions);
            var marathonRepository = new SqlMarathonRepository(databaseOptions);

            _currentUserService = new CurrentUserService(userRepository, bootstrapUserOptions);
            await _currentUserService.InitializeAsync();
            CurrentUserService = _currentUserService;

            var slotMachineService = new SlotMachineService(
                slotMachineStateRepository,
                movieRepository,
                eventRepository,
                userMovieDiscountRepository);

            var slotMachineResultService = new SlotMachineResultService(userMovieDiscountRepository);
            var reelAnimationService = new ReelAnimationService();

            EventRepository = eventRepository;
            TriviaRepository = triviaRepository;
            TriviaRewardRepository = triviaRewardRepository;
            AmbassadorRepository = ambassadorRepository;
            ReferralValidator = new ReferralValidator(ambassadorRepository);
            CurrentUserId = _currentUserService.CurrentUser.Id;
            MovieRepository = movieRepository;
            SlotMachineStateRepository = slotMachineStateRepository;
            UserMovieDiscountRepository = userMovieDiscountRepository;
            ScreeningRepository = screeningRepository;
            SlotMachineService = slotMachineService;
            SlotMachineResultService = slotMachineResultService;
            ReelAnimationService = reelAnimationService;
            MarathonRepository = marathonRepository;
            string localDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            PriceWatcherRepository = new LocalPriceWatcherRepository(localDataFolder);

            viewModel = new MainViewModel(_currentUserService.CurrentUser);
        }
        catch (Exception exception)
        {
            viewModel = MainViewModel.CreateStartupError(BuildStartupErrorMessage(exception));
        }

        CurrentMainWindow = new MainWindow(viewModel, EventRepository!);
        _window = CurrentMainWindow;
        _window.Activate();
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
    }

    private static string BuildStartupErrorMessage(Exception exception)
    {
        return "Check the database setup scripts and the appsettings.json connection string."
            + Environment.NewLine
            + Environment.NewLine
            + exception.Message;
    }
}
