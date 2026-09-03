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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    #endregion

    public enum TreeNodeType
    {
        GenericList = 0,
        DocumentLibrary = 1,
        Unused = 2,
        DiscussionBoard = 3,
        Survey = 4,
        Issue = 5,
        Document = 6,
        //for exchange       
        EOMailBox = 900,
        EOMailFolder = 901,
        EOInfoPathsFolder = 902,
        EORSSFeedsFolder = 903,
        EONotesFolder = 904,
        EOCalendarFolder = 905,
        EOContactsFolder = 906,
        EOTasksFolder = 907,
        EOJournalFolder = 908,
        EOItem = 909,
        EOO365GroupGroup = 921,
        EOO365Group = 922,
    }
}