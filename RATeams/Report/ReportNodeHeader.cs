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



namespace Office365GroupBackup
{
    internal class ReportNodeHeader
    {
        internal const string Success = "T";
        internal const string Fail = "F";

        internal const char Group = 'G';
        internal const char Mailbox = 'M';
        internal const char Folder = 'F';
        internal const char Item = 'I';
        internal const char Team = 'T';
        internal const char Channel = 'C';
        internal const char Conversation = 'R';
        internal const char Email = 'E';
        internal const char Attachment = 'N';
        internal const char Event = 'V';
        internal const char Plan = 'P';
        internal const char Task = 'A';//此处按照单词字母顺序，找到第一个没有被占用的字母
        internal const char Document = 'D';
        internal const char DocumentVersion = 'B';
        internal const char SiteCollection = 'S';
        internal const char Web = 'W';
        internal const char List = 'L';
        internal const char SiteFolder = 'H';
        internal const char SiteAttachment = 'O';

        internal const string Type = "t";
        internal const string Size = "s";
        internal const string Skiped = "isSkipped";
    }
}