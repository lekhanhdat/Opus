import React, { useEffect, useState } from "react";
import {
    Messagebox,
    ShowResultMsg,
    ActionSuccessfulNeedJobToast,
} from "../Common";
import { showToast } from "../../../../Utilities/CommonUtil";
import { IntelligentTermStatusType } from "../Config/Constains";

const Actions = ({ doAction, selectedItems }) => {
    const [isShowDelete, setIsShowDelete] = useState(true);

    useEffect(() => {
        isAlllowDelete();
    }, [selectedItems]);

    const isAlllowDelete = () => {
        let isExsitPendingRemoveItem = selectedItems?.some(
            (item) => item.Status === IntelligentTermStatusType.WillRemoved
        );
        setIsShowDelete(selectedItems.length > 0 && !isExsitPendingRemoveItem);
    };

    const onAddTerm = () => {
        doAction("OPEN_ADD_TERM_PANEL");
    };

    const onRefresh = () => {
        doAction("REFRESH_ACTION");
    };

    const openTrainMessagebox = () => {
        Messagebox({ content: RMResx.RM_ML_Msg_WillTrainTerms, actionFun: onTrain });
    };

    const onTrain = async () => {
        const requestOption = {
            url: "/api/RMMLTermApi/StartMLJob",
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result.MessageType == 0) {
            ActionSuccessfulNeedJobToast();
        } else {
            showToast.error(result.ErrorMessage || RMResx.RM_ML_Train_Failed);
        }
        doAction();
    };

    const openDeleteMessagebox = () => {
        Messagebox({ content: RMResx.RM_ML_Delete_TrainingTerms, actionFun: onDelete });
    };
    
    const onDelete = async () => {
        const requestOption = {
            url: "/api/RMMLTermApi/DeleteTerms",
            data: selectedItems.map((item) => item.Id),
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        ShowResultMsg(result, RMResx.RM_ML_Delete_Team_Success, RMResx.RM_ML_Delete_Team_Failed);
        $$.loading(false);
        doAction();
    };

    return (
        <div className="flex align-center gap-s">
            <R.Button primary={true} classify="theme" text={RMResx.RM_ML_Train_AddTerm} onClick={onAddTerm} />
            <R.Button id="raStartTrainingBtn" icon="fia-train" text={RMResx.RM_ML_Train_TrainBtn} onClick={openTrainMessagebox} />
            {isShowDelete && (
                <R.Button
                    text={RMResx.RM_JS_BCM_Explorer_Button_Cancel}
                    icon="fia-delete"
                    onClick={openDeleteMessagebox}
                />
            )}
            <R.Button icon="fia-refresh" text={RMResx.RM_JS_JM_Refresh_Btn} onClick={onRefresh} />
        </div>
    );
};

export default Actions;
