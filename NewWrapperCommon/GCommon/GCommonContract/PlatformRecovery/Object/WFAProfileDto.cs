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

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

    /// <summary>
    /// WFA Profile
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("WFAProfileDto")]
    public class WFAProfileDto : IProfileContent
    {
        [DataMember]
        [XmlAttribute("id")]
        public string Id { get; set; }

        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute("description")]
        public string Description { get; set; }

        /// <summary>
        /// WFA Server的Root URL
        /// eg. http://10.2.53.21/ or https://10.2.53.21:1329/
        /// </summary>
        [DataMember]
        [XmlAttribute("url")]
        public string URL { get; set; }

        /// <summary>
        /// 登录WFA Server的user name
        /// eg. admin
        /// </summary>
        [DataMember]
        [XmlAttribute("username")]
        public string Username { get; set; }

        /// <summary>
        /// 登录WFA Server的password
        /// </summary>
        [DataMember]
        [XmlAttribute("password")]
        public string Password { get; set; }
    }
}