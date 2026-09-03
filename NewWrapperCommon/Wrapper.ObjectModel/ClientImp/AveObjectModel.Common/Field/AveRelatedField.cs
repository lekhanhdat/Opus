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
    class AveRelatedField : AveClientObject, IAveRelatedField
    {
        private IAveRequest m_Request;
        private AveList m_AveList;
        private AveRelatedFieldCollection m_AveRelatedFieldCollection;

        public AveRelatedField(IAveRequest m_Request, AveList m_AveList, AveRelatedFieldCollection aveRelatedFieldCollection, Dictionary<string, object> relatedFieldProperties)
        {
            this.m_Request = m_Request;
            this.m_AveList = m_AveList;
            this.m_AveRelatedFieldCollection = aveRelatedFieldCollection;
            base.DataCache.AddPropertyies(relatedFieldProperties);
        }
        #region IAveRelatedField Members

        public AveRelationshipDeleteBehavior RelationshipDeleteBehavior
        {
            get
            {
                return base.DataCache.GetProperty<AveRelationshipDeleteBehavior>("RelationshipDeleteBehavior");
            }
        }

        public Guid ListId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("ListId");
            }
        }

        public Guid FieldId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("FieldId");
            }
        }

        #endregion
    }
}