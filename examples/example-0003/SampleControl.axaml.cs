namespace example;

public partial class SampleControl : UserControl, INotifyPropertyChanged
{

    #region property changed

    public new event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    static int instanceCount = 0;

    int _InstanceNr = ++instanceCount;
    public int InstanceNr => _InstanceNr;

    public string HashNr => GetHashCode().ToString();

    public SampleControl()
    {
        InitializeComponent();
    }

}
