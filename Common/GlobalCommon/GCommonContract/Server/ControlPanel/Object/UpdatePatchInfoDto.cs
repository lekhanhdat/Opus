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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [XmlRoot("UpdatePatchInfoDtos")]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpdatePatchInfoDtos : ISystemSettingContent
    {
        [XmlArrayItem("UpdatePatchInfoDto")]
        [DataMember]
        public List<UpdatePatchInfoDto> PatchInfos { get; set; }

        [XmlElement("RecentCheckTime")]
        [DataMember]
        public long RecentCheckTime { get; set; }

        [XmlElement("RecentInstallTime")]
        [DataMember]
        public long RecentInstallTime { get; set; }

        [XmlElement("DocAveVersion")]
        [DataMember]
        public string DocAveVersion { get; set; }

        [XmlElement("IsConnRemoteService")]
        [DataMember]
        public bool IsConnRemoteService { get; set; }

        [XmlElement("IsMaintenanceExpired")]
        [DataMember]
        public bool IsMaintenanceExpired { get; set; }
    }


    [XmlRoot("UpdatePatchInfoDto")]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpdatePatchInfoDto
    {
        //名字
        [XmlElement("Name")]
        [DataMember]
        public string Name { get; set; }

        //文件名字
        [XmlElement("FileName")]
        [DataMember]
        public string FileName { get; set; }

        //类型
        [XmlElement("Type")]
        [DataMember]
        public PatchType Type { get; set; }

        //状态（没有下载，下载未安装）
        [DataMember]
        public PatchStatus Status { get; set; }

        /// <summary>
        /// 是否可以卸载
        /// </summary>
        [XmlElement("CanUninstall")]
        [DataMember]
        public bool CanUninstall { get; set; }

        //大小
        [XmlElement("Size")]
        [DataMember]
        public long Size { get; set; }

        //描述，默认为英语描述
        [XmlElement("EnglishDescription")]
        [DataMember]
        public string EnglishDescription { get; set; }

        //Release Time
        [XmlElement("ReleaseTime")]
        [DataMember]
        public long ReleaseTime { get; set; }

        //Manager 版本
        [XmlElement("Version")]
        [DataMember]
        public string Version { get; set; }

        //Manager 版本
        [XmlElement("DisplayVersion")]
        [DataMember]
        public string DisplayVersion { get; set; }

        //支持的产品
        [XmlElement("ProductName")]
        [DataMember]
        public SupportProduct ProductName { get; set; }

        //GA+运行所依赖的Control Version
        [XmlElement("DependControlVersion")]
        [DataMember]
        public string DependControlVersion { get; set; }

        //依赖sp的version
        [XmlElement("DependVersion")]
        [DataMember]
        public string DependVersion { get; set; }

        //Manager 地址
        [XmlElement("PatchUrl")]
        [DataMember]
        public string PatchUrl { get; set; }

        //Patch的MD5
        [XmlElement("PatchMD5")]
        [DataMember]
        public string PatchMD5 { get; set; }

        //日语Description
        [XmlElement("JapaneseDescription")]
        [DataMember]
        public string JapaneseDescription { get; set; }

        //德语Description
        [XmlElement("GermanDescription")]
        [DataMember]
        public string GermanDescription { get; set; }

        //下载KEY
        [XmlElement("DownLoadKey")]
        [DataMember]
        public string DownLoadKey { get; set; }

        ////可以应用到的Services，用","分割
        //[XmlElement("ApplyServices")]
        //[DataMember]
        //public string ApplyServices { get; set; }

        [XmlElement("ApplyServices")]
        [DataMember]
        public Dictionary<string, List<ServiceType>> ApplyServices { get; set; }

        [XmlElement("CeipHost")]
        [DataMember]
        public string CeipHost { get; set; }

        [XmlElement("CeipPort")]
        [DataMember]
        public int CeipPort { get; set; }

        public CIInfosOfPatch CIInfosOfThePatch { get; set; }

        [DataMember]
        public int InstallProgress { get; set; }

        [DataMember]
        public InstallStatus InstallStatus { get; set; }

        [DataMember]
        public bool IsBrowsePatch { get; set; }

        [DataMember]
        public bool IsIncludeShell { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is UpdatePatchInfoDto))
            {
                return false;
            }
            UpdatePatchInfoDto otherObj = obj as UpdatePatchInfoDto;
            if (this.Name == null || this.FileName == null || this.Version == null)
            {
                return false;
            }

            return this.Name.Equals(otherObj.Name) && this.FileName.Equals(otherObj.FileName) && this.Version.Equals(otherObj.Version);
        }

        public override int GetHashCode()
        {
            if (Name == null || FileName == null || Version == null)
            {
                return 0;
            }
            return Name.GetHashCode() + FileName.GetHashCode() + Version.GetHashCode();
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PatchVersionDto
    {
        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public string DisplayVersion { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PatchType
    {
        [EnumMember]
        Important,
        [EnumMember]
        Optional,
        [EnumMember]
        Critical
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SupportProduct
    {
        [EnumMember]
        DocAve,
        [EnumMember]
        NetApp,
        [EnumMember]
        IBM,
        [EnumMember]
        GovernanceAutomation
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CIInfosOfPatch
    {
        [DataMember]
        public List<string> IncludeCINames { get; set; }

        [DataMember]
        public List<string> NotIncludeCINames { get; set; }

        [DataMember]
        public List<string> DependCINames { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PatchStatus
    {
        [EnumMember]
        UnDownLoad,
        [EnumMember]
        DownloadAndNotInstall,
        //下个版本中添加这个状态
        //[EnumMember]
        //NeedBrowse,
        [EnumMember]
        Installed
    }

    /// <summary>
    /// 只是用来保存进度的
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InstallMessage
    {
        [DataMember]
        public List<UpdatePatchInfoDto> PatchInfos { get; set; }

        [DataMember]
        public List<ServiceDto> ServiceInfos { get; set; }

        [DataMember]
        public bool IsInstallFinished { get; set; }
        
        //ADO-31278
        [DataMember]
        public bool IsInstallShell { get; set; }
        
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum InstallStatus
    {
        [EnumMember]
        UnInstall,

        [EnumMember]
        Installing,

        [EnumMember]
        UnInstalling,

        [EnumMember]
        InstallSuccess,

        [EnumMember]
        UnInstallSuccess,

        [EnumMember]
        RollBackSuccess,

        [EnumMember]
        UnInstallRollBackSuccess,

        [EnumMember]
        RollBackFaild
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UpdateStatus
    {
        [EnumMember]
        CanInstall,

        [EnumMember]
        Installed,

        [EnumMember]
        VersionError
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceUpdateStatusDto
    {
        [DataMember]
        public string UpdateName { get; set; }

        [DataMember]
        public UpdateStatus Status { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PatchEmailItem 
    {
        [DataMember]
        public string PatchName { set; get; }
        [DataMember]
        public string PatchType { set; get; }
        [DataMember]
        public string PatchSize { set; get; }
        [DataMember]
        public string PatchDescription { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PatchEmailDto 
    {
        [DataMember]
        public string UpdateName { set; get; }
        [DataMember]
        public string Type { set; get; }
        [DataMember]
        public string Size { set; get; }
        [DataMember]
        public string Description { set; get; } 

        [DataMember]
        public string MainBodyDescription { set; get; }
        [DataMember]
        public List<PatchEmailItem> Items { set; get; }
    }

    /// <summary>
    /// 由于存在LoadBalance环境，所以SYSTEMsetting中，不作共享数据
    /// 也就是一个Control建一条记录，而不操作一个记录里的List
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InstallerArgument : ISystemSettingContent
    {
        #region Patch Installer 参数
        /// <summary>
        /// 启动Installer的参数
        /// </summary>
        [DataMember]
        public string Argument { get; set; }

        /// <summary>
        /// Patch的名称，Uninstall使用
        /// </summary>
        [DataMember]
        public string PatchName { get; set; }

        #endregion

        #region Patch Control 参数

        /// <summary>
        /// 是否需要启动Patch Control
        /// </summary>
        [DataMember]
        public bool IsNeedPatchControl { get; set; }
        /// <summary>
        /// List<Installer>序列化XML
        /// </summary>
        [DataMember]
        public string AllInfoXML { get; set; }
        /// <summary>
        /// Control Service序列化XML
        /// </summary>
        [DataMember]
        public string MainControlServiceXML { get; set; }
        /// <summary>
        /// Patch Control Port
        /// </summary>
        [DataMember]
        public int PatchControlPort { get; set; }
        /// <summary>
        /// Patch信息(Uninstall使用)
        /// </summary>
        [DataMember]
        public UpdatePatchInfoDto PatchDto { get; set; }

        #endregion

    }
}
