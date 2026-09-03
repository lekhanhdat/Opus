import React, { useState, useEffect} from "react";
import { showToast } from "../../../Utilities/CommonUtil";
import { PickListForDestroyStatusType } from "../../../Constants/Constants";
import { NodeLevel } from "../../../Constants/DAEnums";
import { Messagebox, ActionSuccessfulNeedJobToast } from "../PickListCommon/PickListCommonComponent";
import "../PickListCommon/Index.less";

const ContainerLevel = [NodeLevel.PhyBox];

const Action = ({ isSelectAll, selectedItems, callback, filterOptions, searchText, limitNumberForAction }) =>{
    
    const [showActionBtns, setShowActionBtns] = useState({ showCompleteBtn: false});

    const [isContainerLevel, setIsContainerLevel] = useState(false);

    useEffect( () => {
        getAllowShowActionBtns();
        setIsContainerLevelFun();
    }, [isSelectAll, selectedItems]);

    const showLimitNumberMessagebox = () =>{
        let isMoreThanlimitNumber = selectedItems.length > limitNumberForAction;
        if(isMoreThanlimitNumber){
            showToast.warn(RMResx.RM_RDM_MA_Msg_CheckMoreThanActionLimitCount);
        }
        return isMoreThanlimitNumber;
    };

    const setIsContainerLevelFun = () =>{
        let isContainerLevel = selectedItems.some(item => ContainerLevel.includes(item.NodeType));
        setIsContainerLevel(isContainerLevel);
    };

    const onComplete = () =>{
        let messageboxContent = RMResx.RM_MT_PickList_CompleteDestroyActionMsg;
        if(!isSelectAll){
            if(showLimitNumberMessagebox()){ return; }
            if(isContainerLevel){
                messageboxContent = RMResx.RM_MT_PickListContainerLevelTip; 
            }
            Messagebox({ content: messageboxContent, actionFun: onCompleteForRequest });
        }else{
            Messagebox({ content: messageboxContent, actionFun: onCompleteForRequest });
        }
    };

    const onCompleteForRequest = () =>{
        $$.loading(true);
        let option = {
            url: "/api/PickListApi/DestructionCompelte",
            method: "Post",
            data: getActionParam()
        };
        fetchUtility(option).then((res) => { 
            $$.loading(false); 
            if(isSelectAll || isContainerLevel){
                ActionSuccessfulNeedJobToast();
            }else{
                showToast.success(RMResx.RM_MT_PickList_CompleteDestroySuccessTip); 
            }
            callback();
        }).catch((e) => {
            showToast.error(RMResx.RM_CP_AM_Certificate_OperationFailed_Tip);
            $$.loading(false);
        });
    };

    const getActionParam = () =>{
        let selectedItemIds = selectedItems.map((item)=>{ return  item.Id; });
        let actionParam = { 
            IsSelectAll: isSelectAll, 
            SelectedItemIds: selectedItemIds, 
            IsContainerLevel: isContainerLevel
        }; 
        if(isSelectAll){
            actionParam = {
                FilterOptions: filterOptions,
                SearchText: searchText,
                IsSelectAll: isSelectAll,
            };
        }
        return actionParam;
    };

    const getAllowShowActionBtns = () =>{
        allowShowCompleteBtn();
    };

    const allowShowCompleteBtn = () =>{
        let allSelectItemsStatusIsPedding= false;
        if(selectedItems.length > 0){
            allSelectItemsStatusIsPedding = selectedItems.every((item)=>{ 
                return item.Status == PickListForDestroyStatusType.Pendding;
            });
        }
        showActionBtns.showCompleteBtn = allSelectItemsStatusIsPedding || isSelectAll;
        setShowActionBtns(RM.deepcopy(showActionBtns));
    };

    const getActions = () =>{
        let {showCompleteBtn} = showActionBtns;
        return <React.Fragment>
            {
                showCompleteBtn && 
                <R.Button 
                    icon="fia-status-successful"
                    text={RMResx.RM_MT_PickList_CompleteDestroyBtn} 
                    onClick={onComplete}/>
            }
        </React.Fragment>;
    };

    return <React.Fragment>
        {getActions()}
    </React.Fragment>;
};

export default Action;
  