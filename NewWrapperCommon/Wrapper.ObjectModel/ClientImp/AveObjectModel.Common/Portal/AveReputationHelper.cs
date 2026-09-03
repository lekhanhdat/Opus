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
using AvePoint.ObjectModel.ClientOM;

namespace AvePoint.ObjectModel.Common
{
    public class AveReputationHelper : IAveReputationHelper
    {
        public bool ContainsFields(IAveList list, List<KeyValuePair<Guid, string>> fields)
        {
            throw new NotImplementedException();
        }

        public void DisableReputation(IAveList list)
        {
            ((list as AveList).Request ).SetListRateSetting(list.ParentWebUrl, list.DefaultViewUrl, list.ID, false, null);
        }

        public void EnableReputation(IAveList list, string experience, bool upgrade = false)
        {
            ((list as AveList).Request ).SetListRateSetting(list.ParentWebUrl, list.DefaultViewUrl, list.ID, true, experience);
        }

        public string GetExperience(IAveList list, bool addProperty)
        {
            return ((list as AveList).Request ).GetListExperience(list.ParentWebUrl, list.ID);
        }

        public void SwitchReputation(IAveList list, string newExperience, string oldExperience)
        {
            ((list as AveList).Request ).SetListRateSetting(list.ParentWebUrl, list.DefaultViewUrl, list.ID, true, newExperience);
        }
    }
}
