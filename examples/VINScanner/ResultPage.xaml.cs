using System.Collections.ObjectModel;

namespace VINScanner;

public partial class ResultPage : ContentPage
{
    public ObservableCollection<TableItem> TableItems { get; set; }

    public ResultPage(Dictionary<String, String> dictionary)
    {
        InitializeComponent();
        TableItems = [];
        foreach (var item in dictionary)
        {
            TableItems.Add(new TableItem { Key = item.Key, Value = item.Value });
        }
        BindingContext = this;
    }
}

public class TableItem
{
    public string Key { get; set; }
    public string Value { get; set; }
}
