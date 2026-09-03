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
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Archiver
{
    public class DataOptimizationSettingsForJobHistory
    {
        public ScopeSettings ScopeSettings { get; set; }
        public DefinitionAndActionSettings DefinitionAndActionSettings { get; set; }
        public DataOptimizationSettingsForJobHistory()
        {
            this.ScopeSettings = new()
            {
                WithoutInDateDataInfos = new(),
                SizeRangeDataInfos = new(),
                FileExtensionDataInfos = new()
            };
            this.DefinitionAndActionSettings = new();
        }
    }

    public class ScopeSettings
    {
        public MS365DataType MS365DataType { get; set; }

        public string ModifiedTimeRangeStr { get; set; }

        public List<RMDiscoveryWithoutInDateDataInfo> WithoutInDateDataInfos { get; set; }
        public RMDiscoveryOffice365WithoutDateQueryParameter WithoutDateQueryParameter { get; set; }
        public string SizeRangeStr { get; set; }
        public RMDiscoverySizeRangeDataInfo SizeRangeDataInfos { get; set; }
        public RMDiscoveryOffice365SizeRangeQueryParameter SizeRangeQueryParameter { get; set; }
        public string FileCatagorysStr { get; set; }
        public List<RMDiscoveryFileExtensionDataInfo> FileExtensionDataInfos { get; set; }
    }
    public class DefinitionAndActionSettings
    {
        public InactiveRuleQueryParameter InactiveRuleQueryParameter { get; set; }
        public ROTRuleQueryParameter ROTRuleQueryParameter { get; set; }
        public string DefinitionsStr { get; set; }
        public string DefinitionsJson { get; set; }
        public int ArchiveDataType { get; set; }
        public ProcessActionParameter ProcessActionParameter { get; set; }
        public string DocumentActionStr { get; set; }
        public string DocumentVersionActionStr { get; set; }
    }
}
