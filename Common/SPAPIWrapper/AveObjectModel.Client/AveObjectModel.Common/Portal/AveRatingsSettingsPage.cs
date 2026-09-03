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
    class AveRatingsSettingsPage : IAveRatingsSettingsPage
    {
        private IAveRequest m_Request;

        public bool ContainsFieldById(Guid id, IAveFieldCollection fieldColl)
        {
            throw new NotImplementedException();
        }

        public void DisableRatings(IAveList list)
        {
            if (m_Request == null)
            {
                m_Request = (list.ParentWeb.Site as AveSite).Request as IAveRequest;
            }
            m_Request.SetListRating(list.ParentWebUrl, list.RootFolder.Url, list.ID, false, true);
        }

        public bool IsAlowRated(IAveList list)
        {
            if (m_Request == null)
            {
                m_Request = (list.ParentWeb.Site as AveSite).Request as IAveRequest;
            }
            return m_Request.GetListRated(list.ParentWebUrl, list.ID);
        }


        public bool EnableRatings(IAveList list, bool repropagate, bool isLikesExp)
        {
            if (m_Request == null)
            {
                m_Request = (list.ParentWeb.Site as AveSite).Request as IAveRequest;
            }
            //return m_Request.SetListRating(list.ParentWebUrl, list.RootFolder.Url, list.ID, true);
            return m_Request.SetListRating(list.ParentWebUrl, list.RootFolder.Url, list.ID, true, isLikesExp);
        }

        //13中获得rating experience 
        public string GetListExperience(IAveList list)
        {
            if (m_Request == null)
            {
                m_Request = (list.ParentWeb.Site as AveSite).Request as IAveRequest;
            }
            return m_Request.GetListExperience(list.ParentWebUrl, list.ID);
        }

    }
}