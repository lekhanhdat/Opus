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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EDExportLocationResponse : EDiscoveryResponse
    {
        /// <summary>
        /// 如果export location被plan使用了，那么返回它的名字,格式 xxx,yyy
        /// </summary>
        [DataMember]
        public string BeUsedExportLocationName { get; set; }

        /// <summary>
        /// 是否存在Search result location
        /// </summary>
        [DataMember]
        public bool HasExisted { get; set; }


        /// <summary>
        /// 查询Search Result Location的返回结果
        /// </summary>
        [DataMember]
        public EDExportLocationDto Location { get; set; }


        /// <summary>
        /// Save 操作返回的结果
        /// </summary>
        [DataMember]
        public SaveResultEnum SaveResult { get; set; }


        /// <summary>
        /// 检测unc path是否可用的返回结果
        /// </summary>
        [DataMember]
        public TestResultEnum TestResult { get; set; }


        [DataMember]
        public List<EDExportLocationDto> ExportLocationInfoList { get; set; }

        /// <summary>
        /// Export Location Save 状态
        /// </summary>
        [DataMember]
        public ExportLocationOperateState OperateState { get; set; }

        /// <summary>
        /// Export  Location Name 检查是否存在
        /// </summary>
        [DataMember]
        public ExportLocationOperateState NameExit { get; set; }
        /// <summary>
        /// Export Location Test 结果
        /// </summary>
        [DataMember]
        public ExportLocationOperateState TestState {get; set;}

        [DataMember]
        public IDictionary<string, bool> DeletedStateResult { get; set; }

        [DataMember]
        public Dictionary<string, List<string>> UsedExportLocationPlanName { get; set; }

        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum StateEnum
        {
            [EnumMember]
            FindException = 0,
            [EnumMember]
            IllegalInput = 1
        }


        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum SaveResultEnum
        {
            /// <summary>
            /// Save 失败
            /// </summary>
            [EnumMember]
            Failed = 0,
            /// <summary>
            /// Save 成功
            /// </summary>
            [EnumMember]
            Successful = 1,
            /// <summary>
            /// Test 失败
            /// </summary>
            [EnumMember]
            TestFailed = 2,
            [EnumMember]
            IllegalInput = 3
        }

        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum TestResultEnum
        {
            [EnumMember]
            Unknown = 0,
            /// <summary>
            /// 由于网络因素链接不上
            /// </summary>
            [EnumMember]
            ConnectedFailed = 1,
            /// <summary>
            /// 认证失败, 比如用户名或密码错误
            /// </summary>
            [EnumMember]
            AuthenticationFailed = 2,
            /// <summary>
            /// 认证通过, 但是因为文件夹不存在等因素导致不可访问
            /// </summary>
            [EnumMember]
            Unaccessable = 3,
            /// <summary>
            /// 可以读, 但是因为磁盘空间等因素不可写
            /// </summary>
            [EnumMember]
            Available = 4,
            /// <summary>
            /// 可读可写
            /// </summary>
            [EnumMember]
            AvailableAndNotFull = 5
        }

       

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum  ExportLocationOperateState
    {
        [EnumMember]
        Unknown = 0,
        /// <summary>
        /// (Test UNC Path) 
        /// 由于网络因素链接不上
        /// </summary>
        [EnumMember]
        ConnectedFailed = 1,
        /// <summary>
        /// (Test UNC Path) 
        /// 认证失败, 比如用户名或密码错误
        /// </summary>
        [EnumMember]
        AuthenticationFailed = 2,
        /// <summary>
        /// (Test UNC Path) 
        /// 认证通过, 但是因为文件夹不存在等因素导致不可访问
        /// </summary>
        [EnumMember]
        Unaccessable = 3,
        /// <summary>
        /// (Test UNC Path)
        /// 可以读, 但是因为磁盘空间等因素不可写
        /// </summary>
        [EnumMember]
        Available = 4,
        /// <summary>
        /// (Test UNC Path) 
        /// 可读可写
        /// </summary>
        [EnumMember]
        AvailableAndNotFull = 5,
        /// <summary>
        /// (Save UNC Path)
        /// 操作成功
        /// </summary>
        [EnumMember]
        SaveSuccessful = 6,
        /// <summary>
        /// (Save UNC Path)
        /// 保存 UNC Path 失败
        /// </summary>
        [EnumMember]
        SaveFailed = 7,

        [EnumMember]
        NameExist = 8
    }
}
