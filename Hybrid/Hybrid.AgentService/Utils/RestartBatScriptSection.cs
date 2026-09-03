namespace AvePoint.Hybrid.AgentService.Utils
{
    public static class RestartBatScriptSection
    {
        public const string HEADER =
    @"@echo off
setlocal enabledelayedexpansion

";

        public const string PARAMETERS_DEFINITION =
    @":: =======================================================
:: Parameters
:: %1 = ServiceName
:: %2 = Log file path
:: =======================================================

set ""ServiceName=%~1""
set ""LogFilePath=%~2""
set ""RESTART_EXIT_CODE=0""

";

        public const string VALIDATION =
    @"
if ""%ServiceName%""=="""" (
    set ""RESTART_EXIT_CODE=2""
    goto END_SCRIPT
)

if ""%LogFilePath%""=="""" (
    set ""RESTART_EXIT_CODE=2""
    goto END_SCRIPT
)

for %%D in (""%LogFilePath%"") do set ""LogDir=%%~dpD""

if not exist ""%LogDir%"" (
    mkdir ""%LogDir%""
)

echo [%date% %time%] Start restart service script. > ""%LogFilePath%""
echo ServiceName=%ServiceName% >> ""%LogFilePath%""

sc query ""%ServiceName%"" >nul 2>&1
if errorlevel 1 (
    sc query ""%ServiceName%"" >> ""%LogFilePath%"" 2>&1
    echo [WARN] Service does not exist: %ServiceName% >> ""%LogFilePath%""
    goto END_SCRIPT
)

";

        public const string RESTART_SERVICE =
    @"
timeout /t 3 >nul

for /f ""tokens=3"" %%A in ('sc query ""%ServiceName%"" ^| find /I ""STATE""') do (
    set ""STATE=%%A""
)

echo Current state before restart: !STATE! >> ""%LogFilePath%""

if not ""!STATE!""==""1"" (
    echo Stopping service %ServiceName%... >> ""%LogFilePath%""
    sc stop ""%ServiceName%"" >> ""%LogFilePath%"" 2>&1
    set /a RetryCount=0
    :WAIT_STOP
    for /f ""tokens=3"" %%A in ('sc query ""%ServiceName%"" ^| find /I ""STATE""') do (
        set ""STATE=%%A""
    )
    echo Wait stop state: !STATE!, retry: !RetryCount! >> ""%LogFilePath%""
    if ""!STATE!""==""1"" goto START_SERVICE
    if !RetryCount! geq 15 (
        set ""RESTART_EXIT_CODE=1""
        echo [ERROR] Stop service timeout. >> ""%LogFilePath%""
        goto END_SCRIPT
    )
    set /a RetryCount+=1
    timeout /t 2 >nul
    goto WAIT_STOP
)

:START_SERVICE
echo Starting service %ServiceName%... >> ""%LogFilePath%""
sc start ""%ServiceName%"" >> ""%LogFilePath%"" 2>&1
if errorlevel 1 (
    set ""RESTART_EXIT_CODE=1""
    echo [ERROR] Failed to start service. >> ""%LogFilePath%""
    goto END_SCRIPT
)
set /a RetryCount=0
:WAIT_START
for /f ""tokens=3"" %%A in ('sc query ""%ServiceName%"" ^| find /I ""STATE""') do (
    set ""STATE=%%A""
)
echo Wait start state: !STATE!, retry: !RetryCount! >> ""%LogFilePath%""
if ""!STATE!""==""4"" goto END_SCRIPT
if !RetryCount! geq 15 (
    set ""RESTART_EXIT_CODE=1""
    echo [ERROR] Start service timeout. >> ""%LogFilePath%""
    goto END_SCRIPT
)
set /a RetryCount+=1
timeout /t 2 >nul
goto WAIT_START

";

        public const string FOOTER =
    @"
:END_SCRIPT
echo Restart exit code: %RESTART_EXIT_CODE% >> ""%LogFilePath%""
exit /b %RESTART_EXIT_CODE%

";
    }
}
