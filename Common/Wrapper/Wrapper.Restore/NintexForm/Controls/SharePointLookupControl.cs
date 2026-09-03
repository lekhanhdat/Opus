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
using AvePoint.Wrapper.Common;
using System.Xml;

namespace AvePoint.Wrapper.Restore.NintexForm.Online
{
    class SharePointLookupControl : BaseControl
    {
        public SharePointLookupControl(AveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager)
            : base(web, list, contentTypeId, controlNode, nsManager)
        {
            mWeb = web;
            mList = list;
            mControlNode = controlNode;
            this.nsManager = nsManager;
            this.contentTypeId = contentTypeId;
            AddControlNameSpace();
        }

        public override void ProcessControl(bool isPost)
        {
            base.ProcessControl(isPost);
            UpgradeDisplayModeProperty();
            UpgradeAppearanceProperty();
        }

        private void UpgradeDisplayModeProperty()
        {
            try
            {
                var displayFormatNode = mControlNode.SelectSingleNode("d2p1:DisplayFormat", nsManager);
                if (displayFormatNode != null)
                {
                    string sourceMode = displayFormatNode.InnerText;
                    XmlNode displayModeNode = null;
                    if (string.Equals(sourceMode, "CheckBoxList", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(sourceMode, "ListBox", StringComparison.OrdinalIgnoreCase))
                    {
                        displayModeNode = mControlNode.SelectSingleNode("d2p1:MultipleDisplayMode", nsManager);
                    }
                    else if (string.Equals(sourceMode, "DropDownList", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(sourceMode, "RadioButtonList", StringComparison.OrdinalIgnoreCase))
                    {
                        displayModeNode = mControlNode.SelectSingleNode("d2p1:SingleDisplayMode", nsManager);
                    }
                    else if (string.Equals(sourceMode, "MultiSelect", StringComparison.OrdinalIgnoreCase))
                    {
                        //the value is default, so don't need to do the replace, do nothing here
                        //and MultiSelect's displayMode should be default
                    }

                    if (displayModeNode != null)
                    {
                        displayModeNode.InnerText = sourceMode;
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Upgrade sharepoint lookup control display mode property failed .Error:{0}",e);
            }
        }
        private void UpgradeAppearanceProperty()
        {
            try
            {
                var customNoneText = mControlNode.SelectSingleNode("d2p1:CustomNoneText", nsManager);
                var customPleaseSelectTextNode = mControlNode.SelectSingleNode("d2p1:CustomPleaseSelectText", nsManager);
                if (customPleaseSelectTextNode != null && customNoneText != null)
                {
                    customNoneText.InnerText = customPleaseSelectTextNode.InnerText;
                }

                var referencesNode = mControlNode.SelectSingleNode("d2p1:InsertReferences", nsManager);
                if (referencesNode != null)
                {
                    referencesNode.InnerXml = referencesNode.InnerXml.Replace("UseCustomPleaseSelectText", "UseCustomNoneText");
                }
            }
            catch (Exception e)
            {
                log.Error("Upgrade sharepoint lookup control appearance property failed .Error:{0}", e);
            }
        }

        public override void AddControlNameSpace()
        {
            nsManager.AddNamespace("d3p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint.FormControls");
        }

    }
}
