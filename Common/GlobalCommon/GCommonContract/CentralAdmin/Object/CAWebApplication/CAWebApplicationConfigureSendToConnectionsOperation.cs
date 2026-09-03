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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAWebApplicationConfigureSendToConnectionsOperation : CAOperation
    {
        /// <summary>
        ///     Site Subscription Settings : Allow sites to send to connections outside the site subscription
        ///     true : the checkbox is uncheck;
        ///     false : the checkbox is check;
        /// </summary>
        [DataMember]
        public bool ScopeExternalConnectionsToSiteSubscriptions { get; set; }

        [DataMember]
        public List<CAWebApplicationSendToConnectionOperation> SendToConnections { get; set; }


        /// <summary>
        ///     this two attributes used in test action
        /// </summary>
        [DataMember]
        public string SendToUrl { get; set; }
        [DataMember]
        public bool IsVaild { get; set; }


        /// <summary>
        ///     used when OK button is click
        ///     Add : new connection
        ///     Update : update connection
        ///     None  : none 
        /// </summary>
        [DataMember]
        public CAAction ActionWhenOK { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAWebApplicationSendToConnectionOperation
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public string Explanation { get; set; }

        [DataMember]
        public string SendToUrl { get; set; }

        [DataMember]
        public OfficialFileAction SendToAction { get; set; }

        [DataMember]
        public bool ShowOnSendToMenu { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OfficialFileAction
    {
        [EnumMember]
        Copy = 0,
        [EnumMember]
        Move = 1,
        [EnumMember]
        Link = 2,
    }
}
