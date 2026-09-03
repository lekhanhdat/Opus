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

///********************************************************************
// *
// *  PROPRIETARY and CONFIDENTIAL
// *
// *  This file is licensed from, and is a trade secret of:
// *
// *                   AvePoint, Inc.
// *                   Harborside Financial Center
// *                   9th Fl.   Plaza Ten
// *                   Jersey City, NJ 07311
// *                   United States of America
// *                   Telephone: +1-800-661-6588
// *                   WWW: www.avepoint.com
// *
// *  Refer to your License Agreement for restrictions on use,
// *  duplication, or disclosure.
// *
// *  RESTRICTED RIGHTS LEGEND
// *
// *  Use, duplication, or disclosure by the Government is
// *  subject to restrictions as set forth in subdivision
// *  (c)(1)(ii) of the Rights in Technical Data and Computer
// *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
// *  FAR 52.227-19 (C) (June 1987).
// *
// *  Copyright © 2013-2015 AvePoint® Inc. All Rights Reserved. 
// *
// *  Unpublished - All rights reserved under the copyright laws of the United States.
// *  $Revision:  $
// *  $Author:  $        
// *  $Date:  $
// */



//using System.Runtime.Serialization;
//using AvePoint.GCommon.Contract.Common;

//namespace AvePoint.GCommon.Contract.ReportCenter.Object
//{
//    [DataContract(Namespace = ContractConstants.Namespace)]
//    public class ReportSettingConfig : BaseConfigSetting
//    {
//        [DataMember]
//        public bool IsConfigSettingExits { get; set; } //判断ReportService是否已经配置过

//        [DataMember]
//        public ReportingServiceErrorConstant ErrorType { get; set; } //记录Test的结果

//        [DataMember]
//        public DatebaseSetting DatabaseSetting { get; set; } //记录数据库配置信息

//        [DataMember]
//        public bool IsSharePointIntegrated { get; set; }  //判断SharePointIntegrated是否配置过

//        [DataMember]
//        public DocumentLibaraySetting DocLibSetting { get; set; } //SharePointIntegrated配置信息

//        [DataMember]
//        public WebServiceSetting WebServiceSetting { get; set; } //记录WebService配置信息
//    }

//    [DataContract(Namespace = ContractConstants.Namespace)]
//    public class DatebaseSetting
//    {
//        [DataMember]
//        public bool IsUseTheSameServer { get; set; }

//        [DataMember]
//        public bool IsWindowsAuthentication { get; set; }

//        [DataMember]
//        public string DbServer { get; set; }

//        [DataMember]
//        public string DbName { get; set; }

//        [DataMember]
//        public string Account { get; set; }

//        [DataMember]
//        public string FailoverPartner { get; set; }
//    }

//    [DataContract(Namespace = ContractConstants.Namespace)]
//    public class DocumentLibaraySetting
//    {
//        [DataMember]
//        public string DocLibUrl { get; set; }

//        [DataMember]
//        public string Username { get; set; }
//    }

//    [DataContract(Namespace = ContractConstants.Namespace)]
//    public class WebServiceSetting
//    {
//        [DataMember]
//        public string WebServiceUrl { get; set; }

//        [DataMember]
//        public string Username { get; set; }
//    }
//}
