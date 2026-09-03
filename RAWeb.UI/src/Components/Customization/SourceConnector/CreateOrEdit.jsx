import React, { useEffect, useState } from "react";
import _ from "lodash";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { InputItem, InputItemContainer } from "../../Common/InputItem/index";
import { ColumnsView } from "./Columns/index";
import { ActionMode, ActionStatus, ColumnType, Constant, CustomizeConnectorOrigin, CustomizeConnectorScope, ValidateMode } from "./Common/Constants";
import RouterUrls from "../../../Constants/RouterUrls";
import { MessageBox } from "./Common/MessageBox";
import { showToast } from "../../../Utilities/CommonUtil";
import { Prompt } from 'react-router';

const DefaultConnectorInfo = {
    name: "",
    description: "",
    columnInfoes: [
        {
            id: "20fcf752-cc89-4081-fc59-c1b8c6ab3475",
            name: RMResx.RM_Connecor_RowKey,
            internalName: "rowKey",
            isRequired: true,
            isHidden: true,
            order: -1,
            type: ColumnType.SingleText,
            origin: CustomizeConnectorOrigin.BuildIn,
            scope: CustomizeConnectorScope.Global
        },
        {
            id: "de5e99cb-4fb4-4e25-b732-a1dce71dd048",
            name: RMResx.RM_PRM_PRE_MRR_Column_NameOrTitle,
            internalName: "leafName",
            isRequired: true,
            isHidden: false,
            order: 1,
            type: ColumnType.SingleText,
            origin: CustomizeConnectorOrigin.BuildIn,
            scope: CustomizeConnectorScope.Global
        },
        {
            id: "1339e256-9010-cfb2-5a50-bf2d2d00d461",
            name: RMResx.RM_PRM_PRE_Column_DisposalClass,
            internalName: "termFullPath",
            isRequired: false,
            isHidden: false,
            order: 2,
            type: ColumnType.Taxonomy,
            origin: CustomizeConnectorOrigin.BuildIn,
            scope: CustomizeConnectorScope.Global
        },
        {
            id: "c55a2cc4-2825-42ff-b1d4-fb72b7be7dc5",
            name: RMResx.RM_JS_RDM_Explorer_CreateTime,
            internalName: "timeCreated",
            isRequired: true,
            isHidden: false,
            order: 3,
            type: ColumnType.DateTime,
            origin: CustomizeConnectorOrigin.BuildIn,
            scope: CustomizeConnectorScope.Global
        },
        {
            id: "3ec9a488-90fa-4d62-835f-0df0cd2e9f97",
            name: RMResx.RM_PRM_PRE_Column_ModifiedTime,
            internalName: "timeModified",
            isRequired: true,
            isHidden: false,
            order: 4,
            type: ColumnType.DateTime,
            origin: CustomizeConnectorOrigin.BuildIn,
            scope: CustomizeConnectorScope.Global
        },
        {
            id: "91a08d45-c5dd-43da-b6c4-670f11ac273e",
            name: RMResx.RM_PRM_PRE_Column_Creator,
            internalName: "createBy",
            isRequired: false,
            isHidden: false,
            order: 5,
            type: ColumnType.SingleText,
            origin: CustomizeConnectorOrigin.BuildIn,
            scope: CustomizeConnectorScope.Global
        },
        {
            id: "1f2e8c3f-e49a-473c-bd16-8647258cf15c",
            name: RMResx.RM_PRM_PRE_Column_Modifier,
            internalName: "modifiedBy",
            isRequired: false,
            isHidden: false,
            order: 6,
            type: ColumnType.SingleText,
            origin: CustomizeConnectorOrigin.BuildIn,
            scope: CustomizeConnectorScope.Global
        },
    ]
};

const CreateOrEdit = ({ history }) => {

    const [actionMode, setActionMode] = useState(ActionMode.CREATE);

    const [validateMode, setValidateMode] = useState(ValidateMode.None);

    const [connectorInfo, setConnectorInfo] = useState(DefaultConnectorInfo);

    const [hasChange, setHasChange] = useState(false);

    const [beforeColumnInfoes, setBeforeColumnInfoes] = useState([]);

    useEffect(() => {
        const modeStr = RM.Url.getParam(window.location.href, Constant.ActionMode);
        const mode = parseInt(modeStr);
        setActionMode(mode);
        if (mode === ActionMode.EDIT) {
            onLoadConnectorInfo();
        }
    }, []);

    const onValueChange = (name, value) => {
        const clonedConnectorInfo = _.cloneDeep(connectorInfo);
        clonedConnectorInfo[name] = value;
        setConnectorInfo(clonedConnectorInfo);
        setHasChange(true);
        if (name === "name") {
            setValidateMode(ValidateMode.None);
        }
    };

    const onLoadConnectorInfo = async () => {
        const id = RM.Url.getParam(window.location.href, Constant.EditItem);
        $$.loading(true);
        const requestOption = {
            url: "/api/Connector/Get",
            data: id
        };
        const res = await fetchUtility(requestOption);
        setConnectorInfo(res);
        setBeforeColumnInfoes(res.columnInfoes);
        $$.loading(false);
    };

    const checkColumnsHasDelete = () => {
        for(const item of beforeColumnInfoes) {
            if(connectorInfo.columnInfoes.findIndex(j => j.id === item.id) === -1) {
                return true;
            }
        }

        return false;
    };

    const onSave = async () => {
        setValidateMode(ValidateMode.Static);
        if (_.isEmpty(connectorInfo.name)) {
            return false;
        }

        switch (actionMode) {
            case ActionMode.CREATE:
                await onCreate();
                break;
            case ActionMode.EDIT:
                await onEdit();
                break;
        }

        return true;
    };

    const onCreate = async () => {
        $$.loading(true);
        const requestOptions = {
            url: "/api/Connector/Add",
            data: connectorInfo
        };
        const actionResult = await fetchUtility(requestOptions);
        $$.loading(false);
        if (actionResult.actionStatus === ActionStatus.Succeed) {
            setHasChange(false);
            showToast.success(RMResx.RM_Connector_Create_SuccessMsg);
            history.push({
                pathname: RouterUrls.Connector
            });
        }
        else if (actionResult.actionStatus === ActionStatus.Repeat) {
            setValidateMode(ValidateMode.Request);
        }
    };

    const onEdit = () => {

        const onInternalEdit = async () => {
            $$.loading(true);
            const requestOptions = {
                url: "/api/Connector/Update",
                data: connectorInfo
            };
    
            const actionResult = await fetchUtility(requestOptions);
            $$.loading(false);
            if (actionResult.actionStatus === ActionStatus.Succeed) {
                setHasChange(false);
                showToast.success(RMResx.RM_Connector_Edit_SuccessMsg);
                history.push({
                    pathname: RouterUrls.Connector
                });
            }
            else if (actionResult.actionStatus === ActionStatus.Repeat) {
                setValidateMode(ValidateMode.Request);
            }
        };

        if(checkColumnsHasDelete()) {
            MessageBox.show(
                RMResx.RM_Connector_Edit_DeleteColumnMsg,
                () => {
                    onInternalEdit();
                }
            );
            return;
        }

        onInternalEdit();
    };

    const onCancel = () => {
        history.push({ pathname: RouterUrls.Connector });
    };

    return (
        <div className="reco-source-connector-create-or-edit">
            <section className="header">
                <Prompt message={RMResx.RM_JS_RC_TUR_CancelMessage} when={hasChange} />
                <$g.SiteMap
                    data={[SiteMapLinks.Connector, { text: actionMode === ActionMode.EDIT ? RMResx.RM_JS_Common_Edit : RMResx.RM_JS_Common_Create }]} />
            </section>
            <section className="container">
                <InputItem
                    name={RMResx.RM_Connector_ConnectorName}
                    value={connectorInfo.name}
                    type="text"
                    width={645}
                    height={34}
                    require={true}
                    message={validateMode === ValidateMode.Static ? RMResx.RM_FS_Register_NameInputValidateMessage : RMResx.RM_Connector_ConnectorNameDuplicate}
                    isShowMessage={((validateMode === ValidateMode.Static && _.isEmpty(connectorInfo.name)) || (validateMode == ValidateMode.Request))}
                    onChange={value => onValueChange("name", value)}
                />
                <InputItem
                    name={RMResx.RM_Connector_Description}
                    value={connectorInfo.description}
                    type="textarea"
                    width={645}
                    height={100}
                    require={false}
                    onChange={value => onValueChange("description", value)}
                />
                <InputItemContainer
                    name={RMResx.RM_Connector_Template}
                    require={true}
                >
                    <ColumnsView
                        columnInfoes={connectorInfo.columnInfoes}
                        onChange={value => onValueChange("columnInfoes", value)}
                    />
                </InputItemContainer>
            </section>
            <section className="placeholder">
            </section>
            <section className="actions gap-s">
                <R.Button
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={onCancel} />
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={onSave} />
            </section>
        </div>
    );
};

export default CreateOrEdit;