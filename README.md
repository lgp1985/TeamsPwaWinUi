# TeamsPwaWinUi

A small Windows desktop container for Microsoft Teams on the web, built with C#, WinUI 3, and Microsoft Edge WebView2.

The app opens https://teams.cloud.microsoft/ in a dedicated native window. Teams itself remains the hosted web application; this project provides the Windows shell around it rather than implementing a separate Teams client.

## What it does

- Hosts Teams in a full-window WebView2 control, without browser tabs or an address bar.
- Uses a Mica window backdrop on supported Windows versions.
- Enables WebView2 Windows single sign-on before browser initialization using `AllowSingleSignOnUsingOSPrimaryAccount`. Microsoft controls which Windows-connected accounts appear in its web sign-in flow; sign-in may still be required.
- Pre-allows WebView2 notifications, window management, microphone, and camera permissions for the Teams and Outlook web origins.
- Updates the window icon when the hosted page's favicon changes.

Outlook is included in the permission setup, but Teams is the startup page. There is no separate Outlook view or app-switching UI.

### Windows-connected accounts

The Microsoft web account picker was verified to show both configured work accounts with **Connected to Windows** on the development machine. Availability depends on the Windows accounts, WebView2 Runtime, and organizational policies; this is not a guarantee that every account in Windows Settings will appear.

## Requirements

- A Windows x64 development machine.
- .NET 10 SDK.
- Windows SDK 10.0.26100.0 or a compatible newer SDK.
- Visual Studio with WinUI development and MSIX packaging support to use the packaged launch profile.
- Microsoft Edge WebView2 Runtime on the machine running the app.
- Internet access and an account with access to Teams.

The project targets `net10.0-windows10.0.26100.0` and currently configures the `win-x64` runtime.

## Build and run

### Visual Studio

1. Open [TeamsPwaWinUi.slnx](TeamsPwaWinUi.slnx).
2. Select **Debug** and **x64**.
3. Configure a local package-signing certificate if using the packaged launch profile.
4. Select **TeamsPwaWinUi (Package)** and start debugging.

The project currently references a local development signing certificate. For your own build, configure your certificate in the project's packaging settings and ensure the publisher in [Package.appxmanifest](TeamsPwaWinUi/Package.appxmanifest) matches it. Do not commit private signing keys.

### Command line

From the repository root:

```powershell
dotnet build .\TeamsPwaWinUi\TeamsPwaWinUi.csproj -p:Platform=x64 -p:Configuration=Release -p:AppxPackageSigningEnabled=false
```

This restores dependencies and builds the Release configuration without package signing. It does not install or launch the app, or produce a trusted, signed installer. Use Debug for debugging; use Release for normal use. Release publishing enables ReadyToRun and trimming in the current project settings.

Pass `-p:Platform=x64` explicitly when building from the command line or VS Code to avoid selecting an incompatible solution platform.

### Package and update the installed app

After configuring a trusted signing certificate and matching package publisher:

```powershell
dotnet build .\TeamsPwaWinUi\TeamsPwaWinUi.csproj -p:Platform=x64 -p:Configuration=Release -p:GenerateAppxPackageOnBuild=true -p:AppxBundle=Never
```

The signed MSIX is generated under [TeamsPwaWinUi/AppPackages](TeamsPwaWinUi/AppPackages), in a versioned folder. Close the running app and install the generated Release package with `Add-AppxPackage -Path '<path-to-generated.msix>'`. The signing certificate must be trusted and required package dependencies must be installed on the target machine.

For an in-place update, retain the package identity and publisher, and increment the version in [Package.appxmanifest](TeamsPwaWinUi/Package.appxmanifest) before packaging. Windows rejects changed package contents with the same installed version. Updating in place preserves app data; the existing sign-in session was verified to survive the updates performed during development.

Packaging may warn that `mspdbcmf.exe` is missing. In the verified builds, this prevented symbols-package generation but did not prevent the Release MSIX from being built and installed.

## Project layout

- [MainWindow.xaml](TeamsPwaWinUi/MainWindow.xaml): native window, Mica backdrop, and WebView2 control.
- [MainWindow.xaml.cs](TeamsPwaWinUi/MainWindow.xaml.cs): WebView2 initialization, sign-in options, origin permissions, navigation, and favicon handling.
- [App.xaml.cs](TeamsPwaWinUi/App.xaml.cs): application startup and window creation.
- [TeamsPwaWinUi.csproj](TeamsPwaWinUi/TeamsPwaWinUi.csproj): framework, dependencies, assets, and packaging settings.
- [Package.appxmanifest](TeamsPwaWinUi/Package.appxmanifest): package identity, visual assets, and capabilities.
- [Assets](TeamsPwaWinUi/Assets): packaged icons, splash screen, and tile image.

## Notes and limitations

- Features and availability depend on Teams on the web, WebView2, account policies, and Windows settings. This wrapper does not guarantee feature parity with the official Teams desktop client.
- WebView2 permissions for the configured origins are granted automatically by the app; Windows privacy controls and organizational policies can still restrict access.
- The startup URL and permission origins are defined in [MainWindow.xaml.cs](TeamsPwaWinUi/MainWindow.xaml.cs); there is no settings UI.
- This project is a wrapper around Microsoft's hosted service, not the official Microsoft Teams desktop client.
