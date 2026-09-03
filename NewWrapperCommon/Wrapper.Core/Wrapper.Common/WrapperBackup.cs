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

namespace AvePoint.Wrapper.Backup
{
    public class FullTextIndex
    {
        public string VersionComment { get; set; }

        public string CreatedByDisplayName { get; set; }

        public string CreatedByLoginName { get; set; }

        public string ModifiedByDisplayName { get; set; }

        public string ModifiedByLoginName { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public DateTime Accessed { get; set; }

        public string TimeZoneInfoID { get; set; }

        public string Title { get; set; }

        #region Only for Archiver

        public string ArchiveBy { get; set; }

        public DateTime ArchiveTime { get; set; }

        #endregion Only for Archiver

        public int Size { get; set; }

        public string ContentTypeName { get; set; }

        public List<string> Attachments { get; set; }

        public Dictionary<string, object> ColumnValues { get; set; }

        public void SetCustomColumnValues(Dictionary<string, object> customColumnValues)
        {
            if (customColumnValues == null)
            {
                throw new ArgumentNullException("customColumnValues");
            }
            if (this.ColumnValues == null)
            {
                this.ColumnValues = new Dictionary<string, object>(customColumnValues.Count);
            }
            customColumnValues.ToList().ForEach(
                            pair =>
                            {
                                switch (pair.Key)
                                {
                                    //ArchiveBy,ArchiveTime特殊处理一下
                                    case AveWrapperConstants.ARCHIVE_BY:
                                        this.ArchiveBy = pair.Value.ToString();
                                        break;
                                    case AveWrapperConstants.ARCHIVE_TIME:
                                        this.ArchiveTime = (DateTime)pair.Value;
                                        break;
                                    default:
                                        this.ColumnValues[pair.Key] = pair.Value;
                                        break;
                                }
                            });//Overwrite field
        }
    }

    public class AveSPField
    {
        /// <summary>
        /// IAveField.ID
        /// </summary>
        public Guid FieldId { get; private set; }
        
        public bool IsDisplayColumn { get; private set; }

        /// <summary>
        /// IAveField.ColName
        /// </summary>
        public string ColumnName { get; private set; }
        /// <summary>
        /// IAveField.InternalName
        /// </summary>
        public string BackupName { get; private set; }

        [Obsolete("Use FieldTypeAsString instead.")]
        public AveFieldType FieldType { get; private set; }
        /// <summary>
        /// IAveField.TypeAsString
        /// </summary>
        public string FieldTypeAsString { get; private set; }

        public bool IsHidden { get; private set; }

        /// <summary>
        /// IAveField.Title
        /// </summary>
        public string DisplayName { get; private set; }

        public AveSPField() { }

        public AveSPField(Guid columnId,string backupName, string displayName, string columnName, bool isDisplayColumn, bool isHidden)
            : this(columnId,backupName, displayName, columnName, isDisplayColumn, isHidden, AveFieldType.Invalid) { }

        public AveSPField(Guid columnId, string backupName, string displayName, string columnName, bool isDisplayColumn, bool isHidden, AveFieldType fieldType, string fieldTypeAsString = "Invalid")
        {
            this.BackupName = backupName;
            this.ColumnName = columnName;
            this.IsDisplayColumn = isDisplayColumn;
            this.IsHidden = isHidden;
            this.FieldType = fieldType;
            this.FieldTypeAsString = fieldTypeAsString;
            this.DisplayName = displayName;
            this.FieldId = columnId;
        }
    }

    public class FieldInternalTypeAndGuiTypeMapping
    {
        //Oliver:只在静态构造方法中初始化
        private static readonly Dictionary<string, string> typeMappings = new Dictionary<string, string>();

        private static void InitializeTypeMappings()
        {
            typeMappings.Add("Text", "Single line of text");
            typeMappings.Add("Note", "Multiple lines of text");
            typeMappings.Add("Choice", "Choice (menu to choose from)");
            typeMappings.Add("MultiChoice", "Choice (menu to choose from)_AllowMultiple");
            typeMappings.Add("Number", "Number (1, 1.0, 100)");
            typeMappings.Add("Currency", "Currency ($, ¥, €)");
            typeMappings.Add("DateTime", "Date and Time");
            typeMappings.Add("Lookup", "Lookup (information already on this site)");
            typeMappings.Add("LookupMulti", "Lookup (information already on this site)_AllowMultiple");
            typeMappings.Add("Boolean", "Yes/No (check box)");
            typeMappings.Add("User", "Person or Group");
            typeMappings.Add("UserMulti", "Person or Group_AllowMultiple");
            typeMappings.Add("URL", "Hyperlink or Picture");
            typeMappings.Add("Calculated", "Calculated (calculation based on other columns)");
            typeMappings.Add("TaxonomyFieldType", "Managed Metadata");
            typeMappings.Add("TaxonomyFieldTypeMulti", "Managed Metadata_AllowMultiple");
        }

        public static string GetGuiTypeByInternalType(string internalType)
        {
            if (typeMappings.Count == 0)
            {
                InitializeTypeMappings();
            }
            if (typeMappings.ContainsKey(internalType))
            {
                return typeMappings[internalType];
            }
            else
            {
                return internalType;
            }
        }
    }

    public class FieldInfoForExcel
    {
        public string Title { get; set; }

        public string TypeAsString { get; set; }

        public string TitleAndGuiType { get; set; }
    }

    public class AveSPListContentTypes
    {
        private Dictionary<string, string> mContentTypes = null;

        public AveSPListContentTypes()
        {
            mContentTypes = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public void Add(string contentTypeId, string name)
        {
            mContentTypes[contentTypeId] = name;
        }

        public bool TryGet(string contentTypeId, out string name)
        {
            name = string.Empty;
            if (mContentTypes.ContainsKey(contentTypeId))
            {
                name = mContentTypes[contentTypeId];
                return true;
            }
            return false;
        }

        public bool TryGet(byte[] contentTypeId, out string name)
        {
            string id = ConvertBytesToHex(contentTypeId);
            return TryGet(id, out name);
        }

        public bool Contains(string contentTypeId)
        {
            return mContentTypes.ContainsKey(contentTypeId);
        }

        public bool Contains(byte[] contentTypeId)
        {
            string id = ConvertBytesToHex(contentTypeId);
            return Contains(id);
        }

        private string ConvertBytesToHex(byte[] bts)
        {
            StringBuilder sb = new StringBuilder("0x");
            foreach (byte b in bts)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}
