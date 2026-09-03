import FSDestinationTree from '../../Tree/Instances/FSTree/FSDestinationTree';
import { TreeType } from "../../../../Constants/Constants";
import * as Constants from "./Constants";
import { LicenseHelper } from '../../../../Utilities/CommonUtil';

export default class RuleMove extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.spNodeItem = null;
        this.isUNCPathPattern = /\\\\[\w.-]+\\[^\\?<>*:|/]+/;
        this.state = {
            isMove: false,
            isSpecifyLocation: false,
            elementsEnable: false,
            locationPath: '',
            fileInherit: 0,
            noLocation: false,
            locationValid: '',
            noSelectNode: false,
            isLocationValid: false,
            locationValidType: false,
            currentConflictOptionValue: '1',
            pathValidateFailed: false,
            // isMoveDeclare: false,
        };
        this.bind(['locationPathChange', 'fileConflictOptionChange',
            "onDestTreeSelectedChanged"]);
    }

    componentReceive(action, callback, data) {
        if (action == Constants.dispatchAction.save) {
            this.setValid(callback);
        }
        if (action == Constants.dispatchAction.setData) {
            this.setEchoData(data);
        }
    }

    setEchoData(data) {
        if (!data.IsSpecifyLocation) {
            let destinationTreeData = data.FSTreeStr && JSON.parse(data.FSTreeStr);
            for (let item of destinationTreeData) {
                if (item.CheckNumber == 1) {
                    this.spNodeItem = item;
                }
            }
            this.setState({
                destinationTreeData: destinationTreeData
            });
        } else {
            this.setState({
                locationPath: data.LocationPath,
            });
        }
        this.setState({
            isSpecifyLocation: data.IsSpecifyLocation,
            currentConflictOptionValue: data.FileNameConflictOption
        });
    }

    setValid(callback) {
        let isValid = true;
        let ruleMoveParam = this.getRuleMoveParam();
        if (this.state.isSpecifyLocation) {
            isValid = this.state.locationPath != '' && this.isUNCPathPattern.test(this.state.locationPath);
        } else {
            isValid = !!this.spNodeItem;
        }
        this.setState({
            noSelectNode: !this.spNodeItem,
            pathValidateFailed: this.state.isSpecifyLocation && !isValid
        });
        callback(isValid, ruleMoveParam);
    }

    getRuleMoveParam() {
        let ruleMoveParam = {};
        let isSpecifyLocation = this.state.isSpecifyLocation;
        if (isSpecifyLocation) {
            ruleMoveParam.DestMode = 1;
            ruleMoveParam.LocationPath = this.state.locationPath;
        } else {
            ruleMoveParam.DestMode = 0;
            ruleMoveParam.FSTree = this.spNodeItem;
            let fSTreeInfo = this.ruleMoveTree.getTreeData();
            for (let fSTreeNode of fSTreeInfo) {
                fSTreeNode.DisposeScheduleInfo = null;
            }
            ruleMoveParam.FSTreeStr = JSON.stringify(fSTreeInfo);
        }
        ruleMoveParam.IsSpecifyLocation = this.state.isSpecifyLocation;
        // ruleMoveParam.NotDeclareMovedData = !this.state.isMoveDeclare;
        ruleMoveParam.FileNameConflictOption = this.state.currentConflictOptionValue;
        ruleMoveParam.FileInherit = this.state.fileInherit;
        return ruleMoveParam;
    }

    fileConflictOptionChange(value) {
        this.setState({
            currentConflictOptionValue: value
        });
    }

    locationTypeClick(isSpecifyLocation) {
        this.setState({
            isSpecifyLocation: isSpecifyLocation,
            // noLocation: false,
            isLocationValid: false,
            noSelectNode: false
        });
    }

    //Enter a destination input change
    locationPathChange(value) {
        this.setState({
            locationPath: value,
            isLocationValid: false
        });
    }

    onDestTreeSelectedChanged(nodeItem) {
        this.spNodeItem = nodeItem;
        this.setState({
            noSelectNode: false
        });
    }

    // onMoveDeclareChange(e) {
    //     this.setState({isMoveDeclare: e.target.checked});
    // }
    cancelLocationVlidat = () => {
        this.setState({ isLocationValid: false });
    };

    render() {
        let isSpecifyLocation = this.state.isSpecifyLocation;
        let DestinationTree = FSDestinationTree;
        let locationPathPlaceholder = RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_FS_WaterMark;
        return <div id="rm_createRule_move_container">
            <div id="moveto-records-view-body" className="moveto-records-body">
                <div className="main-title" tabIndex="0">
                    <span>{RMResx.RM_JS_BCM_Explorer_Move_OptionTitle_SpecifyLocation}</span>
                </div>
                <div id="location-container-sp">
                    <div className="location-title">
                        <div className="location-title">
                            <label>
                                <R.Radio
                                    name="ruleActionMoveForFS"
                                    text={RMResx.RM_JS_BCM_Explorer_Move_SelectTreeNode}
                                    checked={!isSpecifyLocation}
                                    disabled={this.state.elementsEnable}
                                    onChange={this.locationTypeClick.bind(this, false)}
                                />
                            </label>
                        </div>
                        <div className='ra-tree' style={{ display: (isSpecifyLocation) ? "none" : "block" }}>
                            <div className="ra-tree-container">
                                <DestinationTree
                                    treeType={TreeType.Move}
                                    ref={r => this.ruleMoveTree = r}
                                    treeData={this.state.destinationTreeData}
                                    onSelectedNodeChanged={this.onDestTreeSelectedChanged} />
                            </div>
                            <$g.ValidationMsg show={this.state.noSelectNode}>
                                {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                            </$g.ValidationMsg>
                        </div>
                        <label className="location-title">
                            <R.Radio
                                name="ruleActionMoveForFS"
                                text={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation}
                                checked={isSpecifyLocation}
                                disabled={this.state.elementsEnable}
                                onChange={this.locationTypeClick.bind(this, true)}
                            />
                        </label>
                    </div>
                    {
                        isSpecifyLocation &&
                        <div className="sub-options-container">
                            <div className="flex">
                                <R.Input
                                    className="location-path"
                                    type="text"
                                    aria-label={locationPathPlaceholder}
                                    disabled={this.state.elementsEnable}
                                    placeholder={locationPathPlaceholder}
                                    value={this.state.locationPath || ""}
                                    onChange={this.locationPathChange}
                                    onBlur={this.archiveActionCustomValidate} />
                            </div>
                            <$g.ValidationMsg show={this.state.pathValidateFailed}>
                                {LicenseHelper.EnableJPMCFileSystemFeature() ? RMResx.RM_FS_Register_PathInputValidateMessage : RMResx.RM_FS_Register_UNCPathInputValidateMessage}
                            </$g.ValidationMsg>
                        </div>
                    }
                </div>
                <div className="file-body">
                    <div className="option-title strong" tabIndex="0">
                        <span>{RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Title}</span>
                    </div>
                    <div className="option-title"><label>
                        <R.Radio
                            name='FS_Move_FileConflictOption'
                            text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Skip}
                            value='1'
                            disabled={this.state.elementsEnable}
                            checked={this.state.currentConflictOptionValue == '1'}
                            onChange={this.fileConflictOptionChange} />
                    </label>
                    </div>
                    <div className="option-title"><label>
                        <R.Radio
                            name='FS_Move_FileConflictOption'
                            text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite}
                            disabled={this.state.elementsEnable}
                            value='2'
                            checked={this.state.currentConflictOptionValue == '2'}
                            onChange={this.fileConflictOptionChange} />
                    </label>
                    </div>
                    <div className="option-title"><label>
                        <R.Radio
                            name='FS_Move_FileConflictOption'
                            text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Rename}
                            disabled={this.state.elementsEnable}
                            value='3'
                            checked={this.state.currentConflictOptionValue == '3'}
                            onChange={this.fileConflictOptionChange} />
                    </label>
                    </div>
                </div>
            </div>
        </div>;
    }
}