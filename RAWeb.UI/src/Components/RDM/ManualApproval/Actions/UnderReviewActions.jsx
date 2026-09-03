import React, { useEffect, useRef, useState } from "react";
import { LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";
import { ManualReviewAction, ManualReviewActionI18Ns, ManualReviewActionIcons, ApprovalStatus, ExtendType, NodeType } from "../Constants/index";
import ApprovalAction from "./ApprovalAction";
import { EscalateAction, ReassignAction } from "./EscalateAction";
import ExportAction from "./ExportAction";
import UnderReviewImportPanel from "../Panels/UnderReviewImportPanel";
import Utility from "../Utility";
import { Messagebox } from "../../../Common/Messagebox";
import ApprovalCommentSettingDialog from "../Panels/ApprovalCommentSettingDialog";
import { ManualTab } from "../Constants/ManualTable";
import _ from "lodash";
import ReclassifyAction from './ReclassifyAction'
import { SourceFlag } from "../../../Common/Constants";

const UnderReviewActions = ({ disabledEscalate, checkedItems, unCheckedItems,isCheckedAll, itemCount, onReload, settingModel, queryDefintion, limitItemsCount, checkedCommentOption, NeedQuickReason, ApprovalCommentQuickReason, InactiveRejects, NeedCustomButton, CustomButtonNames, canDoActionForReclassify, isFSSettingClassificationFolderLevel, filterDefinitions, searchFilterDefinition, isHideReclassifyBtnByApiSetting }) => {

    const escalateRef = useRef();

    const reassignRef = useRef();

    const [showImportPanel, setShowImportPanel] = useState(false);

    const [showApprovalComment, setShowApprovalComment] = useState(false);

    const [approvalAction, setApprovalAction] = useState(ApprovalStatus.Approved);

    const [approvalCommentQuickReason, setApprovalCommentQuickReason] = useState("");

    const [isShowReclassifyButton, setIsShowReclassifyButton] = useState(true);

    const reclassifyActionRef = useRef();

    useEffect(() => {
        if (isHideReclassifyBtnByApiSetting) {
            setIsShowReclassifyButton(false);
            return;
        }

        if (!isHideReclassifyBtnByApiSetting && checkedItems && checkedItems.length) {
            const firstCheckedItem = checkedItems[0];
            const isInvalid = isCheckedAll || checkedItems?.some((item) =>
                item.nodeType !== firstCheckedItem.nodeType
            || item.sourceFlag !== firstCheckedItem.sourceFlag
            || item.retentionStatus === 1
            || ([SourceFlag.Exchange, SourceFlag.OneDrive, SourceFlag.GoogleDrive].includes(item.sourceFlag) && !item.enableClassificationByOpus)
            || (item.sourceFlag === SourceFlag.FileSystem && isFSSettingClassificationFolderLevel));
            setIsShowReclassifyButton(LicenseHelper.EnableRecordsArchiver() && !isInvalid);
        }
    }, [isHideReclassifyBtnByApiSetting, checkedItems, isCheckedAll, isFSSettingClassificationFolderLevel])

    const PreInspect = () => {
        if (checkedItems.length > limitItemsCount) {
            showToast.warn(RMResx.RM_RDM_MA_Msg_CheckMoreThan5000);
            return false;
        }
        return true;
    };

    const JobPreInspect = () => {
        if (unCheckedItems.length > 1000) {
            showToast.warn(RMResx.RM_MA_TasksDeSelected_Limited);
            return false;
        }
        return true;
    };

    const onApprove = () => {
        setApprovalAction(ApprovalStatus.Approved);
        setShowApprovalComment(true);
    };

    const realApprove = (approveComment) => {
        if (isCheckedAll) {
            if(!JobPreInspect()){
                return;
            }

            ApprovalAction.onRunApproveJob(ApprovalStatus.Approved, queryDefintion, approveComment, approvalCommentQuickReason,Utility.getItemIds(unCheckedItems),ExtendType.None, new Date(),() => {
                onReload();
            });
            return;
        }

        if (!PreInspect()) {
            return;
        }

        ApprovalAction.onApprove(Utility.getItemIds(checkedItems), approveComment, ManualTab.UnderReview, approvalCommentQuickReason, () => {
            onReload();
        });
    };

    const onReject = () => {
        setApprovalAction(ApprovalStatus.Rejected);
        const notExtendItems = checkedItems.filter(item => 
            item.extendCount >= settingModel.DisposalExtentionSetting.MaxDelayTimes 
        );

        if (notExtendItems.length > 0) {
            $$.messagedialog(true,
                {
                    width: "550px",
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: (
                        <div>
                            <div className="reco-manual-message-box-comment">
                                {checkedItems.length > 1 ?
                                    RMResx.RM_MA_Extended_ExtendLimitForMore :
                                    RMResx.RM_MA_Extended_ExtendLimitForOne
                                }
                            </div>
                            <div className="reco-manual-message-box-associated">
                                {RMResx.RM_MA_AssociatedRecords}
                            </div>
                            <div className="reco-manual-message-box-associated-items">
                                {
                                    notExtendItems.map(item =>
                                        <div key={item.id} className="reco-manual-message-box-associated-item">{item.leafName}</div>
                                    )
                                }
                            </div>
                        </div>
                    ),
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_OK,
                            primary: true,
                            classify: "theme",
                            onClick: async () => {
                                $$.messagedialog(false);
                            },
                        },
                    ],
                }
            );
            return;
        }
        
        setShowApprovalComment(true);
    };

    const onReclassify = () => {
        // const clonedCheckedItems = _.cloneDeep(checkedItems);
        // let firstContainerId = "";
        // let sourceFlag = SourceFlags.None;
        // for (let index = 0; index < clonedCheckedItems.length; index++) {
        //     const element = clonedCheckedItems[index];
        //     if (index == 0) {
        //         sourceFlag = element.SourceFlag;
        //     }
        //     if (element.ContainerId) {
        //         firstContainerId = element.ContainerId;
        //         break;
        //     }
        // }
        // const containerSourceFlags = [SourceFlags.SP, SourceFlags.Exo, SourceFlags.OneDrive];
        // if (containerSourceFlags.find(s => s == sourceFlag) && (!firstContainerId)) {
        //     handleShowReclassifyMessageBox(RMResx.RM_JS_BCM_Reclassify_MissingContainerIdErrorMessage);
        //     return;
        // }
        
        // const itemsId = [];
        // clonedCheckedItems.forEach(item => {
        //     itemsId.push(item.Id);
        // });
        // const option = {
        //     url: '/api/RecordsExplorerApi/CheckItemsInTheSameSecurityGroup',
        //     method: "POST",
        //     data: itemsId,
        // };
        
        // fetchUtility(option)
        //     .then((res) => {
        //         if (res) {
        //             onOpenReclassifyPanel();
        //         } else {
        //             handleShowReclassifyMessageBox(RMResx.RM_JS_BCM_Reclassify_Message);
        //         }
        //     });
        reclassifyActionRef.current?.onOpenReclassifyPanel();
    }

    const realReject = (rejectComment,extendType,customeExtendDate) => {

        if (isCheckedAll) {
            if(!JobPreInspect()){
                return;
            }
            ApprovalAction.onRunApproveJob(ApprovalStatus.Rejected, queryDefintion, rejectComment, approvalCommentQuickReason,  Utility.getItemIds(unCheckedItems), extendType, customeExtendDate , () => {
                onReload();
            });
            return;
        }

        if (!PreInspect()) {
            return;
        }
  
        ApprovalAction.onReject(Utility.getItemIds(checkedItems), rejectComment, ManualTab.UnderReview, approvalCommentQuickReason, extendType, customeExtendDate ,() => {
            onReload();
        });
    };

    const onEscalate = () => {

        if (!PreInspect()) {
            return;
        }

        escalateRef.current.onShow(Utility.getItemIds(checkedItems));
    };

    const onReassign = () => {

        if (!PreInspect()) {
            return;
        }

        reassignRef.current.onShow(Utility.getItemIds(checkedItems));
    };

    const onExport = () => {
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: ExportAction.onExport.bind(this, queryDefintion)});
    };

    const onImport = () => {
        setShowImportPanel(true);
    };

    const onHide = () => {
        setShowImportPanel(false);
        setShowApprovalComment(false);
        setApprovalCommentQuickReason("");
    };

    const onChange = (args) => {
        setApprovalCommentQuickReason(args);
    };

    return (
        <div className="reco-manual-review-actions">
            <div
                className="reco-manual-review-actions-buttons"
                style={{
                    visibility: checkedItems.length === 0 && !isCheckedAll ? "hidden" : "visible",
                }}
            >
                <div style={{
                    visibility: "visible",
                    display: "flex",
                    columnGap: "8px",
                }}
                >
                    <R.Button
                        primary={true}
                        classify="theme"
                        text={ManualReviewActionI18Ns.get(ManualReviewAction.Export)}
                        onClick={onExport}
                    />
                    <R.Button
                        primary={false}
                        classify="default"
                        text={ManualReviewActionI18Ns.get(ManualReviewAction.Import)}
                        icon={ManualReviewActionIcons.get(ManualReviewAction.Import)}
                        onClick={onImport}
                    />
                </div>
                <R.Button
                    primary={false}
                    classify="default"
                    text={Utility.getCustomButtonNames(NeedCustomButton, CustomButtonNames).approveButtonName}
                    icon={ManualReviewActionIcons.get(ManualReviewAction.Approve)}
                    onClick={onApprove}
                />
                <R.Button
                    primary={false}
                    classify="default"
                    text={Utility.getCustomButtonNames(NeedCustomButton, CustomButtonNames).rejectButtonName}
                    icon={ManualReviewActionIcons.get(ManualReviewAction.Reject)}
                    onClick={onReject}
                />
                {isShowReclassifyButton && (
                    <R.Button
                        primary={false}
                        classify="default"
                        text={ManualReviewActionI18Ns.get(ManualReviewAction.Reclassify)}
                        icon={ManualReviewActionIcons.get(ManualReviewAction.Reclassify)}
                        onClick={onReclassify}
                    />
                )}
                {
                    (!isCheckedAll && !Utility.checkHasApprovedItem(checkedItems)) &&
                    <>
                        <div>
                            <R.ButtonGroup
                                type="action"
                                tooltip={RMResx.RM_PRM_PRE_More}
                            >                  
                                <R.Button
                                    primary={false}
                                    classify="default"
                                    text={ManualReviewActionI18Ns.get(ManualReviewAction.Reassign)}
                                    onClick={onReassign}
                                />
                                {
                                    !disabledEscalate
                                    &&
                                    <R.Button
                                        primary={false}
                                        classify="default"
                                        text={ManualReviewActionI18Ns.get(ManualReviewAction.Escalate)}
                                        onClick={onEscalate}
                                    />
                                }
                            </R.ButtonGroup>
                        </div>
                    </>
                }
            </div>
            <div className="reco-manual-review-actions-desc">
                {
                    RMResx.RM_Common_SelectTableItemsCounter.format(isCheckedAll ?  itemCount - unCheckedItems.length : checkedItems.length, itemCount)
                }
            </div>

            <EscalateAction ref={escalateRef} onReload={onReload} />
            <ReassignAction ref={reassignRef} onReload={onReload} />
            <UnderReviewImportPanel show={showImportPanel} onHide={onHide}></UnderReviewImportPanel>
            <ApprovalCommentSettingDialog 
                show={showApprovalComment} 
                onHide={onHide}
                checkedCommentOption={checkedCommentOption}
                action={approvalAction}
                onApprove={realApprove}
                onReject={realReject}
                NeedQuickReason={NeedQuickReason}
                ApprovalCommentQuickReason={ApprovalCommentQuickReason}
                InactiveRejects={InactiveRejects}
                CheckQuickReason={approvalCommentQuickReason}
                onChange={onChange}
                LatestExtendType = {settingModel.DisposalExtentionSetting.LatestExtendType}
                LatestExtendNumber = {settingModel.DisposalExtentionSetting.LatestExtendNumber}
                checkedItems = {checkedItems}
                needCustomButton={NeedCustomButton}
                customButtonNames={CustomButtonNames}
            />
            <ReclassifyAction
                ref={reclassifyActionRef}
                checkedItems={checkedItems}
                isCheckedAll={isCheckedAll}
                canDoActionForReclassify={canDoActionForReclassify}
                filterDefinitions={filterDefinitions}
                searchFilterDefinition={searchFilterDefinition}
                onReload={onReload}
            />
        </div>
    );
};

export default UnderReviewActions;