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

namespace RAExportCommon
{
    public static class VEOCommonString
    {
        public const string SIGNER = "AvePoint";
        public const string STRINGEMPTY = "";
        public const string M128_File_Encoding_PlainTextEncodedAsPlainText = @"The content of the DocumentData element is a text file. The characters in the text file conform to UTF-8 encoded Unicode. Unicode is defined in ‘The Unicode Standard’, Version 8.0.0, The Unicode Consortium, or the equivalent ISO 10646:2014 and Amendment 1. The ampersand (and), single quote, double quote, greater than, and less than characters have been escaped using the standard XML escape conventions '&amp;', '&lt;', and '&gt;' respectively.";
        public const string M128_File_Encoding_PlainTextEncodedAsBase64 = @"The content of the DocumentData element is a text file. The characters in the text file conform to UTF-8 encoded Unicode. Unicode is defined in ‘The Unicode Standard’, Version 8.0.0, The Unicode Consortium, or the equivalent ISO 10646:2014 and Amendment 1. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_HTML_Hypertext_Markup_Language = @"The content of the DocumentData element is an HTML file. The file conforms to ‘HTML 4.01 Specification’. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_CSS_Cascading_Style_Sheets = @"The content of the DocumentData element is a CSS stylesheet. The file conforms to ‘Cascading Style Sheets Level 2 Revision 1’. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_XML_Extensible_Markup_Language = @"The content of the DocumentData element is an XML document. The file conforms to ‘Extensible Markup Language (XML) 1.0 (Fifth Edition)’. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_PDFOrPDFA = @"The content of the DocumentData element is a PDF file. For details of PDF see ‘ISO 32000-1:2008 – Document management – Portable document format – Part 1: PDF 1.7’. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8, ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_DOCOrDOCX = @"The content of the DocumentData element is a document represented in the format used by the Microsoft Word program. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_XLSOrXLSX = @"The content of the DocumentData element is a spreadsheet represented in the format used by the Microsoft Excel program. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_PPTOrPPTX = @"The content of the DocumentData element is a document represented in the format used by the Microsoft Powerpoint program. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_TIFF = @"The content of the DocumentData element is a TIFF image. The file conforms to ‘TIFF Revision 6.0, Final – June 3, 1992’. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_JPEGOrJFIF = @"The content of the DocumentData element is a JPEG image. The file conforms to ISO 10918-1:1994, ‘Information Technology – Digital compression and coding of continuous-tone still images: Requirements and guidelines’ (also known as ITU-T T.81), and the ‘JPEG File Interchange Format’, version 1.02. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_JPEG_2000 = @"The content of the DocumentData element is a JPEG 2000 image. The file conforms to ISO 15444-1:2004, ‘Information Technology – JPEG 2000 image coding system: Core coding system’ (also known as ITU-T T.800). The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_MPEG4_Video_MP_File_Format = @"The content of the DocumentData element is an MPEG-4 video stream encoded in the MP file format. The file conforms to the following parts of ISO/IEC 14496 ‘Information Technology – Coding of audiovisual objects’, Part 1:2004 (including Amd 1: 2005 and Amd 8:2004), Part 2:2004 (including Cor 1:2004, Amd 1:2004, and Amd 2:2005), Part 3:2004, and Part 14:2003 (including Cor 1:2006). The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_MPEG4_Video_AVC_File_Format = @"The content of the DocumentData element is an MPEG-4 video stream encoded in the AVC file format. The file conforms to the following parts of ISO/IEC 14496 ‘Information Technology – Coding of audiovisual objects’, Part 1:2004 (including Amd 1: 2005 and Amd 8:2004), Part 2:2004 (including Cor 1:2004, Amd 1:2004, and Amd 2:2005), Part 3:2004, Part 10:2005, and Part 15:2004 (including Cor 1:2006, and Amd 1:2006). The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_WARC_Web_Archive = @"The content of the DocumentData element is a WARC archive. The file conforms to ISO 28500:2009 ‘WARC file format’. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_CSVEncodedAsBase64 = @"The content of the DocumentData element is a text file containing a CSV. The file conforms to RFC4180 ‘Common format and MIME type for Comma-Separated Values (CSV) files’. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_CSVEncodedAsText = @"The content of the DocumentData element is a text file containing a CSV. The file conforms to RFC4180 ‘Common format and MIME type for Comma-Separated Values (CSV) files’. The characters in the text file conform to UTF-8 encoded Unicode. Unicode is defined in ‘The Unicode Standard, Version 8.0.0, The Unicode Consortium, or the equivalent ISO 10646:2014 and Amendment 1. The ampersand (and), single quote, double quote, greater than, and less than characters have been escaped using the standard XML escape conventions '&amp;', '&lt;', and '&gt;' respectively.";
        public const string M128_File_Encoding_MP3_MPEG1_And_MPEG2_Audio_Layer_III = @"The content of the DocumentData element is an MP3 file. The file conforms to Audio Layer III of ISO 11172-3:1993 (MPEG-1 Part 3) or ISO 13818-3:1998 (MPEG-2 Part 3). The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_MP4Audio_MPEG4_Audio = @"The content of the DocumentData element is an MPEG-4 audio stream. The file conforms to Part 3:2004 of ISO/IEC 14496 ‘Information Technology – Coding of audio-visual objects’. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_WAVOrLPCM = @"The content of the DocumentData element is a WAV file with the contained audio stream encoded using LPCM (Linear Pulse Code Modulation). The file conforms to ‘Multimedia Programming Interface and Data Specifications 1.0’. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding’.";
        public const string M128_File_Encoding_MIME_Email_Encoded_As_Text = @"The content of the DocumentData element is a MIME file conformant with RFC2049 ‘Multipurpose Internet Mail Extensions (MIME) Part Five: Conformance Criteria and Examples,’ Freed & Borenstein, Nov 1996. The characters in the text file conform to UTF-8 encoded Unicode. Unicode is defined in ‘The Unicode Standard, Version 8.0.0, The Unicode Consortium, or the equivalent ISO 10646:2014 and Amendment 1. The ampersand (and), single quote, double quote, greater than, and less than characters have been escaped using the standard XML escape conventions '&amp;', '&lt;', and '&gt;' respectively.";
        public const string M128_File_Encoding_MIME_Email_Encoded_In_Base64 = @"The content of the DocumentData element is a MIME file conformant with RFC2049 ‘Multipurpose Internet Mail Extensions (MIME) Part Five: Conformance Criteria and Examples,’ Freed & Borenstein, Nov 1996. The file has been encoded using Base64, which is defined in IETF RFC 2045 ‘Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies’, Section 6.8 ‘Base64 Content-Transfer-Encoding ’.";
    }
}
