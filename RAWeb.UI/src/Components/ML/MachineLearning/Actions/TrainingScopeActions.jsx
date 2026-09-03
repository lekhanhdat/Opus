import { useEffect, useState } from "react";
import { RAMessageType, TrainingMode } from "../Config/Constains";
import { Messagebox, ShowResultMsg } from "../Common";
import { showToast } from "../../../../Utilities/CommonUtil";

function TrainingScopeActions({ doAction, trainingScopeInfo, selectedItems }) {
    const [isShowDelete, setIsShowDelete] = useState(false);

    useEffect(() => {
        setIsShowDelete(selectedItems.length > 0);
    }, [selectedItems]);

    const openDeleteMessagebox = () => {
        Messagebox({ content: RMResx.RM_ML_Delete_TrainingScopes, actionFun: onDelete });
    };

    const onDelete = async () => {
        const requestOption = {
            url: "/api/TrainingScopeApi/DeleteTrainingScopeManually",
            method: "POST",
            data: selectedItems.map((item) => ({ Id: item.Id, TermId: item.TermId, FileName: item.FileName })),
        };
        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result.MessageType === RAMessageType.Successful) {
            ShowResultMsg(result, RMResx.RM_ML_Delete_Scope_Success, RMResx.RM_ML_Delete_Scope_Scope);
        } else {
            showToast.error(result.ErrorMessage);
        }
        doAction();
    };

    return (
        <div className="flex align-center gap-s">
            <R.Button
                primary={true}
                classify="theme"
                text={RMResx.RM_ML_TrainingScope_ManageBtn}
                onClick={() => doAction("OPEN_MANAGE_SCOPE_PANEL")}
            />
            {trainingScopeInfo.trainingScopeOption === TrainingMode.Manual && (
                <R.Button
                    icon="fia-plus"
                    text={RMResx.RM_ML_TrainingScope_AddBtn}
                    onClick={() => doAction("OPEN_ADD_SCOPE_PANEL")}
                />
            )}
            {isShowDelete && (
                <R.Button
                    icon="fia-delete"
                    text={RMResx.RM_ML_TrainingScope_DeleteBtn}
                    onClick={openDeleteMessagebox}
                />
            )}
        </div>
    );
}

export default TrainingScopeActions;
