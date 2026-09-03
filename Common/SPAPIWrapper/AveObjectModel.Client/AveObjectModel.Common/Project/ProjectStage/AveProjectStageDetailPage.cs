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
using System.Threading.Tasks;

using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveProjectStageDetailPage : AveClientObject, IAveProjectStageDetailPage
    {
        private IAveRequest mRequest;
        private IAveProjectDetailPage mPage;

        public AveProjectStageDetailPage(IAveRequest request, Dictionary<string, object> prop)
        {
            this.mRequest = request;
            base.DataCache.AddPropertyies(prop);
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }

            set
            {
                base.DataCache.AddChangedProperty("Description", value);
            }
        }

        public IAveProjectDetailPage Page
        {
            get
            {
                if (this.mPage == null)
                {
                    var pageProp = base.DataCache.GetProperty<Dictionary<string, object>>("Page");
                    this.mPage = new AveProjectDetailPage(this.mRequest, pageProp);
                    base.DataCache.RemoveProperty("Page");
                }
                return this.mPage;
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public int Position
        {
            get
            {
                return base.DataCache.GetProperty<int>("Position");
            }

            set
            {
                base.DataCache.AddChangedProperty("Position", value);
            }
        }

        public bool RequiresAttention
        {
            get
            {
                return base.DataCache.GetProperty<bool>("RequiresAttention");
            }

            set
            {
                base.DataCache.AddChangedProperty("RequiresAttention", value);
            }
        }
    }
}
