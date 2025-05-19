# .NET MAUI VIN Scanner 
The project demonstrates how to use **.NET MAUI** and **Dynamsoft Capture Vision SDK** to build a VIN scanner for **Android** and **iOS**.

https://github.com/user-attachments/assets/0df4dd6b-945e-4296-99b4-a2d69e9ba949

## Prerequisites
- Visual Studio 2022 or Visual Studio Code
- Install [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- Install MAUI Workloads:
    
    ```bash
    dotnet workload install maui
    ```
    
- Obtain a [free trial license key](https://www.dynamsoft.com/customer/license/trialLicense/?product=dcv&package=cross-platform) for Dynamsoft Capture Vision.

## Getting Started
1. Set the license key in the `MainPage.xaml.cs` file:
    
    ```csharp
    LicenseManager.InitLicense("LICENSE-KEY");
    ```

2. Build and run the app.
