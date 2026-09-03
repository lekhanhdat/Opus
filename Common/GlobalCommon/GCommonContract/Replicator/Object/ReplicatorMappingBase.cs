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



namespace AvePoint.GCommon.Contract.Replicator.Object
{
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Tree.Object;

    [KnownType(typeof(ReplicatorOnlineMapping))]
    [KnownType(typeof(ReplicatorImportMapping))]
    [KnownType(typeof(ReplicatorExportMapping))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public abstract class ReplicatorMappingBase
    {
        /// <summary>
        /// 后台更新mapping时使用，防止load出来后id丢失。
        /// </summary>
        [DataMember]
        public string ContentId { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public bool Enable { get; set; }

        [DataMember]
        public DateTime LastModifyTime { get; set; }

        [DataMember]
        public DateTime CreateTime { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public string SrcPath { get; set; }

        [DataMember]
        public string DestPath { get; set; }

        [DataMember]
        public string MappingProfileId { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public int SPVersion { get; set; }

        public abstract ReplicatorMappingType MappingType { get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorOnlineMapping : ReplicatorMappingBase
    {
        [DataMember]
        public NodeLevel SelectedNodeLevel { get; set; }
        [DataMember]
        public bool EnableRealTime { get; set; }

        [DataMember]
        public FarmDto SrcFarm { get; set; }

        [DataMember]
        public FarmDto DestFarm { get; set; }

        [DataMember]
        public SPTreeNodeDto SrcItems { get; set; }

        [DataMember]
        public SPTreeNodeDto DestItems { get; set; }

        [DataMember]
        public string SrcAgentGroupId { get; set; }

        [DataMember]
        public string DestAgentGroupId { get; set; }

        [DataMember]
        public ReplicatorDirection Direction { get; set; }

        [DataMember]
        public ReplicationEvent EventHandlerTypes { get; set; }

        [DataMember]
        public string SrcBackupPlanId { get; set; }

        [DataMember]
        public string DestBackupPlanId { get; set; }

        public override ReplicatorMappingType MappingType
        {
            get { return ReplicatorMappingType.Online; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorImportMapping : ReplicatorMappingBase
    {
        [DataMember]
        public string DestAgentGroupId { get; set; }

        [DataMember]
        public SPTreeNodeDto DestItems { get; set; }

        [DataMember]
        public FarmDto DestFarm { get; set; }

        [DataMember]
        public FSTreeNodeDto ImportTree { get; set; }

        [DataMember]
        public SPTreeNodeDto ImportDetailTree { get; set; }

        [DataMember]
        public string DestBackupPlanId { get; set; }

        public override ReplicatorMappingType MappingType
        {
            get { return ReplicatorMappingType.Import; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorExportMapping : ReplicatorMappingBase
    {
        [DataMember]
        public FarmDto SrcFarm { get; set; }

        [DataMember]
        public SPTreeNodeDto SrcItems { get; set; }

        [DataMember]
        public string SrcAgentGroupId { get; set; }

        [DataMember]
        public string DestLocationId { get; set; }

        public override ReplicatorMappingType MappingType
        {
            get { return ReplicatorMappingType.Export; }
        }
    }

    public class ReplicatorMappingPathHandler
    {
        /// <summary>
        /// 该方法作用：除去Mapping SourcePath或DestPath中的"Root Folder/"
        /// </summary>
        /// <param name="treeNodeDto"></param>
        /// <returns></returns>
        public static string GenerateMappingFullPath(SPTreeNodeDto treeNodeDto)
        {
            SPTreeNodeDto tempTreeNode = treeNodeDto;
            string selTempPath = string.Empty;
            while (true)
            {
                switch (tempTreeNode.Level)
                {
                    case NodeLevel.Root:
                    case NodeLevel.Farm:
                    case NodeLevel.Folders:
                    case NodeLevel.RootFolder:
                    case NodeLevel.Lists:
                        break;
                    case NodeLevel.WebApplication:
                    case NodeLevel.SiteCollection:
                        {
                            selTempPath = tempTreeNode.FullPath;
                        }
                        break;
                    case NodeLevel.Site:
                        {
                            selTempPath = tempTreeNode.FullPath;
                            if (tempTreeNode.Name.Equals("."))
                            {
                                selTempPath = selTempPath + "/.";
                            }
                        }
                        break;
                    case NodeLevel.Library:
                    case NodeLevel.List:
                        {
                            selTempPath += "/" + tempTreeNode.Name;
                        }
                        break;
                    default:
                        {
                            selTempPath += "/" + tempTreeNode.Name;//name是folder的名字                        
                        }
                        break;
                }
                if (tempTreeNode.CheckNumber == 1)
                {
                    break;
                }
                tempTreeNode = tempTreeNode.Children[0];                
            }
            return selTempPath;
        }
    }
}
