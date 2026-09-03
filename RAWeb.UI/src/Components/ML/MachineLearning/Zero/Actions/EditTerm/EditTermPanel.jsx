import { useState, forwardRef, useImperativeHandle } from "react";

import { ShowResultMsg } from "../../../Common";

const EditTermPanel = ({ doAction }, ref) => {
    const [showEditTermPanel, setShowEditTermPanel] = useState(false);

    const [termData, setTermData] = useState(null);

    useImperativeHandle(ref, () => ({
        openEditTermPanel: (data) => {
            setShowEditTermPanel(true);
            setTermData(data[0]);
        },
    }));

    const onCloseEditTermPanel = () => {
        setShowEditTermPanel(false);
        setTermData(null);
    };

    const onEditTerm = async () => {
        if (!$$.verify("allValidation")) {
            return false;
        }
        const requestOption = {
            url: `/api/RMMLTermApi/UpdateDescription`,
            method: "POST",
            data: {
                Id: termData.Id,
                Description: termData.Description,
                Name: termData.Name,
            },
        };
        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        const hasError = result.HasError;
        if (!hasError) {
            setShowEditTermPanel(false);
            doAction("EDIT_TERM");
        }
        ShowResultMsg(
            result,
            RMResx.RM_ML_EditTerm_Success_Tip,
            RMResx.RM_ML_EditTerm_Failed_Tip
        );
        return !hasError;
    };

    const verifyLength = (value) => {
        if (value && value.length > 5000) {
            return RMResx.RM_ML_Train_EditTerm_DescriptionValidate;
        }
        return true;
    }

    return (
        <div>
            <R.Panel
                id="raMtEditTermPanel"
                header={RMResx.RM_ML_Train_EditTerm_PanelTitle}
                size={664}
                status={{ show: showEditTermPanel }}
                onHide={onCloseEditTermPanel}
                destroy={true}
            >
                <R.Validation>
                    <div id="allValidation">
                        <div className="margin-bottom-l">
                            <div style={{ marginBottom: 4 }} className="ra-form-label">
                                <div tabIndex={0} className="input-label">
                                    {RMResx.RM_ML_Train_EditTerm_Name}
                                </div>
                            </div>
                            <div tabIndex={0}>
                                {termData?.Name}
                            </div>
                        </div>
                        <div className="margin-bottom-l">
                            <div className="ra-form-label">
                                <div className="require input-label">
                                    {RMResx.RM_ML_Train_EditTerm_Description}
                                </div>
                            </div>
                            <R.Validation
                                element="Input"
                                require
                                rules={{
                                    verifyLength,
                                }}
                            >
                                <R.Input
                                    id="raMtTermDescIpt"
                                    type="textarea"
                                    height={264}
                                    resize="vertical"
                                    value={termData?.Description}
                                    aria={{
                                        ariaLabel:
                                            RMResx.RM_ML_Train_EditTerm_Description,
                                        "aria-required": true,
                                    }}
                                    onChange={(value) =>
                                        setTermData((prev) => ({
                                            ...prev,
                                            Description: value,
                                        }))
                                    }
                                />
                            </R.Validation>
                        </div>
                    </div>
                </R.Validation>
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={onCloseEditTermPanel}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={onEditTerm}
                    />
                </>
            </R.Panel>
        </div>
    );
};

export default forwardRef(EditTermPanel);
