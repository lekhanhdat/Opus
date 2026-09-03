/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
namespace AvePoint.Hybrid.AgentService.RecordsCloudAgentUpgrader
{

    public static class RecordsBatScriptSection
    {
        public const string HEADER =
    @"@echo off
setlocal enabledelayedexpansion

";

        public const string REQUIRE_ADMIN = @"
net session >nul 2>&1
if !errorlevel! neq 0 (
    echo [FATAL] Administrator privileges required.
    echo Please run this script as Administrator.
    exit /b 999
)

";

        public const string PARAMETERS_DEFINITION =
    @":: =======================================================
:: Parameters
:: %1 = ServiceName
:: %2 = Service username (optional)
:: %3 = Service password (optional)
:: %4 = Installer full path (MSI or MSP)
:: %5 = Log file path
:: =======================================================

set ""ServiceName=%1""
set ""ServiceUser=%2""
set ""ServicePass=%3""
set ""InstallerPath=%4""
set ""LogFilePath=%5""
set ""UPGRADE_EXIT_CODE=0""

";

        public const string BASIC_VALIDATION =
    @"
if %ServiceName%=="""" (
    echo [FATAL] Missing ServiceName
    set ""UPGRADE_EXIT_CODE=2""
    goto END_SCRIPT
)

if %InstallerPath%=="""" (
    echo [FATAL] Missing InstallerPath
    set ""UPGRADE_EXIT_CODE=2""
    goto END_SCRIPT
)

echo [%date% %time%] Starting upgrade for %ServiceName%
echo Installer: %InstallerPath%
echo Log: %LogFilePath%

for %%D in (%LogFilePath%) do set ""LogDir=%%~dpD""

if not exist ""%LogDir%"" (
    echo Creating log directory: %LogDir%
    mkdir %LogDir%
    if errorlevel 1 (
        echo [FATAL] Failed to create log directory: %LogDir%
        set ""UPGRADE_EXIT_CODE=2""
        goto END_SCRIPT
    )
)

";

        public const string ADVANCED_VALIDATION =
    @"
if not exist ""%InstallerPath%"" (
    echo [FATAL] Installer file NOT FOUND: %InstallerPath%
    set ""UPGRADE_EXIT_CODE=2""
    goto END_SCRIPT
)

sc.exe query ""%ServiceName%"" >nul 2>&1
if errorlevel 1 (
    echo [FATAL] Service ""%ServiceName%"" DOES NOT EXIST.
    set ""UPGRADE_EXIT_CODE=2""
    goto END_SCRIPT
)

echo Validation passed. Proceeding upgrade...

";

        public const string PRE_UPGRADE =
    @"
echo Checking service [%ServiceName%] status...
for /f ""tokens=3"" %%A in ('sc query %ServiceName% ^| find /I ""STATE""') do (
    set ""STATE=%%A""
)
echo The service's state is: %STATE%

if ""!STATE!""==""4"" (
    @@KILL_WORKER_BLOCK@@
    echo The service [%ServiceName%] is running so need to stop this service...
    net stop %ServiceName% >nul 2>&1
    if !errorlevel! neq 0 (
        :WAIT_STOP
        for /f ""tokens=3"" %%A in ('sc.exe query %ServiceName% ^| find /I ""STATE""') do (
            set ""STATE=%%A""
        )
        if not ""!STATE!""==""1"" (
            timeout /t 2 >nul
            goto WAIT_STOP
        )
    )    
)
echo Service [%ServiceName%] stopped.

";
        public const string KILL_WORKER_MARK = "@@KILL_WORKER_BLOCK@@";
        public const string KILL_WORKER =
    @"
echo Killing RecordsAgentWorker.exe if running...
tasklist /FI ""IMAGENAME eq RecordsAgentWorker.exe"" >nul
if !errorlevel! neq 1 (
    taskkill /F /IM RecordsAgentWorker.exe >nul 2>&1
)else (
    echo RecordsAgentWorker.exe is not running.
)

";

        public const string INSTALLATION =
    @"
echo Running package installation...

echo InstallerPath=[%InstallerPath%]

for %%A in (%InstallerPath%) do (
    set ""Ext=%%~xA""
)

if /I ""%Ext%""=="".msi"" (
    msiexec /i %InstallerPath% /qn /norestart ADDLOCAL=ALL /l*v %LogFilePath%
) else if /I ""%Ext%""=="".msp"" (
    msiexec /p %InstallerPath% /qn /norestart /l*v %LogFilePath%
) else (
    echo [FATAL] Unknown installer extension: %Ext%
    set ""UPGRADE_EXIT_CODE=3""
    goto END_SCRIPT
)

set MSI_RC=!errorlevel!
echo MSI exit code: %MSI_RC%

if %MSI_RC%==0 (
    echo MSI Install succeeded
    set REBOOT_REQUIRED=0
    goto NO_ROLLBACK
)

if %MSI_RC%==3010 (
    echo MSI Install succeeded, reboot required
    set REBOOT_REQUIRED=1
    goto NO_ROLLBACK
)

if %MSI_RC%==1641 (
    echo [WARN] Install succeeded, reboot initiated
    set REBOOT_REQUIRED=1
    goto END_SCRIPT
)

echo [ERROR] MSI failed with exit code %MSI_RC%
set ""UPGRADE_EXIT_CODE=%MSI_RC%""
goto END_SCRIPT

";

        public const string UPGRADE_FAILED_LABEL =
@"
:UPGRADE_FAILED
echo [ERROR] Upgrade failed. Attempting rollback if enabled...

";

        public const string ROLLBACK_INSTALLATION =
    @"
echo Rolling back package installation , path [%InstallerPath%]...

if /I ""%Ext%""=="".msi"" (
    msiexec /i %InstallerPath% /qn /norestart ADDLOCAL=ALL /l*v %LogFilePath%
) else if /I ""%Ext%""=="".msp"" (
    msiexec /p %InstallerPath% /qn /norestart /l*v %LogFilePath%
)

set MSI_RC=!errorlevel!
echo Rollback MSI exit code: %MSI_RC%

if %MSI_RC%==0 goto NO_ROLLBACK
if %MSI_RC%==3010 goto NO_ROLLBACK
if %MSI_RC%==1641 goto END_SCRIPT

echo [ERROR] Rollback MSI/MSP incomplete installation with exit code %MSI_RC%
set ""UPGRADE_EXIT_CODE=%MSI_RC%""
goto END_SCRIPT

";

        public const string REAPPLY_SERVICE_ACCOUNT =
    @"
:NO_ROLLBACK
if /I ""%Ext%""=="".msi"" (
    if not %ServiceUser%=="""" (
        echo Updating service logon user...
        sc.exe config %ServiceName% obj= %ServiceUser% password= %ServicePass%

        if !errorlevel! neq 0 (
            echo [ERROR] Failed to update service account.
            set ""UPGRADE_EXIT_CODE=4""
            goto END_SCRIPT
        )
    )
) else (
    echo MSP detected. Skip re-applying service account.
)

";

        public const string START_SERVICE =
    @"
sc.exe start %ServiceName% >nul 2>&1
if !errorlevel! == 0 (
    :WAIT_START
     for /f ""tokens=3"" %%A in ('sc.exe query %ServiceName% ^| find /I ""STATE""') do (
        set ""STATE=%%A""
    )   

    if ""!STATE!""==""1"" (
        timeout /t 2 >nul
        goto WAIT_START
    )    
    echo Service started.
)else (
    echo [ERROR] Failed to start service %ServiceName%.
)

";

        public const string FOOTER =
    @"
:END_SCRIPT
if ""%UPGRADE_EXIT_CODE%""==""0"" (
    echo [SUCCESS] Upgrade completed.
) else (
    echo [FAILED] Upgrade terminated with exit code %UPGRADE_EXIT_CODE%.
)

exit /b %UPGRADE_EXIT_CODE%

";
    }
}

