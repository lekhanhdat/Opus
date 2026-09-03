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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService
{
    public class Constants
    {
        //public string 
        public static string DefaultLogPath = "";
        public static string RecordsBrowserExe = "RecordsAgentBrowser.exe";
        public static readonly string RecordsWorkerExe = "RecordsAgentWorker.exe";

        //registry key
        /// <summary>
        /// Everytime the product id is changed, it should be added to this list.
        /// </summary>
        public static List<string> PackageIds => new List<string>()
        {
            "{C349C41A-87AE-4155-9431-FD2B559FF23E}",
            "{EE780EAB-9923-49B0-A080-181A1DBD0E6C}",
            "{0C16D78A-20C8-4F5A-A558-7CB6E49CA3B6}",
            "{2F470578-5AD8-420C-85D3-5C3424756C2A}",
            "{83E9AA50-2A25-4A9F-BAC0-36D75961E46F}",
            "{AFCD33F0-D6C1-45EC-912C-0A32C954B0F9}",
            "{DF5D64B0-C0C8-99C3-8650-031A7B9ADE3A}",
            "{C03C109A-B679-4CFB-9E0C-C849DEB8A9CA}",
            "{EB827043-A51B-475B-8AE6-C0E7A0A46520}",
            "{6FA0CE2E-51D0-4EAA-B09E-A11802E38F94}",
            "{5D58EC0E-8F86-40E3-91CB-DCBEE8B3249A}",
            "{A3F4E8B1-2C5D-4E6F-9A0B-1C2D3E4F5A6B}",
            "{BB30AFE3-5DA2-4AA0-B401-A983E4DD18BF}",
            "{C079F427-130D-451F-9148-B0B59965FF47}",
            "{3F6A9C2E-8D41-4B5F-9A72-1E6C0D8F4B91}",
            "{3F2504E0-4F89-11D3-9A0C-0305E82C3301}",
            "{A3F7C9D2-8B41-4E6A-9F0D-1C2E7B5A8D93}",
            "{21058273-16C6-48F9-B141-213E7BF44700}",
            "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"
        };
        //public const string PackageId = "{AFCD33F0-D6C1-45EC-912C-0A32C954B0F9}";
        //public const string OldPackageId = "{DF5D64B0-C0C8-99C3-8650-031A7B9ADE3A}";
        public const string RegistryUninstall = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        public const string RegistryDisplayVersion = @"DisplayVersion";

        public class AgentConfigurationFileName
        {
            public static readonly string AgentConfigFile_VCEnvConfig = "AgentCommonVCEnv.config";
            public static readonly string AgentConfigFile_ServiceVersionConfig = "ServiceVersion.config";
            public static readonly string AgentCommonOffice365LanguageMappingFile = @"\data\WrapperCommon\AgentCommonOffice365WrapperLanguageMapping.xml";
            public static readonly string AgentImportTreeFile = "AgentImportTree.xml";
            public static readonly string AgentConfigFile_Log4netConfig = "AgentLog4net.config";
            public static readonly string AgentConfigFile_AgentCommonIocConfigurations = "AgentCommonIocConfigurations.config";
            public static readonly string AgentConfigFile_AgentCommonIocPropertiesConfigurations = "AgentCommonIocPropertiesConfigurations.config";
        }

        public class AgentBinaryName
        {
         
            public static readonly string COMMON_GET_FARM_ID_EXE_NAME = "AgentCommonGetFarmID.exe";
            public static readonly string COMMON_ROLECHECKER_2013 = "AgentCommonSPRoleChecker.exe";//SP2013 &SP2016 use same one
            public static readonly string COMMON_BROWSER_NAME = "AgentCommonBrowser";

            public static readonly string COMMON_AUTOSCAN_NAME = "AgentCommonAutoScan.exe";
            public static readonly string COMMON_APIUtility_Name = "AgentCommonAPIUtility";
            public static readonly string MIGRATION_BROWSER_NAME = "AgentCommonMigrationBrowser";

            #region Health Analyzer
            public static readonly string HealthAnalyzer_SP2010HEALTHANALYZER_EXE_NAME = "SP2010HealthAnalyzer.exe";
            public static readonly string HealthAnalyzer_SP2013HEALTHANALYZER_EXE_NAME = "AgentCommonHealthAnalyzer.exe";
            #endregion

            #region
            public static readonly string AccountChangedFlagFile = "CurrentAccount.dat";
            #endregion

        }

    }



}
