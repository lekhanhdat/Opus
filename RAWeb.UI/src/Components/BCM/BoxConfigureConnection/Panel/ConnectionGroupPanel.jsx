import _ from "lodash";
import React, { useState, useImperativeHandle, forwardRef, Fragment, useRef } from "react";
import { ActionMode, ResponseErrorType } from "../config";
import Input from "../Components/Input";
import AddConnectionTable from "../Table/AddConnectionTable";
import { useStableCallback } from "../../../Common/Hooks";
import { showToast } from "../../../../Utilities/CommonUtil";
import AddConnectionPanel from "./AddConnectionPanel";

const DefaultConnectionGroupInfo = {
    name: "",
    description: "",
    connections: [],
};

const ConnectionGroupPanel = ({ onReload }, ref) => {

    const validationRef = useRef(null);

    const addConnectionRef = useRef(null);

    const [isShow, setIsShow] = useState(false);

    const [connectionGroupInfo, setConnectionGroupInfo] = useState(_.cloneDeep(DefaultConnectionGroupInfo));

    const [checkedConnections, setCheckedConnections] = useState([]);

    const [availableConnections, setAvailableConnections] = useState([]);

    const [actionMode, setActionMode] = useState(ActionMode.Create);

    useImperativeHandle(ref, () => ({
        onShow: (willModifyConnectionGroupInfo) => {
            setIsShow(true);
            setCheckedConnections([]);
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
        const requestOption = {
            url: "/api/BoxConnection/GetConnectionsWithoutRelatedGroup",
        };
        const response = await fetchUtility(requestOption);
        setAvailableConnections((_.isNil(response) || _.isEmpty(response)) ? [] : response);
    };

    const onHide = () => {
        setIsShow(false);
    };

    const onSave = useStableCallback(async () => {
        const clonedConnectionGroupInfo = _.cloneDeep(connectionGroupInfo);
        if (!$$.verify(validationRef.current)) {
            return false;
        }

        clonedConnectionGroupInfo.name = clonedConnectionGroupInfo.name.trim();
        clonedConnectionGroupInfo.description = clonedConnectionGroupInfo.description.trim();

        $$.loading(true);
        const requestOption = {
            url: actionMode === ActionMode.Create ? "/api/BoxConnection/AddConnectionGroup" : "/api/BoxConnection/UpdateConnectionGroup",
            data: clonedConnectionGroupInfo
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.isSuccessful) {
            setIsShow(false);
            showToast.success(RMResx.RM_Box_Register_ConnGroup_SaveSuccess);
            onReload();
        } else {
            if (response.responseErrorType === ResponseErrorType.NameExists) {
                showToast.error(RMResx.RM_FS_Register_SameGroupNameErrorMessage);
            } else {
                showToast.error(response.responseMessage);
            }
            return false;
        }
    });

    const onChange = (columnName, value) => {
        const clonedConnectionGroupInfo = _.cloneDeep(connectionGroupInfo);
        clonedConnectionGroupInfo[columnName] = value;
        setConnectionGroupInfo(clonedConnectionGroupInfo);
    };

    const onConnectionCheckedChange = () => {
        const checkedConnections = connectionGroupInfo.connections.filter(item => item.checked);
        setCheckedConnections(checkedConnections);
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

    const onConnectionsWillAdded = (willAddedConnections) => {
        const clonedConnectionGroupInfo = _.cloneDeep(connectionGroupInfo);
        clonedConnectionGroupInfo.connections.forEach(item => item.checked = false);
        clonedConnectionGroupInfo.connections = clonedConnectionGroupInfo.connections.concat(willAddedConnections);
        const connections = availableConnections.filter(item => !willAddedConnections.some(i => i.id === item.id));
        setConnectionGroupInfo(clonedConnectionGroupInfo);
        setAvailableConnections(connections);
        setCheckedConnections([]);
    };

    const verifyCharacters = (value) => {
        if (value.trim().length > 4000) {
            return RMResx.RM_Box_Register_DesLengthLimit;
        }
        return true;
    };

    return (
        <Fragment>
            <R.Panel
                id="reco-box-panel"
                header={actionMode === ActionMode.Create ? RMResx.RM_FS_Register_CreateConnectionGroup : RMResx.RM_FS_Register_EditConnectionGroup}
                size={660}
                status={{ show: isShow }}
                onHide={onHide}
                destroy={true}
            >
                <div className="br" slot="header">
                    <span className="reco-box-panel-header">{RMResx.RM_FS_Register_CreateConnectionGroup_SubTitle}</span>
                </div>
                <div className="reco-box-content">
                    <R.Validation>
                        <div ref={validationRef}>
                            <Input
                                name={RMResx.RM_FS_Register_GroupName}
                                value={connectionGroupInfo.name}
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
                                        value={connectionGroupInfo.description}
                                        onChange={value => onChange("description", value)}
                                        aria={{ ariaLabel: RMResx.RM_FS_Register_Description }}
                                    />
                                </R.Validation>
                            </div>
                            <div className="margin-bottom-m">
                                {
                                    checkedConnections.length === 0 ?
                                        <R.Button
                                            icon="fia-plus"
                                            text={RMResx.RM_FS_Register_Add}
                                            onClick={() => addConnectionRef.current.onShow(availableConnections)}
                                        /> :
                                        <R.Button
                                            icon="fia-delete"
                                            text={RMResx.RM_FS_Register_Remove}
                                            onClick={onConnectionsWillRemoved}
                                        />
                                }
                            </div>
                            <AddConnectionTable
                                key={Math.random()}
                                tableId="reco-box-conn-gorup-table"
                                items={connectionGroupInfo.connections}
                                onChangeChecked={onConnectionCheckedChange}
                            />
                        </div>
                    </R.Validation>
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSave} />
                </>
            </R.Panel>

            <AddConnectionPanel
                ref={addConnectionRef}
                onAddConnection={onConnectionsWillAdded}
            />
        </Fragment>
    );
};

export default forwardRef(ConnectionGroupPanel);