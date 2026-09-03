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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FrontEndDeploymentPlanDto : AbstractDMPlanDto
    {
        [DataMember]
        public List<FrontEndDestInfoForGui> FEDestInfo { set; get; }

        [DataMember]
        public FrontEndOptionForGui FEOption { set; get; }

        [DataMember]
        public List<ScheduleDto> ScheduleDtos { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FrontEndDestInfo
    {
        [DataMember]
        public FarmDto DestFarm { set; get; }

        [DataMember]
        public SPTreeNodeDto DestIISTree { get; set; }

        [DataMember]
        [XmlAttribute("enableRollback")]
        public bool EnableRollback { set; get; }
        [DataMember]
        [XmlAttribute("overWriteLevel")]
        public String OverWriteLevel { set; get; }
        [DataMember]
        [XmlAttribute("rollbackAreaSize")]
        public int RollbackAreaSize { set; get; }
        [DataMember]
        [XmlAttribute("rollbackTimes")]
        public int RollbackTimes { set; get; }
        [DataMember]
        [XmlAttribute("rollbackAreaLocation")]
        public String RollbackAreaLocation { set; get; }
        [DataMember]
        [XmlAttribute("rollbackAreaUnit")]
        public String RollbackAreaUnit { set; get; }
        [DataMember]
        [XmlAttribute("isAcceptIIS")]
        public bool IsAcceptIIS { set; get; }
        [DataMember]
        [XmlAttribute("isIISSecurity")]
        public bool isIISSecurity { set; get; }
        [DataMember]
        [XmlAttribute("useNameToCompare")]
        public int UseNameToCompare { set; get; }
        [DataMember]
        [XmlAttribute("isAcceptFileSystem")]
        public bool IsAcceptFileSystem { set; get; }
        [DataMember]
        [XmlAttribute("isAcceptGAC")]
        public bool IsAcceptGAC { set; get; }
        [DataMember]
        [XmlAttribute("isAcceptFeature")]
        public bool IsAcceptFeature { set; get; }
        [DataMember]
        [XmlAttribute("isAcceptSiteDef")]
        public bool IsAcceptSiteDef { set; get; }
        [DataMember]
        [XmlAttribute("isKeepFile")]
        public bool IsKeepFile { set; get; }
        [DataMember]
        [XmlAttribute("isFolderMapping")]
        public bool IsFolderMapping { set; get; }
        [DataMember]
        public List<IISConfigInfo> IISConfigInfo { set; get; }
        //[DataMember]
        //public List<FileSystemMapping> FSMapping { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FrontEndDestInfoForGui : FrontEndDestInfo
    {
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FrontEndOption
    {

        [DataMember]
        [XmlAttribute("restartIIS")]
        public bool RestartIIS { set; get; }
        [DataMember]
        [XmlAttribute("version")]
        public bool Version { set; get; }
        [DataMember]
        [XmlAttribute("locationSetupId")]
        public string LocationSetupId { set; get; }
        [DataMember]
        [XmlAttribute("versionDescription")]
        public string VersionDescription { set; get; }
        [DataMember]
        [XmlAttribute("filterId")]
        public string FilterId { set; get; }
        [DataMember]
        [XmlAttribute("DestFilterId")]
        public string DestFilterId { set; get; }
        /// <summary>
        /// 存储选中节点的level值
        /// </summary>
        [DataMember]
        [XmlAttribute("selectedTreeNodeLevel")]
        public NodeLevel SelectedTreeNodeLevel { get; set; }
        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        [XmlAttribute("conflictResolutionOption")]
        public DPMConflictResolution ConflictResolutionOption { get; set; }
        [DataMember]
        public STSADMCommandInfo STSADMCmdInfo { get; set; }

        [DataMember]
        [XmlAttribute("includeContent")]
        public bool IncludeContent
        { 
            get{return includeContent;}
            set { includeContent = value; }
        }
        private bool includeContent = true;

        [DataMember]
        [XmlAttribute("includeWebConfiguration")]
        public bool IncludeWebConfiguration { get; set; }
        [DataMember]
        [XmlAttribute("webConfigs")]
        public List<string> WebConfigs { get; set; }
        [DataMember]
        [XmlAttribute("includeParentProperties")]
        public bool IncludeParentProperties { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FrontEndOptionForGui : FrontEndOption
    {
        [DataMember]
        public STSADMCommand STSADMCmd { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class STSADMCommand
    {
        [DataMember]
        public string Cmd { get; set; }
        [DataMember]
        public FarmDto Farm { get; set; }
        [DataMember]
        public ServiceDto Agent { get; set; }
    }

    /// <summary>
    /// 这个类用来存储数据哭的
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class STSADMCommandInfo
    {
        [DataMember]
        [XmlAttribute("cmd")]
        public string Cmd { get; set; }

        [DataMember]
        [XmlAttribute("agentId")]
        public string AgentId { get; set; }

        [DataMember]
        [XmlAttribute("farmId")]
        public string FarmId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IISConfigInfo
    {
        [DataMember]
        [XmlAttribute("id")]
        public string Id { get; set; }
        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }
        [DataMember]
        [XmlAttribute("tcpPort")]
        public string TcpPort { get; set; }
        [DataMember]
        [XmlAttribute("description")]
        public string Description { get; set; }
        [DataMember]
        [XmlAttribute("localPath")]
        public string LocalPath { set; get; }
        [DataMember]
        [XmlAttribute("isDeployWebConfig")]
        public bool IsDeployWebConfig { set; get; }
        [DataMember]
        [XmlAttribute("isContainWebConfig")]
        public bool IsContainWebConfig { set; get; }
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class FileSystemMapping
    //{
    //}

    [DataContract]
    public enum FEWPlanType : int
    {
        [EnumMember]
        Deploy = 0,
        [EnumMember]
        Export = 1,
        [EnumMember]
        Import = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FrontEndOptionInfo
    {
        [DataMember]
        public List<FrontEndDestInfo> FEDestInfo { set; get; }

        [DataMember]
        public FrontEndOption FEOption { set; get; }

    }

}
