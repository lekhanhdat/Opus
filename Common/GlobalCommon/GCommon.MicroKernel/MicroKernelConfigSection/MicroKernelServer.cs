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




namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Configuration;
    #endregion

    public class MicroKernelServer : ConfigurationElement
    {
        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("id", DefaultValue = "id", IsKey = true, IsRequired = true)]
        public String Id
        {
            get { return this["id"] as String; }
            set { this["id"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "name")]
        [StringValidator(InvalidCharacters = ".")]
        public String Name
        {
            get { return this["name"] as String; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("type", DefaultValue = "type")]
        public String Type
        {
            get { return this["type"] as String; }
            set { this["type"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("assemblyQualifiedType", DefaultValue = "", IsRequired = true)]
        public String AssemblyQualifiedType
        {
            get { return this["assemblyQualifiedType"] as String; }
            set { this["assemblyQualifiedType"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("path", DefaultValue = "path")]
        public String Path
        {
            get { return this["path"] as String; }
            set { this["path"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("isSameAssembly", DefaultValue = "false")]
        public Boolean IsSameAssembly
        {
            get { return Convert.ToBoolean(this["isSameAssembly"]); }
            set { this["isSameAssembly"] = value; }
        }
    }
}
