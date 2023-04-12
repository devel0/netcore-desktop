namespace example.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();

        Title = AppDomain.CurrentDomain.FriendlyName + " ( GridAutoRow )";

        Width = 600;
        Height = 400;
    }

}