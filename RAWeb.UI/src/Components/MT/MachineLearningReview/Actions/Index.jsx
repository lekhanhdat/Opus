import React, {useRef } from "react";
import _ from "lodash";
import { showToast } from "../../../../Utilities/CommonUtil";
import { ActionTypes, ApprovalStatus, ActionI18Ns, FilterOptions} from "../Constants/index";
import ApprovalAction from "./ApprovalAction";
import ReclassifyPanel from "./ReclassifyAction";
import { ReassignAction } from "./EscalateAction";
import Utility from "../Utility";
import UpdateNotifications from "../NotificationsContent/UpdateNotifications";
import { SourceFlag } from "../../../Common/Constants";


const Actions = ({ checkedItems, isCheckedAll, onReload, queryDefintion, limitItemsCount, filterOptions }) =>{

    const reassignRef = useRef();

    const reclassifyPanelRef = useRef();

    const updateNotificationsRef = useRef();

    const PreInspect = (action) => {
        if (checkedItems.length > limitItemsCount) {
            showToast.warn(RMResx.RM_RDM_MA_Msg_CheckMoreThanActionLimitCount);
            return false;
        }
        return true;
    };

    const getIsSameSecurityGroupItems = () => {
        let option = {
            url: '/api/RecordsExplorerApi/CheckItemsInTheSameSecurityGroup',
            method: "POST",
            data: Utility.getItemIds(checkedItems)
        };
        return fetchUtility(option);
    };
    
    const onApprove = () => {
        if (isCheckedAll) {
            ApprovalAction.onRunApproveJob(ApprovalStatus.Approved, queryDefintion, () => {
                onReload();
            });
            return;
        }

        if (!PreInspect(ActionTypes.Approve)) {
            return;
        }

        ApprovalAction.onApprove(Utility.getItemIds(checkedItems), (jobId) => {
            
            if(jobId){
                updateNotificationsRef.current.updateNotificationTimer(jobId, ActionI18Ns.get(ActionTypes.Approve));
                return;
            }

            onReload();
        });
    };

    const onReassign = () => {

        if (!PreInspect(ActionTypes.Reassign)) {
            return;
        }

        reassignRef.current.onShow(Utility.getItemIds(checkedItems));
    };

    const openReclassifyPanel = async() => {
        if(!isCheckedAll){
            
            if(!PreInspect(ActionTypes.Reclassfy)){
                return; 
            }
           
            if(!(await getIsSameSecurityGroupItems())){
                showToast.error(RMResx.RM_JS_BCM_Reclassify_Message);
                return;
            }
        }
        
        reclassifyPanelRef.current.onShow();
    };

    const reclassifyCallback = (jobId) => {

        if(jobId){
            updateNotificationsRef.current.updateNotificationTimer(jobId, ActionI18Ns.get(ActionTypes.Reclassfy));
            return;
        }
        onReload();
    };

    const showActions = () => {
        let isSelectItem = checkedItems.length > 0;
        let showActionsConfig = {
            showReassignBtn: false,
            showReclassify: false,
            showApprove: false
        };
        const firstCheckedItem = checkedItems[0] || {};
        const isHideReclassifyButton = isCheckedAll || checkedItems?.some((item) => 
            item.nodeType !== firstCheckedItem.nodeType || item.sourceFlag !== firstCheckedItem.sourceFlag
        )
        if(isSelectItem){
            let isSingleLocal = _.cloneDeep(filterOptions).
                find((item)=> item.FilterOption === FilterOptions.MLWorkspace)
                ?.AttacheValue.length === 1;
            
            showActionsConfig.showReassignBtn = !isCheckedAll;
            showActionsConfig.showReclassify = !isHideReclassifyButton || isCheckedAll && isSingleLocal;
            showActionsConfig.showApprove = true;
        }
        return showActionsConfig;
    };

    const renderActionBtns = () => {
        let {showReassignBtn, showReclassify, showApprove} = showActions();
        return <div className="flex align-center gap-s">
            {
                showApprove && <R.Button 
                    primary={true}
                    classify="theme" 
                    text={RMResx.RM_MA_Approve} 
                    onClick={onApprove}
                />
            }
            {
                showReclassify && <R.Button 
                    icon="fia-reclassify" 
                    text={RMResx.RM_JS_BCM_Explorer_ChangeTerm} 
                    onClick={openReclassifyPanel} 
                />
            }
            {
                showReassignBtn && <R.Button 
                    icon="fia-export-settings" 
                    text={RMResx.RM_MA_Reassign} 
                    onClick={onReassign}
                />
            }
        </div>;
    };

    return <div>
        {renderActionBtns()}
        <ReassignAction 
            ref={reassignRef} 
            callback={onReload} 
        />
        <ReclassifyPanel 
            ref={reclassifyPanelRef} 
            isCheckedAll={isCheckedAll}
            checkedItems={checkedItems}
            queryDefintion={queryDefintion}
            callback={reclassifyCallback}
            displayingPage="recordForReview"
        />
        <UpdateNotifications 
            ref={updateNotificationsRef}
            checkedItems={checkedItems} 
            callback={onReload}
        />
    </div>;
};

export default Actions;