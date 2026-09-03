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
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.Wrapper.Restore.NintexForm.Online
{
    class AttachmentControl : BaseControl
    {
        private static AveLogger mLogger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        static Dictionary<string, string> defaultValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "MinimumAttachments","0" },
            { "MaximumAttachments","100"}
        };
        public AttachmentControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
        }

        public override void ProcessControl(bool isPost)
        {
            try
            {
                base.ProcessControl(isPost);
                var referencesNode = GetPropertyNode(GetXPath("InsertReferences"));
                if (referencesNode != null)
                {
                    var tempPrefix = GetNodePrefixAndAddNameSpace(referencesNode);
                    foreach (XmlNode node in referencesNode.ChildNodes)
                    {
                        var keyNode = node.SelectSingleNode(GetXPath(tempPrefix, "Key"), nsManager);
                        var valueNode = node.SelectSingleNode(GetXPath(tempPrefix, "Value"), nsManager);
                        if (keyNode != null && valueNode != null)
                        {
                            int value;
                            string defaultValue;
                            if ((!int.TryParse(valueNode.InnerText, out value))
                                && (defaultValues.TryGetValue(keyNode.InnerText, out defaultValue)))
                            {
                                valueNode.InnerText = defaultValue;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occurred handling repeating control. Error: {0}", e);
            }
        }

        public string GetNodePrefixAndAddNameSpace(XmlNode node)
        {
            string prefix = node.GetPrefixOfNamespace("http://schemas.microsoft.com/2003/10/Serialization/Arrays");
            nsManager.AddNamespace(prefix, "http://schemas.microsoft.com/2003/10/Serialization/Arrays");
            return prefix;
        }

        //public override void AddControlNameSpace()
        //{
        //    nsManager.AddNamespace("d4p1", "http://schemas.microsoft.com/2003/10/Serialization/Arrays");
        //}
    }
}
