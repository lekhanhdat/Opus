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
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.IOC
{
    abstract class BaseServiceNode
    {
        public BaseServiceNode(System.Xml.XmlElement node)
        {
            if(node == null)
            {
                throw new ArgumentNullException("node");
            }

            Id = node.GetAttribute("id");
            Scope = (WrapperSPMode)Enum.Parse(typeof(WrapperSPMode), node.GetAttribute("scope"), false);
            var versionAsString = node.GetAttribute("version");

            if((!string.IsNullOrEmpty(versionAsString)) && (versionAsString.Length != 1 || versionAsString[0] != '*'))
            {
                Version = new Version(versionAsString);
            }

            var typeAsString = node.GetAttribute("type");

            ReflectionOnlyType = Type.ReflectionOnlyGetType(typeAsString, true, false);
        }
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Scope
        /// </summary>
        public WrapperSPMode Scope { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public Type ReflectionOnlyType { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; set; }
    }

    class DeploymentNode : BaseServiceNode
    {
        public DeploymentNode(System.Xml.XmlElement node) : base(node) { }
    }
}
