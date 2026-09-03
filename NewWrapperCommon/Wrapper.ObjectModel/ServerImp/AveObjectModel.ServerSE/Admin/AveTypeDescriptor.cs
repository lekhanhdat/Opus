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


 
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.BusinessData.Administration;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveTypeDescriptor : AveAccessControlledMetadataObject, IAveTypeDescriptor
    {
        private TypeDescriptor mTypeDescriptor;
        private AveTypeDescriptorCollection mChildTypeDescriptors;

        public AveTypeDescriptor(TypeDescriptor typeDescriptor)
            : base(typeDescriptor)
        {
            mTypeDescriptor = typeDescriptor;
        }

        public IAveTypeDescriptorCollection ChildTypeDescriptors
        {
            get
            {
                if (mChildTypeDescriptors == null)
                {
                    TypeDescriptorCollection typeDescriptorCollection = mTypeDescriptor.ChildTypeDescriptors;
                    if (typeDescriptorCollection != null)
                    {
                        mChildTypeDescriptors = new AveTypeDescriptorCollection(mTypeDescriptor.ChildTypeDescriptors);
                    }
                }
                return mChildTypeDescriptors;
            }
        }
        
        public IAveTypeDescriptor GetById(uint id, IAveAdministrationMetadataCatalog metadataCatalog)
        {
            return new AveTypeDescriptor(TypeDescriptor.GetById(id, (metadataCatalog as AveAdministrationMetadataCatalog).AdministrationMetadataCatalog));
        }
    }
}
