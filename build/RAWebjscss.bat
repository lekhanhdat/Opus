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
setlocal enabledelayedexpansion
set startTime=%time%
set pa=%cd%
chdir /d ..\RAWeb
set cpa=%cd%\
echo %pa%
echo %cpa%%

::goto end
::note: compression if required, please comment out previous line (front plus '::'), or delete this line, and then submit the file to svn.
for /f "usebackq" %%i in ("%~dp0RAWebjscss.txt") do (
	echo "%cpa%%%i"	
 	chdir /d "%cpa%%%i"	
	for /r . %%a in (*.js) do (
		set cc=%%~dpna
		set cc=!cc:~-4!
		echo !cc!
		if !cc! neq .min ("%~dp0\nodejs\node" "%~dp0\nodejs\node_modules\uglify-js\bin\uglifyjs" "%%~fa" -co "%%~dpna".js)		
        )
	for /r . %%b in (*.css) do (
		set dd=%%~dpnb
		set dd=!dd:~-4!
		echo !dd!
  		if !dd! neq .min ("%~dp0\nodejs\node" "%~dp0\nodejs\node_modules\clean-css\bin\cleancss" "%%~fb" --s1 -o "%%~dpnb".css)
	)
)
chdir /d %pa%

:end
echo finish time: %startTime% - %time%