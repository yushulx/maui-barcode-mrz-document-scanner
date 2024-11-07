using System.Diagnostics;
using Dynamsoft.CameraEnhancer.Maui;
using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.Core.Maui;
using Dynamsoft.License.Maui;
using Dynamsoft.LabelRecognizer.Maui;
using Dynamsoft.CodeParser.Maui;
using Dynamsoft.Utility.Maui;

namespace mrz_scanner_mobile_maui;

public partial class ScanPage : ContentPage, ILicenseVerificationListener, ICapturedResultReceiver, ICompletionListener
{
    public static CameraEnhancer enhancer;
    private CaptureVisionRouter router;
    private bool beepStatus;
    private string currentTemplate;
    private string text;
    private bool succeed = false;
    private bool licenseVerified = false;
    private int birthYear;

    public ScanPage()
    {
        InitializeComponent();
        LicenseManager.InitLicense("DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==", this);
        currentTemplate = "ReadPassportAndId";
        enhancer = new CameraEnhancer();
        router = new CaptureVisionRouter();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (this.Handler != null)
        {
            enhancer.SetCameraView(camera);
            enhancer.EnableEnhancedFeatures(EnumEnhancedFeatures.EF_FRAME_FILTER);
            enhancer.Open();
        }
    }

    public void OnLicenseVerified(bool isSuccess, string message)
    {
        licenseVerified = isSuccess;

        if (!isSuccess)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LayoutInit.IsVisible = true;
                LabelInit.Text = "License initialization failed: " + message;
            });
        }

    }

    public void OnRecognizedTextLinesReceived(RecognizedTextLinesResult result)
    {
        OnLabelTextReceived(result);
    }

    public void OnParsedResultsReceived(ParsedResult result)
    {
        if (!succeed)
        {
            OnParsedResultReceived(result);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        beepStatus = Preferences.Default.Get("status", true);
        UpdateBackground();
        await Permissions.RequestAsync<Permissions.Camera>();
        MultiFrameResultCrossFilter filter = new MultiFrameResultCrossFilter();
        filter.EnableResultCrossVerification(EnumCapturedResultItemType.CRIT_TEXT_LINE, true);
        router?.AddResultFilter(filter);
        try
        {
            router.SetInput(enhancer);
        }
        catch (Exception e)
        {
            e.GetBaseException();
        }
        router.AddResultReceiver(this);
        restartCapture();
        enhancer?.SetColourChannelUsageType(EnumColourChannelUsageType.CCUT_FULL_CHANNEL);
        enhancer?.Open();
    }

    protected override void OnDisappearing()
    {
        succeed = false;
        base.OnDisappearing();
        enhancer?.Close();
        router?.StopCapturing();

    }

    private void UpdateBackground()
    {
        if (beepStatus)
        {
            BtnBeep.Source = "icon_music.png";
        }
        else
        {
            BtnBeep.Source = "icon_music_mute.png";
        }
    }

    public void OnSuccess()
    {
        Debug.WriteLine("success");
    }

    public void OnFailure(int errorCode, string errorMessage)
    {
        Debug.WriteLine(errorMessage);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LayoutStart.IsVisible = true;
            LabelStart.Text = errorMessage;
        });

    }

    void restartCapture()
    {
        router?.StartCapturing(currentTemplate, this);
    }

    void IdBtnClicked(object sender, EventArgs e)
    {
        if (!licenseVerified)
        {
            return;
        }

        IdBtn.Background = Color.FromArgb("#FE8E14");
        BothBtn.Background = Color.FromArgb("#000000");
        PassportBtn.Background = Color.FromArgb("#000000");
        if (!"ReadId".Equals(currentTemplate))
        {
            currentTemplate = "ReadId";
            router.StopCapturing();
            restartCapture();
        }

    }

    void PassportBtnClicked(object sender, EventArgs e)
    {
        if (!licenseVerified)
        {
            return;
        }

        IdBtn.Background = Color.FromArgb("#000000");
        BothBtn.Background = Color.FromArgb("#000000");
        PassportBtn.Background = Color.FromArgb("#FE8E14");
        if (!"ReadPassport".Equals(currentTemplate))
        {
            currentTemplate = "ReadPassport";
            router.StopCapturing();
            restartCapture();
        }
    }

    void BothBtnClicked(object sender, EventArgs e)
    {
        if (!licenseVerified)
        {
            return;
        }
        IdBtn.Background = Color.FromArgb("#000000");
        BothBtn.Background = Color.FromArgb("#FE8E14");
        PassportBtn.Background = Color.FromArgb("#000000");
        if (!"ReadPassportAndId".Equals(currentTemplate))
        {
            currentTemplate = "ReadPassportAndId";
            router.StopCapturing();
            restartCapture();
        }
    }

    void BeepClicked(Object sender, EventArgs e)
    {
        beepStatus = !beepStatus;
        UpdateBackground();
        Preferences.Default.Set("status", beepStatus);
    }

    void OnLabelTextReceived(RecognizedTextLinesResult result)
    {
        if (result.Items == null)
        {
            return;
        }
        List<TextLineResultItem> items = result.Items;
        items.ForEach(item =>
        {
            text += item.Text + "\n\n";
        });
    }

    void OnParsedResultReceived(ParsedResult result)
    {
        if (result.Items == null)
        {
            return;
        }
        if (result.Items.Count() == 0)
        {
            if (text.Length != 0)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CombineErrorTexts(text);
                });

            }
        }
        else
        {
            ImageData data = router.GetIntermediateResultManager().GetOriginalImage(result.OriginalImageHashId);

            Dictionary<String, String> labelMap = AssembleMap(result.Items[0]);
            if (labelMap != null && labelMap.Count != 0)
            {
                if (beepStatus)
                {
                    Feedback.Beep();
                }
                succeed = true;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PushAsync(new ResultPage(labelMap, data));
                    ClearText();
                });

            }
        }
    }

    void ClearText()
    {
        LabelAlert.Text = "";
        LabelError.Text = "";
    }

    void CombineErrorTexts(string text)
    {
        LabelAlert.Text = "Error: Failed to parse the content.";
        LabelError.Text = "The MRZ text is: \n" + text;
    }

    Dictionary<string, string> AssembleMap(ParsedResultItem item)
    {
        Dictionary<string, ParsedField> entry = item.ParsedFields;
        string documentType = "";
        if (item.CodeType.Equals("MRTD_TD1_ID") || item.CodeType.Equals("MRTD_TD2_ID") || item.CodeType.Equals("MRTD_TD2_FRENCH_ID"))
        {
            documentType = "ID";
        }
        else if (item.CodeType.Equals("MRTD_TD3_PASSPORT"))
        {
            documentType = "PASSPORT";
        }

        string number = entry.ContainsKey("passportNumber") ? entry["passportNumber"].Value : entry.ContainsKey("documentNumber") ? entry["documentNumber"].Value : "";

        string firstName = entry.ContainsKey("secondaryIdentifier") ? " " + entry["secondaryIdentifier"].Value : "";

        string lastName = entry.TryGetValue("primaryIdentifier", out var identifier) ? identifier.Value ?? "" : "";
        string name = lastName + firstName;

        if (number == null ||
            !entry.TryGetValue("sex", out _) ||
            !entry.TryGetValue("issuingState", out _) ||
            !entry.TryGetValue("nationality", out _) ||
            !entry.TryGetValue("secondaryIdentifier", out _) ||
            !entry.TryGetValue("primaryIdentifier", out _) ||
            !entry.TryGetValue("dateOfBirth", out _) ||
            !entry.TryGetValue("dateOfExpiry", out _))
        {
            return null;
        }
        int age = -1;
        int expiryYear = 0;
        try
        {
            int year = int.Parse(entry["birthYear"].Value);
            int month = int.Parse(entry["birthMonth"].Value);
            int day = int.Parse(entry["birthDay"].Value);
            expiryYear = int.Parse(entry["expiryYear"].Value) + 2000;
            age = CalculateAge(year, month, day);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.StackTrace);
        }

        Dictionary<string, string> properties = new Dictionary<string, string>
        {
            { "Name", name },
            { "Sex", entry.TryGetValue("sex", out var sex) ? sex.Value : "Unknown" },
            { "Age", age == -1 ? "Unknown" : age + "" },
            { "Document Number", number },
            { "Issuing State", entry.TryGetValue("issuingState", out var issuingState) ? issuingState.Value : "Unknown" },
            { "Nationality", entry.TryGetValue("nationality", out var nationality) ? nationality.Value : "Unknown" },
            {
                "Date of Birth(YY-MM-DD)",
                birthYear + "-" +
                entry["birthMonth"].Value + "-" + entry["birthDay"].Value
            },
            {
                "Date of Expiry(YY-MM-DD)",
                expiryYear + "-" +
                entry["expiryMonth"].Value + "-" + entry["expiryDay"].Value
            },
            { "Personal Number", entry.TryGetValue("personalNumber", out var personalNumber) ? personalNumber.Value : "Unknown" },
            { "Primary Identifier(s)", entry.TryGetValue("primaryIdentifier", out var primaryIdentifier) ? primaryIdentifier.Value : "Unknown" },
            { "Secondary Identifier(s)", entry.TryGetValue("secondaryIdentifier", out var secondaryIdentifier) ? secondaryIdentifier.Value : "Unknown" },
            { "Document Type", documentType }
        };
        return properties;
    }

    private int CalculateAge(int year, int month, int day)
    {
        DateTime now = DateTime.Now;
        int cYear = now.Year;
        int cMonth = now.Month;
        int cDay = now.Day;
        birthYear = 1900 + year;
        int diffYear = cYear - birthYear;
        int diffMonth = cMonth - month;
        int diffDay = cDay - day;
        int age = MinusYear(diffYear, diffMonth, diffDay);
        if (age > 100)
        {
            birthYear = 2000 + year;
            diffYear = cYear - birthYear;
            age = MinusYear(diffYear, diffMonth, diffDay);
        }
        else if (age < 0)
        {
            age = 0;
        }
        return age;
    }

    private int MinusYear(int diffYear, int diffMonth, int diffDay)
    {
        int age = Math.Max(diffYear, 0);
        if (diffMonth < 0)
        {
            age = age - 1;

        }
        else if (diffMonth == 0)
        {
            if (diffDay < 0)
            {
                age = age - 1;
            }
        }
        return age;
    }
}
