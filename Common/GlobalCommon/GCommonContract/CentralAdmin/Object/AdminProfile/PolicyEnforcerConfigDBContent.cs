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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public  class PolicyEnforcerConfigDBContent : IProfileContent
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string DatabaseServer { get; set; }

        [DataMember]
        public string DatabaseName { get; set; }

        [DataMember]
        public PEDatabaseAuthentication Authentication { get; set; }

        [DataMember]
        public string AccountProfileId { get; set; }

        [DataMember]
        public string Account { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public bool UseFailoverDatabase { get; set; }

        [DataMember]
        public string FailoverDatabaseServer { get; set; }

        [DataMember]
        public bool UseConnectionString { get; set; }

        [DataMember]
        public string ConnectionString { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PEDatabaseAuthentication
    {
        [EnumMember]
        Windows = 0,

        [EnumMember]
        SQL = 1
    }

    public enum PEDBConfigReturnValue
    {
        Success = 0,
        /// <summary>
        /// Test时抛异常，Account、Pwd或Database Server有误
        /// </summary>
        LogonDBServerFailed = 1,
        /// <summary>
        /// 创建DB或Table时抛异常，可能是没有权限
        /// </summary>
        CreateDBAndTableFailed = 2,
        /// <summary>
        /// DB存在，表不存在; 或者表存在，但表结构不同. 返回Error信息，建议用户另建一个DB
        /// </summary>
        DBExistTableExistColumsDifferent=3,

    }
}
