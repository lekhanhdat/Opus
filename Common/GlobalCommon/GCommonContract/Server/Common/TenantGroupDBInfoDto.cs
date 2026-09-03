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

namespace AvePoint.GCommon.Contract.Server.Common
{
    /// <summary>
    /// 每个Tenant Group所使用的DB信息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TenantGroupDBInfoDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string TenantGroupId { get; set; }

        /// <summary>
        /// TenantGroup owner name
        /// </summary>
        [DataMember]
        public string AccountName { get; set; }

        /// <summary>
        /// Tenant Group使用的DB
        /// </summary>
        [DataMember]
        public TenantDBInfoDto TenantDBInfo { get; set; }

        /// <summary>
        /// Tenant Group使用的登录DB的用户名
        /// </summary>
        [DataMember]
        public string LoginName { get; set; }

        /// <summary>
        /// Tenant Group使用的登录DB的密码
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        ///  Tenant Group使用DB的SchemaName
        /// </summary>
        [DataMember]
        public string SchemaName { get; set; }

        /// <summary>
        /// Tenant Group使用DB的大小配额,Unit MB
        /// </summary>
        [DataMember]
        public int SizeQuota { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [DataMember]
        public int Status { get; set; }

        public override string ToString()
        {
            return string.Format("TenantGroupDBInfoDto[Id {0}, AccountName {1}, LoginName {2}, SchemaName {3}]", Id, AccountName, LoginName, SchemaName);
        }
       
    }
}
