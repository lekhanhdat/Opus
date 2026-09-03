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
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListRecordDeclarationSettingsOperation : CAOperation
    {
        /// <summary>
        ///     site collection In Place Records Management feature 
        ///     featureId = da2e115b-07e4-49d9-bb2c-35e93bb9fca9
        /// </summary>
        [DataMember]
        public bool IsFeatureActive { get; set; }

        [DataMember]
        public bool ActiveFeature { get; set; }

        /// <summary>
        ///     Manual Record Declaration Availability 
        ///         0 - Use the site collection default setting:  Do not allow the manual declaration of records  
        ///         1 - Always allow the manual declaration of records
        ///         2 - Never allow the manual declaration of records
        /// </summary>
        [DataMember]
        public int ManualAvailability { get; set; }

        /// <summary>
        ///     Automatic Declaration 
        /// </summary>
        [DataMember]
        public bool AutomaticDeclaration { get; set; }
    }
}
