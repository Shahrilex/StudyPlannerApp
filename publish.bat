@echo off
REM ساخت یک فایل exe مستقل که نیازی به نصب .NET Runtime روی سیستم مقصد ندارد.
REM خروجی در: src\StudyPlanner.App\bin\Release\net8.0-windows\win-x64\publish\

dotnet publish src\StudyPlanner.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

echo.
echo تمام شد. فایل exe را از مسیر بالا بردار.
pause
