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
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AvePoint.RA.Web.Models.API
{
    [Serializable]
    [DataContract]
    public class APIClasificationModel
    {
        /// <summary>
        /// Site Url
        /// </summary>
        [DataMember]
        public string SiteCollectionUrl { set; get; }
        [DataMember]
        public string Url { set; get; }
        /// <summary>
        /// Default Term full path, 用于设置和替换Group级别的Term
        /// </summary>
        [DataMember]
        public string DefaultTermPath { set; get; }

        /// <summary>
        /// 用于替换Site级别的classification的Term
        /// </summary>
        [DataMember]
        public string RootTermPath { set; get; }
        [DataMember]
        public string DefaultValue { set; get; }
        /// <summary>
        /// Agent account
        /// </summary>
        [DataMember]
        public string UserName { set; get; }
        /// <summary>
        /// Site Group to register site 
        /// </summary>
        [DataMember]
        public string GroupName { set; get; }
        [DataMember]
        public string User { get; set; }
        [DataMember]
        public string ApplySettingNow { get; set; }
        //public string Token { get; set; }
    }
}