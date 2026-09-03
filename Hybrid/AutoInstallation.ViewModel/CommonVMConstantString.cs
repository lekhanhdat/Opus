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


namespace AutoInstallation.ViewModel
{
    public class CommonVMConstantString
    {
        public const string ENCRYPT_KEY = "AVE";
        public const string WebDataCab = "RelatedRecordsWebData.cab";

        public const string WEBSITE_REGEDIT_REGISTRYKEY =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Avepoint Related Records App WebSite";

        public const string UNINSTALL_FOLDER = "Uninstall";
        public const string UNINSTALL_ICO = "Ico\\UninstallationCaller.ico";
        public const string WEBSITE_INSTALL_FOLDER = "AvePoint Related Records APP WebSite";
        public const string WEBSITE_NAME_UNINSTALLCONFIG = "RecordsUninstallation.Config";
        public const string WEBSITE_NAME_ALLCONFIG = "ConfigFile/AvePoint.RelatedRecords.WebConfigTemplate.xml";
        public const string FOLDER_NAME_ALLCONFIG = "ConfigFile/";
        public const string WEBSITE_NAME_CONFIG = "Web.config";
        public const string RECORDS_DEFAULT_INSTALLFOLDER = @"C:\Program Files\AvePoint\RelatedRecords";
        public const string RECORDS_CERFIFICATEFOLDER = "Certificate";
        public const string RECORDS_CERNAME = "AvePoint_Records_Certificate.cer";
        public const string RECORDS_PFXNAME = "AvePoint_Records_Certificate_{0}.pfx";

        public const string RECORDS_TITLE = "Records";
        public const string TEXT_HTTP = "Http";
        public const string TEXT_HTTPS = "Https";

        public const string XPATH_CONFIG_ROOT = "configuration";

        //public const string TEXT_RESTHOST = "RestHost";
        public const string APPID = "338acafb-e53a-44b6-b236-5307af84c4c9";
        public const string APPSECRET = "D+kNAzYyn6yf0T6JAPwJT6hbHzpFfFzm7G2+WSSHtC0=";
        public const string RECORDSPATH = "records";
        public const string HOMEPATH = "home";
        public const string ENTRYPATH = "entry";

        //public const string TIMERPATHFORUPDATE = @"TimerService/AvePoint.Labs.Records.Timer.Service.exe.config";
        public const string APPSTARTPAGE = "/Pages/Default.aspx";

        public class REGEDIT
        {
            public const string LSAKEYNAME = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa";
            public const string DISABLELOOPBACKCHECK = "DisableLoopbackCheck";
            public const string DISABLELOOPBACKCHECKVALUE = "1";
        }
    }
}