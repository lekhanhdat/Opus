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
@Set StartTime=%time:~0,-3%
 
Set MSBuild="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
%MSBuild%  Build.xml /t:MainDeploy /verbosity:Quiet
%MSBuild%  CopyBinaryList.xml /t:RevIMMainTarget
@Set EndTime=%time:~0,-3%
@Echo Start Time: %StartTime%
@Echo End Time: %EndTime%
@ENDLOCAL
PAUSE