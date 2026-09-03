# /********************************************************************
# *
# *  PROPRIETARY and CONFIDENTIAL
# *
# *  This file is licensed from, and is a trade secret of:
# *
# *                   AvePoint, Inc.
# *                   525 Washington Blvd, Suite 1400
# *                   Jersey City, NJ 07310
# *                   United States of America
# *                   Telephone: +1-201-793-1111
# *                   WWW: www.avepoint.com
# *
# *  Refer to your License Agreement for restrictions on use,
# *  duplication, or disclosure.
# *
# *  RESTRICTED RIGHTS LEGEND
# *
# *  Use, duplication, or disclosure by the Government is
# *  subject to restrictions as set forth in subdivision
# *  (c)(1)(ii) of the Rights in Technical Data and Computer
# *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
# *  FAR 52.227-19 (C) (June 1987).
# *
# *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
# *
# *  Unpublished - All rights reserved under the copyright laws of the United States.
# */
$version = $env:version
$version
c:\Build_Tools\WiX\wix311\candle.exe -dType=New -dVersion="$version" -dPlatform=x64 -arch x64 c:\source\product.wxs
if(!$?){ 
        "------------------------------------"
        "candle.exe error"
        "------------------------------------"
        exit 1
}

c:\Build_Tools\WiX\wix311\light.exe product.wixobj -dWixUILicenseRtf="license.rtf" -out "c:\source\output\x64\new\CloudAgentInstaller_$version.msi" -ext "c:\Build_Tools\WiX\wix311\WixUtilExtension.dll" -ext "c:\Build_Tools\WiX\wix311\WixUIExtension.dll" -ext "c:\Build_Tools\WiX\wix311\WixNetFxExtension.dll"
if(!$?){ 
        "------------------------------------"
        "light.exe error"
        "------------------------------------"
        exit 1
}
reg add HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\AppContext /v Switch.System.DisableTempFileCollectionDirectoryFeature /t REG_SZ /d true /f
reg add HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\.NETFramework\AppContext /v Switch.System.DisableTempFileCollectionDirectoryFeature /t REG_SZ /d true /f

c:\Build_Tools\WiX\wix311\torch.exe -p -xi c:\source\output\x64\old\CloudAgentInstaller_15.11.0.364.wixpdb "c:\source\output\x64\new\CloudAgentInstaller_$version.wixpdb" -out c:\source\diff.wixmst

c:\Build_Tools\WiX\wix311\candle.exe -dVersion="$version" c:\source\Patch.wxs
c:\Build_Tools\WiX\wix311\light.exe Patch.wixobj -out c:\source\patch.wixmsp
c:\Build_Tools\WiX\wix311\pyro.exe c:\source\patch.wixmsp -out "c:\source\CloudAgentInstaller_Upgrade_$version.msp" -t RTM c:\source\diff.wixmst
if(!$?){ 
        "------------------------------------"
        "error building patch"
        "------------------------------------"
#        exit 1
}
