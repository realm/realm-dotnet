FROM mcr.microsoft.com/dotnet/sdk:3.1
RUN ABS_PATH_TO_FIX=/usr/share/dotnet/sdk/$(dotnet --version)/Sdks/Microsoft.NET.Sdk.WindowsDesktop/targets && \
    mv $ABS_PATH_TO_FIX/Microsoft.WinFx.props $ABS_PATH_TO_FIX/Microsoft.WinFX.props && \
    mv $ABS_PATH_TO_FIX/Microsoft.WinFx.targets $ABS_PATH_TO_FIX/Microsoft.WinFX.targets
