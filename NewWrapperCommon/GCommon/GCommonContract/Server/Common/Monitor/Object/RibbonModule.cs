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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RibbonModule
    {
        /// <summary>
        /// Ribbon唯一标识
        /// </summary>
        [DataMember]
        public RibbonItem name { get; set; }

        /// <summary>
        /// Ribbon状态
        /// </summary>
        [DataMember]
        public int state { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RibbonItem : int
    {
        [EnumMember]
        Delete = 1,
        [EnumMember]
        Download = 2,
        [EnumMember]
        ViewDetail = 3,
        [EnumMember]
        DataPageer = 4,
        [EnumMember]
        Stop = 5,
        [EnumMember]
        Paused = 6,
        [EnumMember]
        Resume = 7,
        [EnumMember]
        Index = 8,
        [EnumMember]
        Rollback = 9,
        [EnumMember]
        DateRange = 10,
        [EnumMember]
        DeleteTreeContent = 11,
        [EnumMember]
        Start = 12,
        [EnumMember]
        Restart = 13,
        [EnumMember]
        Mapping = 14,
        [EnumMember]
        CopySnapshot = 15,
        [EnumMember]
        ExportToExcel = 16,
        [EnumMember]
        MainDelete = 17,
        [EnumMember]
        DownloadSearchResults = 20,
        [EnumMember]
        DeadAccountDeletion = 21,
        [EnumMember]
        SearchResult = 22,
        [EnumMember]
        RollbackChanges = 23,
        [EnumMember]
        Maintenance = 24,
        [EnumMember]
        ViewMappings = 25,
        [EnumMember]
        ViewBlob = 26,
        [EnumMember]
        OrphanSitesDeletion = 27,
        [EnumMember]
        ViewItemLife = 28,
        [EnumMember]
        ViewListAccess = 29,
        [EnumMember]
        ViewListDeletion = 30,
        [EnumMember]
        ViewSiteAccess = 31,
        [EnumMember]
        ViewUserLife = 32,
        [EnumMember]
        WebPartManagement = 33,
        [EnumMember]
        ViewWorkFlowStatusReport = 34,
        [EnumMember]
        ViewCustomizedReport = 35,
        [EnumMember]
        ViewUserPermission = 36,
        [EnumMember]
        DuplicateFileTool = 37,
        [EnumMember]
        ViewBestPractice = 38,
        [EnumMember]
        Resync = 48,
        [EnumMember]
        DeleteJobAndData = 111,
    }

    /// <summary>
    /// 这个枚举的值对应MonitorRibbonItem类中的ControlType属性
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RibbonControlType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        JobCount = 1,  //根据job的选择个数对Ribbon进行控制
        [EnumMember]
        Inverse = 2,    //反选情况下对ribbon的控制
        [EnumMember]
        IsSelect = 3,  //正选情况下对ribbon的控制

    }

    /// <summary>
    /// 这个枚举中的值对应MonitorRibbonItem类中的OperateType属性
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OperateType
    {
        [EnumMember]
        Pessimistic = 0,   //悲观状态，表示只要有一个Job的状态导致Ribbon不可用，则Ribbon就是不可用状态。
        [EnumMember]
        Optimistic = 1    //乐观状态，表示只要有一个Ribbon的状态为可用的，则将Ribbon设置为可用状态。
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("JobMonitorRibbon")]
    public class JobMonitorRibbon
    {
        private List<JobRibbonModule> ribbonModules;

        [DataMember]
        [XmlArray(ElementName = "RibbonModules")]
        public List<JobRibbonModule> RibbonModules 
        {
            get
            {
                if (ribbonModules == null)
                {
                    ribbonModules = new List<JobRibbonModule>();
                }
                return ribbonModules;
            }
            set
            {
                ribbonModules = value;
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("JobRibbonModule")]
    public class JobRibbonModule
    {
        private List<MonitorRibbonItem> ribbons;
        [DataMember]
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [DataMember]
        [XmlAttribute("Type")]
        public int Type { get; set; }
        [DataMember]
        [XmlArray(ElementName = "Ribbons")]
        public List<MonitorRibbonItem> Ribbons 
        {
            get
            {
                if (ribbons == null)
                {
                    ribbons = new List<MonitorRibbonItem>();
                }
                return ribbons;
            }
            set
            {
                ribbons = value;
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("MonitorRibbonItem")]
    public class MonitorRibbonItem
    {
        [DataMember]
        [XmlAttribute("Name")]
        public string Name { get; set; }  //Ribbon的名称
        [DataMember]
        [XmlAttribute("Id")]
        public string Id { get; set; }  //Ribbon在页面上的Id
        [DataMember]
        [XmlAttribute("IsDisabled")]
        public bool IsDisabled { get; set; }  //初始化是否可用
        [DataMember]
        [XmlAttribute("IsDisplay")]
        public bool IsDisplay { get; set; }  //初始化过程中是否需要显示
        [DataMember]
        [XmlAttribute("Type")]
        public int Type { set; get; }  //代表Ribbon的类型
        [DataMember]
        [XmlAttribute("JobStates")]
        public string JobStates { set; get; }  //代表所支持的Job状态
        [DataMember]
        [XmlAttribute("ControlType")]
        public string ControlType { set; get; }  //代表是否支持全选或者反选的操作
        [DataMember]
        [XmlAttribute("OperateType")]
        public string OperateType { set; get; }  //代表悲观操作还是，乐观操作
    }
}
