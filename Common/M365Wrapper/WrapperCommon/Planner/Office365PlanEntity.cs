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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeCommonWrapper
{
    [DataContract]
    public class Office365PlanEntity
    {
        [DataMember]
        public Office365PlanBasicProperties BasicProperties { get; set; }
        [DataMember]
        public Office365PlanDetailsProperties DetailsProperties { get; set; }
        [DataMember]
        public List<Office365PlannerBucketProperties>  BucketProperties { get; set; }
    }

    [DataContract]
    public class Office365PlanBasicProperties
    {
        [DataMember]
        public string OdataEtag { get; set; }
        [DataMember]
        public string CreatedDateTime { get; set; }
        [DataMember]
        public string Owner { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Privacy { get; set; }
        [DataMember]
        public string CreateByUserName { get; set; }
        [DataMember]
        public string CreateByUserId { get; set; }
        [DataMember]
        public string CreateByApplicationName { get; set; }
        [DataMember]
        public string CreateByApplicationId { get; set; }
       
    }

    [DataContract]
    public class Office365PlanDetailsProperties
    {
        [DataMember]
        public string OdataContext { get; set; }
        [DataMember]
        public string OdataEtag { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public Dictionary<string, bool> SharedWith { get; set; }
        [DataMember]
        public PlanCategoryDescriptions CategoryDescriptions { get; set; }
        [DataMember]
        public Dictionary<string,string> CategoryDescriptionsDictionary { get; set; }
        #region beta 
        [DataMember]
        public string ContextDetailsId { get; set; }
        [DataMember]
        public string ContextDetailsType { get; set; }
        [DataMember]
        public string ContextDetailsUrl { get; set; }
        #endregion

    }

    [DataContract]
    public class Office365PlannerBucketProperties
    {
        [DataMember]
        public string OdataEtag { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string OrderHint { get; set; }
        [DataMember]
        public string Id { get; set; }
    }

    [DataContract]
    public class PlanCategoryDescriptions
    {
        [DataMember]
        public string Category1 { get; set; }

        [DataMember]
        public string Category2 { get; set; }

        [DataMember]
        public string Category3 { get; set; }

        [DataMember]
        public string Category4 { get; set; }

        [DataMember]
        public string Category5 { get; set; }

        [DataMember]
        public string Category6 { get; set; }
    }
}