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
    public class CAListColumnDefaultValueOperation : CAOperation
    {
        [DataMember]
        public Dictionary<CAFolderNodeInfo, List<CAListDefaultColunmValue>> ListColumnDefaultValues { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFolderNodeInfo
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string ParentId { get; set; }

        [DataMember]
        public bool IsUniqe { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListDefaultColunmValue
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string InternalName { get; set; }

        [DataMember]
        public string TypeDisplayName { get; set; }

        /// <summary>
        ///     if the column type is Managed Metadata,  the DefaultValue format is like "5;#wwww|c2363788-64f5-4f41-875f-813c1706766c" ,  the "www" is the display name
        ///     if the column type is Date Time, the DefaultValue is "[Today]" or UniversalTime, the date format is "yyyy-MM-ddTHH:mm:ssZ", just like defaultValue = dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") 
        ///     if the column type is Yes/No, the DefaulValue is Yes or No
        /// </summary>
        [DataMember]
        public string DefaultValue { get; set; }

        [DataMember]
        public List<CAListTermOperation> MetadataDefaultValues { get; set; }

        [DataMember]
        public bool HasDefault { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public string UsedIn { get; set; }

        [DataMember]
        public bool AllowEmpty { get; set; }

        /// <summary>
        ///     userd by Managed Metadata Type Column, is the default can select Mutiple values in the termset tree .
        /// </summary>
        [DataMember]
        public bool Mutiple { get; set; }

        [DataMember]
        public bool Required { get; set; }

        /// <summary>
        ///     userd by Managed Metadata Type Column, load termset tree
        /// </summary>
        [DataMember]
        public CAListTermSetOperation TermSet { get; set; }

        [DataMember]
        public DefaultColunmFieldType DefaultColunmFieldType { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListTermSetOperation
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<CAListTermOperation> Terms { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListTermOperation
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string InternalId { get; set; }

        [DataMember]
        public string Id { get; set; }

        // root : Depth = 0
        [DataMember]
        public int Depth { get; set; }

        [DataMember]
        public List<CAListTermOperation> Terms { get; set; }
    }


    // Summary:
    //     Specifies a field type for a field.
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DefaultColunmFieldType
    {
        [EnumMember]
        Other = 0,
        [EnumMember]
        DateTime = 1,

        [EnumMember]
        Bool = 2,

        [EnumMember]
        Metadata = 3
    }

}
