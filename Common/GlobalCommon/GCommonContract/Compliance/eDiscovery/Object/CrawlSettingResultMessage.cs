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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CrawlSettingResultMessage : EDBaseMessage
    {
        [DataMember]
        public List<SSADto> SSAList { get; set; }
        [DataMember]
        public List<SSAInstallOrUnInstallResult> SSAInstallOrUnInstallResultList { get; set; }
        [DataMember]
        public List<ContentSourceDto> ContentSourceList { get; set; }
        [DataMember]
        public List<DeleteResult> DeleteResultList { get; set; }
        
        [DataMember]
        public bool RemoveIsSuccessful { get; set; }
        [DataMember]
        public List<string> WebAppUrlList { get; set; }
        [DataMember]
        public bool CrawlNotifySuccessful { get; set; }
        
        /// <summary>
        /// 当Create新的content source成功的时候，需要把sharepoint中创建出来的content source的id返回给server
        /// 当edit操作的时候，发现sharepont中该content source已经不存在了，那么创建一个新的content source，并返回给serveri新content source的id
        /// </summary>
        [DataMember]
        public string ContentSourceIdFromAgent { get; set; }

        /// <summary>
        /// 当Edit Content Source时候，发现该content source已经不存在，那么创建一个新的。
        /// 改属性用来通知server产生了一个新的content source
        /// </summary>
        [DataMember]
        public bool NotExistCreateNewContentSource { get; set; }

        /// <summary>
        /// Create Content Source 后返回的状态.
        /// </summary>
        [DataMember]
        public CreateConetntSourcMessage CreateConetntSourceStatus { get; set; }

        /// <summary>
        /// 如果create/edit的时候，发现webapp被其他的content source占用，那么把被占用的web app url添加到这个list中
        /// 供gui前台提示给用户
        /// </summary>
        [DataMember]
        public List<string> ExistWebAppUrl { get; set; }

        public CrawlSettingResultMessage()
        {
            SSAList = new List<SSADto>();
            SSAInstallOrUnInstallResultList = new List<SSAInstallOrUnInstallResult>();
            ContentSourceList = new List<ContentSourceDto>();
            DeleteResultList = new List<DeleteResult>();
            WebAppUrlList = new List<string>();
            ExistWebAppUrl = new List<string>();
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CreateConetntSourcMessage
    {
        [EnumMember]
        Failed = 0,
        [EnumMember]
        WebAppUrlExisted = 1,
        [EnumMember]
        ContentSourceNameExisted = 2,
        [EnumMember]
        Success = 3
    }
}
