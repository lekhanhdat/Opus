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




namespace AvePoint.GCommon.Contract
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.SharePointBrowser;
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
    #endregion

    /*********************************************************************
     *       This class will be used as the top level entry of 
     *       the docave6 communication system
     ********************************************************************/

    /*********************************************************************
     *    Please put the known type in alphabet order, thanks
     ********************************************************************/
    #region Browser
    [KnownType(typeof(BrowserMessage))]
    #endregion

    #region Central Admin
    [KnownType(typeof(CAMessage))]
    #endregion

    #region CheckUsers
    [KnownType(typeof(CheckUsersMessage))]
    #endregion

    #region Media
    //[KnownType(typeof(MediaMessage))]
    #endregion
  
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CoreMessage 
    {
        [DataMember]
        public String Extension { get; set; }

        [DataMember]
        public CoreServiceInvocationContext InvocationContext { get; set; }
    }
}

