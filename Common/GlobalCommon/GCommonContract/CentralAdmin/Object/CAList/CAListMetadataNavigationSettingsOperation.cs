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






namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListMetadataNavigationSettingsOperation : CAOperation
    {
        [DataMember]
        public bool AutomaticallyManageListIndexing { get; set; }

        [DataMember]
        public List<CAListMetadataNavigationFieldOperation> AvailableHierarchyFields { get; set; }

        [DataMember]
        public List<CAListMetadataNavigationFieldOperation> SelectedHierarchyFields { get; set; }

        [DataMember]
        public List<CAListMetadataNavigationFieldOperation> AvailableKeyFilterFields { get; set; }

        [DataMember]
        public List<CAListMetadataNavigationFieldOperation> SelectedKeyFilterFields { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListMetadataNavigationFieldOperation : IComparable
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        public int CompareTo(object obj)
        {
            CAListMetadataNavigationFieldOperation operation = obj as CAListMetadataNavigationFieldOperation;
            if (operation != null)
            {
                //return this.Title.CompareTo(operation.Title);
                return string.Compare(this.Title, operation.Title, StringComparison.Ordinal);
            }
            else
            {
                throw new ArgumentException("Object is not a CAListMetadataNavigationFieldOperation");
            }
        }
    }
}
