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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentSourceDto : PlanDto
    {
        [DataMember]
        public const string ID_PREFIX = "ContentSource_";
        [DataMember]
        public SSADto SSADto { get; set; }
        [DataMember]
        public bool IsAvailable { get; set; }
        [DataMember]
        public bool isDeleted { get; set; }
        [DataMember]
        public CrawlingStatus CrawlingStatus { get; set; }

        [DataMember]
        public CrawlingAlert CrawlingAlert { get; set; }

        [DataMember]
        public List<string> urlList { get; set; }

        //        [DataMember]
        //        public string ContentSourceId { get; set; }
        [DataMember]
        public CrawlType CrawlType { get; set; }

        [DataMember]
        public SPTreeNodeDto SPTreeNode { get; set; }

        [DataMember]
        public long UpdateStatusTime { get; set; }
        

        public override bool Equals(object obj)
        {
            ContentSourceDto another = obj as ContentSourceDto;
            if (another == null)
            {
                return false;
            }

            if (this.Id.Equals(another.Id)
                && this.Name.Equals(another.Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


        public static void AddIdPrefix(SSADto ssa, List<ContentSourceDto> csList)
        {

            if (csList == null || csList.Count == 0)
            {
                return;
            }
            foreach (ContentSourceDto cs in csList)
            {
                ContentSourceDto.AddIdPrefix(ssa, cs);
            }

        }


        public static void AddIdPrefix(SSADto ssa, ContentSourceDto cs)
        {
            if (cs == null || ssa == null || String.IsNullOrEmpty(cs.Id) || String.IsNullOrEmpty(ssa.Id))
            {
                return;
            }
            SSADto.RemoveIdPrefix(ssa);

            if (cs.Id.IndexOf("_", StringComparison.Ordinal) == -1)
            {
                cs.Id = ID_PREFIX + ssa.Id + "_" + cs.Id;
            }
            SSADto.AddIdPrefix(ssa);
        }


        public static void RemoveIdPrefix(ContentSourceDto cs)
        {
            if (cs == null || String.IsNullOrEmpty(cs.Id))
            {
                return;
            }
            string id = cs.Id;
            if (cs.Id.IndexOf("_", StringComparison.Ordinal) != -1)
            {
                cs.Id = id.Substring(id.LastIndexOf("_", StringComparison.Ordinal) + 1);
            }
        }

        public static void RemoveIdPrefix(List<ContentSourceDto> csList)
        {
            if (csList == null || csList.Count == 0)
            {
                return;
            }
            foreach (ContentSourceDto cs in csList)
            {
                ContentSourceDto.RemoveIdPrefix(cs);
            }
        }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CrawlingStatus
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Idle = 1,
        [EnumMember]
        Crawling = 2,
        [EnumMember]
        Failed = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CrawlingAlert
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        AgentIsDown = 1,
        [EnumMember]
        NotifyFailed = 2,
        [EnumMember ]
        SSADisabled = 3,
        [EnumMember]
        ContentSourceNotExist = 4,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DeleteResult
    {
        [EnumMember]
        Failed = 0,
        [EnumMember]
        Successful = 1
    }

}
