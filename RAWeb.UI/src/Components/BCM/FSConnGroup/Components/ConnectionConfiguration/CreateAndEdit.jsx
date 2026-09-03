import React, { useRef, useState, useEffect, useImperativeHandle, forwardRef } from "react";

import { AccessConnectionType, DCConnectionType } from "./Constants";
import { ConnectionTable, AgentTable } from "./Tables";
import ConnectionsAdd from "./ConnectionsAdd";
import AgentsAdd from "./AgentsAdd";

import Utils from "./Utils";
import { isEnableMultiGeoFeature, isShowActionByDC, LicenseHelper } from "../../../../../Utilities/CommonUtil";

const GetAccessConnectionTypeRadioGroup = (type, isSpecifyDCConnectionType = false) => [
    {
        text: RMResx.RM_FS_Register_SpecifyAgentAccessConn_Type_All,
        title: RMResx.RM_FS_Register_SpecifyAgentAccessConn_Type_All,
        value: AccessConnectionType.All,
        checked: type === AccessConnectionType.All,
        disabled: isSpecifyDCConnectionType
    },
    {
        text: RMResx.RM_FS_Register_SpecifyAgentAccessConn_Type_Specify,
        title: RMResx.RM_FS_Register_SpecifyAgentAccessConn_Type_Specify,
        value: AccessConnectionType.Specify,
        checked: type === AccessConnectionType.Specify || isSpecifyDCConnectionType
    }
];

const GetDCConnectionTypeRadioGroup = (type) => [
    {
        text: RMResx.RM_FS_Register_DC_Default,
        title: RMResx.RM_FS_Register_DC_Default,
        value: DCConnectionType.Default,
        checked: type === DCConnectionType.Default
    },
    {
        text: RMResx.RM_FS_Register_DC_Specific,
        title: RMResx.RM_FS_Register_DC_Specific,
        value: DCConnectionType.Specify,
        checked: type === DCConnectionType.Specify
    }
];

const GetSaveConnectionGroupRequestOption = (data) => ({
    url: "/api/ConnectionRegisterApi/SaveConnectionGroup",
    method: "POST",
    data: data
});

const GetViewConnectionGroupRequestOption = (id) => ({
    url: `/api/ConnectionRegisterApi/GetGroup?id=${id}`,
    method: "GET",
});

const GetMultiGEODCInformationRequestOption = () => ({
    url: `/api/MultiGEODataCenterApi/GetMultiGEODCInformation`,
    method: "GET",
});

const GetValidateConnectionRequestOption = (data) => ({
    url: "/api/ConnectionRegisterApi/ValidateConnections",
    method: "POST",
    data: data
});

const CreateAndEdit = ({ groupId }, ref) => {

    const connectionAddRef = useRef();

    const agentAddRef = useRef();

    const [isCommit, setIsCommit] = useState(false);

    const [isShowTips, setIsShowTips] = useState(false);

    const [tipsMessage, setTipsMessage] = useState("");

    const [connectionGroupName, setConnectionGroupName] = useState("");

    const [description, setDescription] = useState("");

    const [accessConnectionType, setAccessConnectionType] = useState(AccessConnectionType.All);

    const [connections, setConnections] = useState([]);

    const [willRemoveConnectionIds, setWillRemoveConnectionIds] = useState([]);

    const [addedConnections, setAddedConnections] = useState([]);

    const [removedConnections, setRemovedConnections] = useState([]);

    const [showConnectionAddPanel, setShowConnectionAddPanel] = useState({ show: false });

    const [agents, setAgents] = useState([]);

    const [willRemoveAgentIds, setWillRemoveAgentIds] = useState([]);

    const [addedAgents, setAddedAgents] = useState([]);

    const [removedAgents, setRemovedAgents] = useState([]);

    const [showAgentAddPanel, setShowAgentAddPanel] = useState({ show: false });

    const [isValidateConnections, setIsValidateConnections] = useState(false);

    const [hasValidateFailedConn, setHasValidateFailedConn] = useState(false);
    
    const [dcConnectionType, setDCConnectionType] = useState(DCConnectionType.Default);

    const [specifyDCItems, setSpecifyDCItems] = useState([]);

    const isMultiGeoEnabled = isEnableMultiGeoFeature();

    const isMultiGeoMainDC = isShowActionByDC();

    useEffect(() => {
        const handler = async () => {
            if (groupId === null) {
                return;
            }

            $$.loading(true);
            const requestOption = GetViewConnectionGroupRequestOption(groupId);
            const reusltStr = await fetchUtility(requestOption);
            const result = JSON.parse(reusltStr);
            setConnectionGroupName(result.Name);
            setDescription(result.Description);
            setAccessConnectionType(result.AccessConnectionType);
            const backendDCType = Number(result.DataCenterType) === DCConnectionType.Specify 
                ? DCConnectionType.Specify 
                : DCConnectionType.Default;
            setDCConnectionType(backendDCType);
            setSpecifyDCItems([
                {
                    checked: true,
                    DCInternalName: result?.DCInternalName || "",
                    DCDisplayName: result?.DCDisplayName || ""
                }
            ])
            const needUpdateConns = Utils.ConvertToSimpleConnections(result.FSConnections);
            setConnections(needUpdateConns);
            setAddedConnections(needUpdateConns);

            const needUpdateAgents = Utils.ConvertToSimpleAgents(result.Agents);
            setAgents(needUpdateAgents);
            setAddedAgents(needUpdateAgents);
            $$.loading(false);
        };
        handler();
    }, []);

    useEffect(() => {
        if (groupId !== null || !isMultiGeoEnabled) {
            return;
        }
        setDCList();
    }, []);

    useImperativeHandle(ref, () => ({
        Save: async () => {

            if (!validate()) {
                setIsCommit(true);
                return false;
            }
            
            const hasVailidateFailed = await onValidateConnectionTest();
            if (hasVailidateFailed) {
                return;
            }

            $$.loading(true);

            const selectedDC = specifyDCItems.find(item => item.checked)?.DCInternalName || "";

            const data = {
                Name: connectionGroupName,
                Description: description,
                AccessConnectionType: accessConnectionType,
                FSConnections: connections.map(item => ({ Id: item.Id })),
                Agents: accessConnectionType === AccessConnectionType.All ? [] : agents.map(item => ({ Id: item.Id })),
                RemoveFSConnections: removedConnections,
                DataCenterType: dcConnectionType === DCConnectionType.Specify ? 2 : 1,
                DCInternalName: dcConnectionType === DCConnectionType.Specify ? selectedDC : ""
            };

            if (groupId !== null) {
                data.Id = groupId;
            }

            var requestOption = GetSaveConnectionGroupRequestOption(data);
            const resStr = await fetchUtility(requestOption);
            const result = JSON.parse(resStr);
            $$.loading(false);
            if (result.MessageType == 1) {
                setIsShowTips(true);
                
                if (data.Name.trim().length > 255) {
                    setTipsMessage(RMResx.RM_JS_Common_Msg_CannotExceed255);
                } else {
                    setTipsMessage(result.ErrorMessage);
                }
                return false;
            }

            return true;
        },
        OnValidateConnectionTest: onValidateConnectionTest,
    }));

    const validate = () => {
        return connectionGroupName.length > 0 &&
            (accessConnectionType === AccessConnectionType.All ||
                agents.length > 0);
    };

    const onValidateConnectionTest = async () => {

        if (accessConnectionType === AccessConnectionType.Specify && agents.length === 0) {
            setIsValidateConnections(true);
            return;
        }

        $$.loading(true);
        let hasValidateFailed = false;
        const data = {
            ConnectionIds: connections.map(item => item.Id),
            AgentIds: accessConnectionType === AccessConnectionType.All ? [] : agents.map(item => item.Id),
            AccessConnectionType: accessConnectionType,
            TargetDCs: dcConnectionType === DCConnectionType.Specify ? specifyDCItems.filter(item => item.checked).map(item => item.DCInternalName) : []
        };

        const requestOption = GetValidateConnectionRequestOption(data);
        let result = [];
        try {
            result = await fetchUtility(requestOption);
        }
        catch (e) {
            console.error(e);
        }
        const connectionTemps = [...connections];
        connectionTemps.forEach(item => {
            if (result.some(i => i === item.Id)) {
                item.ValidateStatus = true;
            }
            else {
                item.ValidateStatus = false;
                hasValidateFailed = true;
            }
        });
        setConnections(connectionTemps);
        setHasValidateFailedConn(hasValidateFailed);
        $$.loading(false);
        return hasValidateFailed;
    };

    const onRemoveConnections = () => {
        const needUpdateAddedConnections = addedConnections.filter(item => !willRemoveConnectionIds.includes(item.Id));
        const needUpdateConnections = connections.filter(item => !willRemoveConnectionIds.includes(item.Id));
        const needUpdateRemovedConnections = removedConnections.concat(connections.filter(item => willRemoveConnectionIds.includes(item.Id)));
        setAddedConnections([...needUpdateAddedConnections]);
        setConnections([...needUpdateConnections]);
        setRemovedConnections([...needUpdateRemovedConnections]);
        setWillRemoveConnectionIds([]);
    };

    const renderConnectionAddPanel = () => {

        const [connPanelAddDisable, setConnPanelAddDisable] = useState(true);

        const onAddConnection = () => {
            const needAddedConns = connectionAddRef.current.getNeedAddedConnections();
            const needUpdateRemovedConnections = [...removedConnections].filter(item => !needAddedConns.some(i => i.Id === item.Id));
            setConnections([...needAddedConns]);
            setAddedConnections([...needAddedConns]);
            setRemovedConnections(needUpdateRemovedConnections);
            setShowConnectionAddPanel({ show: false });
            setHasValidateFailedConn(false);
            setConnPanelAddDisable(true);
        };

        return (
            <R.Panel
                id="ra-group-conneciton-add"
                header={RMResx.RM_FS_Register_Add}
                size={600}
                status={showConnectionAddPanel}
                destroy={true}
                actionType="back"
                onClose={() => { setShowConnectionAddPanel({ show: false }) }}
            >
                <div className="br" slot="header">
                    <span className="reco-conn-cfg-panel-desc" >{RMResx.RM_FS_Register_EditCorrelateConnections_SubTitle}</span>
                </div>
                <ConnectionsAdd
                    addedConnections={addedConnections}
                    removedConnections={removedConnections}
                    ref={connectionAddRef}
                    onCheckedConn={(checkedIds) => setConnPanelAddDisable(checkedIds.length <= 0)}
                />
                <>
                    <R.Button slot="buttons" id="raConnAddPanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => { setShowConnectionAddPanel({ show: false }); setConnPanelAddDisable(true); }} />
                    <R.Button slot="buttons" id="raConnAddPanelAddBtn" primary classify="theme" text={RMResx.RM_FS_Register_Add} disabled={connPanelAddDisable} onClick={onAddConnection} />
                </>
            </R.Panel>
        );
    };

    const onRemoveAgents = () => {
        const needUpdateAddedAgents = addedAgents.filter(item => !willRemoveAgentIds.includes(item.Id));
        const needUpdateAgents = agents.filter(item => !willRemoveAgentIds.includes(item.Id));
        const needUpdateRemovedAgents = removedAgents.concat(agents.filter(item => willRemoveAgentIds.includes(item.Id)));
        setAddedAgents([...needUpdateAddedAgents]);
        setAgents([...needUpdateAgents]);
        setRemovedAgents([...needUpdateRemovedAgents]);
        setWillRemoveAgentIds([]);
    };

    const onSpecifyDCChange = (item) => {
        setAgents([]);
        setAddedAgents([]);
        setRemovedAgents([]);
        const updateItems = specifyDCItems.map(i => {
            if (i.DCInternalName === item.newValue?.DCInternalName) {
                return { ...i, checked: true };
            } else {
                return { ...i, checked: false };
            }
        });
        setSpecifyDCItems(updateItems);
    }

    const setDCList = async() => {
        $$.loading(true);
        const requestOption = GetMultiGEODCInformationRequestOption();
        const reusltStr = await fetchUtility(requestOption);
        if(reusltStr.DCsSupported?.length > 0){
            setSpecifyDCItems(reusltStr.DCsSupported.filter(item => item.DCInternalName !== reusltStr.MainDC));
        }else{
            setSpecifyDCItems([]);
        }
        
        $$.loading(false);
    }

    const renderAgentAddPanel = () => {

        const [agentPanelAddDisable, setAgentPanelAddDisable] = useState(true);

        const onAddAgent = () => {
            const needAddedAgents = agentAddRef.current.getNeedAddedAgents();
            const needUpdateRemovedAgents = [...removedAgents].filter(item => !needAddedAgents.some(i => i.Id === item.Id));
            setAgents([...needAddedAgents]);
            setAddedAgents([...needAddedAgents]);
            setRemovedAgents(needUpdateRemovedAgents);
            setShowAgentAddPanel({ show: false });
            setAgentPanelAddDisable(true);
        };

        return (
            <R.Panel
                id="ra-group-conneciton-add"
                header={RMResx.RM_FS_Register_AddAgent}
                size={LicenseHelper.EnableJPMCFileSystemFeature() ? 664 : 600}
                status={showAgentAddPanel}
                destroy={true}
                actionType="back"
                onClose={() => { setShowAgentAddPanel({ show: false }) }}
            >
                <div className="br" slot="header">
                    <span className="reco-conn-cfg-panel-desc">{RMResx.RM_FS_Register_EditAgents_SubTitle}</span>
                </div>
                <AgentsAdd
                    addedAgents={addedAgents}
                    removedAgents={removedAgents}
                    onCheckedAgent={(checkedIds) => setAgentPanelAddDisable(checkedIds.length <= 0)}
                    isMultiGeoEnabled={isMultiGeoEnabled}
                    DataCenterName={specifyDCItems.find(item => item.checked)?.DCInternalName || ""}
                    ref={agentAddRef}
                />
                <>
                    <R.Button slot="buttons" id="raConnCancleAgent" text={RMResx.RM_JS_Common_Cancel} onClick={() => { setShowAgentAddPanel({ show: false }); setAgentPanelAddDisable(true); }} />
                    <R.Button slot="buttons" id="raConnAddAgent" primary classify="theme" text={RMResx.RM_FS_Register_AddAgent} disabled={agentPanelAddDisable} onClick={onAddAgent} />
                </>
            </R.Panel>
        );
    };

    return (
        <div className="reco-conn-cfg-wrapper">
            <section className="reco-conn-cfg-item-section" hidden={!isShowTips}>
                <R.Messagebar
                    message={tipsMessage} classify={"error"}
                    onClose={() => {
                        setIsShowTips(false);
                        setTipsMessage("");
                    }} status={{ show: isShowTips }}
                />
            </section>
            <section className="reco-conn-cfg-item-section">
                <div className="reco-conn-cfg-item-title require">
                    {RMResx.RM_FS_Register_GroupName}
                </div>
                <R.Input
                    id="raConnGroupName"
                    name='iptConnGroupName'
                    type='text'
                    width="100%"
                    value={connectionGroupName}
                    onChange={(value) => setConnectionGroupName(value)}
                    aria={{ ariaLabel: RMResx.RM_FS_Register_GroupName }}
                />
                <$g.ValidationMsg show={isCommit && connectionGroupName.length === 0}>
                    {RMResx.RM_FS_Register_NameInputValidateMessage}
                </$g.ValidationMsg>
            </section>
            <section className="reco-conn-cfg-item-section">
                <div className="reco-conn-cfg-item-title">
                    {RMResx.RM_FS_Register_Description}
                </div>
                <R.Input
                    id="raConnGroupDesc"
                    name='iptConnGroupDesc'
                    type='textarea'
                    width="100%"
                    height={100}
                    value={description}
                    onChange={(value) => setDescription(value)}
                    aria={{ ariaLabel: RMResx.RM_FS_Register_Description }}
                />
            </section>
            <section className="reco-conn-cfg-item-section">
                {isMultiGeoMainDC && <div className="reco-conn-cfg-item-button">
                    {
                        willRemoveConnectionIds.length === 0 ?
                            <R.Button
                                id="raConnGroupConnectionCreateBtn"
                                icon="fia-plus"
                                text={RMResx.RM_FS_Register_Add}
                                onClick={() => { setShowConnectionAddPanel({ show: true }) }}
                            /> :
                            <R.Button
                                id="raConnGroupConnectionDeleteBtn"
                                icon="fia-delete"
                                text={RMResx.RM_FS_Register_Remove}
                                onClick={onRemoveConnections}
                            />
                    }
                </div>}
                <ConnectionTable
                    id="reco-group-connection-table"
                    onChangeChecked={(checkedItems) => setWillRemoveConnectionIds(checkedItems.map((item) => item.Id))}
                    items={connections}
                    isAddConnPanel={false}
                />
                <$g.ValidationMsg show={hasValidateFailedConn}>
                    {RMResx.RM_FS_Register_ConnectionValidateMessage}
                </$g.ValidationMsg>
            </section>
            {isMultiGeoEnabled && (
                <>
                    <section className="reco-conn-cfg-item-section">
                        <div className="reco-conn-cfg-item-title require">
                            {RMResx.RM_FS_Register_DC_Type}
                        </div>
                        <R.Radio.Group
                            block
                            name="radiogroup-type-dc"
                            items={GetDCConnectionTypeRadioGroup(dcConnectionType)}
                            disabled={groupId !== null}
                            onChange={(value) => {
                                if (value === DCConnectionType.Specify) {
                                    setAccessConnectionType(AccessConnectionType.Specify);
                                } else {
                                    setSpecifyDCItems(specifyDCItems.map(item => ({ ...item, checked: false })));
                                }
                                setDCConnectionType(value);
                            }}
                        />
                    </section>
                    <section className="reco-conn-cfg-item-section" hidden={dcConnectionType !== DCConnectionType.Specify}>
                        <R.Combobox
                            id="raSpecifyDCCom"
                            width="100%"
                            items={specifyDCItems}
                            textField="DCDisplayName"
                            valueField="DCInternalName"
                            tooltipField="DCDisplayName"
                            checkedField="checked"
                            disabled={groupId !== null}
                            onChange={onSpecifyDCChange}
                        />
                    </section>
                </>
            )}
            <section className="reco-conn-cfg-item-section">
                <div className="reco-conn-cfg-item-title require">
                    {RMResx.RM_FS_Register_SpecifyAgentAccessConn_Type}
                </div>
                <R.Radio.Group
                    block
                    name="radiogroup-type"
                    items={GetAccessConnectionTypeRadioGroup(accessConnectionType, dcConnectionType === DCConnectionType.Specify)}
                    onChange={(value) => setAccessConnectionType(value)}
                />
            </section>
            <section className="reco-conn-cfg-item-section" hidden={(accessConnectionType !== AccessConnectionType.Specify) || (dcConnectionType === DCConnectionType.Specify && specifyDCItems.every(item => !item.checked))}>
                {isMultiGeoMainDC && <div className="reco-conn-cfg-item-button">
                    {
                        willRemoveAgentIds.length === 0 ?
                            <R.Button
                                id="raConnGroupAgentCreateBtn"
                                icon="fia-plus"
                                text={RMResx.RM_FS_Register_AddAgent}
                                onClick={() => { setShowAgentAddPanel({ show: true }) }}
                            /> :
                            <R.Button
                                id="raConnGroupAgentDeleteBtn"
                                icon="fia-delete"
                                text={RMResx.RM_FS_Register_RemoveAgent}
                                onClick={onRemoveAgents}
                            />
                    }

                </div>}
                <AgentTable
                    id="reco-group-agent-table"
                    items={agents}
                    onChangeChecked={(checkedIds) => setWillRemoveAgentIds(checkedIds)}
                />
                <$g.ValidationMsg show={(isCommit || isValidateConnections) && accessConnectionType === AccessConnectionType.Specify && agents.length === 0}>
                    {RMResx.RM_FS_Register_AgentValidateMessage}
                </$g.ValidationMsg>
            </section>
            {renderConnectionAddPanel()}
            {renderAgentAddPanel()}
        </div>
    );
};

export default forwardRef(CreateAndEdit);