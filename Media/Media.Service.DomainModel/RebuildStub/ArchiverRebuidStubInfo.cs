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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Media.Service.DomainModel
{
    public class ArchiverRebuildStubInfo
        : RetentionInfoBase
        , IRebuildStubInfo
    {
        public String FarmName { get; set; }
        public String WebApp { get; set; }
        public String RebuildJobId { get; set; }
        public String JobId { get; set; }
        public String SiteUrl { get; set; }
        public String IndexVolume { get; set; }
        public LogicalDeviceDto IndexLogicalDevice { get; set; }
        public CacheSettingDto CacheSetting { get; set; }
        public string TenantGroupId { get; set; }
        public String MainIndexStorageInfo { get; set; }
        public String StubTemplateName { get; set; }
        public bool KeepStubModifiedAndModifiedBy { get; set; }
        public AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto StubSettingDto { get; set; }

        public ArchiverRebuildStubInfo() { }

        public ArchiverRebuildStubInfo(AvePoint.RA.DB.Model.ArchiverSiteMasterIndex index,
            LogicalDeviceDto indexDeviceDto,
            CacheSettingDto cacheSettingDto,
            RebuildStubInfo rebuildStubInfo,
            AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto stubSettingDto
            )
        {
            this.FarmName = string.Empty;
            this.WebApp = string.Empty;
            this.JobId = index.JobId;
            this.SiteUrl = index.SiteURL;
            this.IndexLogicalDevice = indexDeviceDto;
            this.CacheSetting = cacheSettingDto;
            this.TenantGroupId = TenantLocalValue.LogonGroupId;
            this.MainIndexStorageInfo = index.StorageInfo;
            this.RebuildJobId = rebuildStubInfo.RebuildJobId;
            this.StubTemplateName = rebuildStubInfo.StubTemplateName;
            this.StubSettingDto = stubSettingDto;
            var volumeGenerator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            var volumeParam = new VolumeParameter(this);
            this.IndexVolume = volumeGenerator.GenerateIndexVolume(volumeParam);
            KeepStubModifiedAndModifiedBy = rebuildStubInfo.KeepStubModifiedAndModifiedBy;
        }
    }
}
