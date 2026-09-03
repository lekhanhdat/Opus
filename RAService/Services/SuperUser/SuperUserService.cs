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
using AvePoint.Common.Portal;
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.RMReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SuperUser
{
    public class SuperUserService : RMServiceBase, ISuperUserService
    {
        private IRMMiscProfileDao MiscProfileDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMDatabaseEncryption defaultEncryption=new RMDatabaseAESEncryption();
        private RALogger logger = RALogger.GetInstance(typeof(RMReportService));
        public bool CheckConfigurationProfileExist(SuperUserConfigurationDto dto)
        {
            try
            {
                RMMiscProfile mIDD = new RMMiscProfile() { Name =dto.TenantName,Type= (int)ProfileType.SuperUserConfiguration };
                RMMiscProfile profile = MiscProfileDao.Load(mIDD);
                return profile != null;
            }
            catch (Exception ex)
            {
                logger.Error("Check Configuration Profile Exist Error, error message: {0}", ex.Message.ToString());
                throw;
            }
        }

        public int CreateConfigurationProfile(SuperUserConfigurationDto dto)
        {
            int creatStatus = (int)CreateOrEditStatus.Success;
            try
            {
                logger.Debug("Begin Create Configuration Profile, name is {0}", dto.TenantName);
                RMMiscProfile profile = AssembleProfileDto(dto);
                MiscProfileDao.Create(profile);
                return creatStatus;
            }
            catch (Exception ex)
            {
                logger.Error("Create Configuration Profile Error, error message: {0}", ex.Message.ToString());
                creatStatus = (int)CreateOrEditStatus.Failed;
                return creatStatus;
            }
        }

        public SuperUserResult GetAllTenantInfo(SuperUserResult info)
        {
            try
            {
                List<SuperUserConfigurationDto> dtos = new List<SuperUserConfigurationDto>();
                var siteCollections = RMRemoteNodeDao.GetAuthorisedAllSites();
                logger.Debug("Authorised Remote SiteCollections Count: {0}", siteCollections.Count);
                dtos=GetSuperUserConfigurationsBySiteUrls(siteCollections);
                return GetProfiles(info, dtos);

            }
            catch (Exception ex)
            {
                logger.Error("Get All Tenant Info Error, error message is: {0}", ex.Message.ToString());
                throw ex;
            }
        }
        private SuperUserResult GetProfiles(SuperUserResult pageInfo, List<SuperUserConfigurationDto> dtos)
        {

            int totalRecord = 0;
            List<SuperUserConfigurationDto> profiles = GetProfiles(pageInfo.PageIndex, pageInfo.PageSize, dtos, out totalRecord, "LastModifiedTime", pageInfo.IsDesc, pageInfo.SearchValue);
            foreach (var pro in profiles)
            {
                pageInfo.SuperUserConfigurationDtoList.Add(pro);
            }
            pageInfo.TotalNumber = totalRecord;
            return pageInfo;
        }
        private List<SuperUserConfigurationDto> GetProfiles(int pageIndex, int pageSize, List<SuperUserConfigurationDto> dtos, out int totalRecord, string orderKey, bool isAsc, string serchValue)
        {
            try
            {
                List<SuperUserConfigurationDto> temp = new List<SuperUserConfigurationDto>();
                if (!string.IsNullOrEmpty(serchValue))
                {
                    foreach (var dto in dtos)
                    {
                        if (dto.TenantName.Contains(serchValue))
                        {
                            temp.Add(dto);
                        }
                    }
                }
                else
                {
                    temp = dtos;
                }
                totalRecord = temp.Count();
                return temp.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private List<SuperUserConfigurationDto> GetSuperUserConfigurationsBySiteUrls(List<TreeNodeCollection> siteCollections)
        {
            try
            {
                Dictionary<string, string> nameUrl = new Dictionary<string, string>();
                foreach (var site in siteCollections)
                {
                    logger.Debug("SiteCollection: {0}", site.Scope);
                    string name = site.Scope.Split('/')[2].Split('.')[0].Split('-')[0];
                    if (name != null && name != string.Empty)
                    {
                        nameUrl[name] = site.TenantId;
                    }
                }
                logger.Debug("Assemble Super User Configuration By Site Url, count: {0}", nameUrl.Count);
                List<SuperUserConfigurationDto> result = new List<SuperUserConfigurationDto>();
                foreach (var item in nameUrl)
                {
                    string tenantName = item.Key;
                    if (!result.Exists(r => r.TenantName == tenantName))
                    {
                        SuperUserConfigurationDto dto = new SuperUserConfigurationDto();
                        logger.Debug("Get Profile By Profile Name, name: {0}", tenantName);
                        RMMiscProfile mIDD = new RMMiscProfile() { Name = tenantName, Type = (int)ProfileType.SuperUserConfiguration };
                        RMMiscProfile profile = MiscProfileDao.Load(mIDD);
                        
                        if (profile != null)
                        {
                            var extension = SerializerHelper.DeserializeByDataContractSerializer<SuperUserConfigurationDto>(profile.Extension);
                            dto.TenantId = extension.TenantId;
                            dto.TenantName = extension.TenantName;
                            dto.AppPrincipalId = extension.AppPrincipalId;
                            dto.Key = extension.Key;
                        }
                        else
                        {
                            logger.Debug("Cannot Get Profile By Profile Name, name: {0}", tenantName);
                            dto.TenantId = item.Value;
                            dto.TenantName = tenantName;
                            dto.AppPrincipalId = string.Empty;
                            dto.Key = string.Empty;
                        }
                        result.Add(dto);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Get Super User Configurations By TenantIds Error, error message: {0}", ex.Message.ToString());
                throw ex;
            }
        }
        public async Task<int> UpdateConfigurationProfileAsync(SuperUserConfigurationDto dto)
        {
            int creatStatus = (int)CreateOrEditStatus.Success;
            try
            {
                logger.Debug("Begin Update Configuration Profile, name is {0}", dto.TenantName);
                RMMiscProfile mIDD = new RMMiscProfile() { Name = dto.TenantName, Type = (int)ProfileType.SuperUserConfiguration };
                RMMiscProfile profile = MiscProfileDao.Load(mIDD);
                RMMiscProfile tempDto = new RMMiscProfile();
                if (profile != null)
                {
                    tempDto = profile;
                    var oldKey = SerializerHelper.DeserializeByDataContractSerializer<SuperUserConfigurationDto>(tempDto.Extension).Key;
                    if (!dto.Key.Equals(oldKey))
                    {
                        dto.Key = EncryptToPasswordDtoXmlString(dto.Key);
                    }
                    tempDto.Extension = SerializerHelper.SerializeByDataContractSerializer(dto);
                    await MiscProfileDao.UpdateAsync(profile);
                }
                return creatStatus;
            }
            catch (Exception ex)
            {
                logger.Error("Update Configuration Profile Error, error message: {0}", ex.Message.ToString());
                creatStatus= (int)CreateOrEditStatus.Failed;
                return creatStatus;
            }
        }
        private string EncryptToPasswordDtoXmlString(string data)
        {
            if (data != null)
            {
                try
                {
                    return PortalUtil.Encrypt(data,TenantLocalValue.LogonGroupId);
                }
                catch (Exception ex)
                {
                    logger.Warn("Encrypt by aos sdk failed,try encrypt by old encryption provider.");
                    logger.Error(ex.ToString());
                    return defaultEncryption.EncryptPasswordDtoToXmlString(CryptoUtil.ConvertStringToBytes(data));
                }
            }
            return null;
        }
        private RMMiscProfile AssembleProfileDto(SuperUserConfigurationDto dto)
        {
            dto.Key = EncryptToPasswordDtoXmlString(dto.Key);
            return new RMMiscProfile()
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.TenantName,
                Type = (int)ProfileType.SuperUserConfiguration,
                Extension = SerializerHelper.SerializeByDataContractSerializer(dto)
            };
        }
    }
}
