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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveMetadataListFieldSettings : AveClientObject, IAveMetadataListFieldSettings
    {
        private IAveList m_list;
        private IAveRequest m_Requset;

        public AveMetadataListFieldSettings(IAveList list)
        {
            m_list = list;
            m_Requset = (list.ParentWeb.Site as AveSite).Request;
        }

        public bool EnableKeywordsField
        {
            get
            {
                GetSettingInfo("EnableKeywordsField");
                return base.DataCache.GetProperty<bool>("EnableKeywordsField");
            }
            set
            {
                base.DataCache.AddChangedProperty("EnableKeywordsField", value);
            }
        }

        public bool EnableMetadataPromotion
        {
            get
            {
                GetSettingInfo("EnableMetadataPromotion");
                return base.DataCache.GetProperty<bool>("EnableMetadataPromotion");
            }
            set
            {
                base.DataCache.AddChangedProperty("EnableMetadataPromotion", value);
            }
        }

        public bool ListHasKeywordsField
        {
            get
            {
                GetSettingInfo("ListHasKeywordsField");
                return base.DataCache.GetProperty<bool>("ListHasKeywordsField");
            }
        }

        public bool KeywordsFieldExistsInContentTypes(bool bAdd)
        {
            GetSettingInfo("KeywordsFieldExistsInContentTypes");
            return base.DataCache.GetProperty<bool>("KeywordsFieldExistsInContentTypes");
        }

        public void Update()
        {
            m_Requset.UpdateMetadataListFieldSettings(m_list.ParentWeb.ServerRelativeUrl, m_list.ID, base.DataCache.ChangedProperties);
        }

        private void GetSettingInfo(string propertyName)
        {
            if (base.DataCache.IsPropertyNotLoaded(propertyName))
            {
                Dictionary<string, object> metadataListFieldsettingsProp = m_Requset.GetMetadataListFieldSettings(m_list.ParentWeb.ServerRelativeUrl, m_list.Title, m_list.ID);
                base.DataCache.AddPropertyies(metadataListFieldsettingsProp);
            }
        }
    }
}
