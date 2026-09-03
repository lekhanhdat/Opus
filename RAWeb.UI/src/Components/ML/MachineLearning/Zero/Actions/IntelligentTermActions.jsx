import { useEffect, useState } from "react";

import { Messagebox, ShowResultMsg } from "../../Common";
import { RAMessageType } from "../../Config/Constains";
import { UsageLimitType } from "../Config/Constants";

const Actions = ({ doAction, selectedItems }) => {
    const [isShowEdit, setIsShowEdit] = useState(true);
    const [isShowDelete, setIsShowDelete] = useState(true);

    useEffect(() => {
        isAlllowEdit();
        isAlllowDelete();
    }, [selectedItems]);

    const isAlllowEdit = () => {
        setIsShowEdit(selectedItems.length > 0 && selectedItems.length <= 1); // && !isExsitPendingRemoveItem
    };

    const isAlllowDelete = () => {
        // let isExsitPendingRemoveItem = selectedItems?.some(
        //     (item) => item.Status === IntelligentTermStatusType.WillRemoved
        // );
        setIsShowDelete(selectedItems.length > 0); // && !isExsitPendingRemoveItem
    };

    const handleTermActions = (panelType) => {
        if (panelType) {
            doAction(panelType);
            return;
        }
        onDelete();
    }

    const onCheckPredictionJobRunning = (panelType) => {
        const requestOption = {
            url: "/api/RMMLTermApi/CheckPredictionJobRunning",
            method: "POST",
            data: 1, // Add / update/ delete term
        };
        $$.loading(true);
        fetchUtility(requestOption)
            .then((result) => {
                if (result.MessageType != RAMessageType.Successful) {
                    Messagebox({
                        content: result.ErrorMessage,
                        actionFun: () => {
                            $$.messagedialog(false);
                            onCheckPredictionJobActionFunc(panelType);
                        },
                    });
                } else {
                    if (panelType) {
                        onCheckPredictionJobActionFunc(panelType);
                    } else {
                        onShowDeleteMessagebox();
                    }
                }
            })
            .finally(() => $$.loading(false));
    }

    const onCheckPredictionJobActionFunc = (panelType) => {
        const requestOptionCheck = {
            url: "/api/FeatureUsageLimit/CheckUsageLimit",
            method: "POST",
            data: UsageLimitType.ZeroShot,
        };

        fetchUtility(requestOptionCheck)
            .then((isNotExceed) => {
                if (isNotExceed) {
                    handleTermActions(panelType);
                } else {
                    $$.messagedialog(true, {
                        width: '550px',
                        hideActions: false,
                        title: RMResx.RM_JS_Common_Confirmation,
                        content: RMResx.RM_ML_Zero_CheckUsageLimit_Msg,
                        buttons: [
                            { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: $$.messagedialog(false)}, 
                        ],
                    });
                }
            });
    }

    const onAddTerm = () => {
        onCheckPredictionJobRunning("OPEN_ADD_TERM_PANEL");
    };

    const onEditTerm = () => {
        onCheckPredictionJobRunning("OPEN_EDIT_TERM_PANEL");
    };

    const openDeleteMessagebox = () => {
        onCheckPredictionJobRunning();
    };

    const onShowDeleteMessagebox = async () => {
        await $$.messagedialog(false); // Close the previous message dialog if any
        Messagebox({
            content: RMResx.RM_ML_Delete_TrainingTerms,
            actionFun: onDelete,
        });
        return true;
    }

    const onDelete = async () => {
        const requestOption = {
            url: "/api/RMMLTermApi/DeleteTerms",
            data: selectedItems.map((item) => item.Id),
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        ShowResultMsg(
            result,
            RMResx.RM_ML_Delete_Team_Success,
            RMResx.RM_ML_Delete_Team_Failed
        );
        $$.loading(false);
        doAction();
    };

    return (
        <div className="flex align-center gap-s">
            <R.Button
                primary={true}
                classify="theme"
                text={RMResx.RM_ML_Train_AddTerm}
                onClick={onAddTerm}
            />
            {isShowEdit && (
                <R.Button text={RMResx.RM_ML_Train_EditTerm} icon="fia-edit" onClick={onEditTerm} />
            )}
            {isShowDelete && (
                <R.Button
                    text={RMResx.RM_JS_BCM_Explorer_Button_Cancel}
                    icon="fia-delete"
                    onClick={openDeleteMessagebox}
                />
            )}
        </div>
    );
};

export default Actions;
