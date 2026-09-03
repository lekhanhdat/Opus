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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.Internal.UI;
using AvePoint.Common;
using Microsoft.SharePoint.Administration;
using Microsoft.Office.Server.Administration;
using System.Threading;


namespace AvePoint.ObjectModel.Server13
{

    public class AveConversionPage : IAveConversionPage
    {
        static readonly Guid OfficeServerPremium = new Guid("D5595F62-449B-4061-B0B2-0CBAD410BB51");
        static readonly Guid OfficeServerPremiumTrial = new Guid("88BED06D-8C6B-4E62-AB01-546D6005FE97");
        ConversionPage page = null;
        public AveConversionPage()
        {
            page = new ConversionPage();
            Invoker.CallMethod(page, "IdentifyCurrentInstalledProduct");

        }

        public string CurrentProductLicenseString
        {
            get { return Invoker.CallMethod(page, "GetCurrentProductLicenseString") as String; }
        }

        public bool NeedsConversion
        {
            get { return (Boolean)Invoker.GetProperty(page, "NeedsConversion"); }
        }

        public string ConvertLicenseType(string SharePointKey)
        {
            var page = new ConversionPage();
            Invoker.CallMethod(page, "IdentifyCurrentInstalledProduct");
            var filterKey = SharePointKey.Replace("-", "");
            var verifyString = Invoker.CallMethod(page, "CheckForValidPidKey", filterKey) as String;
            if (String.IsNullOrEmpty(verifyString))
            {
                var jobDefinitionIsTrialInstalledAndConvertTrial = default(SPJobDefinition);
                var jobDefinitiongUpgradeFromStandardToPremium = default(SPJobDefinition);


                var jobDefinitionIsTrialInstalledAndConvertTrialId = default(Guid);
                var jobDefinitiongUpgradeFromStandardToPremiumId = default(Guid);

                bool flagIsTrialInstalledAndConvertTrial = default(Boolean);
                bool flagUpgradeFromStandardToPremium = default(Boolean);

                var installedProduct = (Guid)Invoker.GetProperty(page, "InstalledProduct");
                var convertToProduct = (Guid)Invoker.GetProperty(page, "ConvertToProduct");

                var isTrialInstalled = (Boolean)Invoker.GetProperty(page, "IsTrialInstalled");
                var convertTrial = (Boolean)Invoker.GetProperty(page, "ConvertTrial");
                var upgradeFromStandardToPremium = (Boolean)Invoker.GetProperty(page, "UpgradeFromStandardToPremium");
                var convertOfficeServerPremium = (Boolean)Invoker.GetProperty(page, "ConvertOfficeServerPremium");


                var microsoftOfficeServerAssembly = typeof(Licensing).Assembly;

                if (isTrialInstalled && convertTrial)
                {
                    flagIsTrialInstalledAndConvertTrial = true;
                    jobDefinitionIsTrialInstalledAndConvertTrial = Invoker.CallStaticMethod(
                        microsoftOfficeServerAssembly.GetType("Microsoft.Office.Server.Administration.LicensingConversionJob"),
                        "RegisterToRunNow",
                        installedProduct, convertToProduct) as SPJobDefinition;

                    jobDefinitionIsTrialInstalledAndConvertTrialId = jobDefinitionIsTrialInstalledAndConvertTrial.Id;
                }

                if (upgradeFromStandardToPremium)
                {
                    flagUpgradeFromStandardToPremium = true;
                    var officeServerSkuHelperType = microsoftOfficeServerAssembly.GetType("Microsoft.Office.Server.Administration.OfficeServerSkuHelper");
                    var officeServerSkuHelperObject = Invoker.CreateNewInstance(officeServerSkuHelperType);
                    jobDefinitiongUpgradeFromStandardToPremium = Invoker.CallMethod(officeServerSkuHelperObject, "RegisterSkuUpgradeJob", installedProduct, convertToProduct) as SPJobDefinition;
                    jobDefinitiongUpgradeFromStandardToPremiumId = jobDefinitiongUpgradeFromStandardToPremium.Id;
                }


                var counter = 0;
                var existingJobDefinitionIsTrialInstalledAndConvertTrial = jobDefinitionIsTrialInstalledAndConvertTrial;
                var existingJobDefinitiongUpgradeFromStandardToPremium = jobDefinitiongUpgradeFromStandardToPremium;


                bool flagJobDefinitionIsTrialInstalledAndConvertTrialFailHistory = default(Boolean);
                bool flagJobDefinitiongUpgradeFromStandardToPremiumFailHistory = default(Boolean);

                do
                {
                    Thread.Sleep(0x4e20);
                    if (flagIsTrialInstalledAndConvertTrial && (null != existingJobDefinitionIsTrialInstalledAndConvertTrial))
                    {
                        existingJobDefinitionIsTrialInstalledAndConvertTrial = SPFarm.Local.GetObject(jobDefinitionIsTrialInstalledAndConvertTrialId) as SPJobDefinition;
                        if (null == existingJobDefinitionIsTrialInstalledAndConvertTrial)
                        {
                            foreach (var history in jobDefinitionIsTrialInstalledAndConvertTrial.HistoryEntries)
                            {
                                if (history.Status == SPRunningJobStatus.Failed)
                                {
                                    flagJobDefinitionIsTrialInstalledAndConvertTrialFailHistory = true;
                                }
                            }
                        }
                    }
                    if (flagUpgradeFromStandardToPremium && (null != existingJobDefinitiongUpgradeFromStandardToPremium))
                    {
                        existingJobDefinitiongUpgradeFromStandardToPremium = SPFarm.Local.GetObject(jobDefinitiongUpgradeFromStandardToPremiumId) as SPJobDefinition;
                        if (null == existingJobDefinitiongUpgradeFromStandardToPremium)
                        {
                            foreach (var history in jobDefinitiongUpgradeFromStandardToPremium.HistoryEntries)
                            {
                                if (history.Status == SPRunningJobStatus.Failed)
                                {
                                    flagJobDefinitiongUpgradeFromStandardToPremiumFailHistory = true;
                                }
                            }
                        }
                    }
                    counter++;
                }
                while (((null != existingJobDefinitionIsTrialInstalledAndConvertTrial) || (null != existingJobDefinitiongUpgradeFromStandardToPremium))
                    && (counter < 15));

                if ((flagIsTrialInstalledAndConvertTrial && (null == existingJobDefinitionIsTrialInstalledAndConvertTrial))
                    && !flagJobDefinitionIsTrialInstalledAndConvertTrialFailHistory)
                    return "ServerConversionSuccess";

                if ((flagUpgradeFromStandardToPremium && (null == existingJobDefinitiongUpgradeFromStandardToPremium))
                    && !flagJobDefinitiongUpgradeFromStandardToPremiumFailHistory)
                    return "EnablePremiumFeaturesSuccess";
                else if ((flagUpgradeFromStandardToPremium && (null == existingJobDefinitiongUpgradeFromStandardToPremium))
                    && flagJobDefinitiongUpgradeFromStandardToPremiumFailHistory)
                    return "StandardPremiumConversionFailure";

                if (((flagIsTrialInstalledAndConvertTrial || flagUpgradeFromStandardToPremium) && (!flagIsTrialInstalledAndConvertTrial || ((flagIsTrialInstalledAndConvertTrial && (null == existingJobDefinitionIsTrialInstalledAndConvertTrial)) && !flagJobDefinitionIsTrialInstalledAndConvertTrialFailHistory)))
                    && (!flagUpgradeFromStandardToPremium || ((flagUpgradeFromStandardToPremium && (null == existingJobDefinitiongUpgradeFromStandardToPremium)) && !flagJobDefinitiongUpgradeFromStandardToPremiumFailHistory)))
                {
                    var setupLicensingType = microsoftOfficeServerAssembly.GetType("Microsoft.Office.Server.Administration.SetupLicensing");
                    Invoker.CallStaticMethod(setupLicensingType, "ConvertLicenseStateInFarm", installedProduct, convertToProduct, true);
                    if (convertOfficeServerPremium)
                        Invoker.CallStaticMethod(setupLicensingType, "ConvertLicenseStateInFarm", OfficeServerPremiumTrial, OfficeServerPremium, true);
                }
                return "ServerConversionSuccess";

            }
            else throw new InvalidProductKeyException();

        }
    }
}
