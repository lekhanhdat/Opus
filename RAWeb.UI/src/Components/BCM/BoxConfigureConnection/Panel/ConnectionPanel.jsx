import _ from "lodash";
import { useEffect, useImperativeHandle, useRef } from "react";
import { forwardRef, useState } from "react";
import { ActionMode, AuthenticationType, ResponseErrorType, TypeItems } from "../config";
import Input from "../Components/Input";
import { showToast } from "../../../../Utilities/CommonUtil";
import { useStableCallback } from "../../../Common/Hooks";

const DisplaySecretValue = "123456";

const DefaultConnectionInfo = {
    name: "",
    description: "",
    authenticationType: AuthenticationType.User,
    enterpriseId: "",
    clientId: "",
    clientSecret: "",
    emailAddress: "",
    jsonFileName: "",
    jsonFileContent: [],
    code: "",
    isEdit: false,
    redirectUrl: "",
};

const ConnectionPanel = ({ onReload, codeParam }, ref) => {

    const validationRef = useRef(null);

    const importChooseInput = useRef(null);

    const [isShow, setIsShow] = useState(false);

    const [connectionInfo, setConnectionInfo] = useState(_.cloneDeep(DefaultConnectionInfo));

    const [actionMode, setActionMode] = useState(ActionMode.Create);

    const [typeItems, setTypeItems] = useState(_.cloneDeep(TypeItems));

    useImperativeHandle(ref, () => ({
        onShow: (willModifyConnectionInfo) => {
            setIsShow(true);
            if (_.isNil(willModifyConnectionInfo) || _.isEmpty(willModifyConnectionInfo)) {
                setActionMode(ActionMode.Create);
                setConnectionInfo(_.cloneDeep(DefaultConnectionInfo));
                setTypeItems(_.cloneDeep(TypeItems));
            } else {
                const clonedTypeItems = _.cloneDeep(TypeItems);
                clonedTypeItems.forEach(item => {
                    item.checked = item.value === willModifyConnectionInfo.authenticationType;
                });
                setTypeItems(clonedTypeItems);
                setActionMode(ActionMode.Edit);
                setConnectionInfo(_.cloneDeep(willModifyConnectionInfo));
            }
        }
    }));

    useEffect(() => {
        const actionMode = sessionStorage.getItem("actionMode");

        if (actionMode == ActionMode.Edit) {
            setActionMode(actionMode);
        }
    }, [])

    useEffect(() => {
        const handler = async () => {
            if (codeParam) {
                setIsShow(true);
                $$.loading(true);
                await onActionConnection({ ...connectionInfo, code: codeParam, redirectUrl: window.location.href.split("?")[0] });
                window.history.pushState(null, null, window.location.pathname);
                sessionStorage.removeItem("connectionInfo");
                sessionStorage.removeItem("actionMode");
            }
        };
        handler();
    }, [codeParam])

    useEffect(() => {
        const connectionInfo = JSON.parse(sessionStorage.getItem("connectionInfo"));

        if (connectionInfo) {
            setConnectionInfo(connectionInfo);
        }
    }, [])

    const onHide = () => {
        setIsShow(false);
    };

    const onChange = (columnName, value) => {
        const clonedConnectionInfo = _.cloneDeep(connectionInfo);
        clonedConnectionInfo[columnName] = value;
        setConnectionInfo(clonedConnectionInfo);
    };

    const onTypeChange = (args) => {
        const clonedConnectionInfo = _.cloneDeep(connectionInfo);
        clonedConnectionInfo.authenticationType = args.newValue.value;
        setConnectionInfo(clonedConnectionInfo);
    };

    const onBrowseClick = () => {
        importChooseInput.current.value = "";
        importChooseInput.current.click();
    };

    const onChooseFileChange = () => {
        const clonedConnectionInfo = _.cloneDeep(connectionInfo);
        let filePath = importChooseInput.current.value;
        if (!filePath) {
            return;
        }

        let fileInfo = importChooseInput.current.files[0];
        clonedConnectionInfo.jsonFileName = fileInfo.name;

        let encoder = new TextEncoder(); // 默认使用UTF-8编码
        let reader = new FileReader();
        reader.readAsText(fileInfo, "UTF-8");
        reader.onload = function (e) {
            const val = e.target.result;
            const byteArray = encoder.encode(val);
            clonedConnectionInfo.jsonFileContent = [...byteArray];
            setConnectionInfo(clonedConnectionInfo);
        };
    };

    const onActionConnection = useStableCallback(async (clonedConnectionInfo) => {
        const requestOption = {
            url: (actionMode == ActionMode.Edit || clonedConnectionInfo.isEdit) ? "/api/BoxConnection/UpdateConnection" : "/api/BoxConnection/AddConnection",
            data: clonedConnectionInfo
        };

        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.isSuccessful) {
            setIsShow(false);
            showToast.success(RMResx.RM_Box_Register_Connection_Success);
            onReload();
        } else {
            setIsShow(true);
            if (response.responseErrorType === ResponseErrorType.NameExists) {
                showToast.error(RMResx.RM_FS_Register_SameConnectionNameErrorMessage);
            } else if (response.responseErrorType === ResponseErrorType.ValidationError) {
                showToast.error(RMResx.RM_Box_Register_Connection_ValidationError);
            } else if (response.responseErrorType === ResponseErrorType.JsonFileInvalid) {
                showToast.error(RMResx.RM_Box_Register_Connection_JsonFileInvalid);
            } else if (response.responseErrorType === ResponseErrorType.EnterpriseIdExists) {
                showToast.error(RMResx.RM_Box_Register_Connection_Exists_JsonFileType);
            } else if (response.responseErrorType === ResponseErrorType.AuthorizationCodeTimeout) {
                showToast.error(RMResx.RM_Box_Register_Connection_Authorization_Code_Timeout);
            } else {
                showToast.error(response.responseMessage);
            }
        }
    })

    const onSave = useStableCallback(async () => {
        const clonedConnectionInfo = _.cloneDeep(connectionInfo);
        if (!$$.verify(validationRef.current)) {
            return false;
        }

        clonedConnectionInfo.name = clonedConnectionInfo.name.trim();
        clonedConnectionInfo.description = clonedConnectionInfo.description.trim();
        if (clonedConnectionInfo.authenticationType === AuthenticationType.User) {
            clonedConnectionInfo.jsonFileName = "";
            clonedConnectionInfo.jsonFileContent = [];
            clonedConnectionInfo.enterpriseId = clonedConnectionInfo.enterpriseId.trim();
            clonedConnectionInfo.clientId = clonedConnectionInfo.clientId.trim();
            clonedConnectionInfo.clientSecret = clonedConnectionInfo.clientSecret.trim();
        } else {
            clonedConnectionInfo.enterpriseId = clonedConnectionInfo.jsonFileContent ? "" : clonedConnectionInfo.enterpriseId;
            clonedConnectionInfo.clientId = "";
            clonedConnectionInfo.clientSecret = "";
            clonedConnectionInfo.emailAddress = "";
            clonedConnectionInfo.code = "";
            clonedConnectionInfo.redirectUrl = "";
        }
        $$.loading(true);
        sessionStorage.setItem("connectionInfo", JSON.stringify({...clonedConnectionInfo, isEdit: actionMode === ActionMode.Edit}));
        sessionStorage.setItem("actionMode", actionMode)
        if (clonedConnectionInfo.authenticationType == AuthenticationType.User) {
            window.location.replace(`https://account.box.com/api/oauth2/authorize?response_type=code&client_id=${clonedConnectionInfo.clientId}&redirect_uri=${window.location.href}`);
        } else {
            onActionConnection(clonedConnectionInfo);
        }
        return false;
    });

    const verifyJsonFile = (value) => {
        if (!value.endsWith(".json")) {
            return RMResx.RM_Box_Register_Connection_JsonFileType;
        }
        return true;
    };

    const verifyCharacters = (value) => {
        if (value.trim().length > 4000) {
            return RMResx.RM_Box_Register_DesLengthLimit;
        }
        return true;
    };

    return (
        <R.Panel
            id="reco-box-panel"
            header={actionMode === ActionMode.Create ? RMResx.RM_FS_Register_CreateConnection : RMResx.RM_FS_Register_EditConnection}
            status={{ show: isShow }}
            size={660}
            destroy={true}
            onHide={onHide}
        >
            <div className="br" slot="header">
                <span className="reco-box-panel-header">{RMResx.RM_Box_Register_Connection_SubTitle}</span>
            </div>
            <div className="reco-box-content">
                <R.Validation>
                    <div ref={validationRef}>
                        <Input
                            name={RMResx.RM_FS_Register_ConnectionName}
                            value={connectionInfo.name}
                            type="text"
                            isEmail={false}
                            isTooLongValidation={true}
                            onChange={value => onChange("name", value)}
                        />
                        <div className="reco-box-input">
                            <div className="reco-box-input-label">{RMResx.RM_FS_Register_Description}</div>
                            <R.Validation
                                element="Input"
                                rules={{
                                    customVerify: verifyCharacters,
                                }}
                            >
                                <R.Input
                                    id="raBoxConfigIpt"
                                    type="textarea"
                                    width={"100%"}
                                    value={connectionInfo.description}
                                    onChange={value => onChange("description", value)}
                                    aria={{ ariaLabel: RMResx.RM_FS_Register_Description }}
                                />
                            </R.Validation>
                        </div>
                        <div className="reco-box-information-title">
                            {RMResx.RM_Box_Register_Connection_InformationTitle}
                        </div>
                        <div className="reco-box-information-section">
                            <div className="reco-box-input">
                                <div id="ariaType" className="reco-box-input-label require">{RMResx.RM_Box_Register_Connection_TypeTitle}</div>
                                <R.Combobox
                                    id="raTypeCom"
                                    width="100%"
                                    searchable={false}
                                    items={typeItems}
                                    textField="name"
                                    valueField="value"
                                    tooltipField="name"
                                    checkedField="checked"
                                    onChange={onTypeChange}
                                    aria="#ariaType"
                                />
                            </div>
                            {connectionInfo.authenticationType === AuthenticationType.User && <div>
                                <Input
                                    name={RMResx.RM_Box_Register_Connection_EnterpriseId}
                                    value={connectionInfo.enterpriseId}
                                    type="text"
                                    isEmail={false}
                                    isTooLongValidation={false}
                                    onChange={value => onChange("enterpriseId", value)}
                                />
                                <Input
                                    name={RMResx.RM_Box_Register_Connection_ClientId}
                                    value={connectionInfo.clientId}
                                    type="text"
                                    isEmail={false}
                                    isTooLongValidation={false}
                                    onChange={value => onChange("clientId", value)}
                                />
                                <Input
                                    name={RMResx.RM_Box_Register_Connection_ClientSecret}
                                    value={actionMode === ActionMode.Create ? connectionInfo.clientSecret : ""}
                                    type="password"
                                    isEmail={false}
                                    isTooLongValidation={false}
                                    onChange={value => onChange("clientSecret", value)}
                                />
                                <Input
                                    name={RMResx.RM_Box_Register_Connection_EmailAddress}
                                    value={connectionInfo.emailAddress}
                                    type="text"
                                    isEmail={true}
                                    isTooLongValidation={false}
                                    onChange={value => onChange("emailAddress", value)}
                                />
                            </div>}
                            {connectionInfo.authenticationType === AuthenticationType.Server && <div className="reco-box-input">
                                <div className="reco-box-input-label require">{RMResx.RM_Box_Register_Connection_JsonFile}</div>
                                <div className="ra-box-file">
                                    <div className="ra-box-file-input">
                                        <R.Validation
                                            element="Input"
                                            require={RMResx.RM_Box_Register_Connection_JsonFileEmpty}
                                            rules={{ customVerify: verifyJsonFile }}
                                        >
                                            <R.Input
                                                id="raJsonFileIpt"
                                                type="text"
                                                value={connectionInfo.jsonFileName}
                                                readonly={true}
                                                aria={{ ariaLabel: RMResx.RM_Box_Register_Connection_JsonFile }}
                                            />
                                        </R.Validation>
                                    </div>
                                    <R.Button
                                        id="raBoxBrowseBtn"
                                        text={RMResx["Gui.Common_a2a87c22-cacd-449b-a6a0-9c0b46f86887"]}
                                        onClick={onBrowseClick}
                                    />
                                </div>
                            </div>}
                        </div>
                    </div>
                </R.Validation>
                <input type="file" id="choosefile" name="fileUp"
                    style={{ display: "none" }} ref={importChooseInput}
                    onChange={onChooseFileChange} />
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSave} />
            </>
        </R.Panel>
    );
};

export default forwardRef(ConnectionPanel);