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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server13
{
    class AveSkuUpgradeJob : AveJobDefinition, IAveSkuUpgradeJob
    {
        private const string mSkuUpgradeJob_Type = "Microsoft.SharePoint.Portal.Administration.SkuUpgradeJob";
        private object mSkuUpgradeJob;

        public AveSkuUpgradeJob()
            : this(AveAssemblyUtility.CreateInstance(mSkuUpgradeJob_Type, new Type[] { }, new object[] { }))
        { }

        public AveSkuUpgradeJob(string name, IAveService service)
            : this(AveAssemblyUtility.CreateInstance(mSkuUpgradeJob_Type, new Type[] { typeof(string), typeof(SPService) }, new object[] { name, (service as AveService).Service }))
        { }

        public AveSkuUpgradeJob(object SkuUpgradeJob)
            : base(SkuUpgradeJob as SPJobDefinition)
        {
            mSkuUpgradeJob = SkuUpgradeJob;
        }

        public Guid FromProduct
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetPropertyValue(mSkuUpgradeJob, "FromProduct");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSkuUpgradeJob, "FromProduct", value);
            }
        }

        public Guid ToProduct
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetPropertyValue(mSkuUpgradeJob, "ToProduct");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSkuUpgradeJob, "ToProduct", value);
            }
        }
    }
}
