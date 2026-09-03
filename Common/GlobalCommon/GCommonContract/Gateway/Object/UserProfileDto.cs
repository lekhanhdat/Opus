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
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    public class UserProfileDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string Extensions { get; set; }
        public int IsLockOut { get; set; }
        public int PasswordAttempCount { get; set; }
        public long LockTime { get; set; }
        public long LastPwdChange { get; set; }
        public int Country { get; set; }
        public int State { get; set; }
        /// <summary>
        /// 用户的名
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// 用户的姓氏
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public string Telephone { get; set; }
        /// <summary>
        /// 公司或组织
        /// </summary>
        public string Organization { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 是否是正式账号
        /// </summary>
        public int IsPurchasedAccount { get; set; }
        /// <summary>
        /// 是否是app用户
        /// </summary>
        public bool IsRegisterByApp { get; set; }
        /// <summary>
        /// App用户安装app的site的URL
        /// </summary>
        public string RegisterSiteUrl { get; set; }
        /// <summary>
        /// 与License Agreement表关联，值就是license agreement记录的guid
        /// </summary>
        public Guid? LicenseAgreementGuid { get; set; }
        /// <summary>
        /// 与License Agreement表关联，值就是license agreement记录的id
        /// </summary>
        public string LicenseAgreementId { get; set; }
        /// <summary>
        /// 表明license的类型是trial还是Enterprise，也就是用户是否是付费用户
        /// </summary>
        public LicenseAgreementType LicenseType { get; set; }
        /// <summary>
        /// 表示用户是否接受license agreement的状态
        /// </summary>
        public LicenseAgreementAccepted LicenseAgreementAccepted { get; set; }
        /// <summary>
        /// 注册用户的过期时间
        /// </summary>
        public long ExpirationTime { get; set; }
        /// <summary>
        /// 注册用户的注册时间
        /// </summary>
        public long RegistrationTime { get; set; }
        /// <summary>
        /// 注册用户的购买用户数量
        /// </summary>
        public int UserSeat { get; set; }
        /// <summary>
        /// 用来区分注册用户和invite用户
        /// </summary>
        public ObjectRoleType UserRole { get; set; }
        /// <summary>
        /// User所在Group的Id
        /// </summary>
        public string GroupId { get; set; }
    }
}
