:: /********************************************************************
:: *
:: *  PROPRIETARY and CONFIDENTIAL
:: *
:: *  This file is licensed from, and is a trade secret of:
:: *
:: *                   AvePoint, Inc.
:: *                   Harborside Financial Center
:: *                   9th Fl.   Plaza Ten
:: *                   Jersey City, NJ 07311
:: *                   United States of America
:: *                   Telephone: +1-800-661-6588
:: *                   WWW: www.avepoint.com
:: *
:: *  Refer to your License Agreement for restrictions on use,
:: *  duplication, or disclosure.
:: *
:: *  RESTRICTED RIGHTS LEGEND
:: *
:: *  Use, duplication, or disclosure by the Government is
:: *  subject to restrictions as set forth in subdivision
:: *  (c)(1)(ii) of the Rights in Technical Data and Computer
:: *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
:: *  FAR 52.227-19 (C) (June 1987).
:: *
:: *  Copyright © 2020 AvePoint® Inc. All Rights Reserved. 
:: *
:: *  Unpublished - All rights reserved under the copyright laws of the United States.
:: */
@ECHO OFF
@SETLOCAL

set azurepackpath="C:\Program Files\Microsoft SDKs\Azure\.NET SDK\v2.9\bin\cspack.exe"

::/* Web & Timer*/
%azurepackpath% cspack ..\RAWeb.Azure\ServiceDefinition.csdef /out:"..\..\Package\CloudRecordsWeb.cspkg" /role:RAWeb;"RevIMWorker.Web" /rolePropertiesFile:RAWeb;"RevIMWorker.Web/webRoleProperties.txt" /role:RATimerWorkerRole;"RevIMWorker.Timer";"RATimerWorkerRole.dll"
copy /y "..\RAWeb.Azure\ServiceConfiguration.Cloud.cscfg" "..\..\Package\CloudRecordsWeb.cscfg"

::/* Agent*/
%azurepackpath% cspack ..\RACloud\ServiceDefinition.csdef /out:"..\..\Package\CloudRecordsAgent.cspkg" /role:RAScheduleJobWorkerRole;"RevIMWorker.ScheduleJob";"RevIMScheduleJobWorkerRole.dll"

copy /y "..\RACloud\ServiceConfiguration.Cloud.cscfg" "..\..\Package\CloudRecordsAgent.cscfg"

::/* Medium Agent*/
%azurepackpath% cspack ..\RACloud\ServiceDefinition_Medium.csdef /out:"..\..\Package\CloudRecordsAgent_Medium.cspkg" /role:RAScheduleJobWorkerRole;"RevIMWorker.ScheduleJob";"RevIMScheduleJobWorkerRole.dll"

::/* Large Agent*/
%azurepackpath% cspack ..\RACloud\ServiceDefinition_Large.csdef /out:"..\..\Package\CloudRecordsAgent_Large.cspkg" /role:RAScheduleJobWorkerRole;"RevIMWorker.ScheduleJob";"RevIMScheduleJobWorkerRole.dll"

::/* AppWeb */
%azurepackpath% cspack ..\RASPApp\RASPAppWeb.Azure\ServiceDefinition.csdef /out:"..\..\Package\CloudRecordsAppWeb.cspkg" /role:RASPAppWeb;"RevIMWorker.ProviderWeb" /rolePropertiesFile:RASPAppWeb;"RevIMWorker.ProviderWeb/appwebRoleProperties.txt" 
copy /y "..\RASPApp\RASPAppWeb.Azure\ServiceConfiguration.Cloud.cscfg" "..\..\Package\CloudRecordsAppWeb.cscfg"

@ENDLOCAL

PAUSE