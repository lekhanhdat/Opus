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
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FilePropertiesMapping
    {
        [DataMember(EmitDefaultValue = false)]
        public List<PropertiesMappingItem> PropertiesMappingItems { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PropertiesMappingItem
    {
        [DataMember(EmitDefaultValue = false)]
        public string FileSystemProperty { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SharePointProperty { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ColumnType ColumnType { get; set; }

        //For the Lookup and MetadataColumn, we may support them later, so leave it here, 
        //If we need to support them, we can refer to the common contract : common\ModuleContract\Migration\Migration.Contract\Contract\Object\File\FileMigrationMappingsSubProfileContent.cs
        //[DataMember]//only used when ColumnType is Lookup
        //public Lookup Lookup { get; set; }

        //[DataMember]//only used when ColumnType is MetadataColumn
        //public MetadataColumn MetadataColumn { get; set; }
    }

    /// <summary>
    /// sharepoint column type
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ColumnType
    {
        [EnumMember]
        [Description("Invalid")]
        Invalid = 0,

        [Description("Single line of text")]
        [EnumMember]
        Text = 1,

        [Description("Multiple lines of text")]
        [EnumMember]
        Note = 2,

        [Description("Multiple lines of text_Plain text")]
        [EnumMember]
        PlainText = 3,

        [Description("Multiple lines of text_Rich text")]
        [EnumMember]
        RichText = 4,

        [Description("Multiple lines of text_Enhanced rich text")]
        [EnumMember]
        EnhancedRichText = 5,

        [Description("Choice_Checkboxes(allow multiple selections)")]
        [EnumMember]
        CheckBoxChoice = 6,

        [Description("Choice_Drop-Down Menu")]
        [EnumMember]
        DropDownChoice = 7,

        [Description("Choice_Radio Buttons")]
        [EnumMember]
        RadioChoice = 8,

        [Description("Number")]
        [EnumMember]
        Number = 9,

        [Description("Date and Time_Date Only")]
        [EnumMember]
        DateOnly = 10,

        [Description("Date and Time_Date & Time")]
        [EnumMember]
        DateAndTime = 11,

        [Description("Yes/No")]
        [EnumMember]
        Boolean = 12,

        [Description("Person or Group")]
        [EnumMember]
        User = 13,

        [Description("Managed Metadata")]
        [EnumMember]
        MetadataColumn = 14,

        [Description("Lookup")]
        [EnumMember]
        Lookup = 15,

        [Description("MultiChoice")]
        [EnumMember]
        MultiChoice = 16,

        [EnumMember]//don't use this value, it is about to be deleted.
        Choice = 18,

        [EnumMember]//don't use this value, it is about to be deleted.
        DateTime = 19,

        [Description("HyperLink")]
        [EnumMember]
        HyperLinkOrPicture = 20,

        [Description("Number_Show as percentage(for example, 50%)")]
        [EnumMember]
        PercentNumber = 21,

        [Description("Currency")]
        [EnumMember]
        CurrencyNumber = 22,

        [Description("All Day Event")]
        [EnumMember]
        AllDayEvent = 23,

        [Description("Calculated")]
        [EnumMember]
        Calculated = 24,
    }
}
