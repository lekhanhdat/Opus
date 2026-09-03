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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.OperationResult
{
    public interface IConnectorOperationResult
    {
        bool HasError { get; }
        OperationResultException Exception { get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConnectorOperationResult : IConnectorOperationResult
    {
        public ConnectorOperationResult()
        {

        }
        public ConnectorOperationResult(bool hasError, OperationResultException exception)
        {
            HasError = hasError;
            Exception = exception;
        }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public OperationResultException Exception { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OperationResultException
    {
        public OperationResultException(OperationResultError error)
        {
            Error = error;
        }

        [DataMember]
        public OperationResultError Error { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OperationResultError : int
    {
        [EnumMember]
        UnKnown, //未知问题

        [EnumMember]
        ParameterError,//参数不合法

        [EnumMember]
        NameIsExist,//名字已存在
        //TODO Connector--add settings error

        [EnumMember]
        ProfileIsInUse,//profile被使用中

        [EnumMember]
        ProfileNotExist,
        //TODO Connector--add rule error

        [EnumMember]
        StubDBHasNotBeenConfigured,

        [EnumMember]
        EBSOrRBSHasNotBeenConfigured,

        [EnumMember]
        SolutionNotDeployed,

        [EnumMember]
        SolutionAndStubDBBothNotConfigured,

        [EnumMember]
        SaveStubDBFailed,

        [EnumMember]
        SaveNotInheritLibraryPathInfoFailed,
        //打破非继承保存失败
        [EnumMember]
        NotAllPathInfoSaved,
        //批量保存PathInfo时未全部保存成功
        [EnumMember]
        NotAllFeatureActived,

        [EnumMember]
        RemoveConnectorInfoFailed,
        //remove失败
        [EnumMember]
        GetUnInheritListSettingIsNull,
        //Agent的方法GetUnInheritListSetting返回值为空
        [EnumMember]
        SaveScheduleTimeEarlierThanCurrent,
        //保存Schedule时间比当前时间早,请查看时间
        [EnumMember]
        NodeOrPhysicalDeviceDtoIsNull,
        //传入的Node或者DeviceDto是空
        [EnumMember]
        NodeIsExist,
        //节点已经存在
        [EnumMember]
        DefaultMappingCannotUpdate,
        //默认mapping不能更改
        [EnumMember]
        DefaultMappingCannotRemove,
        //默认mapping不能删除

        [EnumMember]
        NodeIsNotExist,
        //节点不存在

        [EnumMember]
        ParentIsNotExist,
        //父节点不存在

        [EnumMember]
        inheritedPathIsNotExist,
        //父节点PATH不存在 

        [EnumMember]
        ScheduleIsNotValid,

        [EnumMember]
        SiteCollectionActiveFailed,

        [EnumMember]
        ScheduleStartTimeCannotBeEarlyThanCurrent,

        [EnumMember]
        D5SolutionDeployed,
        [EnumMember]
        D6SolutionNotDeployed,
    }
}

