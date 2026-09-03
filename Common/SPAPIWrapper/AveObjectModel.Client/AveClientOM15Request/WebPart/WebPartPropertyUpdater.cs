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
using Microsoft.SharePoint.Client.WebParts;

namespace AveClientOM15Request
{
    public enum WebPartHelpMode
    {
        /// <summary>Opens a separate browser window, if the browser has this capability. A user must close the window before returning to the Web Parts page. </summary>
        // Token: 0x04002728 RID: 10024
        Modal,
        /// <summary>Opens a separate browser window, if the browser has this capability. A user does not have to close the window before returning to the Web page. </summary>
        // Token: 0x04002729 RID: 10025
        Modeless,
        /// <summary>Replaces the Web Parts page in the browser window.</summary>
        // Token: 0x0400272A RID: 10026
        Navigate
    }
    public enum PartChromeState
    {
        /// <summary>A control and border in a normal state.</summary>
        // Token: 0x0400264F RID: 9807
        Normal,
        /// <summary>A control and border in a collapsed or minimized state.</summary>
        // Token: 0x04002650 RID: 9808
        Minimized
    }
    public enum PartChromeType
    {
        /// <summary>A border setting inherited from the part control's containing zone.</summary>
        // Token: 0x04002652 RID: 9810
        Default,
        /// <summary>A title bar and a border.</summary>
        // Token: 0x04002653 RID: 9811
        TitleAndBorder,
        /// <summary>No border and no title bar.</summary>
        // Token: 0x04002654 RID: 9812
        None,
        /// <summary>A title bar only, without a border.</summary>
        // Token: 0x04002655 RID: 9813
        TitleOnly,
        /// <summary>A border only, without a title bar.</summary>
        // Token: 0x04002656 RID: 9814
        BorderOnly
    }

    abstract class WebPartPropertyUpdater
    {        
        protected WebPartDefinition webpartDefinition;
        protected Microsoft.SharePoint.Client.WebParts.WebPart webpart;
        protected IWebPartPropertyExtractor webpartPropertyExtractor;
        protected AveWebPartCache mMapping;
        public WebPartPropertyUpdater(WebPartDefinition webpartDefinition, string webpartDefinitionXml, IWebPartPropertyExtractor webpartExtractor)
        {
            this.webpartDefinition = webpartDefinition;
            this.webpart = this.webpartDefinition.WebPart;
            this.webpartPropertyExtractor = webpartExtractor;
        }

        public void Update()
        {
            UpdateSharedProperties();
            UpdateIndividualProperties();            
        }
        public void SetMapping(AveWebPartCache mapping)
        {
            mMapping = mapping;
        }

        protected virtual void UpdateSharedProperties()
        {
        }

        protected abstract void UpdateIndividualProperties();
    }

    class CommonWebPartPropertyUpdater : WebPartPropertyUpdater
    {
        private AveWebPartBaseInfo webpartInfo;

        public CommonWebPartPropertyUpdater(WebPartDefinition webpartDefinition, AveWebPartBaseInfo webpartBaseInfo, IWebPartPropertyExtractor webpartExtractor)
            : base(webpartDefinition, webpartBaseInfo.DefinitionXml, webpartExtractor)
        {
            webpartInfo = webpartBaseInfo;
        }

        protected override void UpdateSharedProperties()
        {       
            bool? allowClose = base.webpartPropertyExtractor.GetBoolProperty("AllowClose");
            if (allowClose != null)
            {
                webpart.Properties["AllowClose"] = allowClose.Value;
            }
            bool? allowConnect = base.webpartPropertyExtractor.GetBoolProperty("AllowConnect");
            if (allowConnect != null)
            {
                webpart.Properties["AllowConnect"] = allowConnect.Value;
            }
            bool? allowEdit = base.webpartPropertyExtractor.GetBoolProperty("AllowEdit");
            if (allowEdit != null)
            {
                webpart.Properties["AllowEdit"] = allowEdit.Value;
            }
            bool? allowHide = base.webpartPropertyExtractor.GetBoolProperty("AllowHide");
            if (allowHide != null)
            {
                webpart.Properties["AllowHide"] = allowHide.Value;
            }
            bool? allowMinimize = base.webpartPropertyExtractor.GetBoolProperty("AllowMinimize");
            if (allowMinimize != null)
            {
                webpart.Properties["AllowMinimize"] = allowMinimize.Value;
            }
            bool? allowZoneChange = base.webpartPropertyExtractor.GetBoolProperty("AllowZoneChange");
            if (allowZoneChange != null)
            {
                webpart.Properties["AllowZoneChange"] = allowZoneChange.Value;
            }
            PartChromeType? chromeType = base.webpartPropertyExtractor.GetProperty<PartChromeType>("ChromeType");
            FrameType? frameType = base.webpartPropertyExtractor.GetProperty<FrameType>("FrameType");
            if (chromeType == null)
            {
                chromeType = ConvertToAspPartChromeType(frameType);
            }
            else if (frameType != null && chromeType != ConvertToAspPartChromeType(frameType)) //SAAS-6775 cancel updating ChromeType if webPart's FrameType and ChromeType conflicted
            {
                chromeType = null;
            }
            if (chromeType != null)
            {
                webpart.Properties["ChromeType"] = chromeType;
            }
            PartChromeState? chromeState = base.webpartPropertyExtractor.GetProperty<PartChromeState>("ChromeState");
            FrameState? frameState = base.webpartPropertyExtractor.GetProperty<FrameState>("FrameState");
            if (chromeState == null)
            {
                chromeState = ConvertToAspPartChromeState(frameState);
            }
            else if (frameState != null && chromeState != ConvertToAspPartChromeState(frameState)) //SAAS-6775 cancel updating ChromeState if webPart's FrameState and ChromeState conflicted
            {
                chromeState = null;
            }
            if (chromeState != null)
            {
                webpart.Properties["ChromeState"] = chromeState;
            }
            //PartChromeType? FrameType = base.webpartPropertyExtractor.GetProperty<PartChromeType>("FrameType");
            //if (FrameType != null)
            //{
            //    webpart.Properties["ChromeType"] = FrameType;
            //}

            //PartChromeState? FrameState = base.webpartPropertyExtractor.GetProperty<PartChromeState>("FrameState");
            //if (FrameState != null)
            //{
            //    webpart.Properties["ChromeState"] = FrameState;
            //}

            string catalogIconImageUrl = base.webpartPropertyExtractor.GetProperty("CatalogIconImageUrl");
            if (catalogIconImageUrl != null)
            {
                webpart.Properties["CatalogIconImageUrl"] = catalogIconImageUrl;
            }

            bool? hidden = base.webpartPropertyExtractor.GetBoolProperty("Hidden");
            if (hidden != null)
            {
                webpart.Properties["Hidden"] = hidden.Value;
            }
            string helpUrl = base.webpartPropertyExtractor.GetProperty("HelpUrl");
            if (helpUrl != null)
            {
                webpart.Properties["HelpUrl"] = helpUrl;
            }
            WebPartHelpMode? helpMode = base.webpartPropertyExtractor.GetProperty<WebPartHelpMode>("HelpMode");
            if (helpMode != null)
            {
                webpart.Properties["HelpMode"] = helpMode;
            }
            string titleUrl = base.webpartPropertyExtractor.GetProperty("TitleUrl");
            if (titleUrl != null)
            {
                webpart.Properties["TitleUrl"] = titleUrl;
            }
            string titleIconImageUrl = base.webpartPropertyExtractor.GetProperty("TitleIconImageUrl");
            if (titleIconImageUrl != null)
            {
                webpart.Properties["TitleIconImageUrl"] = titleIconImageUrl;
            }

            string description = base.webpartPropertyExtractor.GetProperty("Description");
            if (description != null)
            {
                webpart.Properties["Description"] = description;
            }
            string authorizationFilter = base.webpartPropertyExtractor.GetProperty("AuthorizationFilter");
            if (authorizationFilter != null)
            {
                webpart.Properties["AuthorizationFilter"] = authorizationFilter;
            }
            string webPartPropertyViewFlags = base.webpartPropertyExtractor.GetProperty("ViewFlags");
            if (!string.IsNullOrEmpty(webPartPropertyViewFlags))
            {
                string[] flags = webPartPropertyViewFlags.Split(',');
                long resultFlag = 0;
                foreach (string flag in flags)
                {
                    if (!string.IsNullOrEmpty(flag))
                    {
                        long temp = Convert.ToInt64(Enum.Parse(typeof(AveViewFlags), flag));
                        resultFlag |= temp;
                    }
                }
                webpart.Properties["ViewFlags"] = resultFlag;
            }
            int? membershipGroupId = base.webpartPropertyExtractor.GetIntProperty("MembershipGroupId");
            if (membershipGroupId.HasValue)
            {
                int groupId = membershipGroupId.Value;
                if (mMapping.UserMapping.ContainsKey(groupId))
                {
                    object obj = mMapping.UserMapping[groupId];
                    string field = "NewId";
                    Type t = obj.GetType();
                    System.Reflection.FieldInfo fi = t.GetField(field);
                    if (fi != null)
                    {
                        groupId = (int)fi.GetValue(obj);
                    }
                }
                webpart.Properties["MembershipGroupId"] = groupId;
            }
        }

        protected override void UpdateIndividualProperties()
        {
            
        }
        private PartChromeType? ConvertToAspPartChromeType(FrameType? frameType)
        {
            if (frameType == null) return null;
            PartChromeType type = PartChromeType.Default;
            switch (frameType)
            {
                case FrameType.None:
                    return PartChromeType.None;

                case FrameType.Standard:
                    return PartChromeType.TitleAndBorder;

                case FrameType.TitleBarOnly:
                    return PartChromeType.TitleOnly;

                case FrameType.Default:
                    return PartChromeType.Default;

                case FrameType.BorderOnly:
                    return PartChromeType.BorderOnly;
            }
            return type;
        }
        private PartChromeState? ConvertToAspPartChromeState(FrameState? frameState)
        {
            if (frameState == null) return null;
            PartChromeState normal = PartChromeState.Normal;
            switch (frameState)
            {
                case FrameState.Normal:
                    return PartChromeState.Normal;

                case FrameState.Minimized:
                    return PartChromeState.Minimized;
            }
            return normal;
        }
    }

    class TagCloudWebPartUpdater : CommonWebPartPropertyUpdater
    {
        public TagCloudWebPartUpdater(WebPartDefinition webpartDefinition, AveWebPartBaseInfo webpartBaseInfo, IWebPartPropertyExtractor webpartExtractor)
            : base(webpartDefinition, webpartBaseInfo, webpartExtractor)
        {
        }

        protected override void UpdateIndividualProperties()
        {
            int? maxTerms = base.webpartPropertyExtractor.GetIntProperty("MaxTerms");
            if (maxTerms != null)
            {
                webpart.Properties["MaxTerms"] = maxTerms.Value;
            }
        }
    }

    class SocialCommentWebPartUpdater : CommonWebPartPropertyUpdater
    {
        public SocialCommentWebPartUpdater(WebPartDefinition webpartDefinition, AveWebPartBaseInfo webpartBaseInfo, IWebPartPropertyExtractor webpartExtractor)
            : base(webpartDefinition, webpartBaseInfo, webpartExtractor)
        {
        }

        protected override void UpdateIndividualProperties()
        {
            //Custom Properties
            int? WebPartPropertyDisplayItems = base.webpartPropertyExtractor.GetIntProperty("WebPartPropertyDisplayItems");
            if (WebPartPropertyDisplayItems != null)
            {
                webpart.Properties["WebPartPropertyDisplayItems"] = WebPartPropertyDisplayItems.Value;
            }
            bool? WebPartPropertyAllowNewComment = base.webpartPropertyExtractor.GetBoolProperty("WebPartPropertyAllowNewComment");
            if (WebPartPropertyAllowNewComment != null)
            {
                webpart.Properties["WebPartPropertyAllowNewComment"] = WebPartPropertyAllowNewComment.Value;
            }
            string WebPartPropertySpecifiedAddress = base.webpartPropertyExtractor.GetProperty("WebPartPropertySpecifiedAddress");
            if (WebPartPropertySpecifiedAddress != null)
            {
                webpart.Properties["WebPartPropertySpecifiedAddress"] = WebPartPropertySpecifiedAddress;
            }
        }
    }

    class XsltListViewWebPart : CommonWebPartPropertyUpdater
    {
        public XsltListViewWebPart(WebPartDefinition webpartDefinition, AveWebPartBaseInfo webpartBaseInfo, IWebPartPropertyExtractor webpartExtractor)
            : base(webpartDefinition, webpartBaseInfo, webpartExtractor)
        {
        }

        protected override void UpdateIndividualProperties()
        {
            bool? InplaceSearchEnabled = base.webpartPropertyExtractor.GetBoolProperty("InplaceSearchEnabled");
            if (InplaceSearchEnabled != null)
            {
                webpart.Properties["InplaceSearchEnabled"] = InplaceSearchEnabled.Value;
            }
        }
    }


    #region enum data
    internal enum FrameType
    {
        None,
        Standard,
        TitleBarOnly,
        Default,
        BorderOnly
    }
    internal enum FrameState
    {
        Minimized = 1,
        Normal = 0
    }
    #endregion

}
