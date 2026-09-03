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
declare interface IRelatedRecordsCommandSetStrings {
  Related_App_RelatedRecords: string;
  Related_App_RelatedRecords_ViewDetails: string;
  Related_App_Common_Save: string;
  Related_App_Common_Cancel: string;
  Related_App_Common_Delete: string;
  Related_App_Common_Yes: string;
  Related_App_Common_No: string;
  Related_App_EditRelatedRecord_Error_Msg: string;
  Related_App_EditRelatedRecord_File_Error_Msg: string;
  Related_App_RelatedColumn_Name: string;
  Related_App_RelatedColumn_Location: string;
  Related_App_RelatedColumn_Action: string;
  Related_App_AddRelatedBtn: string;
  Related_App_AddRelatedTitle: string;
  Related_App_AddRelated_SearchBox_Placeholder: string;
  Related_App_AddRelated_SearchBox_Placeholder_Physical: string;
  Related_App_AddRelated_SearchResult_Empty: string;
  Related_App_Detail_Overview: string;
  Related_App_Detail_RelatedLabel_Name: string;
  Related_App_Detail_RelatedLabel_Location: string;
  Related_App_Detail_RelatedLabel_RecordID: string;
  Related_App_Detail_RelatedLabel_Term: string;
  Related_App_Detail_Disposal_Info: string;
  Related_App_Detail_RelatedLabel_Rule_Name: string;
  Related_App_Detail_RelatedLabel_Rule_Action: string;
  Related_App_Detail_RelatedLabel_Action_Duedate: string;
  Related_App_Detail_Hold_Information: string;
  Related_App_Detail_RelatedLabel_On_Hold: string;
  Related_App_Detail_RelatedLabel_Hold_Title: string;
  Related_App_Detail_RelatedLabel_Comment: string;
  Related_App_Detail_RelatedLabel_Hold_By: string;
  Related_App_Detail_RelatedLabel_Hold_Until: string;
  Related_App_Detail_Declared_Information: string;
  Related_App_Detail_RelatedLabel_Declared_As_Record: string;
  Related_App_Detail_PhysicalRecords: string;
  Related_App_Detail_Onpremises: string;
  Related_App_Detail_Id: string;
  Related_App_AddRecord_Failed_Msg: string
}

declare module 'RelatedRecordsCommandSetStrings' {
  const strings: IRelatedRecordsCommandSetStrings;
  export = strings;
}
