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
using System.Reflection;
using System.Text;

namespace AvePoint.Wrapper.Core.IOC
{
    /// <summary>
    /// O365Authentication Node
    /// </summary>
    class O365AuthenticationNode
    {
        public O365AuthenticationNode(System.Xml.XmlElement node)
        {
            if(node == null)
            {
                throw new ArgumentNullException("node");
            }

            this.Id = node.GetAttribute("id");
            this.Scope = node.GetAttribute("scope");

            var typeAsString = node.GetAttribute("type");

            TypeAsString = typeAsString;

            try
            {
                this.ReflectionOnlyType = Type.ReflectionOnlyGetType(typeAsString, true, false);
            }
            catch(TypeLoadException)
            {
                /*ignore this exception由于ReflectionOnly会检查type类型，由于13 dll (frameworkshi 4.5)引用的接口是定义在framework 3.5下，
                 *这个时候会提出type load exception，但是正常初始化没问题，所以忽略该异常信息。
                 */
            }
        }
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Scope
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public Type ReflectionOnlyType { get; set; }

        /// <summary>
        /// Type As String
        /// </summary>
        public string TypeAsString { get; set; }
    }
}
