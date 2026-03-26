using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using MovieApp.Ui.Views;

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
    public static IEventRepository? EventRepository { get; private set; }
    public static ITriviaRepository? TriviaRepository { get; private set; }
    public static ITriviaRewardRepository? TriviaRewardRepository { get; private set; }
    public static IAmbassadorRepository? AmbassadorRepository { get; private set; }
    public static IReferralValidator? ReferralValidator { get; private set; }
    public static MainWindow? CurrentMainWindow { get; private set; }
    public static IConfigurationRoot? Configuration { get; private set; }
    public static IMarathonRepository? MarathonRepository { get; private set; }
    public static IFavoriteEventService? FavoriteEventService { get; private set; }
    public static INotificationService? NotificationService { get; private set; }
    public static int CurrentUserId { get; private set; }
    public static IMovieRepository? MovieRepository { get; private set; }
    public static IUserSlotMachineStateRepository? SlotMachineStateRepository { get; private set; }
    public static IUserMovieDiscountRepository? UserMovieDiscountRepository { get; private set; }
    public static IScreeningRepository? ScreeningRepository { get; private set; }
    public static SlotMachineService? SlotMachineService { get; private set; }
    public static SlotMachineResultService? SlotMachineResultService { get; private set; }
    public static ReelAnimationService? ReelAnimationService { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainViewModel viewModel;
        ResetRuntimeServices();

        try
        {
            var configuration = BuildConfiguration();
            Configuration = configuration;

            var connectionString = configuration["Database:ConnectionString"];
            var databaseOptions = new DatabaseOptions
            {
                ConnectionString = connectionString
                    ?? throw new InvalidOperationException("Missing configuration value 'Database:ConnectionString'."),
            };
            var bootstrapUserOptions = new BootstrapUserOptions
            {
                AuthProvider = configuration["Authentication:BootstrapUser:AuthProvider"]
                    ?? throw new InvalidOperationException("Missing configuration value 'Authentication:BootstrapUser:AuthProvider'."),
                AuthSubject = configuration["Authentication:BootstrapUser:AuthSubject"]
                    ?? throw new InvalidOperationException("Missing configuration value 'Authentication:BootstrapUser:AuthSubject'."),
            };

            var userRepository = new SqlUserRepository(databaseOptions);
            var eventRepository = new SqlEventRepository(databaseOptions);
            var triviaRepository = new SqlTriviaRepository(databaseOptions);
            var triviaRewardRepository = new SqlTriviaRewardRepository(databaseOptions);
            var ambassadorRepository = new SqlAmbassadorRepository(databaseOptions);
            var favoriteEventRepository = new SqlFavoriteEventRepository(databaseOptions);
            var notificationRepository = new SqlNotificationRepository(databaseOptions);
            var movieRepository = new SqlMovieRepository(databaseOptions);
            var slotMachineStateRepository = new SqlUserSlotMachineStateRepository(databaseOptions);
            var userMovieDiscountRepository = new SqlUserRewardRepository(databaseOptions);
            var screeningRepository = new SqlScreeningRepository(databaseOptions);
            var marathonRepository = new SqlMarathonRepository(databaseOptions);

            _currentUserService = new CurrentUserService(userRepository, bootstrapUserOptions);
            await _currentUserService.InitializeAsync();

            CurrentUserService = _currentUserService;
            EventRepository = eventRepository;
            TriviaRepository = triviaRepository;
            TriviaRewardRepository = triviaRewardRepository;
            AmbassadorRepository = ambassadorRepository;
            ReferralValidator = new ReferralValidator(ambassadorRepository);
            FavoriteEventService = new FavoriteEventService(favoriteEventRepository, eventRepository);
            NotificationService = new NotificationService(notificationRepository, favoriteEventRepository, eventRepository);
            CurrentUserId = _currentUserService.CurrentUser.Id;
            MovieRepository = movieRepository;
            SlotMachineStateRepository = slotMachineStateRepository;
            UserMovieDiscountRepository = userMovieDiscountRepository;
            ScreeningRepository = screeningRepository;
            MarathonRepository = marathonRepository;

            SlotMachineService = new SlotMachineService(
                slotMachineStateRepository,
                movieRepository,
                eventRepository,
                userMovieDiscountRepository);
            SlotMachineResultService = new SlotMachineResultService(userMovieDiscountRepository);
            ReelAnimationService = new ReelAnimationService();

            viewModel = new MainViewModel(_currentUserService.CurrentUser);
        }
        catch (Exception exception)
        {
            ResetRuntimeServices();
            viewModel = MainViewModel.CreateStartupError(BuildStartupErrorMessage(exception));
        }

        CurrentMainWindow = new MainWindow(viewModel);
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

    /// <summary>
    /// Clears the application-wide runtime services before a new startup attempt
    /// or after a startup failure.
    /// </summary>
    private void ResetRuntimeServices()
    {
        _currentUserService = null;
        CurrentUserService = null;
        EventRepository = null;
        TriviaRepository = null;
        TriviaRewardRepository = null;
        AmbassadorRepository = null;
        ReferralValidator = null;
        Configuration = null;
        MarathonRepository = null;
        FavoriteEventService = null;
        NotificationService = null;
        CurrentUserId = 0;
        MovieRepository = null;
        SlotMachineStateRepository = null;
        UserMovieDiscountRepository = null;
        ScreeningRepository = null;
        SlotMachineService = null;
        SlotMachineResultService = null;
        ReelAnimationService = null;
    }

    private static string BuildStartupErrorMessage(Exception exception)
    {
        return "The application could not connect to the configured database."
            + Environment.NewLine
            + "Verify the database exists, the schema is applied, and appsettings.json points to a reachable SQL Server instance."
            + Environment.NewLine
            + Environment.NewLine
            + exception.Message;
    }
}
