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
using System.Text;

namespace AvePoint.Wrapper.Common
{
   public class AveDocFlags
    {
       public const int CONTAIN_HYPERLINK_DOC = 0x00000008;

       public const int CHECKED_OUT_DOC = 0x00000020;

       public const int HAVE_BINARYSTREAM_DOC = 0x00000100;
       public const int CHECKOUT_TO_CLIENTSYSTEM_DOC = 0x00000200;
       public const int NAMESPACE_ENTRY_DOC = 0x00000800;


       #region /************************** The function of the bit is not clear and some are unused ***********************************/

       public const int CONTAIN_DYNAMIC_CONTENT_DOC = 0x00000001;
       public const int SUBIMAGE_OF_ANOTHERDOCUMENT_DOC = 0x00000002;
       public const int PARSER_AVAILABLE_WHENSAVED_DOC = 0x00000004;

       public const int ASSOCIATE_RESOURCE_IN_PRIVATEFOLDER_DOC = 0x00000010;
       public const int UNGHOSTED_DOC = 0x00000040;
       public const int CONTAIN_WEBPART_DOC = 0x00000080;

       public const int HAS_CHILDDOCUMENT_DOC = 0x00000400;

       public const int HAS_PROPERTY_IN_METAINFO_DOC = 0x00002000;
       public const int MUST_BE_UNGHOSTED_WHEN_UNDIRTIED_DOC = 0x00004000;
       public const int WHEN_0_BYTEDOC_REQUIRED_CHECKOUT_DOC = 0x00008000;
       #endregion
       /// <summary>
       /// The Document is a type which can contain hyperlinks.
       /// </summary>
       public static bool IsContainHyperLinkDoc(int value)
       {
           return (value & CONTAIN_HYPERLINK_DOC) != 0;
       }

       /// <summary>
       /// The Document is currently checked out to a User.
       /// </summary>
       public static bool IsCheckedOutDoc(int value)
       {
           return (value & CHECKED_OUT_DOC) != 0;
       }

       /// <summary>
       /// The Document is a type which can have a binary stream.
       /// </summary>
       public static bool IsHaveStreamDoc(int value)
       {
           return (value & HAVE_BINARYSTREAM_DOC) != 0;
       }

       /// <summary>
       /// The Document is currently checked out to a location on the user's client system.
       /// </summary>
       public static bool IsCheckOutToClientSystemDoc(int value)
       {
           return (value & CHECKOUT_TO_CLIENTSYSTEM_DOC) != 0;
       }

       /// <summary>
       /// The Document is only a namespace entry for a List Item. 
       /// (i.e. it corresponds to an Item in a non-Document Library List that should be filtered out from file system-centric enumerations).
       /// </summary>
       public static bool IsNameSpcaceEntryDoc(int value)
       {
           return (value & NAMESPACE_ENTRY_DOC) != 0;
       }


        #region /***************************** This is the property that relation to the bit that not clear now  *************************************/

       /// <summary>
       /// This Document contains dynamic content to be sent through the CAML interpreter, 
       /// an implementation-specific dynamic content generation component. 
       /// An example of this would be a category Web Bot present in the source of the Page.
       /// </summary>
       public static bool IsContainDynamicContentDoc(int value)
       {
           return (value & CONTAIN_DYNAMIC_CONTENT_DOC) != 0;
       }

       /// <summary>
       /// The Document is a sub image of another Document. 
       /// This is strongly correlated to the ExcludedType value in the security enumeration, and is set if this is an automatically generated
       /// thumbnail or web image based on another item in the store.
       /// </summary>
       public static bool IsSubImageOfAnotherDocumentDoc(int value)
       {
           return (value & SUBIMAGE_OF_ANOTHERDOCUMENT_DOC) != 0;
       }

       /// <summary>
       /// The Document is a type for which there was a registered parser available at the time it was saved. 
       /// A parser is an implementation-specific component that can extract data and Metadata from a Document, 
       /// which can then be used to build a list of hyperlinks and Fields for Content Types.
       /// </summary>
       public static bool IsParserAvailableWhenSavedDoc(int value)
       {
           return (value & PARSER_AVAILABLE_WHENSAVED_DOC) != 0;
       }

       /// <summary>
       /// The Document has an associated resource in the private Folder that should be renamed in parallel 
       /// when this file is renamed. An example of this is the count file for a hit counter Web Bot.
       /// </summary>
       public static bool IsAssociateResourceInPrivateFolderDoc(int value)
       {
           return (value & ASSOCIATE_RESOURCE_IN_PRIVATEFOLDER_DOC) != 0;
       }

       /// <summary>
       /// The Document is Unghosted.
       /// </summary>
       public static bool IsUngostedDoc(int value)
       {
           return (value & UNGHOSTED_DOC) != 0;
       }

       /// <summary>
       /// The Page contains Web Parts. Defaults to a personalized view (showing Web Parts that are specific to the user that browsed to the Page).
       /// </summary>
       public static bool IsContainWebPartDoc(int value)
       {
           return (value & CONTAIN_WEBPART_DOC) != 0;
       }

       /// <summary>
       /// The Document has child Documents created by the Document transformations feature.
       /// </summary>
       public static bool IsHasChildDocumentDoc(int value)
       {
           return (value & HAS_CHILDDOCUMENT_DOC) != 0;
       }

       /// <summary>
       /// The Document has properties in its Metainfo defining a custom order of the Content Types. This is only valid for Folders.
       /// </summary>
       public static bool IsHasPropertyInMetaInfoDoc(int value)
       {
           return (value & HAS_PROPERTY_IN_METAINFO_DOC) != 0;
       }

       /// <summary>
       /// The Document MUST be Unghosted when ―undirtied
       /// (that is, when dependency updates are performed for the Document). 
       /// This is used for Documents such as a Document Library template, 
       /// which is provisioned as Ghosted but ought to be Unghosted to demote Content Type information on the containing Document Library 
       /// whenever that information is updated.
       /// </summary>
       public static bool IsMustBeUnGostedWhenUndirtiedDoc(int value)
       {
           return (value & MUST_BE_UNGHOSTED_WHEN_UNDIRTIED_DOC) != 0;
       }

       /// <summary>
       /// Used when a 0 byte Document is saved to a Document Library with required check out and at least one required Field.
       /// This is common in migration scenarios or with the use of older versions of the Windows WebDAV redirector against the Windows SharePoint Service WebDAV implementation.
       /// </summary>
       public static bool IsWhen0ByteDocRequiredCheckOutDoc(int value)
       {
           return (value & WHEN_0_BYTEDOC_REQUIRED_CHECKOUT_DOC) != 0;
       }
       #endregion




    }
}
