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
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;

namespace AvePoint.GCommon.Contract.SharePointBrowser
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NetSharePathContract : BrowserContractBase
    {
        #region Get From Server
        /// <summary>
        /// Deployment Manager Dash Board功能，用于将Id分配给相应ExportLocationDto的标示
        /// </summary>
        [DataMember]
        public string LocationId { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string Domain { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string EncryptedPassword { get; set; }
        [DataMember]
        public PathType PathType { get; set; }
        [DataMember]
        public string MediaStorageXri { get; set; }
        [DataMember]
        public string RelativePath { get; set; }
        [DataMember]
        public ExtensionAction ExtensionAction { get; set; }
        #endregion

        #region Return To Server
        [DataMember]
        public int ReturnCode { get; set; }    //0 is successful, otherwise are failed
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public NetLocationExist LocationExist { get; set; }//check if location is exist default is unknown
        /// <summary>
        /// 每个模块所用的空间
        /// </summary>
        [DataMember]
        public ulong CategoryUseSpace { get; set; }

        /// <summary>
        /// 其他应用空间
        /// </summary>
        [DataMember]
        public ulong OtherUseSpace { get; set; }

        /// <summary>
        /// 剩余空间
        /// </summary>
        [DataMember]
        public ulong RemainingSpace { get; set; }

        [DataMember]
        public string Language { get; set; }
        #endregion

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PathType
    {
        [EnumMember]
        Local,
        [EnumMember]
        NetShare
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExtensionAction
    {
        [EnumMember]
        None,
        [EnumMember]
        CreateNew,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum NetLocationExist
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        Exist,
        [EnumMember]
        NotExist,
    }
}
