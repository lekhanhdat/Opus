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

namespace RAExportCommon
{
    public class EXOFileVEOParameters
    {

        public string VTitle { get; set; }
        //public string VItemUrl { get; set; }
        //public Dictionary<string, object> VFields { get; set; }

        public string VFileID { get; set; }
        public string VParentVEOID { get; set; }
        //public string VLibraryID { get; set; }
        public string VCreatedTime { get; set; }
        public string VModifiedTime { get; set; }
        public string VAuthor { get; set; }
        public string VEditor { get; set; }
        //public string VRelationType { get; set; }
        //public string VContentType { get; set; }
        //public string VSecurityClassification { get; set; }
        //public string VDisposalAuthrisation { get; set; }
        //public string VSentence { get; set; }


        public EXOFileVEOParameters(string Title, string FileID, string ParentVEOID, string CreatedTime, string ModifiedTime, string Author, string Editor)
        {
            this.VTitle = Title;
            this.VFileID = FileID;
            this.VParentVEOID = ParentVEOID;
            this.VCreatedTime = CreatedTime;
            this.VModifiedTime = ModifiedTime;
            this.VAuthor = Author;
            this.VEditor = Editor;
            //this.VRelationType = RelationType;
            //this.VContentType = ContentType;
            //CTMapping mappingName = VCTConfigurationManager.GetMappingName(ContentType);
            //if (mappingName == null)
            //{
            //    mappingName = VCTConfigurationManager.GetMappingName("Default");
            //    if (mappingName == null)
            //    {
            //        throw new Exception("Can not get the default mapping from config file.");
            //    }
            //}
            //this.VSecurityClassification = mappingName.SecurityClassification;
            //this.VDisposalAuthrisation = mappingName.DisposalAuthrisation;
            //this.VSentence = mappingName.Sentence;
        }
    }
}
