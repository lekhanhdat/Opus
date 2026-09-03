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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RAExportCommon
{
    public class RecordVEOParameters
    {

        public string VItemID { get; set; }
        public string VItemUrl { get; set; }
        public Dictionary<string, object> VFields { get; set; }
        public string VWebLanguage { get; set; }
        public string VFileName { get; set; }
        public string VLibraryName { get; set; }
        public string VLibraryID { get; set; }
        public string VParenetFileIdentifier { get; set; }
        public CustomerMapping VCustomerMapping { get; set; }
        //public string VContentType { get; set; }
        //public string VSecurityClassification { get; set; }
        //public string VDisposalAuthrisation { get; set; }
        //public string VSentence { get; set; }
        public List<UsageConditionChange> VUsageConditionChanges { get; set; }

        public RecordVEOParameters(string ItemID, string ItemUrl, string FileName, string LibraryID, string WebLanguage, Dictionary<string, object> fields, string ContentType, RecordVEOXML recordVEOXML)
        {
            this.VItemID = ItemID;
            this.VItemUrl = ItemUrl;
            this.VFileName = FileName;
            this.VWebLanguage = WebLanguage;
            this.VFields = fields;
            this.VLibraryID = LibraryID;
            this.VCustomerMapping = new CustomerMapping(fields);


            //this.VContentType = ContentType;


            //CTMapping m = VCTConfigurationManager.GetMappingName(ContentType);
            //if (m == null)
            //{
            //    m = VCTConfigurationManager.GetMappingName("Default");
            //    if (m == null)
            //    {

            //    }
            //}
            //VSecurityClassification = m.SecurityClassification;
            //VDisposalAuthrisation = m.DisposalAuthrisation;
            //VSentence = m.Sentence;
        }

        public RecordVEOParameters(string ItemID, string ItemUrl, string FileName, string LibraryID, string LibraryName, string WebLanguage, Dictionary<string, object> fields, string ParenetFileIdentifier, string ContentType)
        {
            this.VItemID = ItemID;
            this.VItemUrl = ItemUrl;
            this.VFileName = FileName;
            this.VWebLanguage = WebLanguage;
            this.VFields = fields;
            this.VLibraryID = LibraryID;
            this.VLibraryName = LibraryName;
            this.VCustomerMapping = new CustomerMapping(fields);
            this.VParenetFileIdentifier = ParenetFileIdentifier;
            //this.VContentType = ContentType;
            //CTMapping m = VCTConfigurationManager.GetMappingName(ContentType);
            //if (m == null)
            //{
            //    m = VCTConfigurationManager.GetMappingName("Default");
            //    if (m == null)
            //    {

            //    }
            //}
            //VSecurityClassification = m.SecurityClassification;
            //VDisposalAuthrisation = m.DisposalAuthrisation;
            //VSentence = m.Sentence;
        }

        public FileFormatType GetFileFormatType()
        {
            OrdinalIgnoreCaseComparison oc = new OrdinalIgnoreCaseComparison();
            string mExtensionName = NameFactory.GetExtensionName(VFileName);
            List<string> offices = new List<string>();
            offices.Add("doc");
            offices.Add("xls");
            offices.Add("ppt");
            offices.Add("docx");
            offices.Add("xlsx");
            offices.Add("pptx");
            if (mExtensionName.Equals("PDF", StringComparison.OrdinalIgnoreCase))
            {
                return FileFormatType.PDF;
            }
            else if (offices.Contains(mExtensionName, oc))
            {
                return FileFormatType.Office;
            }
            else
            {
                return FileFormatType.None;
            }
        }

        private class OrdinalIgnoreCaseComparison : IEqualityComparer<string>
        {

            public bool Equals(string x, string y)
            {
                if (x.Equals(y, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }

    }

    public class CustomerMapping
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public string Title;
        public string Description;
        public string ReferenceNo;
        public string ReferenceType;
        public string DateReceived;
        public string DateOfOriginal;
        public string OriginatingAuthor;
        public string ReviewDate;
        public string EventName;
        public string PeopleInImage;
        public string PeopleInVideo;
        public string LocationType;
        public string LocationValue;
        public string URL;
        public string Copyright;
        public string CopyrightLicenceName;
        public string CopyrightLicenceType;
        public string Section;
        public string SubSection;
        public string Agency;
        public string Group;
        public string Division;
        public string Branch;
        public string SecurityClassification;
        public string DisseminatedLineMarker;
        public string Language;
        public string Resolution;
        public string Duration;
        public string Description2;
        public string FileName;
        public string VersionNo;
        public string CreatedBy;
        public string Date;
        public string Time;
        public string ModifiedBy;
        public string LastModifiedDateTime;
        public string Disposal;
        public string Format;
        public string DocumentId;
        public string VEOMetadata;
        public List<string> M40;


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public CustomerMapping(Dictionary<string, object> fields)
        {
            if (fields == null)
                throw new ArgumentNullException();

            M40 = new List<string>();

            foreach (KeyValuePair<string, object> pair in fields)
            {
                string temp = string.Empty;
                if (pair.Key != string.Empty && pair.Value != null && pair.Value.ToString() != string.Empty)
                {
                    try
                    {
                        if (pair.Value is Byte[])
                        {
                            temp = GetTCompressedString((Byte[])pair.Value);
                        }
                        else if (pair.Value is DateTime)
                        {
                            temp = VaultCover.ConverTimeToLocal((DateTime)pair.Value).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        }
                        else
                        {
                            temp = pair.Value.ToString();
                        }

                        //if (pair.Key.Equals("Title", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.Title = temp;
                        //}
                        if (pair.Key.Equals("Description", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Description = temp;
                        }
                        else if (pair.Key.Equals("Reference No.", StringComparison.OrdinalIgnoreCase))
                        {
                            this.ReferenceNo = temp;
                        }
                        else if (pair.Key.Equals("Reference Type", StringComparison.OrdinalIgnoreCase))
                        {
                            this.ReferenceType = temp;
                        }
                        //else if (pair.Key.Equals("Date Received", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.DateReceived = temp;
                        //}
                        else if (pair.Key.Equals("Section", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Section = temp;
                        }
                        else if (pair.Key.Equals("Sub-Section", StringComparison.OrdinalIgnoreCase))
                        {
                            this.SubSection = temp;
                        }
                        else if (pair.Key.Equals("Agency", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Agency = temp;
                        }
                        else if (pair.Key.Equals("Group", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Group = temp;
                        }
                        else if (pair.Key.Equals("Division", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Division = temp;
                        }
                        else if (pair.Key.Equals("Branch", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Branch = temp;
                        }
                        else if (pair.Key.Equals("Security Classification", StringComparison.OrdinalIgnoreCase))
                        {
                            this.SecurityClassification = temp;
                        }
                        else if (pair.Key.Equals("Disseminated Line Marker", StringComparison.OrdinalIgnoreCase))
                        {
                            this.DisseminatedLineMarker = temp;
                        }
                        else if (pair.Key.Equals("Language", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Language = temp;
                        }
                        else if (pair.Key.Equals("File Name", StringComparison.OrdinalIgnoreCase))
                        {
                            this.FileName = temp;
                        }
                        //else if (pair.Key.Equals("Version No", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.VersionNo = temp;
                        //}
                        //else if (pair.Key.Equals("Created By", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.CreatedBy = temp;
                        //}
                        //else if (pair.Key.Equals("Date", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.Date = temp;
                        //}
                        //else if (pair.Key.Equals("Time", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.Time = temp;
                        //}
                        //else if (pair.Key.Equals("Modified By", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.ModifiedBy = temp;
                        //}
                        //else if (pair.Key.Equals("Last Modified date/Time", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.LastModifiedDateTime = temp;
                        //}
                        //else if (pair.Key.Equals("Disposal", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.Disposal = temp;
                        //}
                        else if (pair.Key.Equals("Format", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Format = temp;
                        }
                        //else if (pair.Key.Equals("Document Id", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    this.DocumentId = temp;
                        //}
                        else
                        {
                            this.M40.Add(string.Format("{0}:{1}", pair.Key, temp));
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occurred while setting Customer Mapping.error is {0}", e.ToString());
                    }
                }
            }
        }

        private string GetTCompressedString(byte[] buffer)
        {
            string str;
            int count = 0;
            for (int i = 3; i >= 0; i--)
            {
                count = count << 8;
                count += buffer[i + 8];
            }
            byte[] buffer2 = new byte[count];
            using (MemoryStream stream = new MemoryStream(buffer, 12, buffer.Length - 12))
            {
                using (DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Decompress))
                {
                    //读取前两个byte
                    stream.ReadByte();
                    stream.ReadByte();

                    int bRead = 0;
                    while (bRead < count)
                    {
                        int rd = stream2.Read(buffer2, bRead, count - bRead);
                        if (rd == -1)
                        {
                            throw new IOException("file is unusually small");
                        }
                        bRead += rd;
                    }
                    str = Encoding.UTF8.GetString(buffer2);
                }
            }
            return str;
        }
    }

    public class UsageConditionChange
    {
        public string mModifiedUser { get; private set; }
        public string mModifiedTime { get; private set; }
        public string mValue { get; private set; }

        public UsageConditionChange(string Value, string ModifiedTime, string ModifiedUser)
        {
            mModifiedUser = ModifiedUser;
            mModifiedTime = ModifiedTime;
            mValue = Value;
        }

        public string GenerateDLMString()
        {
            if (!string.IsNullOrEmpty(mModifiedUser))
            {
                return string.Format("The value of Disseminated Line Marker has been changed to {0} by {1} at {2}", mValue, mModifiedUser, mModifiedTime);
            }
            else
            {
                return string.Format("The value of Disseminated Line Marker has been changed to {0} at {1}", mValue, mModifiedTime);
            }
        }
    }
}
