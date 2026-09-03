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
namespace RAGoogleTests.Stubs;

public class DummyApprovalProcess
{
    public const string DummyApprovalProcess1 = @"{
    ""approvalType"": 1,
    ""approvalStages"": [
        {
            ""id"": ""11111111-1111-1111-1111-111111111111"",
            ""order"": 1,
            ""approver"": {
                ""approverId"": ""113436469886373527515"",
                ""tenantId"": null,
                ""approverType"": 0,
                ""approverRoleType"": null,
                ""isUser"": false
            }
        }
    ],
    ""name"": ""DummyApprovalProcess1"",
    ""id"": ""11111111-1111-1111-1111-111111111111""
}";
    public const string DummyApprovalProcess2 = @"{
    ""approvalType"": 1,
    ""approvalStages"": [
        {
            ""id"": ""33333333-3333-3333-3333-333333333333"",
            ""order"": 1,
            ""approver"": {
                ""approverId"": ""113436469886373527515"",
                ""tenantId"": null,
                ""approverType"": 0,
                ""approverRoleType"": null,
                ""isUser"": false
            }
        }
    ],
    ""name"": ""DummyApprovalProcess2"",
    ""id"": ""33333333-3333-3333-3333-333333333333""
}";

    public const string DummyApprovalProcess3 = @"{
    ""approvalType"": 1,
    ""approvalStages"": [
        {
            ""id"": ""11111111-1111-1111-1111-111111111111"",
            ""order"": 1,
            ""approver"": {
                ""approverId"": ""113436469886373527515"",
                ""tenantId"": null,
                ""approverType"": 0,
                ""approverRoleType"": null,
                ""isUser"": false
            }
        },
        {
            ""id"": ""22222222-2222-2222-2222-222222222222"",
            ""order"": 2,
            ""approver"": {
                ""approverId"": ""02bn6wsx3clh9n2"",
                ""tenantId"": null,
                ""approverType"": 0,
                ""approverRoleType"": null,
                ""isUser"": false
            }
        }
    ],
    ""name"": ""DummyApprovalProcess3"",
    ""id"": ""33333333-3333-3333-3333-333333333333""
}";
}