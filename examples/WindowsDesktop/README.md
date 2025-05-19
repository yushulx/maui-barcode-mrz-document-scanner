# Camera-Based .NET MAUI Barcode Scanner for Windows, iOS, and Android
This repository contains the source code for a camera-based barcode scanner application built with .NET MAUI and Dynamsoft Barcode Reader, supporting **Windows**, **iOS**, and **Android**.

## Demo

- Windows

  https://github.com/user-attachments/assets/5d52cd5a-e777-4227-b1e0-3df6506d4b71

- iOS

  https://github.com/user-attachments/assets/d95686b1-c84e-452f-ba15-0dd17dda55e1

- Android

  https://github.com/yushulx/maui-barcode-qrcode-scanner/assets/2202306/b76aae4d-cc59-4370-a2ba-df8d46532713

## Prerequisites
- Obtain a valid [license key](https://www.dynamsoft.com/customer/license/trialLicense/?product=dcv&package=cross-platform) for Dynamsoft Barcode Reader.


## Usage
1. Open the solution in Visual Studio or Visual Studio Code.
2. Replace `LICENSE-KEY` with your own:
    - Windows: Set the license key in `Platforms/Windows/App.xaml.cs`. 

        ```csharp
        string license = "LICENSE-KEY";
        ```
    - Android/iOS: Set the license key in `MainPage.xaml.cs`.
        ```csharp
        LicenseManager.InitLicense("LICENSE-KEY");
        ```
          
3. Run the application.

    ![.NET MAUI Windows Barcode Scanner](https://www.dynamsoft.com/codepool/img/2025/02/dotnet-maui-windows-multi-barcode-scanner.png)
