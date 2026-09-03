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
global using Microsoft.Graph;
global using Microsoft.Graph.Models;
global using Microsoft.Graph.Models.ODataErrors;
global using Microsoft.Kiota.Abstractions;
global using Microsoft.Kiota.Abstractions.Authentication;
global using Microsoft.Kiota.Abstractions.Helpers;
global using Microsoft.Kiota.Abstractions.Serialization;
global using Microsoft.Kiota.Abstractions.Store;
global using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
global using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
global using Microsoft365.Authentication.TokenProvider;
global using Microsoft365.Common;
global using Microsoft365.Common.Middleware;
global using Microsoft365.Common.Middleware.Handlers;
global using Microsoft365.Common.Service;
global using Microsoft365.Configuration;
global using Microsoft365.Graph.Authentication;
global using Microsoft365.Graph.Core;
global using Microsoft365.Graph.Extensions;
global using Microsoft365.Graph.Middleware;
global using Microsoft365.Graph.Service.ExportItems;
global using Microsoft365.Graph.Service.ImportItems;
global using Microsoft365.Graph.Service.Mailboxes;
global using Microsoft365.Graph.Util;
//global using Microsoft365Backup.CommonUtil.Http;
//global using Microsoft365Backup.Logger;
global using System.Buffers;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using System.Runtime.CompilerServices;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading;

//alis
global using PagingCallback = System.Func<(string? Nextlink, string? Deltalink, Microsoft.Graph.PagingState State), System.Threading.Tasks.Task>;
global using MailFolderMessagesDeltaRequestBuilder = Microsoft.Graph.Users.Item.MailFolders.Item.Messages.Delta.DeltaRequestBuilder;
global using MailFolderMessagesDetaGetResponse = Microsoft.Graph.Users.Item.MailFolders.Item.Messages.Delta.DeltaGetResponse;
global using MailFoldersDeltaGetResponse = Microsoft.Graph.Users.Item.MailFolders.Delta.DeltaGetResponse;
global using MailFoldersDeltaRequestBuilder = Microsoft.Graph.Users.Item.MailFolders.Delta.DeltaRequestBuilder;

//beta
global using GraphBeta = Microsoft.Graph.Beta;
global using GraphBetaExportItems = Microsoft.Graph.Beta.Admin.Exchange.Mailboxes.Item.ExportItems;
global using GraphBetaModels = Microsoft.Graph.Beta.Models;
global using GraphBetaODataErrors = Microsoft.Graph.Beta.Models.ODataErrors;
global using GraphBetaRootFolderDelta = Microsoft.Graph.Beta.Admin.Exchange.Mailboxes.Item.Folders.Delta;
global using GraphBetaSubFolderDelta = Microsoft.Graph.Beta.Admin.Exchange.Mailboxes.Item.Folders.Item.ChildFolders.Delta;
global using GraphBetaItemsDelta = Microsoft.Graph.Beta.Admin.Exchange.Mailboxes.Item.Folders.Item.Items.Delta; // Changed alias name and path
global using GraphBetaMailboxFolderItemRequestBuilder = Microsoft.Graph.Beta.Admin.Exchange.Mailboxes.Item.Folders.Item.MailboxFolderItemRequestBuilder;

//V1
global using GraphV1ItemsDelta = Microsoft.Graph.Admin.Exchange.Mailboxes.Item.Folders.Item.Items.Delta;
