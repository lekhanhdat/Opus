import React, { useState, useImperativeHandle, forwardRef, Fragment, useRef } from "react";
import _ from "lodash";
import InputItem from "../Components/InputItem";
import { useStableCallback } from "../Hooks/index";
import ConnectionAddPanel from "./ConnectionAddPanel";
import ConnectionSimpleTable from "../Tables/ConnectionSimpleTable";
import { ActionMode, Columns, ErrorType } from "../Constants/index";

const GetUpsertRequestOption = (connectionInfo) => ({
    url: "/api/AzureFileShareConnection/UpsertGroup",
    data: connectionInfo
});

const GetAvailableConnectionsRequestOption = () => ({
    url: "/api/AzureFileShareConnection/GetConnectionsWithoutRelatedGroup",
});

const DefaultConnectionGroupInfo = {
    name: "",
    description: "",
    connections: [],
};

const DefaultValidateInfo = {
    name: false,
};

const ConnectionGroupPanel = ({ onReload }, ref) => {

    const connectionAddRef = useRef();

    const [show, setShow] = useState(false);

    const [connectionGroupInfo, setConnectionGroupInfo] = useState(_.cloneDeep(DefaultConnectionGroupInfo));

    const [validateInfo, setValidateInfo] = useState(_.cloneDeep(DefaultValidateInfo));

    const [availableConnections, setAvailableConnections] = useState([]);

    const [checkedConnections, setCheckedConnections] = useState([]);

    const [actionMode, setActionMode] = useState(ActionMode.Create);

    const [errorMessage, setErrorMessage] = useState("");

    const [errorType, setErrorType] = useState(ErrorType.None);

    useImperativeHandle(ref, () => ({
        onShow: (willModifyConnectionGroupInfo) => {
            setShow(true);
            setCheckedConnections([]);
            setValidateInfo(_.cloneDeep(DefaultValidateInfo));
            setErrorType(ErrorType.None);
            initAvailableConnection();
            if (_.isNil(willModifyConnectionGroupInfo) || _.isEmpty(willModifyConnectionGroupInfo)) {
                setActionMode(ActionMode.Create);
                setConnectionGroupInfo(_.cloneDeep(DefaultConnectionGroupInfo));
                return;
            }
            setActionMode(ActionMode.Edit);
            setConnectionGroupInfo(_.cloneDeep(willModifyConnectionGroupInfo));
        }
    }));

    const initAvailableConnection = async () => {
        const requestOption = GetAvailableConnectionsRequestOption();
        const connections = await fetchUtility(requestOption);
        setAvailableConnections(connections);
    };

    const onHide = () => {
        setShow(false);
    };

    const onSave = useStableCallback(async () => {
        setErrorMessage("");
        const clonedConnectionGroupInfo = _.cloneDeep(connectionGroupInfo);
        if (!validate(clonedConnectionGroupInfo)) {
            resetValidateInfo(clonedConnectionGroupInfo, true);
            return false;
        }

        if (clonedConnectionGroupInfo.name.trim().length > 255) {
            setErrorType(ErrorType.TooLongName);
            setErrorMessage(RMResx.RM_JS_Common_Msg_CannotExceed255);
            return false;
        }

        $$.loading(true);
        const requestOption = GetUpsertRequestOption(clonedConnectionGroupInfo);
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if(response.isSucceed) {
            setShow(false);
            onReload();
        }
        else{
            setErrorType(response.responseErrorType);
            if (response.responseErrorType === ErrorType.RepeatName) {
                setErrorMessage(RMResx.RM_FS_Register_SameConnectionNameErrorMessage);
            }
            if(response.responseErrorType !== ErrorType.None && response.responseErrorType !== ErrorType.InternalError) {
                return false;
            }
            setShow(false);
        }
        
    });

    const onChange = (columnName, value) => {
        const clonedConnectionGroupInfo = _.cloneDeep(connectionGroupInfo);
        clonedConnectionGroupInfo[columnName] = value;
        setConnectionGroupInfo(clonedConnectionGroupInfo);
        resetValidateInfo(clonedConnectionGroupInfo, false);
    };

    const validate = (clonedConnectionGroupInfo) => {
        for (const key of ["name"]) {
            const value = clonedConnectionGroupInfo[key];
            if (_.isNil(value) || _.isEmpty(value)) {
                return false;
            }
        }

        return true;
    };

    const resetValidateInfo = (clonedConnectionGroupInfo, needShowValidateMessage) => {
        const clonedValidateInfo = _.cloneDeep(validateInfo);
        const keys = Object.keys(clonedValidateInfo);
        keys.forEach(key => {
            if (!_.isNil(clonedConnectionGroupInfo[key]) && !_.isEmpty(clonedConnectionGroupInfo[key])) {
                clonedValidateInfo[key] = false;
                return;
            }

            if (needShowValidateMessage) {
                clonedValidateInfo[key] = true;
            }
        });
        setValidateInfo(clonedValidateInfo);
    };

    const onConnectionCheckedChange = () => {
        const checkedConnections = connectionGroupInfo.connections.filter(item => item.checked);
        setCheckedConnections(checkedConnections);
    };

    const onConnectionsWillAdded = (willAddedConnections) => {
        const clonedConnectionGroupInfo = _.cloneDeep(connectionGroupInfo);
        clonedConnectionGroupInfo.connections.forEach(item => item.checked = false);
        clonedConnectionGroupInfo.connections = clonedConnectionGroupInfo.connections.concat(willAddedConnections);
        const connections = availableConnections.filter(item => !willAddedConnections.some(i => i.id === item.id));
        setConnectionGroupInfo(clonedConnectionGroupInfo);
        setAvailableConnections(connections);
        setCheckedConnections([]);
    };

    const onConnectionsWillRemoved = () => {
        const clonedConnectionGroupInfo = _.cloneDeep(connectionGroupInfo);
        clonedConnectionGroupInfo.connections.forEach(item => item.checked = false);
        clonedConnectionGroupInfo.connections = clonedConnectionGroupInfo.connections.filter(item => !checkedConnections.some(i => i.id === item.id));
        let clonedAvailableConnections = _.cloneDeep(availableConnections);
        clonedAvailableConnections = clonedAvailableConnections.concat(checkedConnections);
        setConnectionGroupInfo(clonedConnectionGroupInfo);
        setAvailableConnections(clonedAvailableConnections);
        setCheckedConnections([]);
    };

    return (
        <Fragment>
            <R.Panel
                id="reco-az-panel"
                header={actionMode === ActionMode.Create ? RMResx.RM_FS_Register_CreateConnectionGroup : RMResx.RM_FS_Register_EditConnectionGroup}
                size={660}
                status={{ show: show }}
                onHide={onHide}
                destroy={false}
            >
                <div className="br" slot="header">
                    <span className="reco-az-panel-header">{RMResx.RM_FS_Register_CreateConnectionGroup_SubTitle}</span>
                </div>
                <div>
                    <div style={{marginBottom: "24px"}} hidden={!errorMessage}>
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
                        name={Columns.ConnectionGroupName}
                        value={connectionGroupInfo.name}
                        type="text"
                        height={34}
                        require={true}
                        message={RMResx.RM_FS_Register_NameInputValidateMessage}
                        isShowMessage={validateInfo.name}
                        onChange={value => onChange("name", value)}
                    />
                    <InputItem
                        name={Columns.Description}
                        value={connectionGroupInfo.description}
                        type="textarea"
                        height={100}
                        require={false}
                        onChange={value => onChange("description", value)}
                    />
                    <div style={{ marginBottom: "16px" }}>
                        {
                            checkedConnections.length === 0 ?
                                <R.Button
                                    icon="fia-plus"
                                    text={RMResx.RM_FS_Register_Add}
                                    onClick={() => connectionAddRef.current.onShow(availableConnections)}
                                /> :
                                <R.Button
                                    icon="fia-delete"
                                    text={RMResx.RM_FS_Register_Remove}
                                    onClick={onConnectionsWillRemoved}
                                />
                        }
                    </div>
                    <ConnectionSimpleTable
                        key={Math.random()}
                        tableId="reco-az-conn-simple-gorup-table"
                        items={connectionGroupInfo.connections}
                        onChangeChecked={onConnectionCheckedChange}
                    />
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSave} />
                </>
            </R.Panel>
            <ConnectionAddPanel
                ref={connectionAddRef}
                onAddConnection={onConnectionsWillAdded}
            />
        </Fragment>
    );
};

export default forwardRef(ConnectionGroupPanel);