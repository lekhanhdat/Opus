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
        public SharePointLookupControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
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
                var displayFormatNode = mControlNode.SelectSingleNode(GetXPath("DisplayFormat"), nsManager);
                if (displayFormatNode != null)
                {
                    string sourceMode = displayFormatNode.InnerText;
                    XmlNode displayModeNode = null;
                    if (string.Equals(sourceMode, "CheckBoxList", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(sourceMode, "ListBox", StringComparison.OrdinalIgnoreCase))
                    {
                        displayModeNode = mControlNode.SelectSingleNode(GetXPath("MultipleDisplayMode"), nsManager);
                    }
                    else if (string.Equals(sourceMode, "DropDownList", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(sourceMode, "RadioButtonList", StringComparison.OrdinalIgnoreCase))
                    {
                        displayModeNode = mControlNode.SelectSingleNode(GetXPath("SingleDisplayMode"), nsManager);
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
                var customNoneText = mControlNode.SelectSingleNode(GetXPath("CustomNoneText"), nsManager);
                var customPleaseSelectTextNode = mControlNode.SelectSingleNode(GetXPath("CustomPleaseSelectText"), nsManager);
                if (customPleaseSelectTextNode != null && customNoneText != null)
                {
                    customNoneText.InnerText = customPleaseSelectTextNode.InnerText;
                }


                var useCustomNoneText = mControlNode.SelectSingleNode(GetXPath("UseCustomNoneText"), nsManager);
                var useCustomPleaseSelectTextNode = mControlNode.SelectSingleNode(GetXPath("UseCustomPleaseSelectText"), nsManager);
                if (useCustomPleaseSelectTextNode != null && useCustomNoneText != null)
                {
                    useCustomNoneText.InnerText = useCustomPleaseSelectTextNode.InnerText;
                }


                var referencesNode = mControlNode.SelectSingleNode(GetXPath("InsertReferences"), nsManager);
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

    }
}
