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
declare interface IOpusCustomizationStrings {
  JPMC_App_Classify: string;
  JPMC_App_Title: string;
  JPMC_App_Save: string;
  JPMC_App_Cancel: string;
  JPMC_App_Close: string;
  JPMC_App_ProgressIndicatorDescription: string;
  JPMC_App_RecordCode: string;
  JPMC_App_ClassCode: string;
  JPMC_App_CountryCode: string;
  JPMC_App_RetentionType: string;
  JPMC_App_StartDate: string;
  JPMC_App_CreateDate: string;
  JPMC_App_ProcessStatus: string;
  JPMC_App_ProcessStatus_Waiting: string;
  JPMC_App_ProcessStatus_Running: string;
  JPMC_App_ProcessStatus_Success: string;
  JPMC_App_ProcessStatus_Failed: string;
  JPMC_App_ProcessComment: string;
  JPMC_App_ProgressIndicatorLabel: string;
  JPMC_App_Msg_Classify_Error: string;
  JPMC_App_Msg_Action_NoPermission: string;
  JPMC_App_Msg_Action_NoPermission_Declared: string;
  JPMC_App_Msg_Action_NoPermission_Undeclared: string;
  JPMC_App_Msg_Error_Declared: string;
  JPMC_App_Msg_Error_Undeclared: string;
  JPMC_App_Msg_Classify_Hold: string;
  JPMC_App_Msg_Classify_UnsupportedObj: string;
  JPMC_App_Msg_Classify_Checkout_Final: string;
  JPMC_App_Msg_UnauthorizedAccessRootWeb: string;
  JPMC_App_Msg_Classify_NotCheckoutUser: string;
  JPMC_App_Msg_Classify_ClassFieldNotFound: string;
  JPMC_App_Msg_Classify_ClassCodeNotFound: string;
  JPMC_App_Msg_Classify_ChannelFolderWaring: string;
  JPMC_App_Msg_Classify_Successful: string;
  JPMC_App_Msg_Classify_InProcess: string;
  JPMC_App_Msg_Classify_Skip: string;
  JPMC_App_Msg_Classify_Exception: string;
  JPMC_App_Msg_Classify_NeedUpdateApp: string;
}

declare module 'OpusCustomizationStrings' {
  const strings: IOpusCustomizationStrings;
  export = strings;
}
