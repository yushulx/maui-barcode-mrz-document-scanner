namespace mrz_scanner_mobile_maui;

using Dynamsoft.Core.Maui;

public partial class ResultPage : ContentPage
{
    public ResultPage(Dictionary<String, String> labelMap, ImageData imageData)
    {
        InitializeComponent();
        if (labelMap.Count > 0)
        {
            VerticalLayout.Add(ChildView("Document Type:", labelMap["Document Type"]));
            VerticalLayout.Add(ChildView("Document Number:", labelMap["Document Number"]));
            VerticalLayout.Add(ChildView("Full Name:", labelMap["Name"]));
            VerticalLayout.Add(ChildView("Sex:", labelMap["Sex"].First().ToString().ToUpper()));
            VerticalLayout.Add(ChildView("Age:", labelMap["Age"]));
            VerticalLayout.Add(ChildView("Issuing State:", labelMap["Issuing State"]));
            VerticalLayout.Add(ChildView("Nationality:", labelMap["Nationality"]));
            VerticalLayout.Add(ChildView("Date of Birth(YYYY-MM-DD):", labelMap["Date of Birth(YY-MM-DD)"]));
            VerticalLayout.Add(ChildView("Date of Expiry(YYYY-MM-DD):", labelMap["Date of Expiry(YY-MM-DD)"]));

            var imageControl = new Image
            {
                HeightRequest = 400,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            imageControl.Source = imageData.ToImageSource();
            VerticalLayout.Add(imageControl);
        }
    }

    IView ChildView(string label, string text)
    {
        return new VerticalStackLayout
            {
                new Label
                {
                    Text = label,
                    TextColor = Color.FromArgb("AAAAAA"),
                    FontSize = 16,
                    Padding = new Thickness(0, 20, 0, 0),
                },
                new Entry
                {
                    Text = text,
                    TextColor = Colors.White,
                    FontSize = 16,
                    BackgroundColor = Colors.Transparent, // Optional: makes the entry look more like a label
                    IsReadOnly = false, // Allows text editing
                }
            };

    }
}
