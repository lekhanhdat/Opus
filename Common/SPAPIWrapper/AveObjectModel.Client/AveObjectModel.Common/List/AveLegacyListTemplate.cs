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

namespace AvePoint.ObjectModel.Common
{
    class AveLegacyListTemplate : IAveLegacyListTemplate
    {

        #region IAveLegacyListTemplate Members

        public string LookupAssociatedFeatureId(ref AveListTemplateType templateType)
        {
            string str = null;
            AveListTemplateType type = templateType;
            if (type <= AveListTemplateType.MeetingUser)
            {
                switch (type)
                {
                    case AveListTemplateType.GenericList:
                        return "00BFEA71-DE22-43B2-A848-C05709900100";

                    case AveListTemplateType.DocumentLibrary:
                        return "00BFEA71-E717-4E80-AA17-D0C71B360101";

                    case AveListTemplateType.Survey:
                        return "00BFEA71-EB8A-40B1-80C7-506BE7590102";

                    case AveListTemplateType.Links:
                        return "00BFEA71-2062-426C-90BF-714C59600103";

                    case AveListTemplateType.Announcements:
                        return "00BFEA71-D1CE-42de-9C63-A44004CE0104";

                    case AveListTemplateType.Contacts:
                        return "00BFEA71-7E6D-4186-9BA8-C047AC750105";

                    case AveListTemplateType.Events:
                        return "00BFEA71-EC85-4903-972D-EBE475780106";

                    case AveListTemplateType.Tasks:
                        return "00BFEA71-A83E-497E-9BA0-7A5C597D0107";

                    case AveListTemplateType.DiscussionBoard:
                        return "00BFEA71-6A49-43FA-B535-D15C05500108";

                    case AveListTemplateType.PictureLibrary:
                        return "00BFEA71-52D4-45B3-B544-B1C71B620109";

                    case AveListTemplateType.DataSources:
                        return "00BFEA71-F381-423D-B9D1-DA7A54C50110";

                    case AveListTemplateType.WebTemplateCatalog:
                    case AveListTemplateType.WebPartCatalog:
                    case AveListTemplateType.ListTemplateCatalog:
                    case AveListTemplateType.MasterPageCatalog:
                    case AveListTemplateType.SolutionCatalog:
                    case AveListTemplateType.ThemeCatalog:
                        return null;

                    case AveListTemplateType.UserInformation:
                    case ((AveListTemplateType)0x7c):
                    case ((AveListTemplateType)0x7d):
                    case ((AveListTemplateType)0x7e):
                    case ((AveListTemplateType)0x7f):
                    case ((AveListTemplateType)0x80):
                    case ((AveListTemplateType)0x81):
                    case ((AveListTemplateType)0x83):
                    case ((AveListTemplateType)0x84):
                    case ((AveListTemplateType)0x85):
                    case ((AveListTemplateType)0x86):
                    case ((AveListTemplateType)0x87):
                    case ((AveListTemplateType)0x88):
                    case ((AveListTemplateType)0x89):
                    case ((AveListTemplateType)0x8a):
                    case ((AveListTemplateType)0x8b):
                    case ((AveListTemplateType)0x8d):
                    case ((AveListTemplateType)0x8e):
                    case ((AveListTemplateType)0x8f):
                    case ((AveListTemplateType)0x90):
                    case ((AveListTemplateType)0x91):
                    case ((AveListTemplateType)0x92):
                    case ((AveListTemplateType)0x93):
                    case ((AveListTemplateType)0x94):
                    case ((AveListTemplateType)0x95):
                        return str;

                    case AveListTemplateType.XMLForm:
                        return "00BFEA71-1E1D-4562-B56A-F05371BB0115";

                    case AveListTemplateType.NoCodeWorkflows:
                    case AveListTemplateType.NoCodePublic:
                        return "00BFEA71-F600-43F6-A895-40C0DE7B0117";

                    case AveListTemplateType.WorkflowProcess:
                        return "00BFEA71-2D77-4A75-9FCA-76516689E21A";

                    case AveListTemplateType.WebPageLibrary:
                        return "00BFEA71-C796-4402-9F2F-0EB9A6E71B18";

                    case AveListTemplateType.CustomGrid:
                        return "00BFEA71-3A1D-41D3-A0EE-651D11570120";

                    case AveListTemplateType.DataConnectionLibrary:
                        return "00BFEA71-DBD7-4F72-B8CB-DA7AC0440130";

                    case AveListTemplateType.WorkflowHistory:
                        return "00BFEA71-4EA5-48D4-A4AD-305CF7030140";

                    case AveListTemplateType.GanttTasks:
                        return "00BFEA71-513D-4CA0-96C2-6A47775C0119";

                    case AveListTemplateType.InvalidType:
                        return str;

                    case AveListTemplateType.Meetings:
                    case AveListTemplateType.Agenda:
                    case AveListTemplateType.MeetingUser:
                        break;
                }
                return str;
            }
            switch (type)
            {
                case AveListTemplateType.TextBox:
                case AveListTemplateType.HomePageLibrary:
                case AveListTemplateType.MeetingObjective:
                    break;
                case AveListTemplateType.ThingsToBring:
                    return str;
                case AveListTemplateType.IssueTracking:
                    return "00BFEA71-5932-4F9C-AD71-1557E5751100";
                default:
                    return str;
            }        
            return null;

        }

        #endregion
    }
}
