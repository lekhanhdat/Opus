import React, { useState, useImperativeHandle, forwardRef } from "react";
import _ from "lodash";
import InputItem from "../Components/InputItem";
import { useStableCallback } from "../Hooks/index";
import { ActionMode, Columns, ErrorType, ValidateStatus } from "../Constants";

const GetUpsertRequestOption = (connectionInfo) => ({
    url: "/api/AzureFileShareConnection/UpsertConnection",
    data: connectionInfo
});

const GetValidateRequestOption = (connectionInfo) => ({
    url: "/api/AzureFileShareConnection/ValidateConnectionInfo",
    data: connectionInfo
});

const DefaultConnectionInfo = {
    name: "",
    description: "",
    accessEndPoint: "",
    fileShareName: "",
    accountName: "",
    accountKey: "",
};

const DefaultValidateInfo = {
    name: false,
    accessEndPoint: false,
    fileShareName: false,
    accountName: false,
    accountKey: false
};

const ConnectionPanel = ({ onReload }, ref) => {

    const [show, setShow] = useState(false);

    const [connectionInfo, setConnectionInfo] = useState(_.cloneDeep(DefaultConnectionInfo));

    const [validateInfo, setValidateInfo] = useState(_.cloneDeep(DefaultValidateInfo));

    const [actionMode, setActionMode] = useState(ActionMode.Create);

    const [errorMessage, setErrorMessage] = useState("");

    const [errorType, setErrorType] = useState(ErrorType.None);

    const [validateStatus, setValidateStatus] = useState(ValidateStatus.None);

    useImperativeHandle(ref, () => ({
        onShow: (willModifyConnectionInfo) => {
            setShow(true);
            setValidateInfo(_.cloneDeep(DefaultValidateInfo));
            setValidateStatus(ValidateStatus.None);
            setErrorType(ErrorType.None);
            if (_.isNil(willModifyConnectionInfo) || _.isEmpty(willModifyConnectionInfo)) {
                setConnectionInfo(_.cloneDeep(DefaultConnectionInfo));
                setActionMode(ActionMode.Create);
                return;
            }
            setActionMode(ActionMode.Edit);
            setConnectionInfo(_.cloneDeep(willModifyConnectionInfo));
        }
    }));

    const onHide = () => {
        setShow(false);
    };

    const onSave = useStableCallback(async () => {
        setErrorMessage("");
        const clonedConnectionInfo = _.cloneDeep(connectionInfo);
        if (!validate(clonedConnectionInfo)) {
            resetValidateInfo(clonedConnectionInfo, true);
            return false;
        }

        if (clonedConnectionInfo.name.trim().length > 255) {
            setErrorType(ErrorType.TooLongName);
            setErrorMessage(RMResx.RM_JS_Common_Msg_CannotExceed255);
            return false;
        }

        $$.loading(true);
        const requestOption = GetUpsertRequestOption(clonedConnectionInfo);
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.isSucceed) {
            setShow(false);
            onReload();
        }
        else {
            setErrorType(response.responseErrorType);
            if (response.responseErrorType === ErrorType.RepeatName) {
                setErrorMessage(RMResx.RM_FS_Register_SameConnectionNameErrorMessage);
            }
            if (response.responseErrorType === ErrorType.ValidateError) {
                setValidateStatus(ValidateStatus.Failed);
            }
            if (response.responseErrorType !== ErrorType.None && response.responseErrorType !== ErrorType.InternalError) {
                return false;
            }
            setShow(false);
        }
    });

    const onValidationTest = async () => {
        const clonedConnectionInfo = _.cloneDeep(connectionInfo);
        if (!validate(clonedConnectionInfo)) {
            resetValidateInfo(clonedConnectionInfo, true);
            return;
        }

        $$.loading(true);
        const requestOption = GetValidateRequestOption(connectionInfo);
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result) {
            setValidateStatus(ValidateStatus.Succeed);
        }
        else {
            setValidateStatus(ValidateStatus.Failed);
        }
    };

    const onChange = (columnName, value) => {
        const clonedConnectionInfo = _.cloneDeep(connectionInfo);
        clonedConnectionInfo[columnName] = value;
        setConnectionInfo(clonedConnectionInfo);
        resetValidateInfo(clonedConnectionInfo, false);
    };

    const validate = (clonedConnectionInfo) => {
        for (const key of ["name", "accessEndPoint", "fileShareName", "accountName", "accountKey"]) {
            const value = clonedConnectionInfo[key];
            if (_.isNil(value) || _.isEmpty(value)) {
                return false;
            }
        }

        return true;
    };

    const resetValidateInfo = (clonedConnectionInfo, needShowValidateMessage) => {
        const clonedValidateInfo = _.cloneDeep(validateInfo);
        const keys = Object.keys(clonedValidateInfo);
        keys.forEach(key => {
            if (!_.isNil(clonedConnectionInfo[key]) && !_.isEmpty(clonedConnectionInfo[key])) {
                clonedValidateInfo[key] = false;
                return;
            }

            if (needShowValidateMessage) {
                clonedValidateInfo[key] = true;
            }
        });
        setValidateInfo(clonedValidateInfo);
    };

    return (
        <R.Panel
            id="reco-az-panel"
            header={actionMode === ActionMode.Create ? RMResx.RM_FS_Register_CreateConnection : RMResx.RM_FS_Register_EditConnection}
            size={660}
            status={{ show: show }}
            onHide={onHide}
            destroy={false}
        >
            <div className="br" slot="header">
                <span className="reco-az-panel-header">{RMResx.RM_AZFS_Register_Connection_SubTitle}</span>
            </div>
            <div>
                <div style={{ marginBottom: "24px" }} hidden={!errorMessage}>
                    <R.Messagebar
                        message={errorMessage}
                        classify={"error"}
                        onClose={e => {
                            setErrorMessage("");
                            setErrorType(ErrorType.None);
                        }}
                        status={{ show: errorType === ErrorType.RepeatName || errorType === ErrorType.TooLongName }}
                    />
                </div>
                <InputItem
                    name={Columns.ConnectionName}
                    value={connectionInfo.name}
                    type="text"
                    height={34}
                    require={true}
                    message={RMResx.RM_FS_Register_NameInputValidateMessage}
                    isShowMessage={validateInfo.name}
                    onChange={value => onChange("name", value)}
                />
                <InputItem
                    name={Columns.Description}
                    value={connectionInfo.description}
                    type="textarea"
                    height={100}
                    require={false}
                    onChange={value => onChange("description", value)}
                />
                <div className="reco-az-input-highlight">
                    {RMResx.RM_AZFS_Register_FileStorage}
                </div>
                <div className="reco-az-highlight-section">
                    <InputItem
                        name={Columns.AccessEndPoint}
                        value={connectionInfo.accessEndPoint}
                        type="text"
                        height={34}
                        require={true}
                        message={RMResx.RM_FS_Register_NameInputValidateMessage}
                        isShowMessage={validateInfo.accessEndPoint}
                        onChange={value => onChange("accessEndPoint", value)}
                        placeholder={"https://storage-account-name.file.core.windows.net"}
                    />
                    <InputItem
                        name={Columns.ShareName}
                        value={connectionInfo.fileShareName}
                        type="text"
                        height={34}
                        require={true}
                        message={RMResx.RM_FS_Register_NameInputValidateMessage}
                        isShowMessage={validateInfo.fileShareName}
                        onChange={value => onChange("fileShareName", value)}
                    />
                    <InputItem
                        name={Columns.AccountName}
                        value={connectionInfo.accountName}
                        type="text"
                        height={34}
                        require={true}
                        message={RMResx.RM_FS_Register_NameInputValidateMessage}
                        isShowMessage={validateInfo.accountName}
                        onChange={value => onChange("accountName", value)}
                    />
                    <InputItem
                        name={Columns.AccountKey}
                        value={connectionInfo.accountKey}
                        type="password"
                        height={34}
                        require={true}
                        message={RMResx.RM_FS_Register_NameInputValidateMessage}
                        isShowMessage={validateInfo.accountKey}
                        onChange={value => onChange("accountKey", value)}
                    />
                    <div className="reco-az-validate-btn">
                        <R.Button
                            text={RMResx.RM_FS_Register_ValidationTest}
                            onClick={onValidationTest}
                        />
                    </div>
                </div>
                <div className="reco-az-validate-message" hidden={validateStatus === ValidateStatus.None}>
                    <div className={validateStatus === ValidateStatus.Succeed  ? "succeed" : "failed"}>
                        {
                            validateStatus === ValidateStatus.Succeed ?
                                RMResx.RM_AZFS_Register_Validate_Succeed
                                :
                                RMResx.RM_AZFS_Register_Validate_Failed
                        }
                    </div>
                </div>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSave} />
            </>
        </R.Panel>
    );
};

export default forwardRef(ConnectionPanel);