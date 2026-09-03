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

namespace AvePoint.Common.Portal
{
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.Cloud;
    using Cloud.Sdk.Data.CloudInsights;
    using System;
    using System.Text;

    public class CloudInsightsUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(CloudInsightsUtil));

        public static StorageSettingDto GetStorageSetting(string tenantId)
        {
            StorageSettingDto result = null;
            try
            {
                var apiClient = AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient(GCommonRoleConfiguration.PortalCloudInsightsApiURL, tenantId);
                var model = PortalUtil.Execute(() => apiClient.StorageService.GetStorageSetting());
                if(model != null)
                {
                    result = new StorageSettingDto(model);
                }
            }
            catch (Exception e)
            {
                logger.Error("GetStorageSasUri failed." + e.ToString());
                throw;
            }
            return result;
        }

        public static bool IsEnableRDC(string tenantId)
        {
            //获取AOS中Report Data Collection中是否开启了 Enable data collection和SharePoint Online
            var result = false;
            try
            {
                var apiClient = AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient(GCommonRoleConfiguration.PortalCloudInsightsApiURL, tenantId);
                var settings = PortalUtil.Execute(() => apiClient.SettingsService.GetCollectionSetting());
                result = settings != null && settings.SharePointActivityEnabled && settings.ActivityDataEnabled;
                logger.Info("Get management activity api settings result is " + result);
            }
            catch (Exception e)
            {
                logger.Error("Get management activity Api settings failed." + e.ToString());
            }
            return result;
        }

        public static bool IsEnabled(string tenantId)
        {
            //获取AOS中Report Data Collection中是否开启了 Enable data collection
            var result = false;
            try
            {
                var apiClient = AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient(GCommonRoleConfiguration.PortalCloudInsightsApiURL, tenantId);
                var settings = PortalUtil.Execute(() => apiClient.SettingsService.GetCollectionSetting());
                result = settings != null && settings.ActivityDataEnabled &&(
                    settings.SharePointActivityEnabled || 
                    settings.ExchangeActivityEnabled || 
                    settings.AzureADActivityEnabled ||
                    settings.O365GeneralActivityEnabled );
                logger.Info("Get management activity api settings result is " + result);
            }
            catch (Exception e)
            {
                logger.Error("Get management activity Api settings failed." + e.ToString());
            }
            return result;
        }

        public static bool GetSPAudit(string tenantId)
        {
            try
            {
                var apiClient = AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient(GCommonRoleConfiguration.PortalCloudInsightsApiURL, tenantId);
                var model = PortalUtil.Execute(() => apiClient.SettingsService.GetAdvanceSetting());
                return model == null ? false : model.SaveSPAudit;
            }
            catch (Exception e)
            {
                logger.Error("GetSPAudit failed." + e.ToString());
                throw;
            }
        }
    }

    public class StorageSettingDto
    {
        public TenantStorageType StorageType { get; set; }
        public AmazonS3Dto AmazonS3Dto { get; set; }
        public string AzureStorageSas { get; set; }

        public StorageSettingDto() { }

        public StorageSettingDto(StorageSettingModel model)
        {
            StorageType = (TenantStorageType)model.StorageType;
            if(model.AmazonS3Model != null)
            {
                AmazonS3Dto = new AmazonS3Dto(model.AmazonS3Model);
            }
            AzureStorageSas = model.AzureStorageSas;
        }

        // return true, pass the vialedate the property by type
        public bool VialedatePropertyByStorageType()
        {
            var result = false;
            if(StorageType == TenantStorageType.Default || StorageType == TenantStorageType.AzureStorage)
            {
                result = !string.IsNullOrEmpty(AzureStorageSas);
            }
            else if(StorageType == TenantStorageType.AmazonS3)
            {
                result = !AmazonS3Dto.HasPropertyIsNull();
            }
            return result;
        }
    }

    public class AmazonS3Dto
    {
        public string AccountKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public int Region { get; set; }

        public AmazonS3Dto() { }

        public AmazonS3Dto(AmazonS3Model model)
        {
            AccountKey = model.AccountKey;
            SecretKey = model.SecretKey;
            BucketName = model.BucketName;
            Region = model.Region;
        }

        public bool HasPropertyIsNull()
        {
            return string.IsNullOrEmpty(this.AccountKey) || string.IsNullOrEmpty(this.SecretKey) || string.IsNullOrEmpty(this.BucketName);
        }

        public override string ToString()
        {
            string builder = $"AccountKey: {AccountKey}, SecretKey: {SecretKey}, BucketName: {BucketName}, Region: {Region}";
            return $"connString" + Convert.ToBase64String(Encoding.UTF8.GetBytes(builder));
        }
    }

    public enum TenantStorageType
    {
        Default = 0,
        AzureStorage = 1,
        AmazonS3 = 2
    }
}
