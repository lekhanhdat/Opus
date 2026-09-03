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


namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    using System.Text;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using GCommon.Contract.Server.ControlPanel.Cryptography;
    using Storage;
    #endregion

    public class IndexCacheManagerParameter
    {
        public String IndexName { get; set; }
        public String StorageInfo { get; set; }
        public String IndexVolume { get; set; }
        public Boolean NeedDownLoad { get; set; }
        public Boolean NeedRenameIndexName { get; set; }
        public IXSystem CacheSystem { get; set; }
        public IXSystem StorageSystem { get; set; }
        public CacheSettingDto CacheSetting { get; set; }
        public byte DataMode { get; set; }
        public DataEncryptionInfo EncryptionInfo { get; set; }
        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("IndexCacheManagerParameter: StorageInfo:");
            sb.Append(this.StorageInfo);
            sb.Append(" IndexVolume: ");
            sb.Append(this.IndexVolume);
            sb.Append(" IndexName: ");
            sb.Append(this.IndexName);
            sb.Append(" CacheSetting Id: ");
            sb.Append(this.CacheSetting.Id);
            sb.Append(" Name: ");
            sb.Append(this.CacheSetting.ServiceName);
            sb.Append(" NeedDownLoad: ");
            sb.Append(this.NeedDownLoad);
            return sb.ToString();
        }
    }
}