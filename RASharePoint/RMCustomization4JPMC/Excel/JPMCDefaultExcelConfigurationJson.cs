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
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Excel
{
    public class JPMCDefaultExcelConfigurationJson
    {
        public static string Default_JSON_String = @"{
          ""SheetConfigs"": [
            {
              ""SheetName"": ""Site Stats"",
              ""Columns"": [
                {
                  ""ConfigKey"": ""Site ID"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site ID""
                },
                {
                  ""ConfigKey"": ""Site Name"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site Name""
                },
                {
                  ""ConfigKey"": ""Site URL"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site URL""
                },
                {
                  ""ConfigKey"": ""Site Status"",
                  ""PropertyName"": ""Site Status"",
                  ""DisplayName"": ""Site Status""
                },
                {
                  ""ConfigKey"": ""Information Owner"",
                  ""PropertyName"": ""Information Owner"",
                  ""DisplayName"": ""Primary Information Owner""
                },
                {
                  ""ConfigKey"": ""Alternate Owner"",
                  ""PropertyName"": ""Alternate Owner"",
                  ""DisplayName"": ""Alternate Information Owner""
                },
                {
                  ""ConfigKey"": ""Data Classification"",
                  ""PropertyName"": ""Data Classification"",
                  ""DisplayName"": ""Data Classification""
                },
                {
                  ""ConfigKey"": ""Site Template"",
                  ""PropertyName"": ""Site Template"",
                  ""DisplayName"": ""Site Template""
                },
                {
                  ""ConfigKey"": ""OBR"",
                  ""PropertyName"": ""OBR"",
                  ""DisplayName"": ""OBR""
                },
                {
                  ""ConfigKey"": ""LOB1"",
                  ""PropertyName"": ""LOB1"",
                  ""DisplayName"": ""LOB1""
                },
                {
                  ""ConfigKey"": ""LOB2"",
                  ""PropertyName"": ""LOB2"",
                  ""DisplayName"": ""LOB2""
                },
                {
                  ""ConfigKey"": ""LOB3"",
                  ""PropertyName"": ""LOB3"",
                  ""DisplayName"": ""LOB3""
                },
                {
                  ""ConfigKey"": ""LOB4"",
                  ""PropertyName"": ""LOB4"",
                  ""DisplayName"": ""LOB4""
                },
                {
                  ""ConfigKey"": ""Total Libraries"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Libraries""
                },
                {
                  ""ConfigKey"": ""Total Active Records"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Active Records""
                },
                {
                  ""ConfigKey"": ""Total Archived Records"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Archived Records""
                },
                {
                  ""ConfigKey"": ""Total Destroyed Records"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Destroyed Records""
                },
                {
                  ""ConfigKey"": ""Total Managed Records"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Managed Records""
                }
              ]
            },
            {
              ""SheetName"": ""Libraries"",
              ""Columns"": [
                {
                  ""ConfigKey"": ""Site ID"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site ID""
                },
                {
                  ""ConfigKey"": ""Library Name"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Library Name""
                },
                {
                  ""ConfigKey"": ""Library URL"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Library URL""
                },
                {
                  ""ConfigKey"": ""Library Type"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Library Type""
                },
                {
                  ""ConfigKey"": ""Total Active Records"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Active Records""
                },
                {
                  ""ConfigKey"": ""Total Archived Records"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Archived Records""
                },
                {
                  ""ConfigKey"": ""Total Destroyed Records"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Destroyed Records""
                },
                {
                  ""ConfigKey"": ""Total Managed Records"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Managed Records""
                }
              ]
            },
            {
              ""SheetName"": ""DERs"",
              ""Columns"": [
                {
                  ""ConfigKey"": ""Site ID"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site ID""
                },
                {
                  ""ConfigKey"": ""Site URL"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site URL""
                },
                {
                  ""ConfigKey"": ""Information Owner"",
                  ""PropertyName"": ""Information Owner"",
                  ""DisplayName"": ""Primary Information Owner""
                },
                {
                  ""ConfigKey"": ""Alternate Owner"",
                  ""PropertyName"": ""Alternate Owner"",
                  ""DisplayName"": ""Alternate Information Owner""
                },
                {
                  ""ConfigKey"": ""RCC Country"",
                  ""PropertyName"": ""RCC Country"",
                  ""DisplayName"": ""Country Code""
                },
                {
                  ""ConfigKey"": ""RCC"",
                  ""PropertyName"": ""RCC"",
                  ""DisplayName"": ""Record Class Code""
                },
                {
                  ""ConfigKey"": ""Record Status"",
                  ""PropertyName"": ""Record Status"",
                  ""DisplayName"": ""Record Status""
                },
                {
                  ""ConfigKey"": ""LOB1"",
                  ""PropertyName"": ""LOB1"",
                  ""DisplayName"": ""LOB1""
                },
                {
                  ""ConfigKey"": ""LOB2"",
                  ""PropertyName"": ""LOB2"",
                  ""DisplayName"": ""LOB2""
                },
                {
                  ""ConfigKey"": ""LOB3"",
                  ""PropertyName"": ""LOB3"",
                  ""DisplayName"": ""LOB3""
                },
                {
                  ""ConfigKey"": ""LOB4"",
                  ""PropertyName"": ""LOB4"",
                  ""DisplayName"": ""LOB4""
                },
                {
                  ""ConfigKey"": ""Total Active Records"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Active Records""
                },
                {
                  ""ConfigKey"": ""Total Record Volume(GB)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Record Volume(GB)""
                },
                {
                  ""ConfigKey"": ""Total Records Eligible Destruction Today(Count)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Records Eligible Destruction Today(Count)""
                },
                {
                  ""ConfigKey"": ""Total Records Eligible Destruction Today Volume(GB)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Records Eligible Destruction Today Volume(GB)""
                },
                {
                  ""ConfigKey"": ""Total Records Eligible Disposed Till Date(Count)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Records Eligible Disposed Till Date(Count)""
                },
                {
                  ""ConfigKey"": ""Total Records Eligible Disposed Till Date Volume(GB)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Total Records Eligible Disposed Till Date Volume(GB)""
                },
                {
                  ""ConfigKey"": ""Record Pending Approval  (0-60 Days)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Record Pending Approval  (0-60 Days)""
                },
                {
                  ""ConfigKey"": ""Record Pending Approval  (60-90 Days)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Record Pending Approval  (60-90 Days)""
                },
                {
                  ""ConfigKey"": ""Record Pending Approval  (90 -180 Days)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Record Pending Approval  (90 -180 Days)""
                },
                {
                  ""ConfigKey"": ""Record Pending Approval  (180-365 Days)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Record Pending Approval  (180-365 Days)""
                },
                {
                  ""ConfigKey"": ""Record Pending Approval  (>365 Days)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Record Pending Approval  (>365 Days)""
                }
              ]
            },
            {
              ""SheetName"": ""RCCs"",
              ""Columns"": [
                {
                  ""ConfigKey"": ""Site ID"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site ID""
                },
                {
                  ""ConfigKey"": ""Site URL"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site URL""
                },
                {
                  ""ConfigKey"": ""Information Owner"",
                  ""PropertyName"": ""Information Owner"",
                  ""DisplayName"": ""Primary Information Owner""
                },
                {
                  ""ConfigKey"": ""Alternate Owner"",
                  ""PropertyName"": ""Alternate Owner"",
                  ""DisplayName"": ""Alternate Information Owner""
                },
                {
                  ""ConfigKey"": ""RCC Country"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""RCC Country""
                },
                {
                  ""ConfigKey"": ""Record Class code(Term)"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Record Class code(Term)""
                },
                {
                  ""ConfigKey"": ""RCC Status"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""RCC Status""
                },
                {
                  ""ConfigKey"": ""LOB1"",
                  ""PropertyName"": ""LOB1"",
                  ""DisplayName"": ""LOB1""
                },
                {
                  ""ConfigKey"": ""LOB2"",
                  ""PropertyName"": ""LOB2"",
                  ""DisplayName"": ""LOB2""
                },
                {
                  ""ConfigKey"": ""LOB3"",
                  ""PropertyName"": ""LOB3"",
                  ""DisplayName"": ""LOB3""
                },
                {
                  ""ConfigKey"": ""LOB4"",
                  ""PropertyName"": ""LOB4"",
                  ""DisplayName"": ""LOB4""
                },
                {
                  ""ConfigKey"": ""Record Count"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Record Count""
                }
              ]
            },
            {
              ""SheetName"": ""All Sites"",
              ""Columns"": [
                {
                  ""ConfigKey"": ""Site ID"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site ID""
                },
                {
                  ""ConfigKey"": ""Site URL"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site URL""
                },
                {
                  ""ConfigKey"": ""Site Status"",
                  ""PropertyName"": ""Site Status"",
                  ""DisplayName"": ""Site Status""
                },
                {
                  ""ConfigKey"": ""Site Name"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site Name""
                },
                {
                  ""ConfigKey"": ""Site Description"",
                  ""PropertyName"": ""N/A"",
                  ""DisplayName"": ""Site Description""
                },
                {
                  ""ConfigKey"": ""Site Owner SID"",
                  ""PropertyName"": ""Site Owner SID"",
                  ""DisplayName"": ""Site Owner SID""
                },
                {
                  ""ConfigKey"": ""Site Owner Name"",
                  ""PropertyName"": ""Site Owner Name"",
                  ""DisplayName"": ""Site Owner Name""
                },
                {
                  ""ConfigKey"": ""Alternate Owner SID"",
                  ""PropertyName"": ""Alternate Owner SID"",
                  ""DisplayName"": ""Alternate Owner SID""
                },
                {
                  ""ConfigKey"": ""Alternate Owner Name"",
                  ""PropertyName"": ""Alternate Owner Name"",
                  ""DisplayName"": ""Alternate Owner Name""
                },
                {
                  ""ConfigKey"": ""Developer SID"",
                  ""PropertyName"": ""Developer SID"",
                  ""DisplayName"": ""Developer SID""
                },
                {
                  ""ConfigKey"": ""JADE Classification"",
                  ""PropertyName"": ""JADE Classification"",
                  ""DisplayName"": ""JADE Classification""
                },
                {
                  ""ConfigKey"": ""Template"",
                  ""PropertyName"": ""Template"",
                  ""DisplayName"": ""Template""
                },
                {
                  ""ConfigKey"": ""Quota"",
                  ""PropertyName"": ""Quota"",
                  ""DisplayName"": ""Quota""
                },
                {
                  ""ConfigKey"": ""Size [MB]"",
                  ""PropertyName"": ""Size [MB]"",
                  ""DisplayName"": ""Size [MB]""
                },
                {
                  ""ConfigKey"": ""Version"",
                  ""PropertyName"": ""Version"",
                  ""DisplayName"": ""Version""
                },
                {
                  ""ConfigKey"": ""Site Creation Date"",
                  ""PropertyName"": ""Site Creation Date"",
                  ""DisplayName"": ""Site Creation Date""
                },
                {
                  ""ConfigKey"": ""Cost Center"",
                  ""PropertyName"": ""Cost Center"",
                  ""DisplayName"": ""Cost Center""
                },
                {
                  ""ConfigKey"": ""Sort Code"",
                  ""PropertyName"": ""Sort Code"",
                  ""DisplayName"": ""Sort Code""
                },
                {
                  ""ConfigKey"": ""Billing Cost Center"",
                  ""PropertyName"": ""Billing Cost Center"",
                  ""DisplayName"": ""Billing Cost Center""
                },
                {
                  ""ConfigKey"": ""Billing Sort Code"",
                  ""PropertyName"": ""Billing Sort Code"",
                  ""DisplayName"": ""Billing Sort Code""
                },
                {
                  ""ConfigKey"": ""Allow Cross LOB Cost Center"",
                  ""PropertyName"": ""Allow Cross LOB Cost Center"",
                  ""DisplayName"": ""Allow Cross LOB Cost Center""
                },
                {
                  ""ConfigKey"": ""Regulatory Compliance"",
                  ""PropertyName"": ""Regulatory Compliance"",
                  ""DisplayName"": ""Regulatory Compliance""
                },
                {
                  ""ConfigKey"": ""OBR"",
                  ""PropertyName"": ""OBR"",
                  ""DisplayName"": ""OBR""
                },
                {
                  ""ConfigKey"": ""Last Attested"",
                  ""PropertyName"": ""Last Attested"",
                  ""DisplayName"": ""Last Attested""
                },
                {
                  ""ConfigKey"": ""LOB1"",
                  ""PropertyName"": ""LOB1"",
                  ""DisplayName"": ""LOB1""
                },
                {
                  ""ConfigKey"": ""LOB2"",
                  ""PropertyName"": ""LOB2"",
                  ""DisplayName"": ""LOB2""
                },
                {
                  ""ConfigKey"": ""LOB3"",
                  ""PropertyName"": ""LOB3"",
                  ""DisplayName"": ""LOB3""
                },
                {
                  ""ConfigKey"": ""LOB4"",
                  ""PropertyName"": ""LOB4"",
                  ""DisplayName"": ""LOB4""
                },
                {
                  ""ConfigKey"": ""EEA Nexus"",
                  ""PropertyName"": ""EEA Nexus"",
                  ""DisplayName"": ""EEA Nexus""
                },
                {
                  ""ConfigKey"": ""Process PI"",
                  ""PropertyName"": ""Process PI"",
                  ""DisplayName"": ""Process PI""
                },
                {
                  ""ConfigKey"": ""PI Source"",
                  ""PropertyName"": ""PI Source"",
                  ""DisplayName"": ""PI Source""
                },
                {
                  ""ConfigKey"": ""SEAL ID"",
                  ""PropertyName"": ""SEAL ID"",
                  ""DisplayName"": ""SEAL ID""
                },
                {
                  ""ConfigKey"": ""Controller-Processor"",
                  ""PropertyName"": ""Controller-Processor"",
                  ""DisplayName"": ""Controller-Processor""
                },
                {
                  ""ConfigKey"": ""Countries"",
                  ""PropertyName"": ""Countries"",
                  ""DisplayName"": ""Countries""
                },
                {
                  ""ConfigKey"": ""Business Purpose"",
                  ""PropertyName"": ""Business Purpose"",
                  ""DisplayName"": ""Business Purpose""
                },
                {
                  ""ConfigKey"": ""Legal Base(s)"",
                  ""PropertyName"": ""Legal Base(s)"",
                  ""DisplayName"": ""Legal Base(s)""
                },
                {
                  ""ConfigKey"": ""Categories of PI Processed"",
                  ""PropertyName"": ""Categories of PI Processed"",
                  ""DisplayName"": ""Categories of PI Processed""
                },
                {
                  ""ConfigKey"": ""Category of Workforce"",
                  ""PropertyName"": ""Category of Workforce"",
                  ""DisplayName"": ""Category of Workforce""
                },
                {
                  ""ConfigKey"": ""Category of Individuals"",
                  ""PropertyName"": ""Category of Individuals"",
                  ""DisplayName"": ""Category of Individuals""
                },
                {
                  ""ConfigKey"": ""Category of Corporate Client/Service Provider"",
                  ""PropertyName"": ""Category of Corporate Client/Service Provider"",
                  ""DisplayName"": ""Category of Corporate Client/Service Provider""
                },
                {
                  ""ConfigKey"": ""Categories of Internal Recipients of Data"",
                  ""PropertyName"": ""Categories of Internal Recipients of Data"",
                  ""DisplayName"": ""Categories of Internal Recipients of Data""
                },
                {
                  ""ConfigKey"": ""Categories of External Recipient of Data"",
                  ""PropertyName"": ""Categories of External Recipient of Data"",
                  ""DisplayName"": ""Categories of External Recipient of Data""
                },
                {
                  ""ConfigKey"": ""Librarians"",
                  ""PropertyName"": ""Librarians"",
                  ""DisplayName"": ""Librarians""
                },
                {
                  ""ConfigKey"": ""Site Admins"",
                  ""PropertyName"": ""Site Admins"",
                  ""DisplayName"": ""Site Admins""
                },
                {
                  ""ConfigKey"": ""Valid Attestation"",
                  ""PropertyName"": ""Valid Attestation"",
                  ""DisplayName"": ""Valid Attestation""
                }
              ]
            }
          ]
        }";
    }
}
