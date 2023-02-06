namespace example.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();

        Title = AppDomain.CurrentDomain.FriendlyName;

        Width = 600;
        Height = 400;
    }

}