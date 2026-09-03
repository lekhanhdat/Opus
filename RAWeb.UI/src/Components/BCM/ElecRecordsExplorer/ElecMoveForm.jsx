import SPDestinationTree from "../../Common/Tree/Instances/SPTree/SPDestinationTree";
import { SourceFlags } from "../../../Constants/Constants";
import "../../../Less/BCM/recordsExplorer.less";
import { LicenseHelper } from "../../../Utilities/CommonUtil";
import { checkPermission } from "../../../Utilities/permissionManager";
import TeamsDestinationTree from "../../Common/Tree/Instances/TeamsTree/TeamsDestinationTree";

const LocationType = {
    inputLocation: '0',
    selectLocation: '1'
};

export default class EleMoveForm extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            isSPItem: this.props.sourceType == SourceFlags.SP,
            selConflictType: '1',
            locationType: LocationType.inputLocation,
            isNoLocation: false,
            noSelectNode: false,
            showTip: { show: false },
            tipType: '',
            tipMsg: '',
            isKeepClassification: true,
            destinationActiveTab: 0,
        };
        this.moveData = {};
        this.selectedTreeNode = null;

        this.bind(['onConflictChange', "locationTypeChange", "checkLocation", "locationPathChange", "onDestTreeSelectedChanged"]);
    }

    componentReceive(type, callback) {
        switch (type) {
            case "onSave":
                this.getMoveData(callback);
                break;
        }
    }

    showMessageTip(type, msg) {
        let tipOption = {
            showTip: { show: true },
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    getMoveData(callback) {
        this.moveData.DestMode = 1;//sp key
        this.moveData.FileInherit = 0; //sp key
        this.moveData.FileNameConflictOption = this.state.selConflictType;
        this.moveData.isKeepClassification = this.state.isKeepClassification;
        this.moveData.CheckLocationObject = {};
        if (this.state.locationType == LocationType.inputLocation) {
            this.moveData.IsSpecifyLocation = true;
            this.moveData.LocationPath = this.state.locationPath;
            this.checkLocation(true, callback);
        } else {
            if (this.selectedTreeNode) {
                this.moveData.IsSpecifyLocation = false;
                this.moveData.SPTree = this.selectedTreeNode;
                callback(this.moveData);
            } else {
                this.setState({ noSelectNode: true });
            }
        }
    }

    locationTypeChange(val) {
        this.setState({
            locationPath: '',
            locationType: val,
            isNoLocation: false,
            showTip: { show: false },
        });
    }

    checkLocation(isSave, callback) {
        let locationPath = this.state.locationPath;
        if (!locationPath) {
            this.setState({ showTip: { show: false }, isNoLocation: true });
            return;
        } else {
            this.setState({ isNoLocation: false });
        }
        $$.loading(true);
        let option = {
            url: '/api/RecordsExplorerApi/CheckSPLocation',
            method: "POST",
            data: { LocationPath: locationPath }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res != "") {
                this.moveData.CheckLocationObject = JSON.parse(res);
                this.showMessageTip("success", RMResx.RM_JS_CP_ES_SuccessToValidateDBSettings);
                if (isSave) {
                    callback(this.moveData);
                }
            } else {
                this.showMessageTip("error", RMResx.RM_JS_Rule_SPDestUrlError);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    locationPathChange(value) {
        this.setState({
            locationPath: value,
        });
    }

    getConflictOptions() {
        let options = [
            { text: RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Skip, value: "1" },
            { text: RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite, value: "2" },
            { text: RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Rename, value: "3" }
        ];
        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.selConflictType == op.value;
            return op;
        });
    }

    onConflictChange(value) {
        this.setState({ selConflictType: value, });
    }

    onDestTreeSelectedChanged(nodeItem) {
        this.selectedTreeNode = nodeItem;
        this.setState({ noSelectNode: false });
    }

    onDestActiveTabChange = (index) => {
        this.setState({ destinationActiveTab: index });
        this.selectedTreeNode = null;
    }

    keepClassificationChange = (isCheck)=>{
        this.setState({isKeepClassification: isCheck});
    }

    renderInputLocation() {
        let locationPathPlaceholder = RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark;
        if (this.state.locationType == LocationType.inputLocation) {
            return <div className='ra-specify-location'>
                <div className="ra-inline-middle">
                    <div className='ra-specify-location-input'>
                        <R.Input
                            id="raSearchMoveLocationIpt"
                            type="text"
                            width={350}
                            height={34}
                            value={this.state.locationPath}
                            onChange={this.locationPathChange}
                            placeholder={locationPathPlaceholder}
                            aria={{ariaLabel:locationPathPlaceholder}}
                        />
                    </div>
                    <div className='inline-block'>
                        <R.Button text={RMResx.RM_RDM_MA_Location_Test} onClick={this.checkLocation} />
                    </div>
                </div>
                <div className='ra-specify-location-valid'>
                    <$g.ValidationMsg show={this.state.isNoLocation}>
                        {RMResx.RM_JS_RDM_CreateRule_Validation_NoInputLocaltion}
                    </$g.ValidationMsg>
                </div>
                {
                    this.state.showTip.show && <div className='ra-specify-location-msgbar'>
                        <R.Messagebar
                            message={this.state.tipMsg}
                            status={this.state.showTip}
                            classify={this.state.tipType} />
                    </div>
                }
            </div>;
        }
    }

    renderSelectLocation() {
        const supportTeamsTree = LicenseHelper.HasUpgradeTeams() && checkPermission("Source_Teams", RM.UserResources);
        let showSelectLocation = this.state.locationType == LocationType.selectLocation;
        return <div className='ra-select-location' style={{ display: (showSelectLocation) ? 'block' : 'none' }}>
            <div className="ra-tree-container ra-select-location-tree">
                {supportTeamsTree ? (
                    <div style={{ padding: "12px 20px" }}>
                        <R.Tabcontrol destroy active={this.state.destinationActiveTab} onChange={this.onDestActiveTabChange}>
                            <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_SharePoint_Tab}>
                                <div className="destination-tab">  
                                    <SPDestinationTree onSelectedNodeChanged={this.onDestTreeSelectedChanged} />
                                </div>
                            </R.TabPanel>
                            <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_Teams_Tab}>
                                <div className="destination-tab">  
                                    <TeamsDestinationTree onSelectedNodeChanged={this.onDestTreeSelectedChanged} />
                                </div>
                            </R.TabPanel>
                        </R.Tabcontrol>
                    </div>
                ) : (
                    <SPDestinationTree onSelectedNodeChanged={this.onDestTreeSelectedChanged} />
                )}
            </div>
            <$g.ValidationMsg show={this.state.noSelectNode}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
            </$g.ValidationMsg>
        </div>;
    }

    renderConflictOption() {
        return <div>
            <div className='ra-file-conflict-option' tabIndex="0">
                {RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Title.replace(":","")}
            </div>
            <R.Radio.Group
                name='conflictOption'
                items={this.getConflictOptions()}
                onChange={this.onConflictChange}
                block={true}
            />
            <div className="margin-top-l">
                <R.Checkbox
                    id="raSearchMoveKeepClassifyChk"
                    text={RMResx.RM_JS_BCM_Rule_Move_IsReclassify}
                    checked={this.state.isKeepClassification}
                    onChange={this.keepClassificationChange} />
            </div>
        </div>;
    }

    render() {
        return <div id={this.props.id}>
            <div className="ra-elec-move">
                <div className="ra-move-specify-location" tabIndex="0">
                    {RMResx.RM_JS_BCM_Explorer_Move_OptionTitle_SpecifyLocation.replace(":","")}
                </div>
                <$g.RadioGroup
                    name="search-move-specify-type"
                    onChange={this.locationTypeChange}
                    value={this.state.locationType}>
                    <$g.RadioOption value={LocationType.inputLocation} text={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation}>
                        {this.renderInputLocation()}
                    </$g.RadioOption>
                    <$g.RadioOption value={LocationType.selectLocation} text={RMResx.RM_JS_BCM_Explorer_Move_SelectTreeNode}>
                        {this.renderSelectLocation()}
                    </$g.RadioOption>
                </$g.RadioGroup>
                {this.renderConflictOption()}
            </div>
        </div>;
    }
}
