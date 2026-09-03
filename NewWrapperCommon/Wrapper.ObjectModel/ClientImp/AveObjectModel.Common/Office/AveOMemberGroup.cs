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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOMemberGroup:AveClientObject,IAveOMemberGroup
    {
        public AveOMemberGroup(Dictionary<string,object>memberGroupProp )
        {
            base.DataCache.AddPropertyies(memberGroupProp);
        }
        public long Count 
        {
            get { return base.DataCache.GetProperty<long>("Count"); }
        }
        public string Description 
        {
            get { return base.DataCache.GetProperty<string>("Description"); }
            set { base.DataCache.AddChangedProperty("Description",value); }
        }
        public string DisplayName 
        {
            get { return base.DataCache.GetProperty<string>("DisplayName"); }
            set { base.DataCache.AddChangedProperty("DisplayName",value); }
        }
        public long Id 
        {
            get { return base.DataCache.GetProperty<long>("Id"); }
        }
        public DateTime LastUpdate 
        {
            get { return base.DataCache.GetProperty<DateTime>("LastUpdate"); }
        }
        public string MailNickName 
        {
            get { return base.DataCache.GetProperty<string>("MailNickName"); }
            set { base.DataCache.AddChangedProperty("MailNickName",value); }
        }
        public Uri PublicUrl 
        {
            get { return base.DataCache.GetProperty<Uri>("PublicUrl"); }
        }
        public AveMembershipSource Source 
        {
            get { return base.DataCache.GetProperty<AveMembershipSource>("Source"); }
        }
        public Guid SourceInternal 
        {
            get { return base.DataCache.GetProperty<Guid>("SourceInternal"); }
        }
        public string SourceReference 
        {
            get { return base.DataCache.GetProperty<string>("SourceReference"); }
            set { base.DataCache.AddChangedProperty("SourceReference",value); }
        }
        public string Url 
        {
            get { return base.DataCache.GetProperty<string>("Url"); }
            set { base.DataCache.AddChangedProperty("Url",value); }
        }


        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
