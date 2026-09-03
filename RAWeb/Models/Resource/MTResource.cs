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
using AvePoint.RA.Contract.RoleAssignments;
using System.Collections.Generic;


namespace AvePoint.RA.Web.Models.Resource
{
	public class MTResource : BaseResource
	{
		public override List<ResourceItem> Get()
		{
			return new List<ResourceItem>()
			{
				 new ResourceItem(){
					Key = ResourceKeys.MT_PickListForLoanRequests,
					Value = ResourceKeys.MT_PickListForLoanRequests.ToUrl(RouterUrl_Root),
					Permission = RMPermissionMasks.PhysicalAdmin,
				 },
                 new ResourceItem(){
					Key = ResourceKeys.MT_PickListForDestruction,
					Value = ResourceKeys.MT_PickListForDestruction.ToUrl(RouterUrl_Root),
					Permission = RMPermissionMasks.PhysicalAdmin,
				 },
                 new ResourceItem(){
                    Key = ResourceKeys.MT_PickListForMovement,
                    Value = ResourceKeys.MT_PickListForMovement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                 },
                 new ResourceItem(){
					Key = ResourceKeys.MT_MachineLearningReview,
					Value = ResourceKeys.MT_MachineLearningReview.ToUrl(RouterUrl_Root),
					Permission = RMPermissionMasks.ManualReviewEnduser,
				 }
			};
		}
	}
}

