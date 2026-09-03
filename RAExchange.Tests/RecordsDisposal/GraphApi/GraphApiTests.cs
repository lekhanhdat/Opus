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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.Data.AosModern;
using ExchangeBackupUtility.Graph;
using ExchangeUtility.Graph;
using Shouldly;
using EWSAuthorizationManger = AvePoint.RA.RAExchange.Authorization.AuthorizationManager;


namespace RAExchange.Tests.RecordsDisposal.GraphApi;

public class GraphApiTests : BaseExoService
{
    private IExchangeFolder InitExchangeGraphFolder()
    {
        var dict = new Dictionary<string, BposInfo>
        {
            { Address, BposInfo }
        };
        AuthorizationManager.Instance.Init(dict, authScopes: AuthScope.MicrosoftGraph);
        var authObject = AuthorizationManager.Instance.GetAuthObjectForGraph(Address);
        var factory = ExchangeBackupUtility.ExchangeFactoryProvider.Create(true);
        var emailBox = new ExchangeMailbox(Address, ExchangeMailboxType.User);
        emailBox.ObjectId = "5add1a1b-82c3-4905-8b40-26f3a50794ee";
        var graphFolder = factory.CreateFolder(emailBox, string.Empty, authObject);
        graphFolder.Open();
        return graphFolder;
    }
    private IExchangePolicyTag InitExchangeGraphPolicyTag()
    {
        var dict = new Dictionary<string, BposInfo>
        {
            { Address, BposInfo }
        };
        AuthorizationManager.Instance.Init(dict, authScopes: AuthScope.MicrosoftGraph);
        var authObject = AuthorizationManager.Instance.GetAuthObjectForGraph(Address);
        return ExchangeFactoryProvider.Create(true).CreatePolicyTag(authObject);
    }

    private ExchangeBackupUtility.ExchangeFolder InitExchangeEWSFolder()
    {
        var dict = new Dictionary<string, BposInfo>
         {
             { Address, BposInfo }
         };
        EWSAuthorizationManger.Instance.Init(dict);
        var ewsFolder = new ExchangeBackupUtility.ExchangeRootFolder(new ExchangeUtility.ExchangeMailbox(Address, ExchangeUtility.ExchangeMailboxType.User),
            EWSAuthorizationManger.Instance.GetAuthObject(Address));
        ewsFolder.Open();
        return ewsFolder;
    }

    [Fact]
    public void OpenFolder_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Assert
        graphFolder.FolderId.ShouldNotBeNullOrEmpty();
        graphFolder.FolderName.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetExchangeGraphToken_ShouldBeSuccessful()
    {
        var tokenService = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId).ModernTokenService;
        var token = await tokenService.GetTokenByAppProfileAsync(IdentityProviderType.CustomAzureApp,
            TokenResourceType.ExchangeGraph,
            "7a5dc2b6-87a6-4847-bb5c-8bea42d17810",
            "0f9ac8e2-f593-475c-b4db-d6a83ce4fa03",
            null,
            TokenType.ApplicationToken);

        token.AccessToken.ShouldNotBeNullOrEmpty();
        token.Error.ShouldBeNullOrEmpty();
    }

    [Fact]
    public void GenerateCurrentSyncState_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Act
        graphFolder.GenerateCurrentSyncState();

        // Assert
        graphFolder.FolderSyncState.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateCurrentItemSyncState_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Act
        graphFolder.GenerateCurrentItemSyncState();

        // Assert
        graphFolder.ItemSyncState.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void GetSubFolders_UsingGraph_ShouldBeSameAsEWS()
    {
        // Arrange
        var ewsFolder = InitExchangeEWSFolder();
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var subGraphFolders = graphFolder.GetAllSubFolders().Where(f => f.FolderType == "IPF.Note");
        var subEwsFolders = ewsFolder.GetAllSubFolders().Where(f => f.FolderType == "IPF.Note");
        var illegalFolderIds = subGraphFolders.Count(subGraphFolder =>  !subEwsFolders.Select(f => f.FolderId).Contains(subGraphFolder.FolderId, StringComparer.OrdinalIgnoreCase));
        // Assert
        subGraphFolders.Count().ShouldBeEquivalentTo(subEwsFolders.Count());
        illegalFolderIds.ShouldBe(0);
        subGraphFolders.Select(f => f.FolderName)
            .ShouldAllBe(f => subEwsFolders.Select(ef => ef.FolderName).Contains(f));
    }

    [Fact]
    public void GetInboxAndCalendarFolder_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var inboxAndCalendarFolder = graphFolder.GetInboxAndCalendarFolder();

        // Assert
        inboxAndCalendarFolder.ShouldNotBeEmpty();
    }

    [Fact]
    public void FindItems_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var folders = graphFolder.GetInboxAndCalendarFolder();
        var inboxFolder = folders.First(f => f.FolderName.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
        inboxFolder.Open();

        Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition extendedPropertyDefinition = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, Microsoft.Exchange.WebServices.Data.MapiPropertyType.String);
        inboxFolder.IsNestleCustomize = true;
        var exchangeItems = inboxFolder.FindItems(100, 0, out var moreAvailable, new Microsoft.Exchange.WebServices.Data.SearchFilter.Exists(extendedPropertyDefinition));

        // Assert
    }
    
    [Fact]
    public async Task GetAllItems_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var folders = graphFolder.GetInboxAndCalendarFolder();
        var inboxFolder = folders.First(f => f.FolderName.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
        inboxFolder.Open();
        
        var exchangeItems = await inboxFolder.GetAllItemsUnderFolder();

        // Assert
    }

    [Fact]
    public void DeleteItem_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var folders = graphFolder.GetInboxAndCalendarFolder();
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
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var folders = graphFolder.GetAllSubFolders();
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
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var folders = graphFolder.GetInboxAndCalendarFolder();
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

            item.TryGetExtendProperty(ExtendProperty.Term, out var customTerm);
            customTerm.ShouldBe("TestValue");
        }
    }

    [Fact]
    public void GetMimeContent_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var folders = graphFolder.GetInboxAndCalendarFolder();
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

    [Fact]
    public void ConvertFromEwsToGraphExtendedPropId_ShouldBeSuccessful()
    {
        // Arrange
        var extProp1 = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(Microsoft.Exchange.WebServices.Data.DefaultExtendedPropertySet.InternetHeaders, "X-Custom-Header", Microsoft.Exchange.WebServices.Data.MapiPropertyType.String);
        var extProp2 = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(new Guid("12345678-1234-1234-1234-1234567890AB"), "CustomName", Microsoft.Exchange.WebServices.Data.MapiPropertyType.Integer);
        var extProp3 = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(new Guid("12345678-1234-1234-1234-1234567890AB"), 0x8000, Microsoft.Exchange.WebServices.Data.MapiPropertyType.Boolean);
        var extProp4 = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(0x9000, Microsoft.Exchange.WebServices.Data.MapiPropertyType.SystemTime);
        
        var extSensitivityLabelProp = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(Microsoft.Exchange.WebServices.Data.DefaultExtendedPropertySet.InternetHeaders, "msip_labels", Microsoft.Exchange.WebServices.Data.MapiPropertyType.String);
        var extCustomClassificationProp = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, Microsoft.Exchange.WebServices.Data.MapiPropertyType.String);

        // Act
        var graphId1 = GraphExtendedPropExtension.ToGraphExtendedPropId(extProp1);
        var graphId2 = GraphExtendedPropExtension.ToGraphExtendedPropId(extProp2);
        var graphId3 = GraphExtendedPropExtension.ToGraphExtendedPropId(extProp3);
        var graphId4 = GraphExtendedPropExtension.ToGraphExtendedPropId(extProp4);

        var sensitivityLabelGraphId = GraphExtendedPropExtension.ToGraphExtendedPropId(extSensitivityLabelProp);
        var customClassificationGraphId = GraphExtendedPropExtension.ToGraphExtendedPropId(extCustomClassificationProp);

        // Assert
        graphId1.ShouldBe("String {00000000-0000-0000-0000-000000000000} Name X-Custom-Header");
        graphId2.ShouldBe("Integer {12345678-1234-1234-1234-1234567890ab} Name CustomName");
        graphId3.ShouldBe("Boolean {12345678-1234-1234-1234-1234567890ab} Id 0x8000");
        graphId4.ShouldBe("SystemTime 0x9000");

        sensitivityLabelGraphId.ShouldBe("String {00020386-0000-0000-c000-000000000046} Name msip_labels");
        customClassificationGraphId.ShouldBe($"String {{{TermColumnInfo.WellKnowTermColumnGuid}}} Id 0x{TermColumnInfo.WellKnowTermColumnId:X4}");
    }

    [Fact]
    public async Task SetRetentionLabel_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var folders = graphFolder.GetInboxAndCalendarFolder();
        var inboxFolder = folders.First(f => f.FolderName.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
        inboxFolder.Open();
        var exchangeItems = inboxFolder.FindItems(100, 0, out var moreAvailable);

        // Assert
        if (exchangeItems.Any())
        {
            var item = exchangeItems.First();
            var retentionLabelId = Guid.Parse("083253a4-e6d3-44b1-a251-2385a3ecfe6b"); // Hanson 5 Years
            var result = await item.SetRetentionLabelAsync(retentionLabelId);

            result.ShouldBeTrue();
        }
    }
    [Fact]
    public async Task GetRetentionLabels_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var policyTag = InitExchangeGraphPolicyTag();
        var tags = await policyTag.GetRetentionLabelsAsync();
        tags.ShouldNotBeNull();
    }
    [Fact]
    public async Task RemovePolicyTag_UsingGraph_ShouldBeSuccessful()
    {
        // Arrange
        var graphFolder = InitExchangeGraphFolder();

        // Act
        var folders = graphFolder.GetInboxAndCalendarFolder();
        var inboxFolder = folders.First(f => f.FolderName.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
        inboxFolder.Open();
        var exchangeItems = inboxFolder.FindItems(100, 0, out var moreAvailable);

        // Assert
        if (exchangeItems.Any())
        {
            var item = exchangeItems.First();
            item.RemovePolicyTag();
        }
    }
}