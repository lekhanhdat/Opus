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
using AvePoint.GCommon.Contract.Gateway.Object;

namespace AvePoint.GCommon.Contract.Server.Common
{
    /// <summary>
    /// DocAve Online中使用的DB的信息，例如AuditorDB  ReportDB
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TenantDBInfoDto
    {
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// DB所在的Server
        /// </summary>
        [DataMember]
        public string ServerName { get; set; }

        [DataMember]
        public string DBName { get; set; }

        ///// <summary>
        ///// DB的超级用户的用户名
        ///// </summary>
        //[DataMember]
        //public string LoginName { get; set; }

        /// <summary>
        /// DB的类型 例如AuditorDB  ReportDB
        /// </summary>
        [DataMember]
        public DBType DBType { get; set; }

        /// <summary>
        /// DB的最大空间, Unit:MB
        /// </summary>
        [DataMember]
        public int MaxSize { get; set; }

        public override string ToString()
        {
            return string.Format("TenantDBInfoDto[Id {0}, ServerName {1}, DBName {2}, DBType {3}]", Id, ServerName, DBName, DBType);
        }

        public static TenantDBInfoDto Clone(TenantDBInfoDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new TenantDBInfoDto
            {
                Id = dto.Id,
                ServerName = dto.ServerName,
                DBName = dto.DBName,
                //LoginName = dto.LoginName,
                DBType = dto.DBType,
                MaxSize = dto.MaxSize
            };
        }
    }
}
