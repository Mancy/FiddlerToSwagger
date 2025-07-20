@echo off
echo Building Fiddler to Swagger YAML Exporter...
echo.

cd workspace

echo Restoring NuGet packages...
nuget restore FiddlerToSwagger.sln
if errorlevel 1 (
    echo ERROR: Failed to restore NuGet packages
    echo Please ensure NuGet is installed and available in PATH
    pause
    exit /b 1
)

echo.
echo Building solution...
msbuild FiddlerToSwagger.sln /p:Configuration=Release /p:Platform="Any CPU" /verbosity:minimal
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo.
echo The extension has been automatically copied to your Fiddler ImportExport folder.
echo Restart Fiddler to use the new extension.
echo.
pause 