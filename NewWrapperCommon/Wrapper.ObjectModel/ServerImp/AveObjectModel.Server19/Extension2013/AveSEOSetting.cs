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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    public class AveSEOSetting:IAveSEOSetting
    {
        private IAveSite mAveSite;
        public AveSEOSetting(IAveSite aveSite)
        {
            mAveSite = aveSite;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Seo")]
        public AveSEOSettings GetObjectData()
        {
            AveSEOSettings settings = new AveSEOSettings();
            if (mAveSite.Features[AveSP2010FeatureDefinitions.PublishingSite] != null)
            {
                
                var SEOSetting = AveAssemblyUtility.InvokeStaticMethod("Microsoft.SharePoint.Publishing.Internal.SeoSettingsFactory", "CreateSeoSettings", new object[] { ((AveSite)mAveSite).Site });
                settings.IncludeCustomMetaTag = (bool)AveAssemblyUtility.GetPropertyValue(SEOSetting, "IncludeCustomMetaTag");
                settings.CanonicalLinkParametersEnabled = (bool)AveAssemblyUtility.GetPropertyValue(SEOSetting, "CanonicalLinkParametersEnabled");
                settings.CustomMetaTag = (string)AveAssemblyUtility.GetPropertyValue(SEOSetting, "CustomMetaTag");
                settings.CanonicalLinkParameters = (string)AveAssemblyUtility.GetPropertyValue(SEOSetting, "CanonicalLinkParameters");
                
            }
            return settings;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Seo")]
        public void SetObjectData(AveSEOSettings settings)
        {
            if (mAveSite.Features[AveSP2010FeatureDefinitions.PublishingSite] != null)
            {
                var SEOSettings = AveAssemblyUtility.InvokeStaticMethod("Microsoft.SharePoint.Publishing.Internal.SeoSettingsFactory", "CreateSeoSettings", new object[] { ((AveSite)mAveSite).Site });


                AveAssemblyUtility.SetPropertyValue(SEOSettings, "IncludeCustomMetaTag", settings.IncludeCustomMetaTag);
                AveAssemblyUtility.SetPropertyValue(SEOSettings, "CustomMetaTag", settings.CustomMetaTag);
                AveAssemblyUtility.SetPropertyValue(SEOSettings, "CanonicalLinkParametersEnabled", settings.CanonicalLinkParametersEnabled);
                AveAssemblyUtility.SetPropertyValue(SEOSettings, "CanonicalLinkParameters", settings.CanonicalLinkParameters);
                AveAssemblyUtility.InvokeMethod(SEOSettings, "Update");
                AveAssemblyUtility.InvokeMethod(SEOSettings, "Dispose");
            }
            
        }
    }
}
