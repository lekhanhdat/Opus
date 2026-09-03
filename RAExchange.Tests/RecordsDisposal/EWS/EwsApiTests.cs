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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Common;
using AvePoint.RA.RAExchange.Authorization;
using Shouldly;

namespace RAExchange.Tests.RecordsDisposal.EWS;

public class EwsApiTests : BaseExoService
{
    private ExchangeBackupUtility.ExchangeFolder InitExchangeEWSFolder()
    {
        var dict = new Dictionary<string, BposInfo>
         {
             { Address, BposInfo }
         };
        AuthorizationManager.Instance.Init(dict);
        var ewsFolder = new ExchangeBackupUtility.ExchangeRootFolder(new ExchangeUtility.ExchangeMailbox(Address, ExchangeUtility.ExchangeMailboxType.User),
            AuthorizationManager.Instance.GetAuthObject(Address));
        ewsFolder.Open();
        return ewsFolder;
    }

    [Fact]
    public void OpenFolder_UsingEWS_ShouldBeSuccessful()
    {
        // Arrange
        var ewsCurrentFolder = InitExchangeEWSFolder();

        // Assert
        ewsCurrentFolder.FolderId.ShouldNotBeNullOrEmpty();
        ewsCurrentFolder.FolderName.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void DeleteItem_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var ewsFolder = InitExchangeEWSFolder();

        // Act
        var folders = ewsFolder.GetAllSubFolders();
        var inboxFolder = folders.First(f => f.FolderName.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
        inboxFolder.Open();

        var exchangeItems = inboxFolder.FindItems(100, 0, out var moreAvailable);

        // Assert
        if (exchangeItems.Any())
        {
            var itemToDelete = exchangeItems.First();
            var result = itemToDelete.DeleteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            result.ShouldBeTrue();
        }
    }

    [Fact]
    public void MoveItem_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var ewsFolder = InitExchangeEWSFolder();

        // Act
        var folders = ewsFolder.GetAllSubFolders();
        var inboxFolder = folders.First(f => f.FolderName.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
        var targetFolder = folders.First(f => f.FolderName.Equals("Deleted Items", StringComparison.OrdinalIgnoreCase));
        inboxFolder.Open();

        var exchangeItems = inboxFolder.FindItems(100, 0, out var moreAvailable);

        // Assert
        if (exchangeItems.Any())
        {
            var itemToMove = exchangeItems.First();
            var result = itemToMove.MoveAsync(targetFolder.FolderId).ConfigureAwait(false).GetAwaiter().GetResult();

            result.ShouldBeTrue();
        }
    }

    [Fact]
    public void SetExtendedProperty_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var ewsFolder = InitExchangeEWSFolder();

        // Act
        var folders = ewsFolder.GetAllSubFolders();
        var inboxFolder = folders.First(f => f.FolderName.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
        inboxFolder.Open();

        var exchangeItems = inboxFolder.FindItems(100, 0, out var moreAvailable);

        // Assert
        if (exchangeItems.Any())
        {
            var item = exchangeItems.First();
            var propDefinition = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, Microsoft.Exchange.WebServices.Data.MapiPropertyType.String);
            var result = item.SetExtendedPropertyAsync(propDefinition, "TestValue").ConfigureAwait(false).GetAwaiter().GetResult();

            result.ShouldBeTrue();
        }
    }

    [Fact]
    public void GetMimeContent_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var ewsFolder = InitExchangeEWSFolder();

        // Act
        var folders = ewsFolder.GetAllSubFolders();
        var inboxFolder = folders.First(f => f.FolderName.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
        inboxFolder.Open();

        var exchangeItems = inboxFolder.FindItems(100, 0, out var moreAvailable);

        // Assert
        if (exchangeItems.Any())
        {
            var item = exchangeItems.First();
            var stream = item.GetMimeContentAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var resultString = new StreamReader(stream).ReadToEnd();

            resultString.ShouldNotBeNullOrEmpty();
        }
    }
}