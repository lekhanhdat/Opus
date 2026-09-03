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
    public class EXORecordVEOParameters
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

        public EXORecordVEOParameters(string ItemID, string ItemUrl, string FileName, string LibraryID, string WebLanguage, Dictionary<string, object> fields)
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

        public EXORecordVEOParameters(string ItemID, string ItemUrl, string FileName, string LibraryID, string LibraryName, string WebLanguage, Dictionary<string, object> fields, string ParenetFileIdentifier, string ContentType)
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
}
