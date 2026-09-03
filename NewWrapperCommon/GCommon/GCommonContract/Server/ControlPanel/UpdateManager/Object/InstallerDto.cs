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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager.Object
{
    /// <summary>
    /// 用来控制各个Installer状态的DTO
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InstallerDto 
    {
        ////安装的服务
        //[DataMember]
        //public Service4PatchDto Service { get; set; }

        ////安装的状态
        //[DataMember]
        //public InstallerStatus Status { get; set; }

        ////需要安装Patch的集合
        //[DataMember]
        //public List<UpdatePatch4PatchDto> InstallPatchs { get; set; }

        ////正在安装的Patch
        ////public UpdatePatch4PatchDto InstallingPatch { get; set; }

        ////正在安装Patch的索引
        //[DataMember]
        //public int InstallIndex { get; set; }

        //[DataMember]
        //public bool IsInstallOver { get; set; }
        ////超时的时间，分钟
        //[DataMember]
        //public long BeginTime { get; set; }

        [DataMember]
        public UpdatePatchInfoDto InstallPatch { get; set; }

        [DataMember]
        public List<ServiceDto> InstallServices { get; set; }

        /// <summary>
        /// en、ja、fr、de
        /// </summary>
        [DataMember]
        public string LanguageString { get; set; }

    }
}
