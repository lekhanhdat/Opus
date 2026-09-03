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

namespace Microsoft.Office.Project.Server.Library
{
	// Token: 0x02000E35 RID: 3637
	public enum PSErrorID
	{
		// Token: 0x04003BCE RID: 15310
		NoError,
		// Token: 0x04003BCF RID: 15311
		Success = 0,
		// Token: 0x04003BD0 RID: 15312
		[ULSParameter(0, "error")]
		ActiveCacheInvalidDataFormat = 12000,
		// Token: 0x04003BD1 RID: 15313
		[ULSParameter(0, "version")]
		ActiveCacheUnsupportedDataFormatVersion,
		// Token: 0x04003BD2 RID: 15314
		[ULSParameter(1, "messageType")]
		[ULSParameter(0, "_messageID")]
		ActiveCacheInvalidQueuedMessageType = 12003,
		// Token: 0x04003BD3 RID: 15315
		[ULSParameter(0, "_messageID")]
		ActiveCacheNullQueuedMessage,
		// Token: 0x04003BD4 RID: 15316
		[ULSParameter(1, "error")]
		[ULSParameter(0, "_messageID")]
		ActiveCacheQueuedMessageExecutionError,
		// Token: 0x04003BD5 RID: 15317
		[ULSParameter(0, "dataSize")]
		ActiveCacheInvalidDataSize,
		// Token: 0x04003BD6 RID: 15318
		ActiveCacheQueueJobAlreadyStarted,
		// Token: 0x04003BD7 RID: 15319
		ActiveCacheInvalidQueuedMessageFormat,
		// Token: 0x04003BD8 RID: 15320
		[ULSParameter(0, "version")]
		ActiveCacheUnsupportedQueuedMessageVersion,
		// Token: 0x04003BD9 RID: 15321
		[ULSParameter(0, "dataType")]
		ActiveCacheUnsupportedQueueDataType = 12011,
		// Token: 0x04003BDA RID: 15322
		ActiveCacheInvalidVersionStampForSave,
		// Token: 0x04003BDB RID: 15323
		ActiveCacheProjectTypeMismatch,
		// Token: 0x04003BDC RID: 15324
		ActiveCacheDataValidationFailed,
		// Token: 0x04003BDD RID: 15325
		[ULSParameter(0, "version")]
		ActiveCacheUnsupportedProjectProfessionalVersion,
		// Token: 0x04003BDE RID: 15326
		ActiveCacheGeneralSQLException,
		// Token: 0x04003BDF RID: 15327
		ActiveCacheIncompleteJobSendCanceled,
		// Token: 0x04003BE0 RID: 15328
		AdminViewNameAlreadyExists = 16600,
		// Token: 0x04003BE1 RID: 15329
		AdminViewInvalidDividerPosition,
		// Token: 0x04003BE2 RID: 15330
		AdminViewDataWasTampered,
		// Token: 0x04003BE3 RID: 15331
		AdminViewMaxDisplayedFieldsNumberExceeded,
		// Token: 0x04003BE4 RID: 15332
		AdminViewCannotDeleteDefaultView,
		// Token: 0x04003BE5 RID: 15333
		AdminViewCannotCopyDefaultView,
		// Token: 0x04003BE6 RID: 15334
		AdminViewRequiredFieldNotPresent,
		// Token: 0x04003BE7 RID: 15335
		AdminLocalCustomFieldInvalid = 19011,
		// Token: 0x04003BE8 RID: 15336
		AdminEnterpriseCustomFieldInvalid,
		// Token: 0x04003BE9 RID: 15337
		AdminNTAccountNotFound = 19032,
		// Token: 0x04003BEA RID: 15338
		AdminUnableToMerge = 20003,
		// Token: 0x04003BEB RID: 15339
		AdminDeleteArchivedProjectsFailed = 25004,
		// Token: 0x04003BEC RID: 15340
		AdminUpdateArchiveScheduleFailed = 25006,
		// Token: 0x04003BED RID: 15341
		AdminArchiveScheduleFailed = 28018,
		// Token: 0x04003BEE RID: 15342
		AdminReadArchivedProjectsListFailed,
		// Token: 0x04003BEF RID: 15343
		AdminReadArchiveScheduleFailed,
		// Token: 0x04003BF0 RID: 15344
		AdminUserAccountNameNull,
		// Token: 0x04003BF1 RID: 15345
		AdminIsWindowsUserNull,
		// Token: 0x04003BF2 RID: 15346
		[ULSParameter(0, "column")]
		AdminInvalidTimePeriodState,
		// Token: 0x04003BF3 RID: 15347
		[ULSParameter(0, "currencycode")]
		AdminGlobalUpdateFailed,
		// Token: 0x04003BF4 RID: 15348
		[ULSParameter(0, "currencycode")]
		AdminGlobalCheckedOut,
		// Token: 0x04003BF5 RID: 15349
		AdminInvalidDatabaseTimeout,
		// Token: 0x04003BF6 RID: 15350
		AdminInvalidDatabaseTimeoutType,
		// Token: 0x04003BF7 RID: 15351
		AdminInvalidEntityType,
		// Token: 0x04003BF8 RID: 15352
		[ULSParameter(0, "mode")]
		AdminInvalidCompatibilityModeChange,
		// Token: 0x04003BF9 RID: 15353
		[ULSParameter(0, "mode")]
		AdminInvalidCompatibilityMode,
		// Token: 0x04003BFA RID: 15354
		[ULSParameter(0, "versions")]
		AdminInvalidProjectProfessionalVersions,
		// Token: 0x04003BFB RID: 15355
		[ULSParameter(0, "version")]
		AdminInvalidProjectProfessionalVersion,
		// Token: 0x04003BFC RID: 15356
		[ULSParameter(0, "versions")]
		AdminTooManyProjectProfessionalVersions,
		// Token: 0x04003BFD RID: 15357
		[ULSParameter(0, "versions")]
		AdminDuplicateProjectProfessionalMajorVersions,
		// Token: 0x04003BFE RID: 15358
		[ULSParameter(0, "flags")]
		AdminInvalidServerFlags,
		// Token: 0x04003BFF RID: 15359
		AdminNullProjectProfessionalVersions,
		// Token: 0x04003C00 RID: 15360
		[ULSParameter(0, "projectuid")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		ArchiveProjectFailure = 25000,
		// Token: 0x04003C01 RID: 15361
		ArchiveProjectsFailed,
		// Token: 0x04003C02 RID: 15362
		ArchiveProjectFailed,
		// Token: 0x04003C03 RID: 15363
		ArchiveResourcesFailed = 25007,
		// Token: 0x04003C04 RID: 15364
		ArchiveCustomFieldsFailed,
		// Token: 0x04003C05 RID: 15365
		[PSObsolete]
		ArchiveSystemSettingsFailed = 25010,
		// Token: 0x04003C06 RID: 15366
		ArchiveCategoriesFailed = 25012,
		// Token: 0x04003C07 RID: 15367
		ArchiveViewsFailed = 25014,
		// Token: 0x04003C08 RID: 15368
		ArchiveGlobalProjectFailed = 25016,
		// Token: 0x04003C09 RID: 15369
		ArchiveReadProjectArchiveRetentionSettingFailed = 25033,
		// Token: 0x04003C0A RID: 15370
		ArchiveInvalidRetentionPolicyValue = 25018,
		// Token: 0x04003C0B RID: 15371
		[ULSParameter(0, "projectuid")]
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(2, "messageID")]
		ArchiveCustomFieldsFailure,
		// Token: 0x04003C0C RID: 15372
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(2, "messageID")]
		ArchiveGlobalProjectFailure,
		// Token: 0x04003C0D RID: 15373
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(0, "projectuid")]
		ArchiveResourcesFailure,
		// Token: 0x04003C0E RID: 15374
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		ArchiveSystemSettingsFailure,
		// Token: 0x04003C0F RID: 15375
		[ULSParameter(2, "messageID")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(3, "stage")]
		ArchiveViewsFailure,
		// Token: 0x04003C10 RID: 15376
		[ULSParameter(4, "blocking")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(3, "stage")]
		ArchiveCategoriesFailure,
		// Token: 0x04003C11 RID: 15377
		[ULSParameter(2, "messageID")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(3, "stage")]
		ResourcePlanPublishFailure,
		// Token: 0x04003C12 RID: 15378
		[ULSParameter(2, "messageID")]
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		RestoreCategoriesFailure,
		// Token: 0x04003C13 RID: 15379
		[ULSParameter(2, "messageID")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(3, "stage")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(0, "projectuid")]
		RestoreCustomFieldsFailure,
		// Token: 0x04003C14 RID: 15380
		[ULSParameter(4, "blocking")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(3, "stage")]
		RestoreGlobalProjectFailure,
		// Token: 0x04003C15 RID: 15381
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		RestoreProjectFailure,
		// Token: 0x04003C16 RID: 15382
		[ULSParameter(0, "projectuid")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(1, "messagetype")]
		RestoreResourcesFailure,
		// Token: 0x04003C17 RID: 15383
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(1, "messagetype")]
		[PSObsolete]
		[ULSParameter(2, "messageID")]
		[ULSParameter(0, "projectuid")]
		RestoreSystemSettingsFailure,
		// Token: 0x04003C18 RID: 15384
		[ULSParameter(1, "messagetype")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		RestoreViewsFailure,
		// Token: 0x04003C19 RID: 15385
		AssignmentNotFound = 120,
		// Token: 0x04003C1A RID: 15386
		AssignmentWrongTrackingMethod = 122,
		// Token: 0x04003C1B RID: 15387
		AssignmentWorkTypeInvalid = 127,
		// Token: 0x04003C1C RID: 15388
		AssignmentRateTableInvalid = 130,
		// Token: 0x04003C1D RID: 15389
		AssignmentAlreadyExists,
		// Token: 0x04003C1E RID: 15390
		AssignmentDuplicateSpecified,
		// Token: 0x04003C1F RID: 15391
		AssignmentUidInvalid,
		// Token: 0x04003C20 RID: 15392
		AssignmentDelayInvalid,
		// Token: 0x04003C21 RID: 15393
		AssignmentCannotEditSummaryTask,
		// Token: 0x04003C22 RID: 15394
		AssignmentInvalid,
		// Token: 0x04003C23 RID: 15395
		AssignmentFieldsInvalidForBudget,
		// Token: 0x04003C24 RID: 15396
		AssignmentAlreadyAssignedToResource,
		// Token: 0x04003C25 RID: 15397
		AssignmentInvalidOwner,
		// Token: 0x04003C26 RID: 15398
		AssignmentDeletedDateTooOld,
		// Token: 0x04003C27 RID: 15399
		CalendarUidInvalid = 77,
		// Token: 0x04003C28 RID: 15400
		[ULSParameter(1, "exceptionName")]
		[ULSParameter(0, "shiftNumber")]
		CalendarOnlyOneShiftIsNull = 13000,
		// Token: 0x04003C29 RID: 15401
		[ULSParameter(0, "exceptionName")]
		CalendarRecurrenceDaysShouldBeNull,
		// Token: 0x04003C2A RID: 15402
		[ULSParameter(0, "exceptionName")]
		CalendarRecurrenceMonthDayShouldBeNull,
		// Token: 0x04003C2B RID: 15403
		[ULSParameter(0, "exceptionName")]
		CalendarRecurrenceMonthShouldBeNull,
		// Token: 0x04003C2C RID: 15404
		[ULSParameter(0, "exceptionName")]
		CalendarRecurrenceMonthShouldNotBeNull,
		// Token: 0x04003C2D RID: 15405
		[ULSParameter(0, "exceptionName")]
		CalendarRecurrencePositionShouldBeNull,
		// Token: 0x04003C2E RID: 15406
		[ULSParameter(0, "exceptionName")]
		CalendarRecurrencePositionShouldNotBeNull,
		// Token: 0x04003C2F RID: 15407
		[ULSParameter(0, "exceptionName")]
		CalendarRecurrenceDaysShouldNotBeNull,
		// Token: 0x04003C30 RID: 15408
		[ULSParameter(0, "exceptionName")]
		CalendarInvalidRecurrenceFrequency,
		// Token: 0x04003C31 RID: 15409
		[ULSParameter(0, "exceptionName")]
		CalendarInvalidRecurrenceType,
		// Token: 0x04003C32 RID: 15410
		[ULSParameter(0, "exceptionName")]
		CalendarInvalidRecurrenceDays,
		// Token: 0x04003C33 RID: 15411
		[ULSParameter(0, "exceptionName")]
		CalendarInvalidCombinationOfMonthDayAndPosition,
		// Token: 0x04003C34 RID: 15412
		[ULSParameter(0, "exceptionName")]
		CalendarInvalidRecurrencePosition,
		// Token: 0x04003C35 RID: 15413
		CalendarCannotModifyExceptionsForCalendarBeingDeleted,
		// Token: 0x04003C36 RID: 15414
		CalendarExceptionConflict,
		// Token: 0x04003C37 RID: 15415
		[ULSParameter(0, "exceptionName")]
		CalendarBadDateValue,
		// Token: 0x04003C38 RID: 15416
		CalendarNotFound = 13021,
		// Token: 0x04003C39 RID: 15417
		CalendarAlreadyExists,
		// Token: 0x04003C3A RID: 15418
		CalendarNameShouldNotBeNull,
		// Token: 0x04003C3B RID: 15419
		CalendarInternalError = 13025,
		// Token: 0x04003C3C RID: 15420
		CalendarNameTooLong = 13027,
		// Token: 0x04003C3D RID: 15421
		CalendarInvalidCalendarName,
		// Token: 0x04003C3E RID: 15422
		CalendarStandardCalendarNotFound = 13031,
		// Token: 0x04003C3F RID: 15423
		[ULSParameter(0, "shiftNumber")]
		CalendarInvalidShifts,
		// Token: 0x04003C40 RID: 15424
		[ULSParameter(0, "projectGuids")]
		CalendarCannotDeleteCalendarUsedByProject,
		// Token: 0x04003C41 RID: 15425
		CalCalendarUniqueIdToDuplicateShouldBeNull = 13035,
		// Token: 0x04003C42 RID: 15426
		CalendarInvalidBaseCalendarUniqueId = 13037,
		// Token: 0x04003C43 RID: 15427
		CalendarInvalidUniqueIdToDuplicate,
		// Token: 0x04003C44 RID: 15428
		[ULSParameter(0, "exceptionName")]
		CalendarUnusedCalendarException,
		// Token: 0x04003C45 RID: 15429
		CalendarCannotDeleteStandardCalendar,
		// Token: 0x04003C46 RID: 15430
		CalendarCannotRenameStandardCalendar,
		// Token: 0x04003C47 RID: 15431
		[ULSParameter(0, "resourceGuids")]
		CalendarCannotDeleteCalendarUsedByEnterpriseResource,
		// Token: 0x04003C48 RID: 15432
		CalendarFilterInvalid,
		// Token: 0x04003C49 RID: 15433
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		CBSGeneralFailure = 17001,
		// Token: 0x04003C4A RID: 15434
		[ULSParameter(0, "QueueMessageBody")]
		CBSDsoNotInstalled,
		// Token: 0x04003C4B RID: 15435
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		CBSASConnectionFailure,
		// Token: 0x04003C4C RID: 15436
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		CBSOlapProcessingFailure,
		// Token: 0x04003C4D RID: 15437
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		CBSMetadataProcessingFailure,
		// Token: 0x04003C4E RID: 15438
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		CBSASServerLockTimeOut,
		// Token: 0x04003C4F RID: 15439
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		CBSOlapDatabaseSetupFailure,
		// Token: 0x04003C50 RID: 15440
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		CBSASEntityLimitation,
		// Token: 0x04003C51 RID: 15441
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "CBSRequest")]
		CBSRequestInvalidArguments,
		// Token: 0x04003C52 RID: 15442
		[ULSParameter(0, "CBSRequest")]
		[ULSParameter(1, "Error")]
		CBSQueueingRequestFailed,
		// Token: 0x04003C53 RID: 15443
		[ULSParameter(0, "Error")]
		CBSUpdateCubeCalculatedMeasureDefintionError,
		// Token: 0x04003C54 RID: 15444
		[ULSParameter(0, "CBSRequest")]
		CBSAttemptToOverwrite = 17013,
		// Token: 0x04003C55 RID: 15445
		[ULSParameter(0, "CustomFieldUID")]
		CBSCustomFieldCannotBeAddedAsDimension,
		// Token: 0x04003C56 RID: 15446
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "CustomFieldUID")]
		CBSCustomFieldFailedToBeAddedAsDimension,
		// Token: 0x04003C57 RID: 15447
		[ULSParameter(0, "CustomFieldUID")]
		CBSCustomFieldCannotBeAddedAsMeasure,
		// Token: 0x04003C58 RID: 15448
		[ULSParameter(0, "CustomFieldUID")]
		[ULSParameter(1, "Error")]
		CBSCustomFieldFailedToBeAddedAsMeasure,
		// Token: 0x04003C59 RID: 15449
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "CBSRequest")]
		CBSDsoTranslatorNotFound,
		// Token: 0x04003C5A RID: 15450
		[ULSParameter(0, "Error")]
		CBSUpdateOlapDBOperationFailure,
		// Token: 0x04003C5B RID: 15451
		[ULSParameter(0, "Error")]
		CBSOlapDBInvalidArguments,
		// Token: 0x04003C5C RID: 15452
		[ULSParameter(0, "Error")]
		CBSOlapDatabaseReadSettingListFailed,
		// Token: 0x04003C5D RID: 15453
		[ULSParameter(0, "Error")]
		CBSOlapDatabaseReadSettingFailed,
		// Token: 0x04003C5E RID: 15454
		[ULSParameter(0, "Error")]
		CBSDeleteOlapDatabaseSetting,
		// Token: 0x04003C5F RID: 15455
		[ULSParameter(0, "Error")]
		CBSSetDefaultOlapDatabase,
		// Token: 0x04003C60 RID: 15456
		[ULSParameter(0, "Error")]
		CBSSetOlapDatabaseEnabled,
		// Token: 0x04003C61 RID: 15457
		[ULSParameter(0, "Error")]
		CBSGetDefaultOlapDatabase,
		// Token: 0x04003C62 RID: 15458
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "CustomFieldUID")]
		CBSCustomFieldFailedToBeAddedAsDimensionOrMeasure,
		// Token: 0x04003C63 RID: 15459
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "OlapDatabaseGuid")]
		CBSOlapDatabaseAssocFieldsSettings,
		// Token: 0x04003C64 RID: 15460
		[ULSParameter(0, "Error")]
		CBSUpdateOlapDBOperationDuplicateOrFailure,
		// Token: 0x04003C65 RID: 15461
		[ULSParameter(0, "Error")]
		CBSErrorReadingDefaultDatabase,
		// Token: 0x04003C66 RID: 15462
		[ULSParameter(0, "Error")]
		CBSCreateOlapDBOperationFailure,
		// Token: 0x04003C67 RID: 15463
		[ULSParameter(0, "Error")]
		CBSSetCubeFieldsSettingsFromListForGroupMeasureFailed,
		// Token: 0x04003C68 RID: 15464
		[ULSParameter(0, "Error")]
		CBSErrorReadingCubeDepartments,
		// Token: 0x04003C69 RID: 15465
		[ULSParameter(0, "Error")]
		CBSErrorMaxOlapDatabaseCountReached,
		// Token: 0x04003C6A RID: 15466
		[ULSParameter(0, "Error")]
		CBSErrorReadingCubeFieldsSettings,
		// Token: 0x04003C6B RID: 15467
		CICOCheckedOutToOtherUser = 10100,
		// Token: 0x04003C6C RID: 15468
		CICOAlreadyCheckedOutToYou,
		// Token: 0x04003C6D RID: 15469
		CICONotCheckedOut,
		// Token: 0x04003C6E RID: 15470
		CICOCheckedOutInOtherSession,
		// Token: 0x04003C6F RID: 15471
		CICOInvalidSessionGuid,
		// Token: 0x04003C70 RID: 15472
		CICOAlreadyCheckedOutInSameSession,
		// Token: 0x04003C71 RID: 15473
		CICOCannotCheckOutVisibilityModeProjectWithMppInDocLib,
		// Token: 0x04003C72 RID: 15474
		CustomFieldInvalidPropertyType = 11500,
		// Token: 0x04003C73 RID: 15475
		CustomFieldInvalidScope = 11503,
		// Token: 0x04003C74 RID: 15476
		CustomFieldScopesMustBeIdentical,
		// Token: 0x04003C75 RID: 15477
		CustomFieldInvalidEntityUID,
		// Token: 0x04003C76 RID: 15478
		CustomFieldHasInvalidPropertiesForNonLookupTableCF,
		// Token: 0x04003C77 RID: 15479
		CustomFieldNonExistentWeightsTableUID,
		// Token: 0x04003C78 RID: 15480
		CustomFieldInvalidName,
		// Token: 0x04003C79 RID: 15481
		CustomFieldInvalidDefault = 11510,
		// Token: 0x04003C7A RID: 15482
		CustomFieldInvalidLookupTableUID,
		// Token: 0x04003C7B RID: 15483
		CustomFieldTypeDoesNotMatchLookupTableMask,
		// Token: 0x04003C7C RID: 15484
		CustomFieldCannotHaveNonLeafNodeDefault,
		// Token: 0x04003C7D RID: 15485
		CustomFieldMatchingOnlyAvailableForResources,
		// Token: 0x04003C7E RID: 15486
		CustomFieldUIDCannotMatchLookupTableUID = 11516,
		// Token: 0x04003C7F RID: 15487
		CustomFieldUIDAlreadyExists,
		// Token: 0x04003C80 RID: 15488
		CustomFieldIDAlreadyExists,
		// Token: 0x04003C81 RID: 15489
		CustomFieldNameAlreadyExists,
		// Token: 0x04003C82 RID: 15490
		CustomFieldInvalidEntity,
		// Token: 0x04003C83 RID: 15491
		CustomFieldMaskDoesNotMatchEntityType,
		// Token: 0x04003C84 RID: 15492
		CustomFieldLowerOrderBitsOutOfRange,
		// Token: 0x04003C85 RID: 15493
		CustomFieldInvalidMaxValues,
		// Token: 0x04003C86 RID: 15494
		CustomFieldCannotModifyCertainValuesOnceDefined,
		// Token: 0x04003C87 RID: 15495
		CustomFieldNonExistentPID = 11526,
		// Token: 0x04003C88 RID: 15496
		CustomFieldCannotChangeBuiltInFields,
		// Token: 0x04003C89 RID: 15497
		CustomFieldSecondaryUidCannotEqualUid,
		// Token: 0x04003C8A RID: 15498
		CustomFieldCannotHaveSecondaryUIDorIDForThisEntityType,
		// Token: 0x04003C8B RID: 15499
		CustomFieldNameMatchesIntrinsicField,
		// Token: 0x04003C8C RID: 15500
		CustomFieldInvalidAggregationType,
		// Token: 0x04003C8D RID: 15501
		CustomFieldProjectFormulaFieldsMustUseFormulaAggregation,
		// Token: 0x04003C8E RID: 15502
		CustomFieldMustSpecifyEitherIDorUID = 11700,
		// Token: 0x04003C8F RID: 15503
		CustomFieldInvalidID,
		// Token: 0x04003C90 RID: 15504
		CustomFieldInvalidUID,
		// Token: 0x04003C91 RID: 15505
		CustomFieldInvalidType,
		// Token: 0x04003C92 RID: 15506
		CustomFieldInvalidTypeColumnFilledIn,
		// Token: 0x04003C93 RID: 15507
		CustomFieldCodeValueDoesNotMatchLookupTable = 11706,
		// Token: 0x04003C94 RID: 15508
		CustomFieldCodeValueIsNotLeafNode,
		// Token: 0x04003C95 RID: 15509
		CustomFieldRowAlreadyExists,
		// Token: 0x04003C96 RID: 15510
		CustomFieldRowDoesNotMatchCorrespondingDefinitionInDB = 11710,
		// Token: 0x04003C97 RID: 15511
		CustomFieldCodeValueAlreadyUsed,
		// Token: 0x04003C98 RID: 15512
		CustomFieldMaxValuesExceeded,
		// Token: 0x04003C99 RID: 15513
		[ULSParameter(0, "mdpropuid")]
		CustomFieldRequiredValueNotProvided,
		// Token: 0x04003C9A RID: 15514
		CustomFieldCannotChangeLookupTable = 11715,
		// Token: 0x04003C9B RID: 15515
		CustomFieldFilterInvalid,
		// Token: 0x04003C9C RID: 15516
		CustomFieldRolldownInvalidOnFormulaFields,
		// Token: 0x04003C9D RID: 15517
		CustomFieldFormulaFieldCannotBeRequired,
		// Token: 0x04003C9E RID: 15518
		CustomFieldFormulaFieldCannotBeWorkflowControlled,
		// Token: 0x04003C9F RID: 15519
		CustomFieldCannotSetValueOnFormulaFields,
		// Token: 0x04003CA0 RID: 15520
		CustomFieldNewPerRequestLimitExcedeed,
		// Token: 0x04003CA1 RID: 15521
		CustomFieldNameIsReservedName,
		// Token: 0x04003CA2 RID: 15522
		CustomFieldNameInvalidForOlapMeasure,
		// Token: 0x04003CA3 RID: 15523
		CustomFieldNameInvalidForOlapDimension,
		// Token: 0x04003CA4 RID: 15524
		CustomFieldSettingsInvalidForOlapMeasure,
		// Token: 0x04003CA5 RID: 15525
		CustomFieldSettingsInvalidForOlapDimension,
		// Token: 0x04003CA6 RID: 15526
		CustomFieldCannotAddRelativeImportanceField,
		// Token: 0x04003CA7 RID: 15527
		CustomFieldCannotAddProjectImpactField,
		// Token: 0x04003CA8 RID: 15528
		CustomFieldInvalidDepartmentUid = 11731,
		// Token: 0x04003CA9 RID: 15529
		CustomFieldCannotModifyDepartmentUidOnBuiltinFields,
		// Token: 0x04003CAA RID: 15530
		CustomFieldCannotHaveBothLookupTableAndMultilineText,
		// Token: 0x04003CAB RID: 15531
		CustomFieldCannotHaveBothFormulaAndMultilineText,
		// Token: 0x04003CAC RID: 15532
		CustomFieldDescriptionExceedsLimit,
		// Token: 0x04003CAD RID: 15533
		CustomFieldOnlyTextFieldsCanHaveMultilineText,
		// Token: 0x04003CAE RID: 15534
		CustomFieldOnlyProjectFieldsCanHaveMultilineText,
		// Token: 0x04003CAF RID: 15535
		CustomFieldCannotChangeWorkflowControlledBehaviorForNonProjectCustomFields,
		// Token: 0x04003CB0 RID: 15536
		CustomFieldIsWorkflowControlledAndCannotBeChanged,
		// Token: 0x04003CB1 RID: 15537
		CustomFieldCannotHaveRequiredFlagWhenWorkflowControlledFlagIsSet,
		// Token: 0x04003CB2 RID: 15538
		CustomFieldFormulaCreatesCircularReference = 11742,
		// Token: 0x04003CB3 RID: 15539
		CustomFieldFormulaContainsInvalidFieldReference,
		// Token: 0x04003CB4 RID: 15540
		CustomFieldFormulaContainsErrors,
		// Token: 0x04003CB5 RID: 15541
		CustomFieldLocalCustomFieldNotDefined,
		// Token: 0x04003CB6 RID: 15542
		CustomFieldGraphicalIndicatorContainsErrors,
		// Token: 0x04003CB7 RID: 15543
		CustomFieldGraphicalIndicatorContainsInvalidFieldReference,
		// Token: 0x04003CB8 RID: 15544
		CustomFieldGraphicalIndicatorTypeMismatch,
		// Token: 0x04003CB9 RID: 15545
		CustomFieldFormulaFieldCannotReferenceWorkflowControlledField,
		// Token: 0x04003CBA RID: 15546
		CustomFieldWorkflowCustomFieldBeingReferencedByFormula,
		// Token: 0x04003CBB RID: 15547
		GeneralRequestInvalidParameter = 6,
		// Token: 0x04003CBC RID: 15548
		GeneralInvalidValue = 11,
		// Token: 0x04003CBD RID: 15549
		GeneralStartDateGTorEQFinishDate = 26,
		// Token: 0x04003CBE RID: 15550
		GeneralQueueOperationInProcess = 29,
		// Token: 0x04003CBF RID: 15551
		[ULSParameter(0, "Exception")]
		GeneralUnhandledException = 42,
		// Token: 0x04003CC0 RID: 15552
		GeneralDuplicateGUIDSpecified = 66,
		// Token: 0x04003CC1 RID: 15553
		[ULSParameter(0, "column")]
		GeneralDateNotValid = 69,
		// Token: 0x04003CC2 RID: 15554
		[ULSParameter(0, "column")]
		GeneralCostInvalid,
		// Token: 0x04003CC3 RID: 15555
		[ULSParameter(0, "column")]
		GeneralWorkInvalid,
		// Token: 0x04003CC4 RID: 15556
		[ULSParameter(0, "column")]
		GeneralDurationInvalid,
		// Token: 0x04003CC5 RID: 15557
		[ULSParameter(0, "column")]
		GeneralUnitsInvalid,
		// Token: 0x04003CC6 RID: 15558
		GeneralOnlyInsertsAllowed,
		// Token: 0x04003CC7 RID: 15559
		GeneralOnlyUpdatesAllowed,
		// Token: 0x04003CC8 RID: 15560
		GeneralSessionInvalid,
		// Token: 0x04003CC9 RID: 15561
		GeneralDependencyUidInvalid = 78,
		// Token: 0x04003CCA RID: 15562
		GeneralNumberInvalid,
		// Token: 0x04003CCB RID: 15563
		GeneralInvalidDataStore,
		// Token: 0x04003CCC RID: 15564
		GeneralDurationOrWorkFormatInvalid = 513,
		// Token: 0x04003CCD RID: 15565
		GeneralRateFormatInvalid = 518,
		// Token: 0x04003CCE RID: 15566
		[ULSParameter(0, "messageID")]
		[ULSParameter(1, "exception")]
		GeneralQueueException = 9131,
		// Token: 0x04003CCF RID: 15567
		GeneralItemDoesNotExist = 10000,
		// Token: 0x04003CD0 RID: 15568
		GeneralLCIDInvalid,
		// Token: 0x04003CD1 RID: 15569
		GeneralRowDoesNotExist,
		// Token: 0x04003CD2 RID: 15570
		[ULSParameter(0, "column")]
		GeneralInvalidColumnValue = 20000,
		// Token: 0x04003CD3 RID: 15571
		GeneralInvalidDataRowState,
		// Token: 0x04003CD4 RID: 15572
		[ULSParameter(0, "dupName")]
		GeneralDuplicatedNames = 20004,
		// Token: 0x04003CD5 RID: 15573
		[ULSParameter(0, "column")]
		GeneralReadOnlyColumn,
		// Token: 0x04003CD6 RID: 15574
		GeneralReadOnlyRow,
		// Token: 0x04003CD7 RID: 15575
		[ULSParameter(0, "column")]
		GeneralNotNullColumn,
		// Token: 0x04003CD8 RID: 15576
		[ULSParameter(0, "objectUID")]
		GeneralObjectAlreadyExists,
		// Token: 0x04003CD9 RID: 15577
		[ULSParameter(0, "parameter")]
		GeneralInvalidObject,
		// Token: 0x04003CDA RID: 15578
		GeneralSecurityAccessDenied,
		// Token: 0x04003CDB RID: 15579
		GeneralInvalidOperation,
		// Token: 0x04003CDC RID: 15580
		GeneralInvalidCharacters,
		// Token: 0x04003CDD RID: 15581
		[ULSParameter(0, "column")]
		GeneralNameTooLong,
		// Token: 0x04003CDE RID: 15582
		GeneralNameCannotBeBlank,
		// Token: 0x04003CDF RID: 15583
		GeneralInvalidOperationOnReadOnlyValue = 20016,
		// Token: 0x04003CE0 RID: 15584
		GeneralInvalidDateOverlap = 20018,
		// Token: 0x04003CE1 RID: 15585
		GeneralParameterCannotBeNull = 20020,
		// Token: 0x04003CE2 RID: 15586
		GeneralDescTooLong,
		// Token: 0x04003CE3 RID: 15587
		[ULSParameter(0, "ObjectUid")]
		[ULSParameter(1, "Permission")]
		GeneralCategoryPermissionDenied,
		// Token: 0x04003CE4 RID: 15588
		[ULSParameter(0, "Permission")]
		GeneralGlobalPermissionDenied,
		// Token: 0x04003CE5 RID: 15589
		[ULSParameter(1, "user")]
		[ULSParameter(2, "resuid")]
		[ULSParameter(0, "method")]
		GeneralNotLicensed,
		// Token: 0x04003CE6 RID: 15590
		[ULSParameter(0, "CancelReason")]
		[ULSParameter(1, "EventName")]
		GeneralActionCanceledByEventHandler = 22000,
		// Token: 0x04003CE7 RID: 15591
		GeneralActionCanceledBecauseServerEventServiceNotFound,
		// Token: 0x04003CE8 RID: 15592
		[ULSParameter(0, "EventName")]
		GeneralActionCanceledBecauseServerEventServiceProblem,
		// Token: 0x04003CE9 RID: 15593
		[ULSParameter(0, "EventName")]
		GeneralActionProceedWithServerEventServiceProblem = 22020,
		// Token: 0x04003CEA RID: 15594
		[ULSParameter(5, "Stage")]
		[ULSParameter(1, "ComputerName")]
		[ULSParameter(3, "MessageType")]
		[ULSParameter(4, "MessageId")]
		[ULSParameter(7, "CancellationMessage")]
		[ULSParameter(0, "JobUID")]
		[ULSParameter(2, "GroupType")]
		[ULSParameter(6, "CorrelationUID")]
		GeneralQueueJobFailed = 26000,
		// Token: 0x04003CEB RID: 15595
		[ULSParameter(0, "JobUID")]
		GeneralQueueInvalidJobUID,
		// Token: 0x04003CEC RID: 15596
		[ULSParameter(0, "TrackingUID")]
		GeneralQueueInvalidTrackingUID,
		// Token: 0x04003CED RID: 15597
		[ULSParameter(0, "JobInfoUIDUID")]
		GeneralQueueInvalidJobInfoUID,
		// Token: 0x04003CEE RID: 15598
		[ULSParameter(0, "CorrelationUID")]
		GeneralQueueInvalidCorrelationUID,
		// Token: 0x04003CEF RID: 15599
		[ULSParameter(0, "CorrelationUID")]
		[ULSParameter(1, "JobUID")]
		[ULSParameter(2, "JobType")]
		GeneralQueueCorrelationBlocked,
		// Token: 0x04003CF0 RID: 15600
		GeneralQueueInvalidMessageType,
		// Token: 0x04003CF1 RID: 15601
		GeneralQueueInvalidJobState,
		// Token: 0x04003CF2 RID: 15602
		GeneralQueueInvalidGroupState,
		// Token: 0x04003CF3 RID: 15603
		GeneralQueueInvalidGroupPriority,
		// Token: 0x04003CF4 RID: 15604
		GeneralQueueInvalidCorrelationPriority,
		// Token: 0x04003CF5 RID: 15605
		GeneralQueueInvalidQueueID,
		// Token: 0x04003CF6 RID: 15606
		GeneralQueueInvalidAdminAction,
		// Token: 0x04003CF7 RID: 15607
		GeneralQueueInvalidStatType,
		// Token: 0x04003CF8 RID: 15608
		GeneralQueueInvalidBlockPolicy,
		// Token: 0x04003CF9 RID: 15609
		GeneralQueueCannotRetryJob,
		// Token: 0x04003CFA RID: 15610
		GeneralQueueInvalidSetting,
		// Token: 0x04003CFB RID: 15611
		GeneralQueueInvalidRendezvousUID,
		// Token: 0x04003CFC RID: 15612
		GeneralDalErrorGettingConnectionStrings,
		// Token: 0x04003CFD RID: 15613
		GeneralDalErrorConnectingToDatabase,
		// Token: 0x04003CFE RID: 15614
		GeneralDalInvalidArgumentCountCreatingFilter,
		// Token: 0x04003CFF RID: 15615
		GeneralDataTableCannotBeNull = 26024,
		// Token: 0x04003D00 RID: 15616
		GeneralDatasetConstraints,
		// Token: 0x04003D01 RID: 15617
		GeneralInvalidDataSetStructure = 26027,
		// Token: 0x04003D02 RID: 15618
		GeneralDalNoRowsUpdated,
		// Token: 0x04003D03 RID: 15619
		GeneralDataTableCannotBeEmpty,
		// Token: 0x04003D04 RID: 15620
		GeneralWSSContentDBNotWritable,
		// Token: 0x04003D05 RID: 15621
		GeneralSPValidateFormDigestError,
		// Token: 0x04003D06 RID: 15622
		GeneralDelegationActiveForCurrentUser,
		// Token: 0x04003D07 RID: 15623
		GeneralDalDatabaseIsReadOnly = 26034,
		// Token: 0x04003D08 RID: 15624
		GeneralDatabaseCommunicationError,
		// Token: 0x04003D09 RID: 15625
		LookupTableMaskNotDefined = 11000,
		// Token: 0x04003D0A RID: 15626
		LookupTableMaskHasTooManyValues,
		// Token: 0x04003D0B RID: 15627
		LookupTableMaskHasGaps,
		// Token: 0x04003D0C RID: 15628
		LookupTableMaskSequenceTypeLimitedToOneLevelDeep,
		// Token: 0x04003D0D RID: 15629
		LookupTableMaskSequenceTypeInvalid,
		// Token: 0x04003D0E RID: 15630
		LookupTableMaskSequenceRequiresAnyLength,
		// Token: 0x04003D0F RID: 15631
		LookupTableMaskSeparatorTooLong,
		// Token: 0x04003D10 RID: 15632
		LookupTableMaskLevelMustBeBlankAcrossLCIDs,
		// Token: 0x04003D11 RID: 15633
		LookupTableMaskSeparatorInvalid,
		// Token: 0x04003D12 RID: 15634
		LookupTableMaskBlankSeparatorInvalidAfterAnyLengthSequence,
		// Token: 0x04003D13 RID: 15635
		LookupTableMaskSequenceLengthInvalid,
		// Token: 0x04003D14 RID: 15636
		LookupTableMaskLevelMustBeOneOrMore,
		// Token: 0x04003D15 RID: 15637
		LookupTableItemDoesNotFitMask = 11050,
		// Token: 0x04003D16 RID: 15638
		LookupTableItemContainsSeparator,
		// Token: 0x04003D17 RID: 15639
		LookupTableItemFullValueTooLong,
		// Token: 0x04003D18 RID: 15640
		LookupTableDuplicateSiblingsDisallowed,
		// Token: 0x04003D19 RID: 15641
		LookupTableSortOrderIndexInvalid,
		// Token: 0x04003D1A RID: 15642
		LookupTableSortOrderIndexDuplicate,
		// Token: 0x04003D1B RID: 15643
		LookupTableSortOrderTypeInvalid,
		// Token: 0x04003D1C RID: 15644
		LookupTableSortOrderMustComeAfterParentSortOrder,
		// Token: 0x04003D1D RID: 15645
		LookupTableSortOrderMustComeBeforeParentNextSiblingSortOrder,
		// Token: 0x04003D1E RID: 15646
		LookupTableInvalidCookieLength = 11060,
		// Token: 0x04003D1F RID: 15647
		LookupTableMustHaveValuesForPrimaryLCIDorJustOneValue,
		// Token: 0x04003D20 RID: 15648
		LookupTableLCIDNotSupportedInLookupTableLanguages,
		// Token: 0x04003D21 RID: 15649
		LookupTableInvalidDescriptionLength,
		// Token: 0x04003D22 RID: 15650
		LookupTableCannotChangeBuiltInTables,
		// Token: 0x04003D23 RID: 15651
		LookupTableCannotChangeTypeOnceCreated,
		// Token: 0x04003D24 RID: 15652
		LookupTableCannotDeleteLTWithDependantCustomField,
		// Token: 0x04003D25 RID: 15653
		LookupTableAllLevelsNotFilled,
		// Token: 0x04003D26 RID: 15654
		LookupTableDuplicateName,
		// Token: 0x04003D27 RID: 15655
		LookupTableInvalidName,
		// Token: 0x04003D28 RID: 15656
		LookupTableDuplicateSiblingPhoneticsDisallowed = 11071,
		// Token: 0x04003D29 RID: 15657
		[Obsolete("The LookupTableItemHasTrailingOrLeadingWhitespace error has been deprecated.")]
		LookupTableItemHasTrailingOrLeadingWhitespace,
		// Token: 0x04003D2A RID: 15658
		LookupTableItemInvalidLookupTable,
		// Token: 0x04003D2B RID: 15659
		LookupTableInvalidPhoneticsLength,
		// Token: 0x04003D2C RID: 15660
		LookupTableAlreadyExists = 11076,
		// Token: 0x04003D2D RID: 15661
		LookupTableInvalidUID = 11078,
		// Token: 0x04003D2E RID: 15662
		LookupTableFilterInvalid,
		// Token: 0x04003D2F RID: 15663
		LookupTableLanguageParameterInvalidWithXmlFilter,
		// Token: 0x04003D30 RID: 15664
		LookupTableInvalidParentStructUid,
		// Token: 0x04003D31 RID: 15665
		LookupTableItemContainsListSeparator,
		// Token: 0x04003D32 RID: 15666
		[ULSParameter(0, "reminderUID")]
		NotificationReminderUnknown = 16050,
		// Token: 0x04003D33 RID: 15667
		[ULSParameter(0, "reminderUID")]
		[ULSParameter(1, "parentReminderUID")]
		NotificationReminderParentNotSubscribed,
		// Token: 0x04003D34 RID: 15668
		[ULSParameter(1, "parentReminderUID")]
		[ULSParameter(0, "reminderUID")]
		NotificationReminderParentNotFound,
		// Token: 0x04003D35 RID: 15669
		[ULSParameter(0, "parentReminderUID")]
		[ULSParameter(1, "reminderUID")]
		NotificationReminderChildStillSubscribed,
		// Token: 0x04003D36 RID: 15670
		[ULSParameter(0, "parentReminderUID")]
		[ULSParameter(1, "reminderUID")]
		NotificationReminderChildNotFound,
		// Token: 0x04003D37 RID: 15671
		[ULSParameter(0, "EmailTypeUID")]
		[ULSParameter(1, "Error")]
		NotificationEMailDeliveryFailed = 16080,
		// Token: 0x04003D38 RID: 15672
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "EmailTypeUID")]
		NotificationQueueMessageFailed = 16082,
		// Token: 0x04003D39 RID: 15673
		[ULSParameter(0, "EmailTypeUID")]
		[ULSParameter(1, "Error")]
		NotificationXSLTTransformationError = 16084,
		// Token: 0x04003D3A RID: 15674
		ProjectGlobalNotFound = 100,
		// Token: 0x04003D3B RID: 15675
		ProjectGlobalCannotBeDeleted,
		// Token: 0x04003D3C RID: 15676
		ProjectNotFound = 1000,
		// Token: 0x04003D3D RID: 15677
		[ULSParameter(0, "projGuid")]
		ProjectAlreadyExists,
		// Token: 0x04003D3E RID: 15678
		ProjectCheckedoutToOtherUser,
		// Token: 0x04003D3F RID: 15679
		ProjectTypeInvalidForCreate,
		// Token: 0x04003D40 RID: 15680
		ProjectParametersInvalid,
		// Token: 0x04003D41 RID: 15681
		ProjectNotCheckedoutToUser = 1006,
		// Token: 0x04003D42 RID: 15682
		ProjectCheckedout,
		// Token: 0x04003D43 RID: 15683
		ProjectTypeInvalid,
		// Token: 0x04003D44 RID: 15684
		ProjectIDInvalid,
		// Token: 0x04003D45 RID: 15685
		ProjectNameTooLong = 1014,
		// Token: 0x04003D46 RID: 15686
		ProjectManagerNameTooLong,
		// Token: 0x04003D47 RID: 15687
		[ULSParameter(0, "projName")]
		ProjectNameInvalid,
		// Token: 0x04003D48 RID: 15688
		ProjectStartDateMissing = 1025,
		// Token: 0x04003D49 RID: 15689
		ProjectNameMissing,
		// Token: 0x04003D4A RID: 15690
		ProjectVersionMissing,
		// Token: 0x04003D4B RID: 15691
		[ULSParameter(0, "projGuid")]
		ProjectDoesNotExist,
		// Token: 0x04003D4C RID: 15692
		ProjectMultipleProjectsInvalid,
		// Token: 0x04003D4D RID: 15693
		ProjectHasWriteLock,
		// Token: 0x04003D4E RID: 15694
		ProjectHasPendingWriteLock,
		// Token: 0x04003D4F RID: 15695
		ProjectHasNoReadLock,
		// Token: 0x04003D50 RID: 15696
		ProjectHasReadLock,
		// Token: 0x04003D51 RID: 15697
		[ULSParameter(0, "projName")]
		ProjectNameAlreadyExists,
		// Token: 0x04003D52 RID: 15698
		ProjectOptCriticalSlackLimitInvalid,
		// Token: 0x04003D53 RID: 15699
		ProjectOptCurrencyPositionInvalid,
		// Token: 0x04003D54 RID: 15700
		ProjectOptCurrencyDigitsInvalid,
		// Token: 0x04003D55 RID: 15701
		ProjectOptCurrencySymbolTooLong,
		// Token: 0x04003D56 RID: 15702
		ProjectCannotDelete,
		// Token: 0x04003D57 RID: 15703
		ProjectCannotAdd,
		// Token: 0x04003D58 RID: 15704
		ProjectOptCurrencySymbolInvalid,
		// Token: 0x04003D59 RID: 15705
		ProjectHasNoWriteLock,
		// Token: 0x04003D5A RID: 15706
		ProjectFilterInvalid,
		// Token: 0x04003D5B RID: 15707
		ProjectTooLarge,
		// Token: 0x04003D5C RID: 15708
		ProjectOptCurrencyCodeNot3Chars,
		// Token: 0x04003D5D RID: 15709
		ProjectOptCurrencyCodeInvalid,
		// Token: 0x04003D5E RID: 15710
		ProjectActualsAreProtected,
		// Token: 0x04003D5F RID: 15711
		ProjectTemplateNotFound,
		// Token: 0x04003D60 RID: 15712
		[ULSParameter(0, "currencycode")]
		ProjectCurrencyCodeInvalid,
		// Token: 0x04003D61 RID: 15713
		ProjectCannotEditCostResource,
		// Token: 0x04003D62 RID: 15714
		ProjectIsNotPublished,
		// Token: 0x04003D63 RID: 15715
		[Obsolete("LWP task import no longer supported")]
		ProjectExceededLWPTaskLimit,
		// Token: 0x04003D64 RID: 15716
		ProjectOptFinishDateInvalid,
		// Token: 0x04003D65 RID: 15717
		[ULSParameter(0, "nItems")]
		ProjectExceededItemsLimit,
		// Token: 0x04003D66 RID: 15718
		[ULSParameter(0, "table")]
		[ULSParameter(1, "column")]
		ProjectColumnNotReadOnly,
		// Token: 0x04003D67 RID: 15719
		ProjectInvalidOwner,
		// Token: 0x04003D68 RID: 15720
		ProjectCantEditPctWrkCompForNonWrkRscs,
		// Token: 0x04003D69 RID: 15721
		ProjectCannotEditMaterialResource,
		// Token: 0x04003D6A RID: 15722
		ProjectCannotEditFieldWhenTaskHasNoWorkAssignment,
		// Token: 0x04003D6B RID: 15723
		ProjectSubProjectNotFound = 1070,
		// Token: 0x04003D6C RID: 15724
		ProjectResourceNotFound = 1100,
		// Token: 0x04003D6D RID: 15725
		ProjectResourceAlreadyExists,
		// Token: 0x04003D6E RID: 15726
		ProjectCannotReplaceResourceWithSelf = 1106,
		// Token: 0x04003D6F RID: 15727
		ProjectCannotChangeLockedTrackingMethod,
		// Token: 0x04003D70 RID: 15728
		ProjectInvalidColumnForCompatibilityMode,
		// Token: 0x04003D71 RID: 15729
		[ULSParameter(0, "updateSequenceNumber")]
		ProjectUpdateInvalidUpdateSequenceNumber = 1151,
		// Token: 0x04003D72 RID: 15730
		[ULSParameter(0, "updateSequenceNumber")]
		ProjectUpdateDuplicateUpdateSequenceNumber,
		// Token: 0x04003D73 RID: 15731
		ProjectUpdateNullUpdateSequenceNumber,
		// Token: 0x04003D74 RID: 15732
		ProjectUpdateNullUpdateColumnNames,
		// Token: 0x04003D75 RID: 15733
		ProjectUpdateInvalidProjectUID,
		// Token: 0x04003D76 RID: 15734
		ProjectUpdateInvalidColumnForUpdate,
		// Token: 0x04003D77 RID: 15735
		ProjectUpdateCannotEditColumn,
		// Token: 0x04003D78 RID: 15736
		ProjectUpdateNoChangesToValidateAndSchedule,
		// Token: 0x04003D79 RID: 15737
		LinkNotFound,
		// Token: 0x04003D7A RID: 15738
		ProjectUpdateInvalidColumnValue,
		// Token: 0x04003D7B RID: 15739
		[ULSParameter(0, "itemID")]
		ProjectCannotDeleteItem,
		// Token: 0x04003D7C RID: 15740
		ProjectUpdateCannotComputeOptIndex,
		// Token: 0x04003D7D RID: 15741
		ProjectCannotUpdateDueToVisibilityMode,
		// Token: 0x04003D7E RID: 15742
		[ULSParameter(0, "exception")]
		ProjectNodeConsistencyException = 9132,
		// Token: 0x04003D7F RID: 15743
		[ULSParameter(0, "exception")]
		ProjectSchedulingEngineException,
		// Token: 0x04003D80 RID: 15744
		ProjectFormulaCalculationException,
		// Token: 0x04003D81 RID: 15745
		ProjectUpdateDatabaseException,
		// Token: 0x04003D82 RID: 15746
		[ULSParameter(0, "exception")]
		ProjectDeleteException,
		// Token: 0x04003D83 RID: 15747
		[ULSParameter(0, "exception")]
		ProjectOperationException,
		// Token: 0x04003D84 RID: 15748
		ProjectCannotComunicateWithPCS,
		// Token: 0x04003D85 RID: 15749
		ProjectPCSSessionInvalid,
		// Token: 0x04003D86 RID: 15750
		ProjectPCSWorkerLoadFailed,
		// Token: 0x04003D87 RID: 15751
		[ULSParameter(0, "projectuid")]
		[ULSParameter(3, "stage")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(2, "messageID")]
		ProjectPublishFailure = 23000,
		// Token: 0x04003D88 RID: 15752
		ProjectCurrencyConflict,
		// Token: 0x04003D89 RID: 15753
		ProjectPublishFailed,
		// Token: 0x04003D8A RID: 15754
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(2, "messageID")]
		ProjectReversePublishFailure = 23004,
		// Token: 0x04003D8B RID: 15755
		ProjectReversePublishFailed = 23003,
		// Token: 0x04003D8C RID: 15756
		[ULSParameter(2, "messageID")]
		[ULSParameter(3, "stage")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		ProjectArchiveRetentionDeleteFailure = 23005,
		// Token: 0x04003D8D RID: 15757
		[ULSParameter(3, "stage")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(4, "blocking")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(0, "projectuid")]
		ProjectDeleteFailure,
		// Token: 0x04003D8E RID: 15758
		[ULSParameter(0, "projectuid")]
		ProjectPublishEnqueueFailure,
		// Token: 0x04003D8F RID: 15759
		[ULSParameter(2, "MessageID")]
		[ULSParameter(1, "JobUID")]
		[ULSParameter(0, "ProjectUID")]
		[ULSParameter(3, "Error")]
		ProjectCheckinFailure,
		// Token: 0x04003D90 RID: 15760
		[ULSParameter(0, "ProjectUID")]
		ProjectCheckinFailed,
		// Token: 0x04003D91 RID: 15761
		ProjectCheckoutFailed,
		// Token: 0x04003D92 RID: 15762
		[ULSParameter(0, "projectuid")]
		ProjectPublishSummaryEnqueueFailure,
		// Token: 0x04003D93 RID: 15763
		ProjectPublishSummaryFailed,
		// Token: 0x04003D94 RID: 15764
		ProjectUpdateScheduledProjectFailure = 26026,
		// Token: 0x04003D95 RID: 15765
		[Obsolete("Do not use this value for development with Project Server.")]
		ProjectSyncProjectEnterpriseEntitiesFailure = 26033,
		// Token: 0x04003D96 RID: 15766
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingAttributeCubeSettingsChangedMessageFailed = 24000,
		// Token: 0x04003D97 RID: 15767
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingBaseCalendarChangeMessageFailed,
		// Token: 0x04003D98 RID: 15768
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingCustomFieldMetadataChangeMessageFailed,
		// Token: 0x04003D99 RID: 15769
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingEntityUserViewChangedMessageFailed,
		// Token: 0x04003D9A RID: 15770
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingFiscalPeriodChangeMessageFailed,
		// Token: 0x04003D9B RID: 15771
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingLookupTableChangeMessageFailed,
		// Token: 0x04003D9C RID: 15772
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingProjectChangeMessageFailed,
		// Token: 0x04003D9D RID: 15773
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingResourceCapacityUpdateMessageFailed,
		// Token: 0x04003D9E RID: 15774
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingResourceChangeMessageFailed,
		// Token: 0x04003D9F RID: 15775
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingTimesheetAdjustMessageFailed,
		// Token: 0x04003DA0 RID: 15776
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingTimesheetClassCreateMessageFailed,
		// Token: 0x04003DA1 RID: 15777
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingTimesheetDeleteMessageFailed,
		// Token: 0x04003DA2 RID: 15778
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingTimesheetPeriodDeleteMessageFailed,
		// Token: 0x04003DA3 RID: 15779
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingTimesheetPeriodMessageFailed,
		// Token: 0x04003DA4 RID: 15780
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingTimesheetSaveMessageFailed,
		// Token: 0x04003DA5 RID: 15781
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingTimesheetStatusChangeMessageFailed,
		// Token: 0x04003DA6 RID: 15782
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingWSSSyncMessageFailed,
		// Token: 0x04003DA7 RID: 15783
		[ULSParameter(0, "Error")]
		ReportingGetSPWebFailed,
		// Token: 0x04003DA8 RID: 15784
		[ULSParameter(0, "ProjectUID")]
		[ULSParameter(1, "SPListType")]
		[ULSParameter(2, "Error")]
		ReportingWssSyncListFailed,
		// Token: 0x04003DA9 RID: 15785
		[ULSParameter(0, "Error")]
		ReportingWssTransferLinksFailed,
		// Token: 0x04003DAA RID: 15786
		[ULSParameter(0, "QueueMessageType")]
		[ULSParameter(2, "Error")]
		[ULSParameter(1, "QueueMessageBody")]
		ReportingQueueMessageSubmitFailed,
		// Token: 0x04003DAB RID: 15787
		[ULSParameter(0, "Error")]
		ReportingWssSyncHRefFailed,
		// Token: 0x04003DAC RID: 15788
		[ULSParameter(0, "JobUID")]
		[ULSParameter(1, "MessageID")]
		[ULSParameter(2, "Error")]
		ReportingSyncGlobalDataMessageFailed,
		// Token: 0x04003DAD RID: 15789
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingRDBRefreshMessageFailed,
		// Token: 0x04003DAE RID: 15790
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingAttributeCubeDepartmentsChangedMessageFailed,
		// Token: 0x04003DAF RID: 15791
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingTimesheetProjectAggregationMessageFailed,
		// Token: 0x04003DB0 RID: 15792
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingRdbBulkDataSyncMessageFailed,
		// Token: 0x04003DB1 RID: 15793
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingWorkflowMetadataSyncMessageFailed,
		// Token: 0x04003DB2 RID: 15794
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingProjectWorkflowInformationSyncMessageFailed,
		// Token: 0x04003DB3 RID: 15795
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingEptSyncMessageFailed,
		// Token: 0x04003DB4 RID: 15796
		[ULSParameter(0, "QueueMessageBody")]
		[ULSParameter(1, "Error")]
		ReportingSummaryProjectPublishMessageFailed,
		// Token: 0x04003DB5 RID: 15797
		[ULSParameter(1, "Error")]
		[ULSParameter(0, "QueueMessageBody")]
		ReportingSolutionCommitedDecisionChangedMessageFailed,
		// Token: 0x04003DB6 RID: 15798
		[ULSParameter(0, "ActionName")]
		[ULSParameter(1, "Error")]
		[Obsolete("Not used.  Upgrade sequence was removed.")]
		ReportingDelayedUpgradeFailed,
		// Token: 0x04003DB7 RID: 15799
		[ULSParameter(0, "Error")]
		ReportingCustomPoolTableCapacityExceeded,
		// Token: 0x04003DB8 RID: 15800
		ResourceNotFound = 2000,
		// Token: 0x04003DB9 RID: 15801
		ResourceAlreadyExists,
		// Token: 0x04003DBA RID: 15802
		ResourceCheckedoutToOtherUser,
		// Token: 0x04003DBB RID: 15803
		ResourceUIDInvalid = 2011,
		// Token: 0x04003DBC RID: 15804
		ResourceNameInvalid = 2016,
		// Token: 0x04003DBD RID: 15805
		ResourceNameTooLong,
		// Token: 0x04003DBE RID: 15806
		ResourceInitialsTooLong,
		// Token: 0x04003DBF RID: 15807
		ResourceCheckedout = 2025,
		// Token: 0x04003DC0 RID: 15808
		ResourceNTAccountInvalid,
		// Token: 0x04003DC1 RID: 15809
		[ULSParameter(0, "resName")]
		[Obsolete("Not used.  Duplicate names are now allowed.")]
		ResourceNameAlreadyInUse,
		// Token: 0x04003DC2 RID: 15810
		[ULSParameter(0, "wresAccount")]
		ResourceNTAccountAlreadyInUse,
		// Token: 0x04003DC3 RID: 15811
		ResourceAdGuidAlreadyInUse,
		// Token: 0x04003DC4 RID: 15812
		[ULSParameter(0, "resName")]
		ResourceHasActuals = 2031,
		// Token: 0x04003DC5 RID: 15813
		ResourceNTAccountTooLong = 2035,
		// Token: 0x04003DC6 RID: 15814
		ResourceEMailAddressTooLong,
		// Token: 0x04003DC7 RID: 15815
		ResourceCodeTooLong,
		// Token: 0x04003DC8 RID: 15816
		ResourceGroupTooLong,
		// Token: 0x04003DC9 RID: 15817
		ResourceWorkGroupInvalid,
		// Token: 0x04003DCA RID: 15818
		ResourceTypeInvalid,
		// Token: 0x04003DCB RID: 15819
		[ULSParameter(0, "resName")]
		ResourceNonWorkResourceWithEMailInvalid = 2044,
		// Token: 0x04003DCC RID: 15820
		rsResourceNameHasTrailingOrLeadingWhitespace = 2046,
		// Token: 0x04003DCD RID: 15821
		ResourceCannotDeleteCallingUserAccount,
		// Token: 0x04003DCE RID: 15822
		ResourceInitialsInvalid,
		// Token: 0x04003DCF RID: 15823
		ResourceAccrueAtInvalid,
		// Token: 0x04003DD0 RID: 15824
		ResourceNonMaterialResourceCannotHaveMaterialLabel,
		// Token: 0x04003DD1 RID: 15825
		ResourceMaterialResourceCannotHaveCertainFields,
		// Token: 0x04003DD2 RID: 15826
		ResourceAvailFromAvailToOverlap,
		// Token: 0x04003DD3 RID: 15827
		[ULSParameter(0, "resName")]
		ResourceInvalidEmailLanguage,
		// Token: 0x04003DD4 RID: 15828
		ResourceBookingTypeInvalid = 2055,
		// Token: 0x04003DD5 RID: 15829
		ResourceCannotReplaceMaterialResourceWithNonMaterialResource,
		// Token: 0x04003DD6 RID: 15830
		ResourceCannotUpdateEnterpriseResource,
		// Token: 0x04003DD7 RID: 15831
		[ULSParameter(0, "locResName")]
		[ULSParameter(1, "entResName")]
		rsResourceCannotAddLocalWithSameNameAsEnterprise,
		// Token: 0x04003DD8 RID: 15832
		ResourceCannotSetRateOnCostResource,
		// Token: 0x04003DD9 RID: 15833
		ResourceCannotSetRateOnMaterialResource,
		// Token: 0x04003DDA RID: 15834
		ResourceCannotSetCanLevelOnNonWorkResource,
		// Token: 0x04003DDB RID: 15835
		ResourceCannotDeleteThisUser,
		// Token: 0x04003DDC RID: 15836
		ResourceCannotDeactivateSelf,
		// Token: 0x04003DDD RID: 15837
		ResourceAvailabilityDateRangesOverlap,
		// Token: 0x04003DDE RID: 15838
		ResourceAvailabilityOutsideTheHireAndTerminationDateRange,
		// Token: 0x04003DDF RID: 15839
		ResourceFilterInvalid,
		// Token: 0x04003DE0 RID: 15840
		ResourceSegmentWithThisEffectiveDateDoesNotExist,
		// Token: 0x04003DE1 RID: 15841
		ResourceSegmentWithThisEffectiveDateAlready,
		// Token: 0x04003DE2 RID: 15842
		[ULSParameter(2, "type")]
		[ULSParameter(0, "resName")]
		[ULSParameter(1, "resUid")]
		[ULSParameter(3, "itemUID")]
		ResourceUserHasItemCheckedOutToItStill,
		// Token: 0x04003DE3 RID: 15843
		ResourceInvalidHireDate,
		// Token: 0x04003DE4 RID: 15844
		ResourceInvalidTerminationDate,
		// Token: 0x04003DE5 RID: 15845
		ResourceCannotChangeExistingResourceType,
		// Token: 0x04003DE6 RID: 15846
		ResourceCannotSetTimesheetManagerOnSpecifiedResource,
		// Token: 0x04003DE7 RID: 15847
		ResourceInvalidTimesheetManager,
		// Token: 0x04003DE8 RID: 15848
		ResourceInvalidAssignmentOwner,
		// Token: 0x04003DE9 RID: 15849
		ResourceCannotCreateCostResource,
		// Token: 0x04003DEA RID: 15850
		ResourceInvalidRbsValue,
		// Token: 0x04003DEB RID: 15851
		ResourceCannotSetAssignmentOwnerOnSpecifiedResource,
		// Token: 0x04003DEC RID: 15852
		ResourceFieldsInvalidForBudget,
		// Token: 0x04003DED RID: 15853
		ResourceHyperlinkInvalid,
		// Token: 0x04003DEE RID: 15854
		ResourceAuthorizationValidOnlyOnWorkResources,
		// Token: 0x04003DEF RID: 15855
		[ULSParameter(3, "itemName")]
		[ULSParameter(0, "resName")]
		[ULSParameter(1, "resUid")]
		[ULSParameter(2, "itemUID")]
		ResourceIsProjectOwner,
		// Token: 0x04003DF0 RID: 15856
		[ULSParameter(3, "itemName")]
		[ULSParameter(1, "resUid")]
		[ULSParameter(2, "itemUID")]
		[ULSParameter(0, "resName")]
		ResourceIsTimesheetManager,
		// Token: 0x04003DF1 RID: 15857
		[ULSParameter(2, "itemUID")]
		[ULSParameter(0, "resName")]
		[ULSParameter(1, "resUid")]
		[ULSParameter(3, "itemName")]
		ResourceIsDefaultAssignmentOwner,
		// Token: 0x04003DF2 RID: 15858
		[ULSParameter(3, "itemName")]
		[ULSParameter(0, "resName")]
		[ULSParameter(1, "resUid")]
		[ULSParameter(2, "itemUID")]
		ResourceIsAssignmentOwner,
		// Token: 0x04003DF3 RID: 15859
		[ULSParameter(3, "itemName")]
		[ULSParameter(2, "itemUID")]
		[ULSParameter(0, "resName")]
		[ULSParameter(1, "resUid")]
		ResourceIsUsedInResourcePlan,
		// Token: 0x04003DF4 RID: 15860
		[ULSParameter(0, "resName")]
		[ULSParameter(2, "itemUID")]
		[ULSParameter(1, "resUid")]
		[ULSParameter(3, "itemName")]
		ResourceCannotDeleteEnterpriseResource,
		// Token: 0x04003DF5 RID: 15861
		ResourceSetResourceAuthorizationFailed,
		// Token: 0x04003DF6 RID: 15862
		ResourceTooManyResourcesSpecifiedToDelete,
		// Token: 0x04003DF7 RID: 15863
		[ULSParameter(0, "resReturned")]
		ResourceTooManyResourcesReturned,
		// Token: 0x04003DF8 RID: 15864
		[ULSParameter(0, "workflowProxyUserUid")]
		ResourceCannotDeleteWorkflowProxyUser,
		// Token: 0x04003DF9 RID: 15865
		[ULSParameter(0, "resUid")]
		ResourceInvalidEmailWithExchangeSync,
		// Token: 0x04003DFA RID: 15866
		[ULSParameter(0, "resUid")]
		ResourceInvalidResourceTypeWithExchangeSync,
		// Token: 0x04003DFB RID: 15867
		[ULSParameter(0, "resUid")]
		ResourceInvalidPrincipalNameWithExchangeSync,
		// Token: 0x04003DFC RID: 15868
		[ULSParameter(0, "resUid")]
		ResourceInvalidAuthenticationTypeWithExchangeSync,
		// Token: 0x04003DFD RID: 15869
		[ULSParameter(0, "resUid")]
		ResourceExchangeSyncFlagAndPrincipalNameMismatch,
		// Token: 0x04003DFE RID: 15870
		[ULSParameter(0, "resName")]
		ResourceUnsupportedUserUpdateInSharePointSecurityMode,
		// Token: 0x04003DFF RID: 15871
		[ULSParameter(0, "resName")]
		ResourceUserResourceCannotBeGenericResource,
		// Token: 0x04003E00 RID: 15872
		[ULSParameter(0, "resName")]
		ResourceIsInTimesheetManagersList,
		// Token: 0x04003E01 RID: 15873
		RestoreProjectFailed = 25003,
		// Token: 0x04003E02 RID: 15874
		RestoreCustomFieldsFailed = 25009,
		// Token: 0x04003E03 RID: 15875
		RestoreSystemSettingsFailed = 25011,
		// Token: 0x04003E04 RID: 15876
		RestoreCategoriesFailed = 25013,
		// Token: 0x04003E05 RID: 15877
		RestoreViewsFailed = 25015,
		// Token: 0x04003E06 RID: 15878
		RestoreGlobalProjectFailed = 25017,
		// Token: 0x04003E07 RID: 15879
		RestoreResourcesFailed = 29021,
		// Token: 0x04003E08 RID: 15880
		RulesNameTooLong = 21001,
		// Token: 0x04003E09 RID: 15881
		RulesDescriptionTooLong,
		// Token: 0x04003E0A RID: 15882
		RulesInvalidRuleType,
		// Token: 0x04003E0B RID: 15883
		RulesInvalidConditionType,
		// Token: 0x04003E0C RID: 15884
		RulesInvalidOperatorType,
		// Token: 0x04003E0D RID: 15885
		RulesInvalidListItemType = 21007,
		// Token: 0x04003E0E RID: 15886
		RulesNameInvalidCharacters,
		// Token: 0x04003E0F RID: 15887
		RulesDescriptionInvalidCharacters,
		// Token: 0x04003E10 RID: 15888
		RulesInvalidValueType,
		// Token: 0x04003E11 RID: 15889
		SecurityObjectNotFound = 19037,
		// Token: 0x04003E12 RID: 15890
		SecurityGroupCouldNotBeCreated = 19001,
		// Token: 0x04003E13 RID: 15891
		SecurityFieldAccessIDInvalid = 19003,
		// Token: 0x04003E14 RID: 15892
		[ULSParameter(0, "catUid")]
		SecurityCannotUpdateFacForNonExistentCategory,
		// Token: 0x04003E15 RID: 15893
		SecurityDuplicateCategoryUid,
		// Token: 0x04003E16 RID: 15894
		SecurityDuplicateGroupUid,
		// Token: 0x04003E17 RID: 15895
		SecurityDuplicateTemplateUid,
		// Token: 0x04003E18 RID: 15896
		SecurityDuplicateUid = 19036,
		// Token: 0x04003E19 RID: 15897
		SecurityInvalidTemplateUidRef = 19008,
		// Token: 0x04003E1A RID: 15898
		SecurityInvalidCategoryUidRef = 19080,
		// Token: 0x04003E1B RID: 15899
		SecurityInvalidProjectUidRef,
		// Token: 0x04003E1C RID: 15900
		SecurityInvalidGroupUidRef,
		// Token: 0x04003E1D RID: 15901
		SecurityInvalidUserUidRef,
		// Token: 0x04003E1E RID: 15902
		SecurityInvalidCategoryPermissionUidRef,
		// Token: 0x04003E1F RID: 15903
		SecurityInvalidGlobalPermissionUidRef,
		// Token: 0x04003E20 RID: 15904
		SecurityInvalidResourceUidRef,
		// Token: 0x04003E21 RID: 15905
		SecurityInvalidGlobalPermission = 19009,
		// Token: 0x04003E22 RID: 15906
		SecurityInvalidCategoryPermission,
		// Token: 0x04003E23 RID: 15907
		SecurityInvalidObjectType = 19035,
		// Token: 0x04003E24 RID: 15908
		SecurityUpdatedGroupNotFound = 19013,
		// Token: 0x04003E25 RID: 15909
		SecurityUpdatedCategoryNotFound,
		// Token: 0x04003E26 RID: 15910
		SecurityUpdatedTemplateNotFound,
		// Token: 0x04003E27 RID: 15911
		SecurityTemplateNotFound = 19034,
		// Token: 0x04003E28 RID: 15912
		SecurityGroupMemberNotFound = 19016,
		// Token: 0x04003E29 RID: 15913
		SecurityDeleteNotSupportedBySetMethod = 19087,
		// Token: 0x04003E2A RID: 15914
		SecurityUserNotFound = 19018,
		// Token: 0x04003E2B RID: 15915
		SecurityNoCategoryRelationForPermission,
		// Token: 0x04003E2C RID: 15916
		SecurityCannotDeleteDefaultGroup,
		// Token: 0x04003E2D RID: 15917
		SecurityCannotDeleteDefaultCategory,
		// Token: 0x04003E2E RID: 15918
		SecurityCategoryCouldNotBeCreated,
		// Token: 0x04003E2F RID: 15919
		SecurityNoCategoryForPermission,
		// Token: 0x04003E30 RID: 15920
		SecurityNoCategoryForObject,
		// Token: 0x04003E31 RID: 15921
		SecurityNoCategoryForRule,
		// Token: 0x04003E32 RID: 15922
		SecurityNoGroupForPermission,
		// Token: 0x04003E33 RID: 15923
		SecurityCannotSetPermissionForFieldGroup,
		// Token: 0x04003E34 RID: 15924
		SecurityInvalidFieldGroup,
		// Token: 0x04003E35 RID: 15925
		SecurityCannotSetOrgPermission,
		// Token: 0x04003E36 RID: 15926
		SecurityInvalidOrgPermission,
		// Token: 0x04003E37 RID: 15927
		SecurityInvalidSecurityRule,
		// Token: 0x04003E38 RID: 15928
		SecurityInvalidProjectCategoryPermissionUidRef = 19088,
		// Token: 0x04003E39 RID: 15929
		SecurityCannotModifyCoreProjectCategoryDataInUpdate,
		// Token: 0x04003E3A RID: 15930
		SecurityProjectCategoryEntitiesDoNotAllowInPlaceChanges,
		// Token: 0x04003E3B RID: 15931
		SecurityCategoryCannotAddRelationForDeletedCategory,
		// Token: 0x04003E3C RID: 15932
		SecurityCategoryCannotAddPermissionForDeletedCategory,
		// Token: 0x04003E3D RID: 15933
		SecurityCategoryCannotAddPermissionForDeletedRelation,
		// Token: 0x04003E3E RID: 15934
		SecurityCategoryCannotDeleteRelationForNewlyAddedCategory,
		// Token: 0x04003E3F RID: 15935
		SecurityCategoryCannotDeletePermissionForNewlyAddedCategory,
		// Token: 0x04003E40 RID: 15936
		SecurityCategoryCannotDeletePermissionForNewlyAddedRelation,
		// Token: 0x04003E41 RID: 15937
		SecurityCategoryCannotHaveDuplicateUserOrGroupUidsForRelation,
		// Token: 0x04003E42 RID: 15938
		SecurityCategoryPermissionMustHaveMatchingRelation,
		// Token: 0x04003E43 RID: 15939
		SecurityCategoryProjectAlreadyHasSecurityProjectCategory,
		// Token: 0x04003E44 RID: 15940
		ServerEventInvalidEventId = 19033,
		// Token: 0x04003E45 RID: 15941
		ServerEventServiceNotFound = 22003,
		// Token: 0x04003E46 RID: 15942
		ServerEventRemoteCouldNotReachProxy = 22005,
		// Token: 0x04003E47 RID: 15943
		ServerEventManagerCouldNotReachRemote,
		// Token: 0x04003E48 RID: 15944
		ServerEventHandlerNotSigned,
		// Token: 0x04003E49 RID: 15945
		ServerEventHandlerMalformedAssemblyName,
		// Token: 0x04003E4A RID: 15946
		ServerEventHandlerOrderInvalid,
		// Token: 0x04003E4B RID: 15947
		ServerEventHandlerDuplicateEntry,
		// Token: 0x04003E4C RID: 15948
		ServerEventHandlerNotFound,
		// Token: 0x04003E4D RID: 15949
		ServerEventHandlerDuplicateName,
		// Token: 0x04003E4E RID: 15950
		ServerEventHandlerNullAssemblyNameAndEndpointUrl,
		// Token: 0x04003E4F RID: 15951
		StatusingInvalidEntity = 3102,
		// Token: 0x04003E50 RID: 15952
		StatusingGetDataForTaskFailed,
		// Token: 0x04003E51 RID: 15953
		StatusingGetTaskOrAssnCntrFailed,
		// Token: 0x04003E52 RID: 15954
		StatusingInvalidPIDForProjCntr,
		// Token: 0x04003E53 RID: 15955
		StatusingDeleteAssnFailed,
		// Token: 0x04003E54 RID: 15956
		StatusingAssnSaveFailed,
		// Token: 0x04003E55 RID: 15957
		StatusingTaskSaveFailed,
		// Token: 0x04003E56 RID: 15958
		StatusingInvalidPID,
		// Token: 0x04003E57 RID: 15959
		StatusingSetDataValueInvalid = 3111,
		// Token: 0x04003E58 RID: 15960
		StatusingSetDataFailed,
		// Token: 0x04003E59 RID: 15961
		StatusingInvalidDelegationStart,
		// Token: 0x04003E5A RID: 15962
		StatusingApprovalUpdateFailed,
		// Token: 0x04003E5B RID: 15963
		StatusingInvalidApprovalType,
		// Token: 0x04003E5C RID: 15964
		StatusingInternalError,
		// Token: 0x04003E5D RID: 15965
		StatusingInvalidUpdateData,
		// Token: 0x04003E5E RID: 15966
		StatusingProjectUpdateFailed,
		// Token: 0x04003E5F RID: 15967
		StatusingInvalidPreviewData,
		// Token: 0x04003E60 RID: 15968
		StatusingInvalidTransaction,
		// Token: 0x04003E61 RID: 15969
		StatusingTooManyResults,
		// Token: 0x04003E62 RID: 15970
		StatusingInvalidInterval,
		// Token: 0x04003E63 RID: 15971
		StatusingApplyUpdatesFailed,
		// Token: 0x04003E64 RID: 15972
		[ULSParameter(2, "MessageID")]
		[ULSParameter(3, "Error")]
		[ULSParameter(1, "JobUID")]
		[ULSParameter(0, "ProjectUID")]
		StatusingApplyUpdatesFailure,
		// Token: 0x04003E65 RID: 15973
		StatusingInvalidWorkData,
		// Token: 0x04003E66 RID: 15974
		StatusingMissingNameAttribute,
		// Token: 0x04003E67 RID: 15975
		StatusingInvalidNameAttribute,
		// Token: 0x04003E68 RID: 15976
		StatusingInvalidData,
		// Token: 0x04003E69 RID: 15977
		[ULSParameter(1, "LineNumber")]
		[ULSParameter(0, "Message")]
		[ULSParameter(2, "LinePosition")]
		StatusingInvalidChangelist = 3130,
		// Token: 0x04003E6A RID: 15978
		[ULSParameter(0, "AssignmentUID")]
		StatusingInsufficientAssignmentRights,
		// Token: 0x04003E6B RID: 15979
		StatusingInvalidChangeNumber,
		// Token: 0x04003E6C RID: 15980
		StatusingPidNotEditable,
		// Token: 0x04003E6D RID: 15981
		StatusingCannotSetTimephasedDataInManualTasks,
		// Token: 0x04003E6E RID: 15982
		StatusingCannotChangeTaskMode,
		// Token: 0x04003E6F RID: 15983
		StatusReportsUnknownError = 12100,
		// Token: 0x04003E70 RID: 15984
		StatusReportsPeriodUnmatched,
		// Token: 0x04003E71 RID: 15985
		StatusReportsPeriodUnavailable,
		// Token: 0x04003E72 RID: 15986
		StatusReportsInvalidFormInput,
		// Token: 0x04003E73 RID: 15987
		SRAInvalidVersion = 27300,
		// Token: 0x04003E74 RID: 15988
		[Obsolete("Not used.  Upgrade sequence was removed.")]
		SRADelayedUpgradeFailed,
		// Token: 0x04003E75 RID: 15989
		TaskIDInvalid = 7001,
		// Token: 0x04003E76 RID: 15990
		TaskNameTooLong = 7003,
		// Token: 0x04003E77 RID: 15991
		TaskTypeInvalid = 7005,
		// Token: 0x04003E78 RID: 15992
		TaskPriorityInvalid,
		// Token: 0x04003E79 RID: 15993
		TaskConstraintTypeInvalid,
		// Token: 0x04003E7A RID: 15994
		TaskNameInvalid,
		// Token: 0x04003E7B RID: 15995
		TaskConstraintTypeRequiresConstraint = 7010,
		// Token: 0x04003E7C RID: 15996
		TaskConstraintTypeCannotHaveConstraintDate,
		// Token: 0x04003E7D RID: 15997
		TaskSummaryTaskCannotBeMilestone = 7013,
		// Token: 0x04003E7E RID: 15998
		TaskFixedCostAccrualInvalid,
		// Token: 0x04003E7F RID: 15999
		TaskPercentCompleteInvalid,
		// Token: 0x04003E80 RID: 16000
		TaskWorkPercentCompleteInvalid,
		// Token: 0x04003E81 RID: 16001
		TaskPhysicalPercentCompleteInvalid,
		// Token: 0x04003E82 RID: 16002
		TaskLinkTypeInvalid,
		// Token: 0x04003E83 RID: 16003
		TaskAlreadyExists,
		// Token: 0x04003E84 RID: 16004
		TaskLinkAlreadyExists,
		// Token: 0x04003E85 RID: 16005
		[ULSParameter(0, "taskGuid")]
		TaskNotFound,
		// Token: 0x04003E86 RID: 16006
		TaskLinkNotFound,
		// Token: 0x04003E87 RID: 16007
		TaskLinkLagInvalid,
		// Token: 0x04003E88 RID: 16008
		TaskUnableToInsert = 7025,
		// Token: 0x04003E89 RID: 16009
		TaskAddPositionInvalid,
		// Token: 0x04003E8A RID: 16010
		TaskOutlineLevelInvalid,
		// Token: 0x04003E8B RID: 16011
		TaskDurationFormatInvalid,
		// Token: 0x04003E8C RID: 16012
		TaskCannotAddWhereSpecified,
		// Token: 0x04003E8D RID: 16013
		TaskEarnedValueMethodInvalid,
		// Token: 0x04003E8E RID: 16014
		TaskCannotModifyProjectSummary,
		// Token: 0x04003E8F RID: 16015
		TaskCannotDeleteProjectSummary,
		// Token: 0x04003E90 RID: 16016
		TaskCannotSetActualCost,
		// Token: 0x04003E91 RID: 16017
		TaskLevelingDelayInvalid,
		// Token: 0x04003E92 RID: 16018
		TaskCannotEditSummary,
		// Token: 0x04003E93 RID: 16019
		TaskCannotCreateSubTasksUnderTasksWithAssignments,
		// Token: 0x04003E94 RID: 16020
		TaskCannotDeleteSubProject,
		// Token: 0x04003E95 RID: 16021
		TaskCannotEditExternal,
		// Token: 0x04003E96 RID: 16022
		TaskCannotDeleteExternal,
		// Token: 0x04003E97 RID: 16023
		TaskLinkCannotDeleteExternal,
		// Token: 0x04003E98 RID: 16024
		TaskCannotModifyNullTask,
		// Token: 0x04003E99 RID: 16025
		TaskCannotModifyLeafTaskWithNoAssignment,
		// Token: 0x04003E9A RID: 16026
		TaskCannotModifyExternalTask,
		// Token: 0x04003E9B RID: 16027
		TaskStatusManagerInvalid,
		// Token: 0x04003E9C RID: 16028
		TaskLinkCyclicDependency,
		// Token: 0x04003E9D RID: 16029
		TaskCannotCreateOrModifySubTasksUnderTasksWithAssignments,
		// Token: 0x04003E9E RID: 16030
		TaskLinkCannotEditExternal,
		// Token: 0x04003E9F RID: 16031
		[ULSParameter(1, "tsMaxHourPerDay")]
		[ULSParameter(2, "tsSumHourPerDay")]
		[ULSParameter(0, "tsColumn")]
		TimesheetMaxHourPerDayExceeded = 3201,
		// Token: 0x04003EA0 RID: 16032
		[ULSParameter(1, "tsMinHourPerTS")]
		[ULSParameter(2, "tsMaxHourPerTS")]
		[ULSParameter(0, "tsSumHourPerTS")]
		TimesheetHoursPerTSLimitExceeded,
		// Token: 0x04003EA1 RID: 16033
		TimesheetUnverifiedTSLineNotAllowed,
		// Token: 0x04003EA2 RID: 16034
		[ULSParameter(0, "mode")]
		TimesheetIncorrectMode,
		// Token: 0x04003EA3 RID: 16035
		TimesheetInvalidApprover,
		// Token: 0x04003EA4 RID: 16036
		[PSObsolete]
		TimesheetFutureReportingNotAllowed,
		// Token: 0x04003EA5 RID: 16037
		[ULSParameter(0, "periodName")]
		TimesheetIncorrectPeriod = 3208,
		// Token: 0x04003EA6 RID: 16038
		TimesheetPeriodClosed,
		// Token: 0x04003EA7 RID: 16039
		TimesheetPendingLines,
		// Token: 0x04003EA8 RID: 16040
		TimesheetInvalidDateRange,
		// Token: 0x04003EA9 RID: 16041
		[ULSParameter(0, "lineClassUid")]
		TimesheetLineClassDisabled,
		// Token: 0x04003EAA RID: 16042
		[ULSParameter(0, "columnName")]
		[ULSParameter(1, "value")]
		TimesheetLineHasNonExistentItem,
		// Token: 0x04003EAB RID: 16043
		TimesheetLineInvalidStatus,
		// Token: 0x04003EAC RID: 16044
		[ULSParameter(2, "projectWorkspaceName")]
		[ULSParameter(0, "projectUID")]
		[ULSParameter(1, "workspaceUrl")]
		WSSCreateSiteFailure = 16400,
		// Token: 0x04003EAD RID: 16045
		WSSCannotCreateWebWithBlankName,
		// Token: 0x04003EAE RID: 16046
		WSSWebAlreadyExists,
		// Token: 0x04003EAF RID: 16047
		[ULSParameter(0, "projectUID")]
		WSSInvalidProjectUID,
		// Token: 0x04003EB0 RID: 16048
		[ULSParameter(0, "projectUID")]
		WSSProjectAlreadyHasSpWeb,
		// Token: 0x04003EB1 RID: 16049
		[ULSParameter(0, "projectUID")]
		[ULSParameter(1, "wssFullUrl")]
		WSSWebDoesNotExist,
		// Token: 0x04003EB2 RID: 16050
		[ULSParameter(0, "projectUid")]
		[ULSParameter(1, "wssFullUrl")]
		WSSSpWebAlreadyLinkedToProject,
		// Token: 0x04003EB3 RID: 16051
		WSSWebHierarchyDoesNotExist,
		// Token: 0x04003EB4 RID: 16052
		[ULSParameter(0, "wssFullUrl")]
		WSSSPWebHasChildren,
		// Token: 0x04003EB5 RID: 16053
		WSSURIInvalidFormat,
		// Token: 0x04003EB6 RID: 16054
		[ULSParameter(0, "projectUID")]
		WSSSyncReportingDataFailed,
		// Token: 0x04003EB7 RID: 16055
		[ULSParameter(0, "inUrl")]
		WSSWorkspaceUrlPathTooLong,
		// Token: 0x04003EB8 RID: 16056
		[ULSParameter(0, "inUrl")]
		WSSWorkspaceNameContainsIllegalChars,
		// Token: 0x04003EB9 RID: 16057
		[ULSParameter(1, "workspaceUrl")]
		[ULSParameter(0, "wssServerUID")]
		WSSInvalidWssServerUid,
		// Token: 0x04003EBA RID: 16058
		[ULSParameter(1, "workspaceUrl")]
		[ULSParameter(0, "projectUID")]
		WSSSyncUsersFailed,
		// Token: 0x04003EBB RID: 16059
		[ULSParameter(0, "wssFullUrl")]
		[ULSParameter(1, "webTemplateLCID")]
		WSSWrongWebTemplateLCID,
		// Token: 0x04003EBC RID: 16060
		[ULSParameter(0, "wssFullUrl")]
		[ULSParameter(1, "webTemplateName")]
		WSSWrongWebTemplate,
		// Token: 0x04003EBD RID: 16061
		[ULSParameter(0, "projectUid")]
		[ULSParameter(1, "wssFullUrl")]
		WSSWebIsNotProjectWorkspace,
		// Token: 0x04003EBE RID: 16062
		WSSWebCannotStartOrEndOnPeriod,
		// Token: 0x04003EBF RID: 16063
		[ULSParameter(0, "projectUID")]
		[ULSParameter(1, "wssFullUrl")]
		WSSCannotDeleteSiteCollection,
		// Token: 0x04003EC0 RID: 16064
		[ULSParameter(0, "wssListUID")]
		WSSListUidInvalid,
		// Token: 0x04003EC1 RID: 16065
		WSSSyncDataSetListUidMismatch,
		// Token: 0x04003EC2 RID: 16066
		WSSSyncDataSetMissingProjectSettingsRow,
		// Token: 0x04003EC3 RID: 16067
		WSSSyncDataSetTaskMappingsNotAllowed,
		// Token: 0x04003EC4 RID: 16068
		WSSSyncDataSetWssListUidEmpty,
		// Token: 0x04003EC5 RID: 16069
		WSSSyncDataNotFound,
		// Token: 0x04003EC6 RID: 16070
		WSSSyncCriticalDataValidationError,
		// Token: 0x04003EC7 RID: 16071
		WSSSyncSharePointListNotAccessibleError,
		// Token: 0x04003EC8 RID: 16072
		WSSSyncInvalidEntityUids,
		// Token: 0x04003EC9 RID: 16073
		WSSSyncInvalidSyncData,
		// Token: 0x04003ECA RID: 16074
		[ULSParameter(1, "wssListItemID")]
		[ULSParameter(0, "wssListUID")]
		WSSSyncSPSummaryTaskAssignedToResourceError,
		// Token: 0x04003ECB RID: 16075
		WSSSyncInsufficientPermissionsToCreateWinUser,
		// Token: 0x04003ECC RID: 16076
		[ULSParameter(0, "CustomFieldName")]
		[ULSParameter(1, "CustomFieldUid")]
		WSSSyncNoDefaultValueForCustomField,
		// Token: 0x04003ECD RID: 16077
		WSSOLPCreateLinkFailure = 18000,
		// Token: 0x04003ECE RID: 16078
		WSSOLPDeleteWebObjectLinkError,
		// Token: 0x04003ECF RID: 16079
		[ULSParameter(0, "wssListUID")]
		WSSInvalidPermissionsToWssList,
		// Token: 0x04003ED0 RID: 16080
		[ULSParameter(0, "projectUID")]
		[ULSParameter(1, "wssFullUrl")]
		WSSWebIsNotUnderDefaultCollection,
		// Token: 0x04003ED1 RID: 16081
		[ULSParameter(0, "url")]
		WSSWorkspaceUrlIsNotUnderPrimaryCollection,
		// Token: 0x04003ED2 RID: 16082
		WSSWorkspacesMustBeRestrictedToDefaultCollection,
		// Token: 0x04003ED3 RID: 16083
		AdSyncUpdateTimerJobFailed = 27002,
		// Token: 0x04003ED4 RID: 16084
		AdSyncDeleteTimerJobFailed,
		// Token: 0x04003ED5 RID: 16085
		[ULSParameter(1, "_SSPAccountName")]
		[ULSParameter(0, "_ADException")]
		AdSyncAdConnectFail = 27006,
		// Token: 0x04003ED6 RID: 16086
		AdMaximumGroupsCountExceeded,
		// Token: 0x04003ED7 RID: 16087
		ResourcePlanProjectPublishIncomplete = 30000,
		// Token: 0x04003ED8 RID: 16088
		ResourcePlanInvalidResourceType,
		// Token: 0x04003ED9 RID: 16089
		ResourcePlanInactiveResourcesDisallowed,
		// Token: 0x04003EDA RID: 16090
		ResourcePlanFilterInvalid,
		// Token: 0x04003EDB RID: 16091
		[ULSParameter(1, "messagetype")]
		[ULSParameter(3, "blocking")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(2, "messageID")]
		ResourcePlanSaveFailure,
		// Token: 0x04003EDC RID: 16092
		[ULSParameter(0, "projectuid")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(3, "blocking")]
		[ULSParameter(1, "messagetype")]
		ResourcePlanCheckinFailure,
		// Token: 0x04003EDD RID: 16093
		[ULSParameter(3, "blocking")]
		[ULSParameter(2, "messageID")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(1, "messagetype")]
		ResourcePlanDeleteFailure,
		// Token: 0x04003EDE RID: 16094
		ResourcePlanInvalidUtilizationType,
		// Token: 0x04003EDF RID: 16095
		ResourcePlanInvalidTimescale,
		// Token: 0x04003EE0 RID: 16096
		ResourcePlanMismatchedJobList,
		// Token: 0x04003EE1 RID: 16097
		ResourcePlanAlreadyExists,
		// Token: 0x04003EE2 RID: 16098
		ResourcePlanInvalidProjectUID,
		// Token: 0x04003EE3 RID: 16099
		[ULSParameter(0, "resUid")]
		ResourcePlanResourceAlreadyExists,
		// Token: 0x04003EE4 RID: 16100
		[ULSParameter(2, "messageID")]
		[ULSParameter(1, "messagetype")]
		[ULSParameter(0, "projectuid")]
		[ULSParameter(3, "blocking")]
		ResourcePlanMigrateFailure,
		// Token: 0x04003EE5 RID: 16101
		ResourcePlanFeatureNotSupported,
		// Token: 0x04003EE6 RID: 16102
		[ULSParameter(0, "AnalysisUid")]
		[ULSParameter(3, "Blocking")]
		[ULSParameter(1, "MessageType")]
		[ULSParameter(2, "MessageID")]
		PlannerSolutionMessageDeleteFailed = 28000,
		// Token: 0x04003EE7 RID: 16103
		[ULSParameter(3, "Blocking")]
		[ULSParameter(2, "MessageID")]
		[ULSParameter(1, "MessageType")]
		[ULSParameter(0, "AnalysisUid")]
		PlannerSolutionMessageCreateFailed,
		// Token: 0x04003EE8 RID: 16104
		PlannerInvalidRBSValueUid,
		// Token: 0x04003EE9 RID: 16105
		PlannerInvalidCustomFieldUid,
		// Token: 0x04003EEA RID: 16106
		PlannerHorizonInvalid,
		// Token: 0x04003EEB RID: 16107
		PlannerHorizonTooBig,
		// Token: 0x04003EEC RID: 16108
		PlannerInvalidBookingType,
		// Token: 0x04003EED RID: 16109
		PlannerInvalidTimeScale,
		// Token: 0x04003EEE RID: 16110
		PlannerInvalidProjectSNET,
		// Token: 0x04003EEF RID: 16111
		PlannerInvalidProjectFNLT,
		// Token: 0x04003EF0 RID: 16112
		PlannerInvalidAnalysisStartDate,
		// Token: 0x04003EF1 RID: 16113
		PlannerInvalidAnalysisDuration,
		// Token: 0x04003EF2 RID: 16114
		PlannerInvalidHorizonStartDate,
		// Token: 0x04003EF3 RID: 16115
		PlannerInvalidHorizonEndDate,
		// Token: 0x04003EF4 RID: 16116
		PlannerInvalidHorizonTimeScale,
		// Token: 0x04003EF5 RID: 16117
		PlannerInvalidAnalysisType,
		// Token: 0x04003EF6 RID: 16118
		PlannerHorizonStartDateDoesNotMatchTimeScale,
		// Token: 0x04003EF7 RID: 16119
		PlannerHorizonEndDateDoesNotMatchTimeScale,
		// Token: 0x04003EF8 RID: 16120
		PlannerAnalysisNoCapacityData = 28037,
		// Token: 0x04003EF9 RID: 16121
		PlannerInvalidSolutionUid = 28100,
		// Token: 0x04003EFA RID: 16122
		PlannerInvalidOptimizerSolutionUid,
		// Token: 0x04003EFB RID: 16123
		PlannerInvalidLookupTableValueUid,
		// Token: 0x04003EFC RID: 16124
		PlannerInvalidEfficientFrontierUid,
		// Token: 0x04003EFD RID: 16125
		PlannerInvalidProjectUid,
		// Token: 0x04003EFE RID: 16126
		PlannerInvalidAllocationThreshold,
		// Token: 0x04003EFF RID: 16127
		PlannerInvalidHiringType = 28109,
		// Token: 0x04003F00 RID: 16128
		PlannerInvalidConstraintType,
		// Token: 0x04003F01 RID: 16129
		PlannerInvalidConstraintValue,
		// Token: 0x04003F02 RID: 16130
		PlannerInvalidRateTable,
		// Token: 0x04003F03 RID: 16131
		PlannerInvalidSolutionForConstraint,
		// Token: 0x04003F04 RID: 16132
		PlannerInvalidSolutionForDependencies,
		// Token: 0x04003F05 RID: 16133
		PlannerInvalidSolutionForScheduling,
		// Token: 0x04003F06 RID: 16134
		PlannerInvalidAnalysisUid,
		// Token: 0x04003F07 RID: 16135
		PlannerInvalidProjectStartDate = 28200,
		// Token: 0x04003F08 RID: 16136
		PlannerInvalidProjectEndDate,
		// Token: 0x04003F09 RID: 16137
		PlannerInvalidProjectFNLTDate = 28203,
		// Token: 0x04003F0A RID: 16138
		PlannerInvalidProjectSNETDate,
		// Token: 0x04003F0B RID: 16139
		PlannerInvalidProjectDuration = 28202,
		// Token: 0x04003F0C RID: 16140
		PlannerCannotCreateSolution = 28900,
		// Token: 0x04003F0D RID: 16141
		PlannerCannotUpdateSolution,
		// Token: 0x04003F0E RID: 16142
		PlannerCannotDeleteSolution,
		// Token: 0x04003F0F RID: 16143
		PlannerCannotCreateMultipleSolutions,
		// Token: 0x04003F10 RID: 16144
		PlannerCannotUpdateMultipleSolutions,
		// Token: 0x04003F11 RID: 16145
		PlannerTableIsReadOnly = 28907,
		// Token: 0x04003F12 RID: 16146
		PlannerCannotCommitSolution,
		// Token: 0x04003F13 RID: 16147
		PlannerFieldIsReadOnly,
		// Token: 0x04003F14 RID: 16148
		PlannerProjectNotInParentSolution,
		// Token: 0x04003F15 RID: 16149
		PlannerProjectNotSelectedInParentSolution,
		// Token: 0x04003F16 RID: 16150
		PlannerProjectNotInParentAnalysis,
		// Token: 0x04003F17 RID: 16151
		PlannerProjectBeyondHorizon,
		// Token: 0x04003F18 RID: 16152
		PlannerResourceAllocationInternalError = 28915,
		// Token: 0x04003F19 RID: 16153
		PlannerResourceAllocationInfeasibleSolution,
		// Token: 0x04003F1A RID: 16154
		[ULSParameter(0, "budget")]
		PlannerProjectEndDateViolatesDependency,
		// Token: 0x04003F1B RID: 16155
		PlannerInvalidProjectsSet = 28919,
		// Token: 0x04003F1C RID: 16156
		PlannerInvalidInputData,
		// Token: 0x04003F1D RID: 16157
		PlannerDecimalOverflowError,
		// Token: 0x04003F1E RID: 16158
		PlannerSolutionMismatchedJobList,
		// Token: 0x04003F1F RID: 16159
		PlannerInvalidForceLookupTableValue,
		// Token: 0x04003F20 RID: 16160
		PlannerNoHiredResource,
		// Token: 0x04003F21 RID: 16161
		OptimizerDepInvalidDepType = 29000,
		// Token: 0x04003F22 RID: 16162
		OptimizerDepInvalidEntityType,
		// Token: 0x04003F23 RID: 16163
		OptimizerDepInvalidPosition = 29003,
		// Token: 0x04003F24 RID: 16164
		OptimizerDepDuplicateDependentProjects,
		// Token: 0x04003F25 RID: 16165
		OptimizerDepInvalidDependency,
		// Token: 0x04003F26 RID: 16166
		OptimizerDepCircularDependency,
		// Token: 0x04003F27 RID: 16167
		OptimizerCannotDeleteDependency,
		// Token: 0x04003F28 RID: 16168
		OptimizerCannotCreateDependency,
		// Token: 0x04003F29 RID: 16169
		OptimizerCannotUpdateDependency,
		// Token: 0x04003F2A RID: 16170
		OptimizerCannotCreateMultipleDependencies,
		// Token: 0x04003F2B RID: 16171
		OptimizerCannotUpdateMultipleDependencies,
		// Token: 0x04003F2C RID: 16172
		OptimizerEngineMatrixNotFilled = 29100,
		// Token: 0x04003F2D RID: 16173
		OptimizerEngineCustomFieldIsNotAConstraint,
		// Token: 0x04003F2E RID: 16174
		OptimizerCouldNotCalculatePrioritiesFromCustomFields,
		// Token: 0x04003F2F RID: 16175
		OptimizerEngineBinaryInfeasibleSolution,
		// Token: 0x04003F30 RID: 16176
		OptimizerEngineBinaryNumericalError,
		// Token: 0x04003F31 RID: 16177
		OptimizerEngineBinaryTimedOut,
		// Token: 0x04003F32 RID: 16178
		OptimizerEngineBinaryMaxedIterations,
		// Token: 0x04003F33 RID: 16179
		OptimizerEngineBinarySubOptimal,
		// Token: 0x04003F34 RID: 16180
		OptimizerEngineBinaryInternalError,
		// Token: 0x04003F35 RID: 16181
		OptimizerInvalidRange = 29200,
		// Token: 0x04003F36 RID: 16182
		OptimizerNonNormalizedWeights,
		// Token: 0x04003F37 RID: 16183
		OptimizerCannotEditPrioritization = 29300,
		// Token: 0x04003F38 RID: 16184
		OptimizerCannotDeletePrioritization,
		// Token: 0x04003F39 RID: 16185
		OptimizerCannotCreatePrioritization,
		// Token: 0x04003F3A RID: 16186
		OptimizerCannotUpdatePrioritization,
		// Token: 0x04003F3B RID: 16187
		OptimizerCannotCalculateDriverPriorities,
		// Token: 0x04003F3C RID: 16188
		OptimizerCannotCreateMultiplePrioritizations,
		// Token: 0x04003F3D RID: 16189
		OptimizerCannotUpdateMultiplePrioritizations,
		// Token: 0x04003F3E RID: 16190
		OptimizerDriverRelationsNotFilled,
		// Token: 0x04003F3F RID: 16191
		OptimizerDriversNotFilled,
		// Token: 0x04003F40 RID: 16192
		OptimizerDriverRelationsInvalidInversedValue,
		// Token: 0x04003F41 RID: 16193
		OptimizerCannotCreatePrioritizationUsingInactiveDrivers,
		// Token: 0x04003F42 RID: 16194
		OptimizerCannotChangePrioritizationType,
		// Token: 0x04003F43 RID: 16195
		OptimizerCannotSpecifyPriorityValuesForCalculatedPrioritizations,
		// Token: 0x04003F44 RID: 16196
		OptimizerCannotNormalizePriorityValues,
		// Token: 0x04003F45 RID: 16197
		OptimizerTooManyDriversInPrioritization,
		// Token: 0x04003F46 RID: 16198
		OptimizerInvalidProjectImpactValue = 29400,
		// Token: 0x04003F47 RID: 16199
		[ULSParameter(0, "DRIVER_UID")]
		[ULSParameter(3, "ITEM_NAME")]
		[ULSParameter(1, "DRIVER_NAME")]
		[ULSParameter(2, "ITEM_UID")]
		OptimizerCannotDeleteDriver,
		// Token: 0x04003F48 RID: 16200
		OptimizerCannotCreateDriver,
		// Token: 0x04003F49 RID: 16201
		OptimizerCannotUpdateDriver,
		// Token: 0x04003F4A RID: 16202
		OptimizerCannotEditDriver,
		// Token: 0x04003F4B RID: 16203
		OptimizerCannotCreateMultipleDrivers,
		// Token: 0x04003F4C RID: 16204
		OptimizerCannotUpdateMultipleDrivers,
		// Token: 0x04003F4D RID: 16205
		OptimizerInvalidRelativeImportanceValue,
		// Token: 0x04003F4E RID: 16206
		OptimizerInvalidDriverUid = 29500,
		// Token: 0x04003F4F RID: 16207
		OptimizerInvalidEntityType,
		// Token: 0x04003F50 RID: 16208
		OptimizerInvalidProjectUid,
		// Token: 0x04003F51 RID: 16209
		OptimizerInvalidCustomFieldUid,
		// Token: 0x04003F52 RID: 16210
		OptimizerInvalidHardConstraintUid,
		// Token: 0x04003F53 RID: 16211
		OptimizerInvalidAnalysisUid,
		// Token: 0x04003F54 RID: 16212
		OptimizerDriverFilterInvalid,
		// Token: 0x04003F55 RID: 16213
		OptimizerPrioritizationFilterInvalid,
		// Token: 0x04003F56 RID: 16214
		OptimizerCannotLoadOptimizationEngine,
		// Token: 0x04003F57 RID: 16215
		OptimizerAnalysisFilterInvalid,
		// Token: 0x04003F58 RID: 16216
		OptimizerSolutionFilterInvalid,
		// Token: 0x04003F59 RID: 16217
		OptimizerDependenciesFilterInvalid,
		// Token: 0x04003F5A RID: 16218
		OptimizerInvalidSolutionUid,
		// Token: 0x04003F5B RID: 16219
		OptimizerInvalidViewUid,
		// Token: 0x04003F5C RID: 16220
		OptimizerInvalidAnalysisType = 29600,
		// Token: 0x04003F5D RID: 16221
		OptimizerInvalidPrioritizationType,
		// Token: 0x04003F5E RID: 16222
		OptimizerCannotDeleteAnalysis,
		// Token: 0x04003F5F RID: 16223
		OptimizerCannotCreateAnalysis,
		// Token: 0x04003F60 RID: 16224
		OptimizerCannotUpdateAnalysis,
		// Token: 0x04003F61 RID: 16225
		OptimizerInvalidPrioritizationUid = 29607,
		// Token: 0x04003F62 RID: 16226
		OptimizerCannotCreateMultipleAnalyses,
		// Token: 0x04003F63 RID: 16227
		OptimizerCannotUpdateMultipleAnalyses,
		// Token: 0x04003F64 RID: 16228
		OptimizerCannotCalculateProjectPriorities,
		// Token: 0x04003F65 RID: 16229
		OptimizerCannotDeleteAnalysisProjectImpact,
		// Token: 0x04003F66 RID: 16230
		OptimizerCannotChangeAnalysisProjects,
		// Token: 0x04003F67 RID: 16231
		OptimizerCannotChangePriorityData,
		// Token: 0x04003F68 RID: 16232
		OptimizerCannotEditAnalysis,
		// Token: 0x04003F69 RID: 16233
		OptimizerInvalidPlannerData,
		// Token: 0x04003F6A RID: 16234
		OptimizerCannotChangeImpactData,
		// Token: 0x04003F6B RID: 16235
		OptimizerInvalidProjectsNumber,
		// Token: 0x04003F6C RID: 16236
		OptimizerCannotAddImpactCFUIDToCFAnalysis,
		// Token: 0x04003F6D RID: 16237
		OptimizerInvalidDepartmentUid,
		// Token: 0x04003F6E RID: 16238
		OptimizerTooManyProjectsInAnalysis,
		// Token: 0x04003F6F RID: 16239
		[ULSParameter(0, "AnalysisUid")]
		[ULSParameter(3, "Blocking")]
		[ULSParameter(1, "MessageType")]
		[ULSParameter(2, "MessageID")]
		QueueAnalysisCannotDeleteAnalysis = 29680,
		// Token: 0x04003F70 RID: 16240
		[ULSParameter(3, "Blocking")]
		[ULSParameter(0, "AnalysisUid")]
		[ULSParameter(1, "MessageType")]
		[ULSParameter(2, "MessageID")]
		QueueAnalysisCannotCreateAnalysis,
		// Token: 0x04003F71 RID: 16241
		[ULSParameter(2, "MessageID")]
		[ULSParameter(3, "Blocking")]
		[ULSParameter(0, "AnalysisUid")]
		[ULSParameter(1, "MessageType")]
		QueueAnalysisCannotUpdateAnalysis,
		// Token: 0x04003F72 RID: 16242
		AnalysisMismatchedJobList = 29690,
		// Token: 0x04003F73 RID: 16243
		OptimizerInvalidForceInLookupTableUid,
		// Token: 0x04003F74 RID: 16244
		OptimizerInvalidForceOutLookupTableUid,
		// Token: 0x04003F75 RID: 16245
		OptimizerDuplicateForceLookupTableUids,
		// Token: 0x04003F76 RID: 16246
		OptimizerInvalidDecisionResult = 29701,
		// Token: 0x04003F77 RID: 16247
		OptimizerInvalidForcedStatus,
		// Token: 0x04003F78 RID: 16248
		OptimizerCannotDeleteSolution,
		// Token: 0x04003F79 RID: 16249
		OptimizerCannotCreateSolution,
		// Token: 0x04003F7A RID: 16250
		OptimizerCannotUpdateSolution,
		// Token: 0x04003F7B RID: 16251
		OptimizerCannotCalculateSolutionStrategicAlignment,
		// Token: 0x04003F7C RID: 16252
		OptimizerCannotCreateMultipleSolutions,
		// Token: 0x04003F7D RID: 16253
		OptimizerCannotUpdateMultipleSolutions,
		// Token: 0x04003F7E RID: 16254
		OptimizerCannotAddPrioritizationToCFAnalysis,
		// Token: 0x04003F7F RID: 16255
		OptimizerTableIsReadOnly,
		// Token: 0x04003F80 RID: 16256
		[ULSParameter(2, "MessageID")]
		[ULSParameter(3, "Blocking")]
		[ULSParameter(0, "AnalysisUid")]
		[ULSParameter(1, "MessageType")]
		OptimizerSolutionCreateMessageFailed,
		// Token: 0x04003F81 RID: 16257
		[ULSParameter(0, "AnalysisUid")]
		[ULSParameter(2, "MessageID")]
		[ULSParameter(3, "Blocking")]
		[ULSParameter(1, "MessageType")]
		OptimizerSolutionDeleteMessageFailed,
		// Token: 0x04003F82 RID: 16258
		OptimizerCannotCalculateEfficientFrontier = 29714,
		// Token: 0x04003F83 RID: 16259
		OptimizerCannotUpdateSolutionProperties,
		// Token: 0x04003F84 RID: 16260
		OptimizerInvalidConstraintPosition,
		// Token: 0x04003F85 RID: 16261
		OptimizerInvalidHardConstraintPosition,
		// Token: 0x04003F86 RID: 16262
		OptimizerInvalidConstraintLimit,
		// Token: 0x04003F87 RID: 16263
		OptimizerInvalidConstraintValue,
		// Token: 0x04003F88 RID: 16264
		OptimizerInvalidSolutionProjectsSet,
		// Token: 0x04003F89 RID: 16265
		OptimizerCannotCommitSolution,
		// Token: 0x04003F8A RID: 16266
		OptimizerInvalidInputData = 29723,
		// Token: 0x04003F8B RID: 16267
		OptimizerInvalidConstraintSet,
		// Token: 0x04003F8C RID: 16268
		OptimizerCannotUpdateAnalysisMetrics,
		// Token: 0x04003F8D RID: 16269
		OptimizerSolutionMismatchedJobList,
		// Token: 0x04003F8E RID: 16270
		OptimizerInvalidForceLookupTableValue,
		// Token: 0x04003F8F RID: 16271
		OptimizerCannotCreateSolutionWhileAnalysisUpdateIsPending,
		// Token: 0x04003F90 RID: 16272
		OptimizerProjectSelectorAtLeastOne = 29800,
		// Token: 0x04003F91 RID: 16273
		WorkflowPhasesCannotCreatePhase = 35000,
		// Token: 0x04003F92 RID: 16274
		WorkflowPhasesCannotUpdatePhase,
		// Token: 0x04003F93 RID: 16275
		WorkflowPhasesCannotDeletePhase,
		// Token: 0x04003F94 RID: 16276
		WorkflowPhaseNameIsRequired,
		// Token: 0x04003F95 RID: 16277
		WorkflowStagesCannotCreateStage,
		// Token: 0x04003F96 RID: 16278
		WorkflowStagesCannotUpdateStage,
		// Token: 0x04003F97 RID: 16279
		WorkflowStagesCannotDeleteStage,
		// Token: 0x04003F98 RID: 16280
		[ULSParameter(0, "STAGE_NAME")]
		WorkflowStagesProjectsInStage,
		// Token: 0x04003F99 RID: 16281
		WorkflowCannotAccessPDPLibrary,
		// Token: 0x04003F9A RID: 16282
		WorkflowInvalidPDPUid,
		// Token: 0x04003F9B RID: 16283
		WorkflowInvalidCustomFieldUid,
		// Token: 0x04003F9C RID: 16284
		WorkflowCustomFieldNotWorkflowControlled,
		// Token: 0x04003F9D RID: 16285
		WorkflowCustomFieldCannotBeRequiredAndReadOnly,
		// Token: 0x04003F9E RID: 16286
		WorkflowInvalidWorkflowPhaseUid,
		// Token: 0x04003F9F RID: 16287
		WorkflowInsertWorkflowPhaseNotAllowed,
		// Token: 0x04003FA0 RID: 16288
		WorkflowInvalidWorkflowStageUid,
		// Token: 0x04003FA1 RID: 16289
		[ULSParameter(0, "PHASE_NAME")]
		WorkflowPhaseHasStages,
		// Token: 0x04003FA2 RID: 16290
		WorkflowStageNameIsRequired = 35020,
		// Token: 0x04003FA3 RID: 16291
		WorkflowStageAtLeastOnePDPIsRequired,
		// Token: 0x04003FA4 RID: 16292
		WorkflowCannotStartWorkflow = 35100,
		// Token: 0x04003FA5 RID: 16293
		WorkflowStatusCannotUpdateStatus,
		// Token: 0x04003FA6 RID: 16294
		WorkflowOnlyProjectsHaveWorkflow,
		// Token: 0x04003FA7 RID: 16295
		WorkflowNoWorkflowsDefined,
		// Token: 0x04003FA8 RID: 16296
		WorkflowInvalidStageForProject,
		// Token: 0x04003FA9 RID: 16297
		WorkflowNoWorkflowForProject,
		// Token: 0x04003FAA RID: 16298
		WorkflowCheckinRequiredAndProjectNotCheckedin,
		// Token: 0x04003FAB RID: 16299
		WorkflowWaitingForRequiredData,
		// Token: 0x04003FAC RID: 16300
		WorkflowFlagCustomFieldsCannotBeRequired,
		// Token: 0x04003FAD RID: 16301
		WorkflowCannotChangeWorkflow,
		// Token: 0x04003FAE RID: 16302
		WorkflowWorkflowStatusPDPNotAllowed,
		// Token: 0x04003FAF RID: 16303
		WorkflowInvalidWorkflowStatusPDPUid,
		// Token: 0x04003FB0 RID: 16304
		WorkflowInvalidStageStatusValue,
		// Token: 0x04003FB1 RID: 16305
		WorkflowCannotCheckinNotify,
		// Token: 0x04003FB2 RID: 16306
		WorkflowCannotCommitNotify,
		// Token: 0x04003FB3 RID: 16307
		WorkflowExceptionStartingWorkflow,
		// Token: 0x04003FB4 RID: 16308
		WorkflowStatusPDPMustBeSupplied,
		// Token: 0x04003FB5 RID: 16309
		WorkflowWorkflowProxyAccountNotFound,
		// Token: 0x04003FB6 RID: 16310
		WorkflowInvalidCurrentStage,
		// Token: 0x04003FB7 RID: 16311
		WorkflowMultipleStagesInProgress,
		// Token: 0x04003FB8 RID: 16312
		[ULSParameter(0, "Error")]
		WorkflowActivityInvalidArgument,
		// Token: 0x04003FB9 RID: 16313
		[ULSParameter(0, "Error")]
		WorkflowMTWConfigurationError,
		// Token: 0x04003FBA RID: 16314
		WorkflowNoStageInProgress,
		// Token: 0x04003FBB RID: 16315
		EnterpriseProjectTypeInvalidEnterpriseProjectTypeUid = 35200,
		// Token: 0x04003FBC RID: 16316
		EnterpriseProjectTypeCannotCreateEnterpriseProjectType,
		// Token: 0x04003FBD RID: 16317
		EnterpriseProjectTypeCannotUpdateEnterpriseProjectType,
		// Token: 0x04003FBE RID: 16318
		EnterpriseProjectTypeCannotDeleteEnterpriseProjectType,
		// Token: 0x04003FBF RID: 16319
		EnterpriseProjectTypeCannotCreateMultipleEnterpriseProjectTypes,
		// Token: 0x04003FC0 RID: 16320
		EnterpriseProjectTypeCannotUpdateMultipleEnterpriseProjectTypes,
		// Token: 0x04003FC1 RID: 16321
		EnterpriseProjectTypeInvalidCreatePDPUid,
		// Token: 0x04003FC2 RID: 16322
		EnterpriseProjectTypeInvalidProjectPlanTemplateUid,
		// Token: 0x04003FC3 RID: 16323
		EnterpriseProjectTypeInvalidWorkspaceTemplateName,
		// Token: 0x04003FC4 RID: 16324
		EnterpriseProjectTypeInvalidWorkflowAssociationUid,
		// Token: 0x04003FC5 RID: 16325
		EnterpriseProjectTypeCannotReadWssSettings,
		// Token: 0x04003FC6 RID: 16326
		EnterpriseProjectTypeCannotReadWssLanguagesAndTemplates,
		// Token: 0x04003FC7 RID: 16327
		EnterpriseProjectTypeInvalidDepartmentUid,
		// Token: 0x04003FC8 RID: 16328
		EnterpriseProjectTypeInvalidUri,
		// Token: 0x04003FC9 RID: 16329
		EnterpriseProjectTypeUriRequiresHttp,
		// Token: 0x04003FCA RID: 16330
		[ULSParameter(0, "ENTERPRISE_PROJECT_TYPE_NAME")]
		EnterpriseProjectTypeCannotDeleteDefault,
		// Token: 0x04003FCB RID: 16331
		EnterpriseProjectTypeCannotChangeDefault,
		// Token: 0x04003FCC RID: 16332
		[ULSParameter(0, "ENTERPRISE_PROJECT_TYPE_NAME")]
		EnterpriseProjectTypeHasProjectsCannotDelete,
		// Token: 0x04003FCD RID: 16333
		EnterpriseProjectTypeCreatePDPIsRequired,
		// Token: 0x04003FCE RID: 16334
		EnterpriseProjectTypeOnlyOneCreatePDPAllowed,
		// Token: 0x04003FCF RID: 16335
		EnterpriseProjectTypeHasWorkflowOnlyCreatePDPAllowed,
		// Token: 0x04003FD0 RID: 16336
		EnterpriseProjectTypeInvalidData,
		// Token: 0x04003FD1 RID: 16337
		EnterpriseProjectNoDefaultEnterpriseProjectTypeDefined,
		// Token: 0x04003FD2 RID: 16338
		EnterpriseProjectTypeAtLeastOnePDPIsRequired,
		// Token: 0x04003FD3 RID: 16339
		EnterpriseProjectTypeWorkflowStatusPDPNotAllowed,
		// Token: 0x04003FD4 RID: 16340
		EnterpriseProjectTypeProjectHasAssociation,
		// Token: 0x04003FD5 RID: 16341
		EnterpriseProjectTypeInvalidProjIdPrefix,
		// Token: 0x04003FD6 RID: 16342
		EnterpriseProjectTypeInvalidProjIdPostfix,
		// Token: 0x04003FD7 RID: 16343
		EnterpriseProjectTypeInvalidProjIdSeed,
		// Token: 0x04003FD8 RID: 16344
		EnterpriseProjectTypeInvalidProjIdMindigit,
		// Token: 0x04003FD9 RID: 16345
		[ULSParameter(0, "methodName")]
		[ULSParameter(1, "errorMessage")]
		InvariantValidationPSIFailed = 40000,
		// Token: 0x04003FDA RID: 16346
		[ULSParameter(0, "errorMessage")]
		ValidationMethodFailed,
		// Token: 0x04003FDB RID: 16347
		[ULSParameter(0, "exception")]
		GeneralExchangeSyncError = 40500,
		// Token: 0x04003FDC RID: 16348
		[ULSParameter(0, "teamMemberUid")]
		[ULSParameter(1, "exchangeResponseText")]
		ExchangeSyncRootFolderCreationFailed,
		// Token: 0x04003FDD RID: 16349
		[ULSParameter(1, "exception")]
		[ULSParameter(0, "teamMemberUid")]
		ExchangeSyncTaskFolderCreationFailed,
		// Token: 0x04003FDE RID: 16350
		[ULSParameter(0, "teamMemberUid")]
		[ULSParameter(1, "exception")]
		ExchangeSyncCouldNotGetRootFolder,
		// Token: 0x04003FDF RID: 16351
		[ULSParameter(1, "exchangeResponseText")]
		[ULSParameter(0, "teamMemberUid")]
		ExchangeSyncCouldNotLoadTaskObject,
		// Token: 0x04003FE0 RID: 16352
		[ULSParameter(0, "teamMemberUid")]
		[ULSParameter(1, "exchangeResponseText")]
		ExchangeSyncNewExchangeTaskCreationFailed,
		// Token: 0x04003FE1 RID: 16353
		[ULSParameter(0, "teamMemberUid")]
		[ULSParameter(1, "effectiveUserUid")]
		[ULSParameter(2, "exception")]
		ExchangeSyncFailedToUpdateCacheForUser,
		// Token: 0x04003FE2 RID: 16354
		[ULSParameter(1, "exchangeResponseText")]
		[ULSParameter(0, "teamMemberUid")]
		ExchangeSyncFailedToUpdateExchangeTask,
		// Token: 0x04003FE3 RID: 16355
		[ULSParameter(0, "teamMemberUid")]
		[ULSParameter(1, "exchangeResponseText")]
		ExchangeSyncSubscriptionUpdateFailed,
		// Token: 0x04003FE4 RID: 16356
		[ULSParameter(0, "teamMemberUid")]
		ExchangeSyncEWSUrlFailed,
		// Token: 0x04003FE5 RID: 16357
		[ULSParameter(0, "teamMemberUid")]
		[ULSParameter(1, "exception")]
		ExchangeSyncExchangeUrlRefreshFailed,
		// Token: 0x04003FE6 RID: 16358
		[ULSParameter(0, "teamMemberUid")]
		[ULSParameter(1, "exception")]
		ExchangeSyncExchangeSubscriptionUpdateForUserFailed,
		// Token: 0x04003FE7 RID: 16359
		[ULSParameter(1, "exception")]
		[ULSParameter(0, "teamMemberUid")]
		ExchangeSyncGeneralProcessingFailure,
		// Token: 0x04003FE8 RID: 16360
		[ULSParameter(1, "exception")]
		[ULSParameter(0, "teamMemberUid")]
		ExchangeSyncDeletionOfTasksInExchangeFailure,
		// Token: 0x04003FE9 RID: 16361
		[ULSParameter(0, "teamMemberUid")]
		ExchangeSyncAttemptedSyncOfInvalidConfiguredResource,
		// Token: 0x04003FEA RID: 16362
		[ULSParameter(1, "exception")]
		[ULSParameter(0, "teamMemberUid")]
		ExchangeSyncRetrievalOfEWSUrlCausedException,
		// Token: 0x04003FEB RID: 16363
		TimelineViewDataDoesNotExist = 42000,
		// Token: 0x04003FEC RID: 16364
		[ULSParameter(1, "delegateUid")]
		[ULSParameter(0, "resUid")]
		UserDelegationExpired = 43000,
		// Token: 0x04003FED RID: 16365
		[ULSParameter(0, "resUid")]
		[ULSParameter(1, "delegateUid")]
		UserDelegationCannotSelfDelegate,
		// Token: 0x04003FEE RID: 16366
		[ULSParameter(0, "delegateUid")]
		UserDelegationInvalidDelegate,
		// Token: 0x04003FEF RID: 16367
		[ULSParameter(0, "resUid")]
		UserDelegationInvalidUser,
		// Token: 0x04003FF0 RID: 16368
		UserDelegationInvalidDates,
		// Token: 0x04003FF1 RID: 16369
		UserDelegationCannotDoubleDelegate,
		// Token: 0x04003FF2 RID: 16370
		[ULSParameter(0, "delegateUid")]
		UserDelegationDelegateCannotLogon,
		// Token: 0x04003FF3 RID: 16371
		[ULSParameter(0, "delegateUid")]
		UserDelegationDelegateIsInactive,
		// Token: 0x04003FF4 RID: 16372
		UserDelegationInvalidFilter,
		// Token: 0x04003FF5 RID: 16373
		[ULSParameter(0, "resUid")]
		UserDelegationUserCannotLogon = 43010,
		// Token: 0x04003FF6 RID: 16374
		[ULSParameter(0, "resUid")]
		UserDelegationUserIsInactive,
		// Token: 0x04003FF7 RID: 16375
		DatabaseUndefinedError = 50000,
		// Token: 0x04003FF8 RID: 16376
		DatabaseCannotInsertDuplicateKeyError,
		// Token: 0x04003FF9 RID: 16377
		DatabaseForeignKeyViolationError,
		// Token: 0x04003FFA RID: 16378
		DatabaseCheckConstraintViolationError,
		// Token: 0x04003FFB RID: 16379
		DatabaseUniqueConstraintViolationError,
		// Token: 0x04003FFC RID: 16380
		DatabaseMismatchedVersion,
		// Token: 0x04003FFD RID: 16381
		DatabaseInvalidColumnNameError,
		// Token: 0x04003FFE RID: 16382
		ProjectDetailPagesStrategicImpactRatingRequired = 32000,
		// Token: 0x04003FFF RID: 16383
		ProjectDetailPagesMissingPDPLinks,
		// Token: 0x04004000 RID: 16384
		ProjectDetailPagesUnavailableWorker,
		// Token: 0x04004001 RID: 16385
		ProjectDetailPagesFailedToLoadProjectInWorker,
		// Token: 0x04004002 RID: 16386
		ProjectDetailPagesWorkerBusy,
		// Token: 0x04004003 RID: 16387
		ProjectDetailPagesMaxUserSessionLimitReached,
		// Token: 0x04004004 RID: 16388
		[ULSParameter(0, "ServerUid")]
		ProjectDetailPagesWorkerOpenedInAnotherServer,
		// Token: 0x04004005 RID: 16389
		AppPermissionInvalidAppPermissionId = 32300,
		// Token: 0x04004006 RID: 16390
		CSOMDelegationNotSupported = 32500,
		// Token: 0x04004007 RID: 16391
		CSOMProjectSiteDoesNotExist,
		// Token: 0x04004008 RID: 16392
		CSOMCannotEnableVisibilityMode,
		// Token: 0x04004009 RID: 16393
		CSOMTaskListIncompatibleWithImport,
		// Token: 0x0400400A RID: 16394
		CSOMProjectSiteInUse,
		// Token: 0x0400400B RID: 16395
		CSOMUnknownUser,
		// Token: 0x0400400C RID: 16396
		EngagementNotFound = 33000,
		// Token: 0x0400400D RID: 16397
		EngagementSetDataValueInvalid,
		// Token: 0x0400400E RID: 16398
		EngagementSetDataFailed,
		// Token: 0x0400400F RID: 16399
		EngagementInvalidPID,
		// Token: 0x04004010 RID: 16400
		EngagementStatusChangeNotPermitted,
		// Token: 0x04004011 RID: 16401
		EngagementStatusChangeNotValid,
		// Token: 0x04004012 RID: 16402
		EngagementTimephasedChangesNotPermitted,
		// Token: 0x04004013 RID: 16403
		EngagementTimephasedChangesNotValid,
		// Token: 0x04004014 RID: 16404
		EngagementTimephasedDataMissing,
		// Token: 0x04004015 RID: 16405
		EngagementsCannotBeTurnedOff,
		// Token: 0x04004016 RID: 16406
		EngagementWorkValueInvalid,
		// Token: 0x04004017 RID: 16407
		EngagementFinishDateValueInvalid,
		// Token: 0x04004018 RID: 16408
		EngagementStartDateNotInitialized
	}
}
