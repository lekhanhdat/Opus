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
using AvePoint.RA.DB.Model;

namespace RAGoogleTests.FilterTests;

public class UserSyncTests
{
    /// <summary>
    /// Provides test data for various user synchronization scenarios.
    /// Each case includes a descriptive scenario name, the initial list of accounts,
    /// and the expected number of accounts after the cleanup logic is run.
    /// </summary>
    public static IEnumerable<object[]> SyncUsersTestData()
    {
        // SCENARIO 1: A list contains only GControl accounts that are not linked to
        // AOS user management. All of them should be removed.
        yield return new object[]
        {
            "Removes GControl accounts when only GControl accounts exist",
            new List<RMAccount>
            {
                new() { UserId = "107072454507836151797", AADId = "107072454507836151797" },
                new() { UserId = "113436469886373527515", AADId = "113436469886373527515" }
            },
            0 // Expected final count
        };

        // SCENARIO 2: A Google account is properly linked AOS user management
        // Neither account should be removed.
        var linkedAadId = "113436469886373527515";
        yield return new object[]
        {
            "Remove a GControl account when linked AOS user management",
            new List<RMAccount>
            {
                new() { UserId = Guid.NewGuid().ToString(), AADId = linkedAadId }, // Standard account
                new() { UserId = linkedAadId, AADId = linkedAadId }               // Linked Google account
            },
            2 // Expected final count
        };

        // SCENARIO 3: A mixed list containing one linked pair and one
        // GControl account. Only the GControl account should be removed.
        var anotherLinkedAadId = "9876543210";
        yield return new object[]
        {
            "Removes only the GControl account from a mixed list",
            new List<RMAccount>
            {
                new() { UserId = Guid.NewGuid().ToString(), AADId = anotherLinkedAadId }, // Standard account
                new() { UserId = anotherLinkedAadId, AADId = anotherLinkedAadId },       // Linked Google account
                new() { UserId = "123456789", AADId = "123456789" }                       // Google account
            },
            2 // Expected final count
        };
    }

    [Theory(DisplayName = "RemoveOrphanedGoogleAccounts: {0}")]
    [MemberData(nameof(SyncUsersTestData))]
    public void RemoveGControlAccounts_ShouldYieldCorrectResult(string scenario, List<RMAccount> dbAccounts, int expectedCount)
    {
        // Act: Run the cleanup logic on the provided list of accounts.
        RemoveGControlAccounts(dbAccounts);

        // Assert: Verify that the list contains the expected number of accounts.
        Assert.Equal(expectedCount, dbAccounts.Count);
    }

    /// <summary>
    /// Removes Google accounts from a list if they do not have a corresponding
    /// standard (GUID-based UserId) account sharing the same AADId.
    /// </summary>
    /// <param name="accounts">The list of accounts to process.</param>
    private void RemoveGControlAccounts(List<RMAccount> accounts)
    {
        // 1. Identify all AAD Ids that belong to a standard account (where UserId is a GUID).
        //    Using a HashSet provides fast lookups.
        var linkedAadIds = accounts
            .Where(acc => Guid.TryParse(acc.UserId, out _))
            .Select(acc => acc.AADId)
            .ToHashSet();

        // 2. Remove any account that is an GControl account.
        //    An account is GControl account if it's a Google account AND its AADId
        //    is NOT in the set of AAD Ids from standard accounts.
        accounts.RemoveAll(acc =>
            IsGoogleAccount(acc) && !linkedAadIds.Contains(acc.AADId)
        );
    }

    /// <summary>
    /// Determines if an account is a "Google account" based on the established convention.
    /// </summary>
    private bool IsGoogleAccount(RMAccount account)
    {
        // A Google account is identified when its UserId and AADId are identical
        // and cannot be parsed as a standard GUID.
        return account.UserId == account.AADId && !Guid.TryParse(account.AADId, out _);
    }
}