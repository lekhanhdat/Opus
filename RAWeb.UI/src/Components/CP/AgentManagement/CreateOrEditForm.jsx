import { AgentSourceType } from "../../../Constants/Constants";
import { isEnableMultiGeoFeature } from "../../../Utilities/CommonUtil";

const enableMultiGeoFeature = isEnableMultiGeoFeature();

const DCConnectionType = {
    Default: 1,
    Specify: 2,
};

const GetDCConnectionTypeRadioGroup = (type) => [
    {
        text: RMResx.RM_FS_Register_DC_Default,
        title: RMResx.RM_FS_Register_DC_Default,
        value: DCConnectionType.Default,
        checked: type === DCConnectionType.Default,
    },
    {
        text: RMResx.RM_FS_Register_DC_Specific,
        title: RMResx.RM_FS_Register_DC_Specific,
        value: DCConnectionType.Specify,
        checked: type === DCConnectionType.Specify,
    },
];

const GetMultiGEODCInformationRequestOption = () => ({
    url: "/api/MultiGEODataCenterApi/GetMultiGEODCInformation",
    method: "GET",
});

let multiGeoDCInfoCache = null;
let multiGeoDCInfoRequest = null;

export default class CreateOrEditForm extends R.Component {
    idAttr = true;
    componentCreate() {
        this.dataCenterListLoaded = false;
        this.selectedSourceTypes = [];
        this.state = {
            displayName: "",
            displayNameValidShow: false,
            showNotSelectSourceValid: false,
            description: '',
            showTip: false,
            tipType: "success",
            tipMsg: "",
            sourceInfo: [
                {
                    icon: 'ra-agent-fs-source-icon',
                    name: RMResx.RM_JS_SPS_TabLabel_FS,
                    checked: false,
                    type: AgentSourceType.FileSystem,
                    id: "raCpAgentFSSource"
                },
                {
                    icon: 'ra-agent-sp-source-icon',
                    name: RMResx.RM_Common_SharePointOnPremise,
                    checked: false,
                    type: AgentSourceType.SharePoint,
                    id: "raCpAgentSPSource"
                }
            ],
            beforeSourceType: 0,
            collectLogStatus: false,
            dcConnectionType: DCConnectionType.Default,
            dataCenterItems: [],
            selectedDataCenterDisplayName: "",
            selectedDataCenterInternalName: "",
            showDataCenterValid: false,
        };
    }

    componentInit() {
        if (enableMultiGeoFeature) {
            this.setDataCenterList(true);
        }
        this.echoData();
    }

    componentReceive(callback, data) {
        this.saveClick(callback, data);
    }

    handleShowMessageBar = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    getEchoSourceType(binaryNum) {
        let sourceTypes = RM.deepcopy(AgentSourceType);
        let currentSelectedSourceTypes = [];
        for (let key in sourceTypes) {
            let sourceType = sourceTypes[key];
            if ((binaryNum & sourceType) == sourceType) {
                currentSelectedSourceTypes.push(sourceType);
            }
        }
        return currentSelectedSourceTypes;
    }

    echoData() {
		let data = this.props.data;
        let currentSelectedSourceTypes = this.getEchoSourceType(data.SourceType);
        for (let item of this.state.sourceInfo) {
            item.checked = currentSelectedSourceTypes.includes(item.type);
        }
        this.setState({
            displayName: data.Name,
            description: data.Description,
            sourceInfo: RM.deepcopy(this.state.sourceInfo),
            beforeSourceType: data.SourceType ? data.SourceType : 0,
            collectLogStatus: data.CollectLog === undefined ? false : data.CollectLog,
            dcConnectionType: DCConnectionType.Default,
            selectedDataCenterDisplayName: "",
            selectedDataCenterInternalName: "",
        }, () => {
            if (enableMultiGeoFeature && multiGeoDCInfoCache) {
                this.applyDataCenterInfo(multiGeoDCInfoCache, data);
            }
        });
    }

    getSourceType() {
        let sourceInfo = this.state.sourceInfo;
        let selectedSourceOptions = sourceInfo.filter((item, index) => { return item.checked; });
        let selectedSourceTypes = selectedSourceOptions.map((item, index) => { return item.type; });
        let binaryNum = 0;
        switch (selectedSourceTypes.length) {
            case 0:
                return null;
            case 1:
                return selectedSourceTypes[0];
            default:
                for (let type of selectedSourceTypes) { binaryNum |= type * 1; }
                return binaryNum;
        }
    }

    saveLogic = (callback, data) => {
        let selectedSourceType = this.getSourceType();
        let displayName = this.state.displayName;
        if (!displayName) {
            this.setState({ displayNameValidShow: true });
            return;
        }
        if (!selectedSourceType) {
            this.setState({ showNotSelectSourceValid: true });
            return;
        }
        const isFileSystemSelected = (selectedSourceType & AgentSourceType.FileSystem) === AgentSourceType.FileSystem;
        if (enableMultiGeoFeature && isFileSystemSelected && this.state.dcConnectionType === DCConnectionType.Specify && !this.state.selectedDataCenterInternalName) {
            this.setState({ showDataCenterValid: true });
            return;
        }
        let param = {
            name: displayName,
            description: this.state.description,
            sourceType: selectedSourceType,
            collectLog: this.state.collectLogStatus,
            DCInternalName: isFileSystemSelected && this.state.dcConnectionType === DCConnectionType.Specify ? this.state.selectedDataCenterInternalName : ""
        };
        let url = "/api/CPAgentMgmtApi/CreateAgent";
        if (data.Id) {
            param.id = data.Id;
            url = "/api/CPAgentMgmtApi/UpdateAgent";
        }
        $$.loading(true);
        let option = {
            data: param,
            url: url,
            method: "POST"
        };
        fetchUtility(option).then((res) => {
            //res为4为重名
            $$.loading(false);
            if (res == 4) {
                this.handleShowMessageBar('error', RMResx.RM_CP_Agent_Create_SameNameExist);
            } else if(res == 5) {
                this.handleShowMessageBar('error', RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
            }
            else {
                callback(true, res);
            }
        });
    }

    saveClick(callback, data) {

        if ((AgentSourceType.FileSystem | this.state.beforeSourceType) !== this.state.beforeSourceType) {
            this.saveLogic(callback, data);
            return;
        }

        const currentSourceType = this.getSourceType();
        if ((AgentSourceType.FileSystem | this.state.beforeSourceType) === this.state.beforeSourceType &&
            (AgentSourceType.FileSystem | currentSourceType) === currentSourceType) {
            this.saveLogic(callback, data);
            return;
        }

        fetchUtility({
            data: data.Id,
            url: "/api/CPAgentMgmtApi/CheckAgentIsUnderGroup",
        }).then((res) => {
            if (!res) {
                this.saveLogic(callback, data);
                return;
            }
            let args = {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_CP_Agent_ModifiedSourceType_Tips,
                buttons: [
                    { text: RMResx.RM_JS_Common_Cancel, onClick: () => $$.messagedialog(false) },
                    { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => { this.saveLogic(callback, data); $$.messagedialog(false); } },
                ]
            };
            $$.messagedialog(true, args);
        });
    }

    onChangeDisplayName = (value) => {
        let inputValue = value.trim();
        let displayNameValidShow = inputValue ? false : true;
        this.setState({
            displayName: inputValue,
            displayNameValidShow: displayNameValidShow
        },()=>{
            $("#raCpAgentDes").focus();
        });
    };

    onChangeDescription = (value) => {
        this.setState({
            description: value.trim()
        });
    };
    
    onChangeCollectLogStatus = (value) => {
        this.setState({
            collectLogStatus: value
        });
    };

    handleSourceChange = (checked, value) => {
        if (this.isLockedFileSystemSource(value)) {
            return;
        }

        let sourceInfo = this.state.sourceInfo;
        for (let source of sourceInfo) {
            if (value == source.type) {
                source.checked = checked;
                break;
            }
        }
        const isFileSystemChecked = sourceInfo.some((item) => item.type === AgentSourceType.FileSystem && item.checked);
        this.setState({
            showNotSelectSourceValid: false,
            sourceInfo: RM.deepcopy(sourceInfo),
            showDataCenterValid: false,
        }, () => {
            if (!isFileSystemChecked) {
                this.setState({
                    dcConnectionType: DCConnectionType.Default,
                    selectedDataCenterDisplayName: "",
                    selectedDataCenterInternalName: "",
                    dataCenterItems: this.state.dataCenterItems.map((item) => ({
                        ...item,
                        checked: false,
                    })),
                });
            }
        });
    }

    handleHideMessageBar = () => {
        this.setState({ showTip: false });
    }

    renderMessageBar() {
        return <R.Messagebar
            message={this.state.tipMsg}
            classify={this.state.tipType}
            status={{ show: this.state.showTip }}
            onClose={this.handleHideMessageBar}
        />;
    }

    renderSourceOptions(sourceOption, key) {
        const disabled = this.isLockedFileSystemSource(sourceOption.type);
        return <div className="source-option" key={key} tabIndex={0}>
            <React.Fragment>
                <R.Checkbox
                    id={sourceOption.id}
                    name="source-checkbox"
                    value={sourceOption.type}
                    ariaLabel={sourceOption.name}
                    checked={sourceOption.checked}
                    disabled={disabled}
                    onChange={this.handleSourceChange}
                />
                <div className={"ra-agent-source-icon " + sourceOption.icon}></div>
                <div className='source-name'>{sourceOption.name}</div>
            </React.Fragment>
        </div>;
    }

    renderDataCenterOptions() {
        if (!enableMultiGeoFeature || !this.isFileSystemSelected()) {
            return null;
        }

        return <>
            <section className="reco-conn-cfg-item-section">
                <div className="reco-conn-cfg-item-title require">
                    {RMResx.RM_FS_Register_Agent_DC}
                </div>
                <R.Radio.Group
                    block
                    name="radiogroup-type-dc"
                    items={GetDCConnectionTypeRadioGroup(this.state.dcConnectionType)}
                    disabled={this.isLockedDataCenter()}
                    onChange={this.onChangeDataCenterType}
                />
            </section>
            <section className="reco-conn-cfg-item-section" hidden={this.state.dcConnectionType !== DCConnectionType.Specify}>
                <div className="reco-conn-cfg-item-title require">
                    {RMResx.RM_FS_Register_DataCenter}
                </div>
                <div>
                    <R.Combobox
                        id="raCpAgentDataCenterCom"
                        width="100%"
                        items={this.state.dataCenterItems}
                        textField="DCDisplayName"
                        valueField="DCInternalName"
                        tooltipField="DCDisplayName"
                        checkedField="checked"
                        value={this.state.selectedDataCenterDisplayName}
                        disabled={this.isLockedDataCenter()}
                        onChange={this.onChangeDataCenter}
                    />
                </div>
                <$g.ValidationMsg show={this.state.showDataCenterValid}>
                    Please select a data center.
                </$g.ValidationMsg>
            </section>
        </>;
    }

    render() {
        let sourceInfo = this.state.sourceInfo;
        return <div id={this.props.id}>
            {this.renderMessageBar()}
            <$g.FormRow label={RMResx.RM_CP_Agent_Column_DisplayName} require={true}>
                <R.Input
                    id="raCpAgentDisplayNameIpt"
                    type="text"
                    width={500}
                    value={this.state.displayName}
                    onChange={this.onChangeDisplayName}
                    placeholder=""
                    aria={{ ariaLabel: RMResx.RM_CP_Agent_Column_DisplayName }}
                />
                <$g.ValidationMsg show={this.state.displayNameValidShow}>
                    {RMResx.RM_CP_Agent_Certificate_Valid_DisplayName}
                </$g.ValidationMsg>
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_CP_Agent_Column_Description}>
                <R.Input
                    id="raCpAgentDes"
                    type="textarea"
                    width={500}
                    value={this.state.description}
                    onChange={this.onChangeDescription}
                    aria={{ ariaLabel: RMResx.RM_CP_Agent_Column_Description }}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_CP_Agent_Column_Source} require={true}>
                {
                    sourceInfo.map((sourceOption, key) => {
                        return this.renderSourceOptions(sourceOption, key);
                    })
                }
                <$g.ValidationMsg show={this.state.showNotSelectSourceValid}>
                    {RMResx.RM_CP_Agent_Valid_NoSelectSource}
                </$g.ValidationMsg>
            </$g.FormRow>
            {this.renderDataCenterOptions()}
            {RM.gData.enableJPMCFileSystemFeature && (
                <$g.FormRow label={RMResx.RM_CP_Agent_Column_CollectLog}>
                    <div tabIndex={0}>
                        <R.Switch
                            id="raCpAgentCollectLogSwitch"
                            checked={this.state.collectLogStatus}
                            onChange={this.onChangeCollectLogStatus}>
                        </R.Switch>
                        <span className='collect-switch'>
                            {this.state.collectLogStatus ? RMResx.RM_CP_Agent_Column_CollectLog_Enable : RMResx.RM_CP_Agent_Column_CollectLog_Disable}
                        </span>
                    </div>
                </$g.FormRow>
            )}

        </div>;
    }

    isFileSystemSelected() {
        return this.state.sourceInfo.some((item) => item.type === AgentSourceType.FileSystem && item.checked);
    }

    onChangeDataCenter = (value) => {
        const selectedItem = value?.newValue || {};
        const updatedItems = this.state.dataCenterItems.map((item) => ({
            ...item,
            checked: item.DCInternalName === selectedItem.DCInternalName,
        }));
        this.setState({
            selectedDataCenterDisplayName: selectedItem.DCDisplayName || "",
            selectedDataCenterInternalName: selectedItem.DCInternalName || "",
            showDataCenterValid: false,
            dataCenterItems: updatedItems,
        });
    };

    onChangeDataCenterType = (value) => {
        if (this.isLockedDataCenter()) {
            return;
        }

        if (value === DCConnectionType.Default) {
            this.setState({
                dcConnectionType: DCConnectionType.Default,
                selectedDataCenterDisplayName: "",
                selectedDataCenterInternalName: "",
                showDataCenterValid: false,
                dataCenterItems: this.state.dataCenterItems.map((item) => ({
                    ...item,
                    checked: false,
                })),
            });
            return;
        }

        this.setState({
            dcConnectionType: DCConnectionType.Specify,
            showDataCenterValid: false,
        }, () => {
            this.ensureDataCenterItems();
        });
    };

    ensureDataCenterItems = () => {
        if (this.state.dataCenterItems.length > 0) {
            if (!this.state.selectedDataCenterInternalName && !this.state.selectedDataCenterDisplayName) {
                const defaultItem = this.state.dataCenterItems[0] || {};
                this.setState({
                    selectedDataCenterDisplayName: defaultItem.DCDisplayName || "",
                    selectedDataCenterInternalName: defaultItem.DCInternalName || "",
                    dataCenterItems: this.state.dataCenterItems.map((item) => ({
                        ...item,
                        checked: item.DCInternalName === defaultItem.DCInternalName,
                    })),
                });
            }
            return;
        }

        this.setDataCenterList();
    };

    setDataCenterList = (isSilent = false) => {
        if (!enableMultiGeoFeature || this.dataCenterListLoaded) {
            return;
        }

        if (multiGeoDCInfoCache) {
            this.applyDataCenterInfo(multiGeoDCInfoCache);
            this.dataCenterListLoaded = true;
            return;
        }

        if (!isSilent) {
            $$.loading(true);
        }
        if (!multiGeoDCInfoRequest) {
            multiGeoDCInfoRequest = fetchUtility(GetMultiGEODCInformationRequestOption());
        }

        multiGeoDCInfoRequest.then((res) => {
            multiGeoDCInfoCache = res;
            this.applyDataCenterInfo(res);
            this.dataCenterListLoaded = true;
            if (!isSilent) {
                $$.loading(false);
            }
        }).catch((e) => {
            if (!isSilent) {
                $$.loading(false);
            }
        }).finally(() => {
            multiGeoDCInfoRequest = null;
        });
    }

    applyDataCenterInfo = (res, agentData = this.props.data) => {
        const dcItems = res?.DCsSupported || [];
        const mainDCId = res?.MainDC || "";
        const others = dcItems
            .filter((x) => x.DCInternalName !== mainDCId)
            .sort((a, b) => (a.DCDisplayName || "").localeCompare(b.DCDisplayName || ""));
        const sortedDCItems = [...others].map((item) => ({
            ...item,
            checked: false,
        }));

        const targetInternalName = this.state.selectedDataCenterInternalName || agentData?.DCInternalName || agentData?.DataCenterName || "";
        const targetDisplayName = this.state.selectedDataCenterDisplayName || agentData?.DCDisplayName || "";
        const matchedSpecificDC = sortedDCItems.find((item) => item.DCInternalName === targetInternalName)
            || sortedDCItems.find((item) => item.DCDisplayName === targetDisplayName)
            || null;

        const updatedDCItems = sortedDCItems.map((item) => ({
            ...item,
            checked: item.DCInternalName === matchedSpecificDC?.DCInternalName,
        }));

        this.setState({
            dataCenterItems: updatedDCItems,
            dcConnectionType: matchedSpecificDC ? DCConnectionType.Specify : DCConnectionType.Default,
            selectedDataCenterDisplayName: matchedSpecificDC?.DCDisplayName || "",
            selectedDataCenterInternalName: matchedSpecificDC?.DCInternalName || "",
            showDataCenterValid: false,
        });
    }

    isLockedFileSystemSource = (sourceType) => {
        return enableMultiGeoFeature
            && !!this.props.data?.Id
            && sourceType === AgentSourceType.FileSystem
            && this.hasFileSystemDataCenter();
    }

    isLockedDataCenter = () => {
        return enableMultiGeoFeature
            && !!this.props.data?.Id
            && this.hasFileSystemDataCenter();
    }

    hasFileSystemDataCenter = () => {
        return !!this.props.data?.DCInternalName || !!this.props.data?.DCDisplayName;
    }
}