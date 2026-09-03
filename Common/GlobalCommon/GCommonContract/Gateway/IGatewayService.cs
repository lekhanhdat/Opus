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
using System.ServiceModel;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Gateway.Object;
using AvePoint.GCommon.Contract.Server.Login;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.Gateway
{
    /// <summary>
    /// 为Gateway Service提供的接口
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IGatewayService
    {
        /// <summary>
        /// 获取Dashboard的各项信息，结果会根据userFilter进行过滤
        /// 
        /// Exceptions
        ///     LoginException
        ///         认证异常
        /// </summary>
        /// <param name="credential">
        /// BuildinAccountCredential.UserName 认证的用户名
        ///                       LoginCredential  HMACSHA1(SHA1(认证的密码的))
        /// </param>
        /// <param name="accountFilters">
        /// 需要过滤的Account的Pattern例如test@avepoint.com或者*@avepoint.com
        /// *是通配符
        /// </param>
        /// <returns>
        /// 返回内容包括如下
        ///   ServiceDetails
        ///   StorageUsage
        ///   DBUsages
        ///   JobPeakInfo
        /// </returns>
        [OperationContract]
        DashboardInfoResponseMessage GetDashboardInfo(BuildinAccountCredential credential, List<string> accountFilters);

        /// <summary>
        /// 获取UserSeatInfo
        /// 
        /// Exceptions
        ///     LoginException
        ///         认证异常
        /// </summary>
        /// <param name="credential">
        /// BuildinAccountCredential.UserName 认证的用户名
        ///                       LoginCredential  HMACSHA1(SHA1(认证的密码的))
        /// </param>
        /// <param name="accountFilters">
        /// 需要过滤的Account的Pattern例如test@avepoint.com或者*@avepoint.com
        /// *是通配符
        /// </param>
        /// <param name="startTime">
        /// </param>
        /// <param name="endTime">
        /// </param>
        /// <returns>
        /// empty or datas
        /// </returns>
        [OperationContract]
        List<UserSeatInfo> GetUserSeatInfo(BuildinAccountCredential credential, List<string> accountFilters, DateTime startTime, DateTime endTime);

        /// <summary>
        /// for dashboard
        /// </summary>
        /// <param name="credential">
        /// </param>
        /// <param name="accountFilters">
        /// 需要过滤的Account的Pattern例如test@avepoint.com或者*@avepoint.com
        /// *是通配符
        /// </param>
        /// <returns>
        /// </returns>
        [OperationContract]
        List<PlanSummaryDto> GetPlanSummaries(BuildinAccountCredential credential, List<string> accountFilters);

        /// <summary>
        /// 从Usage Db 获取tenant user 和当前server control DB进行比较，获取新创建的user.
        /// </summary>
        /// <param name="tenantIdsInUsageDb">key->tenantId</param>
        /// <returns>新创建的user</returns>
        [OperationContract]
        AvePoint.GCommon.Contract.Gateway.Object.StorageUsageDto GetNewTenantUsers(Dictionary<string, int> tenantIdsInUsageDb);

        /// <summary>
        /// 检查这个DataCenter的状态
        /// </summary>
        /// <returns>
        /// </returns>
        [OperationContract]
        [Obsolete("use GetHealthStatus")]
        DataCenterHealthStatus CheckHealthStatus();

        /// <summary>
        /// 获取DataCenter的health Report
        /// </summary>
        /// <returns>
        /// </returns>
        [OperationContract]
        List<ServiceHealthReport> GetHealthReport();
    }

    #region old health checker
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DataCenterHealthStatus
    {
        public DataCenterHealthStatus()
        {
            BrokenServcices = new List<ServiceHealthStatus>();
        }

        [DataMember]
        public HealthStatusType Status { get; set; }

        /// <summary>
        /// 出问题的Service列表,当Status为Warning或者Error时这个包含对应的Service的信息
        /// </summary>
        [DataMember]
        public List<ServiceHealthStatus> BrokenServcices { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceHealthStatus
    {
        public ServiceHealthStatus()
        {
            BrokenObjectNames = new List<string>();
        }

        [DataMember]
        public HealthStatusType Status { get; set; }

        [DataMember]
        public ServiceItemType Type { get; set; }

        /// <summary>
        /// 出问题的对象名字列表，可能为null
        /// Type = ControlDB时，为空
        /// Type = TenantDB时为无法访问的Tenant DB name
        /// Type = TimerService时为超时的Server Name of Timer Service，特殊情况：没有任何一个timer时也会导致Service状态为error，但是BrokenObjectNames为空。
        /// 
        /// </summary>
        [DataMember]
        public List<string> BrokenObjectNames { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum HealthStatusType
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Good = 1,
        [EnumMember]
        Warning = 2,
        [EnumMember]
        Error = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ServiceItemType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ControlDB = 1,
        [EnumMember]
        TenantDB = 2,
        [EnumMember]
        TimerService = 3,
        [EnumMember]
        ControlService = 4,
    }
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum HealthStatus
    {
        [EnumMember]
        Up = 0,
        [EnumMember]
        Warning = 1,
        [EnumMember]
        Down = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ServiceType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        WebService = 1,
        [EnumMember]
        TimerService = 3,
        [EnumMember]
        ControlDatabase = 4,
        [EnumMember]
        PolicyEnforcerDatabase = 5,
        [EnumMember]
        AuditorDatabase = 6,
        [EnumMember]
        ReplicatorDatabase = 7,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceHealthReport
    {
        public ServiceHealthReport()
        {
            this.ServiceInstances = new List<ServiceInstanceReport>();
        }

        [DataMember]
        public String ServiceName { get; set; }

        [DataMember]
        public ServiceType ServiceType { get; set; }

        [DataMember]
        public List<ServiceInstanceReport> ServiceInstances { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceInstanceReport
    {
        [DataMember]
        public String InstanceName { get; set; }

        [DataMember]
        public HealthStatus InstanceStatus { get; set; }

        [DataMember]
        public String Details { get; set; }
    }

    public class ServiceReportConstants
    {
        public static readonly string WebService = "Control Service";
        public static readonly string TimerService = "Timer Service";
        public static readonly string AuditorDatabase = "Auditor Database";
        public static readonly string PEDatabase = "Policy Enforcer Database";
        public static readonly string ControlDatabase = "Control Database";
        public static readonly string ReplicatorDatabase = "Replicator Database";
        public static readonly string WebInstances0 = "Control.Web_IN_0";
        public static readonly string WebInstances1 = "Control.Web_IN_1";
    }
}
