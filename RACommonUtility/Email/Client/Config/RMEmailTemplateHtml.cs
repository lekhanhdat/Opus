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
namespace AvePoint.RA.RACommonUtility.Email.Client.Config
{
    public static class RMEmailTemplateHtml
    {
        public const string BASIC_TEMPLATE = @"
<!DOCTYPE HTML PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html lang='en-US'>
<head>
    <title></title>
    <style type='text/css'>
        p {
            margin: 0;
            padding: 0;
        }

        .eContent {
            font-family: 'segoe ui', sans-serif;
            font-size: 14px;
            font-weight: 10;
        }

        .requestLink {
            display: inline;
            margin-right: 3px;
        }

        .manualLink {
            display: inline;
        }

        .requestLink a {
            color: #3572b0;
            text-decoration: none;
        }

        .manualLink a {
            color: #3572b0;
            text-decoration: none;
        }

        .requestLink a:focus, a:hover, a:active {
            text-decoration: underline;
            color: #600;
        }

        .manualLink a:focus, a:hover, a:active {
            text-decoration: underline;
            color: #600;
        }

        a {
            text-decoration: none;
        }

        a:hover {
            text-decoration: underline;
        }
    </style>
</head>
<body>
    @Body
    @Copyright
</body>
</html>
";
        public const string COPYRIGHT = @"
<table style='width:100%;border-top:1px solid #d7d7d7;font-size:13px;font-family:segoe ui;color:#858484;margin-top:8px;'>
    <tr>
        <td style='padding:3px;display:inline-block;'>Enterprise Software Service</td>
        <td style='width:50px;display:inline-block;'></td>
        <td style='padding:3px;display:inline-block;'>© @EndYear AvePoint ® Inc. All Rights Reserved.</td>
    </tr>
</table>
";

        public const string PHYSICAL_REQUEST_MANAGEMENT_LINK = @"<div class='requestLink'><a href='@Link'>AvePoint Cloud Records > Request Management</a></div>";

        public const string PHYSICAL_REQUEST_REVIEW_LINK = @"<div class='requestLink'><a href='@Link' class='requestLink'>AvePoint Cloud Records > My Tasks > Requests for Review</a></div>";

        public const string PHYSICAL_REQUEST_MANAGEMENT_LINK_OPUS = @"<div class='requestLink'><a href='@Link' class='requestLink'>AvePoint Opus > My Tasks > Requests for Review</a></div>";

        public const string MANUAL_REVIEW_LINK = @"<div class='manualLink'><a href='@Link' title='@Link'>@Automation > @Title</a></div>";

        public const string JOB_NOTIFICATION_REVIEW_LINK = @"<div class='requestLink'><a href='@Link' class='requestLink'>AvePoint Opus > Job monitor</a></div>";
        public const string JOB_NOTIFICATION_BORROWER_LINK = @"<div class='requestLink'><a href='@Link' class='requestLink'>AvePoint Opus > Physical Records > Explorer</a></div>";

        public const string HOLD_NOTIFICATION_REVIEW_LINK = @"<div class='requestLink'><a href='@Link' class='requestLink'>AvePoint Opus > Manage Holds</a></div>";
    }
}
