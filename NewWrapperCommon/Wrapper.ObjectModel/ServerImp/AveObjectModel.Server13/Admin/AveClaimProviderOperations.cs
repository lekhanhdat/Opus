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
using Microsoft.SharePoint.Administration.Claims;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.WebControls;
using System.Linq;

namespace AvePoint.ObjectModel.Server13
{
    class AveClaimProviderOperations : IAveClaimProviderOperations
    {
        public AveClaimProviderOperations()
        { }

        public IAvePickerEntity[] Resolve(Uri context, AveClaimProviderOperationOptions mode, string[] providerNames, string[] entityTypes, IAveClaim resolveInput)
        {
            PickerEntity[] pickerEntities = SPClaimProviderOperations.Resolve(context, (SPClaimProviderOperationOptions)mode, providerNames, entityTypes, (resolveInput as AveClaim).Claim);
            return GetAvePickerEntitys(pickerEntities);
        }

        public IAvePickerEntity[] Resolve(Uri context, AveClaimProviderOperationOptions mode, string[] providerNames, string[] entityTypes, string resolveInput)
        {
            PickerEntity[] pickerEntities = SPClaimProviderOperations.Resolve(context, (SPClaimProviderOperationOptions)mode, providerNames, entityTypes, resolveInput);
            return GetAvePickerEntitys(pickerEntities);
        }

        internal IAvePickerEntity[] GetAvePickerEntitys(PickerEntity[] pickerEntities)
        {
            AvePickerEntity[] avePickerEntity = new AvePickerEntity[pickerEntities.Length];
            for (int i = 0; i < pickerEntities.Length; i++)
            {
                avePickerEntity[i] = new AvePickerEntity(pickerEntities[i]);
            }
            return avePickerEntity;
        }

        public IAveProviderHierarchyTree[] Search(Uri context, AveClaimProviderOperationOptions mode, string[] providerNames, string[] entityTypes, string searchPattern, int maxCount)
        {
            var trees = SPClaimProviderOperations.Search(context, (SPClaimProviderOperationOptions)mode, providerNames, entityTypes, searchPattern, maxCount);
            return trees.Select(t => t == null ? null : new AveProviderHierarchyTree(t)).ToArray();
        }
    }
}
