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


namespace AvePoint.GCommon.Contract.Media.TCPRequest.Restore
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    /// <summary>
    /// TODO VDB重构之后，这个类不再使用，media端已经去掉了相关引用，agent端去掉之后，可以删除
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StartMediaVdbFileServiceResult
    {
        [DataMember]
        public String MediaVdbWcfServiceUrl { get; set; }

        [DataMember]
        public Boolean MappingSucceed { get; set; }

        [DataMember]
        public String AgentFolderOnMedia { get; set; }

        private Dictionary<String, String> errorMsg = new Dictionary<String, String>();

        [DataMember]
        public Dictionary<String, String> ErrorMsg
        {
            get
            {
                return errorMsg;
            }
            set
            {
                errorMsg = value;
            }
        }

        public override String ToString()
        {
            return String.Format("Media Vdb Wcf Service Url: {0}, Mapping Succeed: {1}, Agent Folder On Media: {2}",
                this.MediaVdbWcfServiceUrl,
                this.MappingSucceed,
                this.AgentFolderOnMedia);
        }
    }
}
