:: /********************************************************************
:: *
:: *  PROPRIETARY and CONFIDENTIAL
:: *
:: *  This file is licensed from, and is a trade secret of:
:: *
:: *                   AvePoint, Inc.
:: *                   525 Washington Blvd, Suite 1400
:: *                   Jersey City, NJ 07310
:: *                   United States of America
:: *                   Telephone: +1-201-793-1111
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
:: *  Copyright © 2017-2024 AvePoint® Inc. All Rights Reserved. 
:: *
:: *  Unpublished - All rights reserved under the copyright laws of the United States.
:: */
candle.exe -dType=Old -dVersion=15.11.0.364 -dPlatform=x64 -arch x64 product.wxs -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\WixUtilExtension.dll"
light.exe product.wixobj -out .\output\x64\old\CloudAgentInstaller_15.11.0.364.msi -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\WixUtilExtension.dll" -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\WixUIExtension.dll" -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\WixNetFxExtension.dll"
candle.exe -dType=New -dVersion=1.0.0.0 -dPlatform=x64 -arch x64 product.wxs -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\WixUtilExtension.dll"
light.exe product.wixobj -out .\output\x64\new\setup.msi -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\WixUtilExtension.dll" -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\WixUIExtension.dll" -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\WixNetFxExtension.dll"
torch.exe -p -xi .\output\x64\old\Setup.wixpdb .\output\x64\new\Setup.wixpdb -out .\patch\x64\diff.wixmst
candle.exe -dVersion=1.0.0.0 Patch.wxs
light.exe Patch.wixobj -out .\patch\x64\patch.wixmsp
pyro.exe patch\x64\patch.wixmsp -out patch\x64\patch.msp -t RTM patch\x64\diff.wixmst