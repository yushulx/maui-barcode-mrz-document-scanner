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
- Install the .NET 10 SDK.
- Obtain a valid [license key](https://www.dynamsoft.com/customer/license/trialLicense/?product=dcv&package=cross-platform) for Dynamsoft Barcode Reader.
- Install the .NET MAUI workloads for the platforms you want to run:

```bash
dotnet workload install maui maui-windows android ios
```

- This project targets `.NET 10`:
  - Windows: `net10.0-windows10.0.19041.0`
  - Android: `net10.0-android36.0`
  - iOS: `net10.0-ios26.0`

- Android: install the Android SDK and connect a physical device with USB debugging enabled.
- iOS: requires Xcode on a Mac and a connected physical iPhone or iPad for camera access. If you work on Windows, you still need access to a Mac build host for iOS builds.


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

## Run on Windows, Android, and iOS

### Visual Studio

Visual Studio is the easiest way to run this multi-target project.

1. Open `BarcodeQrScanner.sln`.
2. Select one of these debug targets:
  - `Windows Machine` for Windows.
  - A connected Android device for Android.
  - A connected iPhone or iPad for iOS.
3. Press `F5` or click `Run`.

### Command line

Restore the project first:

```bash
dotnet restore .\BarcodeQrScanner.csproj
```

Run a specific target framework:

- Windows:

```bash
dotnet build -t:Run -f net10.0-windows10.0.19041.0 .\BarcodeQrScanner.csproj
```

- Android:

```bash
dotnet build -t:Run -f net10.0-android36.0 .\BarcodeQrScanner.csproj
```

- iOS:

On macOS, run the iOS target from Terminal on a connected physical device. Use the device UDID and the iOS runtime identifier:

```bash
dotnet build -t:Run -f net10.0-ios26.0 -p:RuntimeIdentifier=ios-arm64 -p:_DeviceName=YOUR_DEVICE_UDID .\BarcodeQrScanner.csproj
```

### Notes

- The minimum supported versions in this project are Android 21, iOS 15.0, and Windows 10 build 19041.
- If the Windows CLI build is pulled into Android/iOS restore and fails on package download, Visual Studio with the `Windows Machine` target is the most reliable way to launch the Windows app.
- If Android is not detected, verify that the physical device is connected and USB debugging is enabled.
- If iOS is not available in the target list, confirm that Xcode is installed on the Mac build host and that the iPhone or iPad is connected.

## Blog
[Cross-Platform .NET MAUI Barcode Scanner: Merging Mobile and Desktop Projects](https://www.dynamsoft.com/codepool/maui-ios-android-windows-camera-barcode-scanner.html)
