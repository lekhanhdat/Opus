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
using System.Text;

namespace AvePoint.RA.Common.RAProcess.Extractor
{
    public class RMPipeExtractorDefinition
    {
        public const string PIPE_NAME = "recfti";

        public const string EXTRACT_EXE_NAME = "RAExtractor.exe";

        public const string EXTRACT_DLL_NAME = "RAExtractor.dll";

        public const string EXTRACT_PROCESS_NAME = "RAExtractor";

        public const string EXTRACT_PRODUCER_MESSAGE_CONTAINER_PATH = "Producer";

        public const string EXTRACT_CONSUMER_MESSAGE_CONTAINER_PATH = "Consumer";
    }

    public class RMPipeExtractorRequestData
    {
        public string IndexDBUniqueId { get; set; }

        public string FilePath { get; set; }

        public string FileType { get; set; }

        public int LetterCountLimit { get; set; }
    }

    public class RMPipeExtractorReponseData
    {
        public bool Succeed { get; set; }

        public string ErrorMessage { get; set; }

        public string FilePath { get; set; }

        public string IndexDBUniqueId { get; set; }
    }
}
