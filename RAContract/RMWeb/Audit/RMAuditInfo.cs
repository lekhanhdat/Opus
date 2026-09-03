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
using CommonModel.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Audit
{
    public class RMAuditInfo
    {
        public int Id { set; get; }

        /// <summary>
        /// 执行操作的用户名
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 用户所在组
        /// </summary>
        public string Role { get; set; }

        public List<AuditItem> ModifyContent { get; set; }
        /// <summary>
        /// 该模块的代号
        /// </summary>
        public AuditModule Module { get; set; }
        /// <summary>
        /// 该Function代替页面的Module显示
        /// </summary>
        public AuditCategory Category { get; set; }

        /// <summary>
        /// Xml格式的详细信息
        /// </summary>
        //public string Content
        //{
        //    set { }
        //    get
        //    {
        //        return SerializerHelper.SerializeToXmlString(ModifyContent);
        //    }
        //}

        /// <summary>
        /// 该操作的代号（如删除操作）
        /// </summary>
        public AuditAction Action { get; set; }

        /// <summary>
        /// 该操作成功与否
        /// </summary>
        public int Status { get; set; }//0:Successfule,1:Failed,2:Exception

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime ExecuteOn { get; set; }

        public string Method { get; set; }

        /// <summary>
        /// 操作对象
        /// </summary>
        public string Object { get; set; }

        public string UserName { get; set; }

        /// <summary>
        /// 代理方法中的异常
        /// </summary>
        public Exception E { get; set; }

        public object OtherInfos { get; set; }

        public bool NotNeedRecordAudit { get; set; }

        public string ClientIP { get; set; }
    }

    public class AuditItem
    {
        public Guid Id { get; set; }
        public string TargetSetting { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
        public int Deep { get; set; }
    }

    public enum AuditStatus
    {
        [Description("RM_RC_Audit_Status_Successful")]
        Successful = 0,
        [Description("RM_RC_Audit_Status_Failed")]
        Failed = 1,
        //[Description("RM_RC_Audit_Status_Exception")]
        //Exception = 2
    }
    [DataContract]
    public enum DisplayColumn
    {
        [EnumMember]
        Time = 0,
        [EnumMember]
        User = 1,
        [EnumMember] 
        Role = 2,
        [EnumMember]
        DocAveModule = 3,
        [EnumMember]
        Object = 4,
        [EnumMember]
        Action = 5,
        [EnumMember]
        Status = 6
    }
}
