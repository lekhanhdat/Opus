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
using AvePoint.GCommon.Contract.Storage.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Service.Services.RMReport;
using Storage;
using Media.Common.ClassicStorageApi;
using System.Collections.Immutable;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;

namespace AvePoint.RA.Service.Services.StorageDevice
{
    public class StorageManagerUtil
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RMReportService));

        private static ImmutableDictionary<string, string> regionMapping = ImmutableDictionary.CreateRange(new KeyValuePair<string, string>[]
        {
            KeyValuePair.Create("US Gov Virginia","usgovvirginia"),
            KeyValuePair.Create("A2G3Prod AOS (Gov Virginia)","usgovvirginia"),
            KeyValuePair.Create("Switzerland North (Zurich)","switzerlandn"),
            KeyValuePair.Create("Germany West Central (Frankfurt)","germanywc"),
            KeyValuePair.Create("China North", "chinanorth" ),
            KeyValuePair.Create("APAC - Singapore","southeastasia"),
            KeyValuePair.Create("APAC - Australia","australiasoutheast"),
            KeyValuePair.Create("US - East","eastus"),
            KeyValuePair.Create("France Central (Paris)","centralfrance"),
            KeyValuePair.Create("UK South (London)","uksouth"),
            KeyValuePair.Create("EMEA - Netherlands","westeurope"),
            KeyValuePair.Create("EMEA - Ireland","northeurope"),
            KeyValuePair.Create("Japan West - Osaka","japanwest"),
            KeyValuePair.Create("Canada Central (Toronto)","canadacentral"),
            KeyValuePair.Create("Korea Central (Seoul)","koreacentral"),
            KeyValuePair.Create("Central India (Pune)","centralindia"),
            KeyValuePair.Create("East US 2 (Virginia)","eastus2" ),
            KeyValuePair.Create("Australia East (New South Wales)","australiasoutheast" ),
            KeyValuePair.Create("GAOnlineTest Trunk(Beijing)","southeastasia"),
            KeyValuePair.Create("AOSBR DEV","southeastasia"),
            //opus eck中找到的
            KeyValuePair.Create("RecordsCITest","southeastasia"),
            KeyValuePair.Create("US-East","eastus"),

            //开发环境
            KeyValuePair.Create("RevIM","southeastasia"),
            KeyValuePair.Create("RevIM_3","southeastasia")

                        //{ "DEV_3_31","asiaeast" },
        });

        private static ImmutableDictionary<string, string> azureRegionIpPageUrlMapping = ImmutableDictionary.CreateRange(new KeyValuePair<string, string>[]
        {
            KeyValuePair.Create("production","https://www.microsoft.com/en-gb/download/details.aspx?id=56519"),
            KeyValuePair.Create("test","https://www.microsoft.com/en-gb/download/details.aspx?id=56519"),
            KeyValuePair.Create("21V China North","https://www.microsoft.com/en-sg/download/details.aspx?id=57062"),
            KeyValuePair.Create("US Gov Virginia","https://www.microsoft.com/en-us/download/details.aspx?id=57063"),
            KeyValuePair.Create("A2G3 AOS (Gov Virginia)","https://www.microsoft.com/en-us/download/details.aspx?id=57063"),
            KeyValuePair.Create("A2G3Prod AOS (Gov Virginia)","https://www.microsoft.com/en-us/download/details.aspx?id=57063"),
            //opus  eck 中找到的
            KeyValuePair.Create("Gov Virginia","https://www.microsoft.com/en-us/download/details.aspx?id=57063"),
            //开发环境
            KeyValuePair.Create("dev","https://www.microsoft.com/en-gb/download/details.aspx?id=56519"),
            KeyValuePair.Create("GCP", "https://www.microsoft.com/en-gb/download/details.aspx?id=56519"),
            KeyValuePair.Create("GCP Test", "https://www.microsoft.com/en-gb/download/details.aspx?id=56519")
        });

        public static StorageOpenValidResult ValidateDeviceWithCustomRetry(StorageDeviceDto dto)
        {
            StorageOpenValidResult result = new StorageOpenValidResult() { SystemHealth = XSystemHealth.AvailableAndNotFull };
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.ConnectionString))
                {
                    result.SystemHealth = XSystemHealth.Unaccessable;
                    logger.Info("The device connetion string is null.");
                    return result;
                }
                SetDeviceValidationRetryCount(dto);
                string str = dto.BuildXRI();

                IXSystem xSystem = XFactoryCommon.InstanceSystem(str);
                xSystem.Open();
                result = xSystem.Validate();
                RemoveDeviceValidationRetryCount(dto);
            }
            catch (Exception ex)
            {
                RemoveDeviceValidationRetryCount(dto);
                result = new StorageOpenValidResult() { SystemHealth = XSystemHealth.Unknown };
                logger.Error("Validate device with custom retry error: {0}.", ex.ToString());
            }
            return result;
        }
        public static void RemoveDeviceValidationRetryCount(StorageDeviceDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.ConnectionString))
            {
                logger.Info("The physical device connection string is null.");
                return;
            }
            string id = dto.Id;
            dto.Id = null;
            //string xriString = dto.BuildValidateXRI();
            var xri = ConnectionBuilder.ValueOf(dto.ConnectionString);
            if (xri.Params.ContainsKey(XRIParameterKeys.RETRY_COUNT))
            {
                xri.Params.Remove(XRIParameterKeys.RETRY_COUNT);
            }
            dto.ConnectionString = xri.ToString();
            dto.Id = id;
            logger.Info("Remove retry count from physical device: {0}.", dto.Name);
        }
        public static void SetDeviceValidationRetryCount(StorageDeviceDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.ConnectionString))
            {
                logger.Info("The physical device connection string is null.");
                return;
            }
            string id = dto.Id;
            dto.Id = null;
            //string xriString = dto.BuildValidateXRI();
            var xri = ConnectionBuilder.ValueOf(dto.ConnectionString);
            xri.Params[XRIParameterKeys.RETRY_COUNT] = "2";
            dto.ConnectionString = xri.ToString();
            dto.Id = id;
            logger.Info("Set retry count as 2 to physical device: {0}.", dto.Name);
        }

        public static string GetAzureRegionOfDataCenter()
        {
            string result = string.Empty;
            regionMapping.TryGetValue(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER], out result);
            return result;
        }

        public static string GetDownloadAzureIpRangesPageUrlByEnviroment(string enviroment)
        {
            foreach(var item in azureRegionIpPageUrlMapping)
            {
                if (item.Key.Equals(enviroment, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Value;
                }
            }
            return string.Empty;
        }



    }

}
