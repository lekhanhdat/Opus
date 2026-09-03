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




namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRVssProviderDto
    {
        /// <summary>
        /// Class identifier of the component registered in the local machine's COM catalog.
        /// </summary>
        [DataMember]
        public Guid ClassId { get; set; }

        ///<summary>
        ///Identifies the provider who supports shadow copies of this class.
        ///</summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Id")]
        public Guid ProviderId { get; set; }

        /// <summary>
        /// The provider name.
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Name")]
        public string ProviderName { get; set; }

        /// <summary>
        /// The provider type. See Alphaleonis.Win32.Vss.VssProviderType for more information.
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Type")]
        public VssProviderType ProviderType { get; set; }

        /// <summary>
        /// The provider version in readable format.
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Version")]
        public string ProviderVersion { get; set; }
        /// <summary>
        /// A System.Guid uniquely identifying the version of a provider.
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "VersionId")]
        public Guid ProviderVersionId { get; set; }
        /// <summary>
        /// compare by ProviderId & ProviderVersionId
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is PRVssProviderDto)
            {
                if (this.ProviderId == (obj as PRVssProviderDto).ProviderId
                    && this.ProviderVersionId == (obj as PRVssProviderDto).ProviderVersionId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ProviderId.GetHashCode();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum VssProviderType
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        System = 1,
        [EnumMember]
        Software = 2,
        [EnumMember]
        Hardware = 3,
    }
}
