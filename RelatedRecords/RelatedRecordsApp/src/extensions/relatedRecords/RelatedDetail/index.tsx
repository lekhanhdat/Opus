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
import * as React from 'react';
import { ExtensionContext } from '@microsoft/sp-extension-base';
import { TooltipHost, Spinner, TooltipOverflowMode, SpinnerSize } from '@fluentui/react';
import { css } from '@fluentui/react/lib/Utilities';

import * as HttpClientUtil from '../../../common/HttpClientUtil';
import { NodeType } from '../constants/node-type';
import { IRecordDetail, IRelatedDetailArgs, IRelatedRecordItem } from '../types/related-record';
import * as strings from 'RelatedRecordsCommandSetStrings';

interface IRelatedDetailProps {
    context: ExtensionContext
    payloadData: IRelatedRecordItem | null;
    classNames: any
}

const YesOrNo = {
    0: strings.Related_App_Common_Yes,
    1: strings.Related_App_Common_No,
}

function RelatedDetail(props: IRelatedDetailProps) {
    const { context, payloadData, classNames } = props;

    const [isLoading, setIsLoading] = React.useState<boolean>(false)
    const [recordData, setRecordData] = React.useState<IRecordDetail | null>(null);

    React.useEffect(() => {
        if (payloadData) {
            let args: IRelatedDetailArgs;

            if (payloadData.sourceFlag !== 1) { // is Physical folder / record
                args = {
                    UniqueId: payloadData.uniqueId,
                    SourceFlag: payloadData.sourceFlag,
                };
            } else {
                // is SP file
                args = {
                    ListId: payloadData.listId,
                    WebId: payloadData.webId,
                    UniqueId: payloadData.uniqueId,
                    ListItemId: payloadData.listItemId,
                    SiteUrl: payloadData.siteUrl,
                    SiteId: payloadData.siteId,
                    SourceFlag: payloadData.sourceFlag,
                };
            }

            const handleGetRecordDetail = async () => {
                setIsLoading(true);
                try {
                    const res = await HttpClientUtil.callRecordsApi(context, "/API/AppActions/GetRelatedRecordDetail", args)
                    setRecordData(JSON.parse(res).Summary);
                } catch (error) {
                    // Show error message or something in here
                } finally {
                    setIsLoading(false);
                }
            }

            handleGetRecordDetail();
        }
    }, [payloadData])

    const values = React.useMemo(() => {
        const safeAccess = (path: string): string => {
            return path.split(".").reduce((acc: any, key: string) => {
                if (acc && typeof acc === "object") {
                    return acc[key];
                }
                return "";
            }, recordData);
        };

        return {
            "Detail_Name": safeAccess("LeafName"),
            "Related_App_Detail_RelatedLabel_Name": safeAccess("LeafName"),
            "Related_App_Detail_RelatedLabel_Location": safeAccess("FullPath"),
            "Related_App_Detail_RelatedLabel_RecordID": safeAccess("RecordId"),
            "Related_App_Detail_RelatedLabel_Term": safeAccess("Term"),
            "Related_App_Detail_RelatedLabel_Disable_Retention": safeAccess("TermSettings"),
            "Related_App_Detail_RelatedLabel_Rule_Name": safeAccess("RuleName"),
            "Related_App_Detail_RelatedLabel_Rule_Action": safeAccess("DisposalAction"),
            "Related_App_Detail_RelatedLabel_Action_Duedate": safeAccess("DisposalDate"),
            "Related_App_Detail_RelatedLabel_On_Hold": safeAccess("HoldStatus"),
            "Related_App_Detail_RelatedLabel_Hold_Title": safeAccess("HoldSetting.Name"),
            "Related_App_Detail_RelatedLabel_Comment": safeAccess("HoldSetting.Description"),
            "Related_App_Detail_RelatedLabel_Hold_By": safeAccess("HoldBy"),
            "Related_App_Detail_RelatedLabel_Hold_Until": safeAccess("HoldReleaseTime"),
            "Related_App_Detail_RelatedLabel_Declared_As_Record": safeAccess("DeclareAsRecord"),
        }
    }, [recordData])

    if (isLoading) {
        return (
            <div>
                <Spinner size={SpinnerSize.medium} style={{ marginRight: 0 }} className={classNames.searchResultLoading} />
            </div>
        );
    }

    return (
        <div className='related-records-detail-panel-wrapper'>
            <h3 tabIndex={0} aria-label={values.Detail_Name}>{values.Detail_Name}</h3>

            {/* Overview */}
            <div className="related-records-detail-panel-section">
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_Overview}>{strings.Related_App_Detail_Overview}</p>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Name}>{strings.Related_App_Detail_RelatedLabel_Name}</p>
                    <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Name}>{values.Related_App_Detail_RelatedLabel_Name}</p>
                </div>
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Location}>{strings.Related_App_Detail_RelatedLabel_Location}</p>
                    <TooltipHost
                        id={"location"}
                        overflowMode={TooltipOverflowMode.Self}
                        hostClassName={css(classNames.itemText)}
                        content={values.Related_App_Detail_RelatedLabel_Location}
                    >
                        {payloadData?.sourceFlag === 1 ? (
                            <a
                                href={values.Related_App_Detail_RelatedLabel_Location}
                                target='_blank'
                                className="related-records-detail-panel-section-info_location"
                                tabIndex={0}
                                aria-label={values.Related_App_Detail_RelatedLabel_Location}
                            >
                                {values.Related_App_Detail_RelatedLabel_Location}
                            </a>
                        ) : (
                            <span
                                tabIndex={0}
                                aria-label={values.Related_App_Detail_RelatedLabel_Location}
                            >
                                {values.Related_App_Detail_RelatedLabel_Location}
                            </span>
                        )}
                    </TooltipHost>
                </div>
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_RecordID}>{strings.Related_App_Detail_RelatedLabel_RecordID}</p>
                    <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_RecordID}>{values.Related_App_Detail_RelatedLabel_RecordID}</p>
                </div>
                {payloadData?.nodeType === NodeType.PhyFolder && (
                    <div className='related-records-detail-panel-section-info'>
                        <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Term}>{strings.Related_App_Detail_RelatedLabel_Term}</p>
                        <TooltipHost
                            id={"Term"}
                            overflowMode={TooltipOverflowMode.Self}
                            hostClassName={css(classNames.itemText)}
                            content={values.Related_App_Detail_RelatedLabel_Term}
                        >
                            <span tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Term}>{values.Related_App_Detail_RelatedLabel_Term}</span>
                        </TooltipHost>
                        <span style={{ marginTop: -6 }} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Disable_Retention}>{values.Related_App_Detail_RelatedLabel_Disable_Retention}</span>
                    </div>
                )}
            </div>

            {/* Disposal information */}
            <div className="related-records-detail-panel-section">
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_Disposal_Info}>{strings.Related_App_Detail_Disposal_Info}</p>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Rule_Name}>{strings.Related_App_Detail_RelatedLabel_Rule_Name}</p>
                    <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Rule_Name}>{values.Related_App_Detail_RelatedLabel_Rule_Name}</p>
                </div>
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Rule_Action}>{strings.Related_App_Detail_RelatedLabel_Rule_Action}</p>
                    <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Rule_Action}>{values.Related_App_Detail_RelatedLabel_Rule_Action}</p>
                </div>
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Action_Duedate}>{strings.Related_App_Detail_RelatedLabel_Action_Duedate}</p>
                    <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Action_Duedate}>{values.Related_App_Detail_RelatedLabel_Action_Duedate}</p>
                </div>
            </div>

            {/* Hold information */}
            <div className="related-records-detail-panel-section">
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_Hold_Information}>{strings.Related_App_Detail_Hold_Information}</p>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_On_Hold}>{strings.Related_App_Detail_RelatedLabel_On_Hold}</p>
                    <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_On_Hold ? YesOrNo[0] : YesOrNo[1]}>{values.Related_App_Detail_RelatedLabel_On_Hold ? YesOrNo[0] : YesOrNo[1]}</p>
                </div>
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Hold_Title}>{strings.Related_App_Detail_RelatedLabel_Hold_Title}</p>
                    <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Hold_Title}>{values.Related_App_Detail_RelatedLabel_Hold_Title}</p>
                </div>
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Comment}>{strings.Related_App_Detail_RelatedLabel_Comment}</p>
                    <TooltipHost
                        id={"Comment"}
                        overflowMode={TooltipOverflowMode.Self}
                        hostClassName={css(classNames.itemText)}
                        content={values.Related_App_Detail_RelatedLabel_Comment}
                    >
                        <span tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Comment}>{values.Related_App_Detail_RelatedLabel_Comment}</span>
                    </TooltipHost>
                </div>
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Hold_By}>{strings.Related_App_Detail_RelatedLabel_Hold_By}</p>
                    <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Hold_By}>{values.Related_App_Detail_RelatedLabel_Hold_By}</p>
                </div>
                <div className='related-records-detail-panel-section-info'>
                    <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Hold_Until}>{strings.Related_App_Detail_RelatedLabel_Hold_Until}</p>
                    <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Hold_Until}>{values.Related_App_Detail_RelatedLabel_Hold_Until}</p>
                </div>
            </div>

            {/* Declared information */}
            {payloadData?.sourceFlag === 1 && (
                <div className="related-records-detail-panel-section">
                    <div className='related-records-detail-panel-section-info'>
                        <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_Declared_Information}>{strings.Related_App_Detail_Declared_Information}</p>
                        <p className="related-records-detail-panel-section-info_label" tabIndex={0} aria-label={strings.Related_App_Detail_RelatedLabel_Declared_As_Record}>{strings.Related_App_Detail_RelatedLabel_Declared_As_Record}</p>
                        <p className={`${classNames.itemText} related-records-detail-panel-section-info_value`} tabIndex={0} aria-label={values.Related_App_Detail_RelatedLabel_Declared_As_Record ? YesOrNo[0] : YesOrNo[1]}>{values.Related_App_Detail_RelatedLabel_Declared_As_Record ? YesOrNo[0] : YesOrNo[1]}</p>
                    </div>
                </div>
            )}
        </div>
    )
}

export default RelatedDetail