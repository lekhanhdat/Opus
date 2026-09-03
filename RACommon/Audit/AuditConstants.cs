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

namespace AvePoint.RA.Common.Audit
{
    public class AuditConstants
    {
        public const string Audit_Physical_Request_Comment = "Audit_Physical_Request_Comment";
        public const string Audit_Physical_Request_Hold_User = "Audit_Physical_Request_Hold_User";
        public const string Audit_Physical_Request_EndTime = "Audit_Physical_Request_EndTime";
        public const string Audit_Physical_Request_File_Title = "Audit_Physical_Request_File_Title";
        public const string Audit_MachineLearning_EnableAutoApply_Title = "Audit_MachineLearning_EnableAutoApply_Title";
        public const string Audit_MachineLearning_Update_Description = "Audit_MachineLearning_Update_Description";
        public const string Audit_MachineLearning_SwitchMode = "Audit_MachineLearning_SwitchMode";
        public const string Audit_MachineLearning_ChangeTrainingOption = "RM_Audit_ML_TrainingScopeOption";
        public const string Audit_MachineLearning_FromLocation_SourceFlag = "RM_Audit_ML_ContentSource";
        public const string Audit_MachineLearning_FromLocation_Location = "RM_Audit_ML_Location";
    }
}
