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
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Server.Common
{
    public class CacheSettingService : IMCacheSettingService
    {
        //public IAgentDao _serviceDao { get; set; }

       // private RALogger logger = RALogger.GetInstance(typeof(CacheSettingService));

        //public List<CacheSettingDto> LoadAllCacheSettings()
        //{
        //    List<CacheSettingDto> result = new List<CacheSettingDto>();
        //    IList<ServiceDto> services = _serviceDao.GetAllAgents();
        //    foreach (ServiceDto dto in services)
        //    {
        //        if (dto.CacheInfo != null && !"".Equals(dto.CacheInfo.Trim()) && dto.Mode == ServiceState.UP && dto.Status == ServiceActive.ACTIVE)
        //        {
        //            result.Add(GetCacheInfo(dto));
        //        }
        //    }
        //    return result;
        //}
        public CacheSettingDto GetBrowserCacheInfo()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CacheLocation");
            CacheSettingDto result = new CacheSettingDto()
            {
                EnableThreshold = true,
                //ServiceName = Dns.GetHostName(),
                Id = null,

                Extension = new CacheSettingExtension()
                {
                    Host = "localhost",
                    Path = new List<PathMap>() { new PathMap() { DiskInfo = new DiskInfoDto() { Id = Guid.NewGuid().ToString(), Path = path, Type = DeviceType.LocalPath }, Index = 1 } },
                    Retention = new int[] { 0, 15 },
                    ServiceType = 4,
                    Threshold = new int[] { 0, 1024 },
                },
                LimitFreeSpace = 0,
            };
            return result;
        }

        public void CleanAllBrowerCacheInfo()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CacheLocation");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        //public int UpdateCacheSetting(CacheSettingDto setting)
        //{
        //    ServiceDto dto = _serviceDao.GetAgent(setting.Id);
        //    dto.CacheInfo = SerializeCacheInfo(setting);
        //    _serviceDao.UpdateAgent(dto);
        //    return 0;
        //}

        //public StorageOpenValidResult GetDriveInfo(string path, ServiceDto dto)
        //{
        //    PhysicalDeviceDto pd = ConvertPhysicalDeviceInfo(dto, path);
        //    //string ss = pd.BuildXRI();
        //    try
        //    {
        //        IXSystem xSystem = XFactory.InstanceSystem(pd.BuildXRI());
        //        StorageOpenValidResult result = xSystem.Open();
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Cannot open the remote device." + dto.Address + "\\" + path, ex);
        //        return null;
        //    }
        //}

        public PhysicalDeviceDto ConvertPhysicalDeviceInfo(ServiceDto dto, string path)
        {
            //IEncryption encryption = EncryptionFactory.GetDefaultEncryption();
            PhysicalDeviceDto pdInfo = new PhysicalDeviceDto();
            string wholePath = "\\\\" + dto.Address + "\\" + path.Replace(":", "$");
            pdInfo = PhysicalDeviceDto.GenterateFS(wholePath, dto.UserName, dto.Password);
            pdInfo.Id = dto.Id;
            return pdInfo;
        }
    }
}
