import { StorageTypeIndex } from "../../../Constants/Constants";
import "../../../Less/CP/configurationStorageSettings.less";
import { getUserGuildTagPage, showToast } from "../../../Utilities/CommonUtil";
import { DataRadioValue, DateUnit, MessageType, RetentionDataTimeRadioValue, StorageTypeCol, TierValue } from "../CPConstants";
import Amazon from "./StorageType/Amazon";
import AzureBlob from "./StorageType/AzureBlob";
import Box from "./StorageType/Box";
import CompatibleStorage from "./StorageType/CompatibleStorage";
import Dropbox from "./StorageType/Dropbox";
import FTP from "./StorageType/FTP";
import NetApp from "./StorageType/NetApp";
import Rackspace from "./StorageType/Rackspace";
import SFTP from "./StorageType/SFTP";
import Google from "./StorageType/Google";
import Enviroments from "../../../Constants/Enviroments";
import { productKeys, storageKeys } from "../../../Utilities/Constant";

export default class StoragePanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.firstMoveDataRuleIndex = -1;
        this.retentionDefaultObj = {
            SetupDataRetention: false,
            RetentionDataTimeType: RetentionDataTimeRadioValue.ArchivedTime,
            KeepValue: "",
            ArchiveDateUnit: DateUnit.Year,
            DeleteTheData: true,
            RemoveOrphanedStub: true,
            RemoveTheJob: false,
            IsSoftDelete: false,
            SoftDeleteKeepValue: "",
            SoftDeleteDateUnit: DateUnit.Week,
            IsMarkDataTier: false,
            TierType: TierValue.ColdTier,
            IsMove: false,
            MoveDeviceId: "",
            StorageList: []
        };
        this.defaultRetentionList = [RM.deepcopy(this.retentionDefaultObj)];
        this.defaultDeviceId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        this.state = {
            storageData: {
                Type: StorageTypeIndex.AzureBlob,
                UseCompression: true,
                CompressionSpeed: 5,
                mCurrentXRI: { Params: {}, VIM: "azure_vim" }
            },
            storageTypeList: RM.deepcopy(StorageTypeCol),
            storageTypeDisable: false,
            advanced: false,
            extend: "",
            allSecurityProfile: [],
            isShowWarnMsg: { show: false },
            tipMsg: "",
            retentions: this.defaultRetentionList,
            isSystemStorage: true,
            isShowMoveDataRadio: false,
        };
    }

    componentInit() {
        this.loadStorageSetting();
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    loadStorageSetting() {
        if (this.props.cellStorageId) {
            $$.loading(true);
            let option = {
                url: "/api/StorageDevice/GetStorageDeviceById",
                method: "POST",
                data: this.props.cellStorageId,
            };
            fetchUtility(option).then((res) => {
                $$.loading(false);
                let getStorageData = res;

                if (this.props.indexDeviceId == this.props.cellStorageId) {
                    this.setState({ isShowWarnMsg: { show: true }, tipMsg: RMResx.RM_AR_CP_GSS_EditIndexWarn });
                } else if (getStorageData.IsUsingDevice) {
                    this.setState({ isShowWarnMsg: { show: true }, tipMsg: RMResx.RM_AR_CP_GSS_IsUsedStorageWarn });
                }
                this.state.storageTypeList.forEach(item => {
                    item.checked = item.index == getStorageData.Type;
                });
                if (getStorageData.ArchiveRetentionRules) {
                    getStorageData.ArchiveRetentionRules.forEach(item => {
                        item.KeepValue = item.KeepValue === 0 ? "" : item.KeepValue;
                        item.SoftDeleteKeepValue = item.SoftDeleteKeepValue === 0 ? "" : item.SoftDeleteKeepValue;
                        item.TierType = item.TierType === 0 ? TierValue.ColdTier : item.TierType;
                    });
                }

                this.setState({
                    storageTypeDisable: true,
                    storageTypeList: RM.deepcopy(this.state.storageTypeList),
                    storageData: getStorageData,
                    retentions: getStorageData.ArchiveRetentionRules && getStorageData.ArchiveRetentionRules.length > 0 ? getStorageData.ArchiveRetentionRules : this.state.retentions,
                    advanced: (getStorageData.mCurrentXRI.Params.advanced || "").toLowerCase() == "true",
                    extend: (getStorageData.mCurrentXRI.Params.advanced || "").toLowerCase() == "true" ? getStorageData.mCurrentXRI.Params.extendedparameters : "",
                    isSystemStorage: this.props.cellStorageId.toLowerCase() == this.defaultDeviceId || getStorageData.IsSystemStorage,
                }, () => {
                    if (this.refStorage) {
                        this.refStorage.setData(getStorageData);
                        this.refStorage.setDisabled && this.refStorage.setDisabled(this.state.isSystemStorage);
                    }
                    this.getStorageList();
                });
            }).catch((e) => {
                $$.loading(false);
            });
        } else {
            this.setState({
                isSystemStorage: false,
                storageData: this.state.storageData,
                retentions: this.state.retentions,
                advanced: false,
            }, () => {
                if (this.refStorage) {
                    this.refStorage.setData(this.state.storageData);
                }
                this.getStorageList();
            });
        }
    }

    getStorageList() {
        let isFilter = false;
        let option = {
            url: "/api/StorageDevice/GetStorageDevices",
            method: "POST",
            data: isFilter,
        };
        fetchUtility(option).then((res) => {
            this.filterStorageList = [];
            res.StorageIdAndNameList.forEach(item => {
                item.checked = false;
                if (item.Id != this.props.cellStorageId) {
                    this.filterStorageList.push(item);
                }
            });
            this.updateMoveStorageCom();
            this.setState({ isShowMoveDataRadio: res.EnableMoveToAnotherLocation });
            if (!res.EnableMoveToAnotherLocation) {
                this.firstMoveDataRuleIndex = this.state.retentions.findIndex(r => r.IsMove);
                if (this.firstMoveDataRuleIndex !== -1) {
                    const newRetentions = this.state.retentions.slice(0, this.firstMoveDataRuleIndex + 1);
                    this.setState({ retentions: [...newRetentions] });
                }
            }
        }).catch((e) => {
        });
    }

    updateMoveStorageCom() {
        let allCheckedStorageId = [];
        this.state.retentions.forEach(retention => {
            if (retention.MoveDeviceId) {
                allCheckedStorageId.push(retention.MoveDeviceId);     //all checked storage from retentions list
            }
        });

        this.state.retentions.forEach(retention => {
            var tempStorageList = [];
            RM.deepcopy(this.filterStorageList).forEach(item => {
                item.checked = item.Id == retention.MoveDeviceId ? true : false;

                if (item.checked || allCheckedStorageId.indexOf(item.Id) == -1) {
                    tempStorageList.push(item);
                }
            });
            retention.StorageList = tempStorageList;
        });

        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
    }

    getDateOptions(index, state) {
        let options = [
            { text: RMResx.RM_AR_CP_GSS_Day, value: DateUnit.Day },
            { text: RMResx.RM_AR_CP_GSS_Week, value: DateUnit.Week },
            { text: RMResx.RM_AR_CP_GSS_Month, value: DateUnit.Month },
            { text: RMResx.RM_JS_RDM_CreateRule_Unit_Years, value: DateUnit.Year },
        ];
        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.retentions[index][state] == op.value;
            return op;
        });
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }

        let storageDataObj = this.state.storageData;
        let copyRetention = RM.deepcopy(this.state.retentions);
        copyRetention.forEach(item => {
            item.StorageList = [];
            item.KeepValue = item.KeepValue === "" ? 0 : item.KeepValue;
            item.SoftDeleteKeepValue = item.SoftDeleteKeepValue === "" ? 0 : item.SoftDeleteKeepValue;
        });
        storageDataObj.ArchiveRetentionRules = copyRetention;
        if (this.refStorage) {
            let params = this.refStorage.getParams();
            params.advanced = this.state.advanced;
            if (this.state.advanced) {
                params.extendedparameters = this.state.extend;
            }
            storageDataObj.mCurrentXRI.Params = params;
            storageDataObj.mCurrentXRI.VIM = this.state.storageData.mCurrentXRI.VIM;
        }

        if (storageDataObj.Type === StorageTypeIndex.AzureBlob) {
            this.onCheckAzureRegion(storageDataObj, callback);
        } else {
            this.onCreateOrEditStorageDevice(storageDataObj, callback);
        }
    }

    onCheckAzureRegion = (storageDataObj, callback) => {
        $$.loading(true);
        let option = {
            url: `/api/StorageDevice/CheckAzureRegion?accessPoint=${this.state.storageData.mCurrentXRI.Params.accesspoint}&accountName=${this.state.storageData.mCurrentXRI.Params.name}&storageDeviceId=${this.state.storageData.Id}`,
            method: "Get",
        };

        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == MessageType.Successful) {
                this.onCreateOrEditStorageDevice(storageDataObj, callback);
            } else if (result.MessageType == MessageType.Failed) {
                $$.messagedialog(true, {
                    width: "550px",
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: RMResx.RM_AR_Storage_DC_Unmatch_WarnMessage,
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                                $$.messagedialog(false);
                                return;
                            }
                        },
                        {
                            text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                                $$.messagedialog(false);
                                this.onCreateOrEditStorageDevice(storageDataObj, callback);

                            }
                        }
                    ]
                });

            } else if (result.MessageType == MessageType.Exception) {
                showToast.error(RMResx.RM_AR_Storage_Unknow_ErrorMessage);
                this.onCreateOrEditStorageDevice(storageDataObj, callback);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onCreateOrEditStorageDevice = (storageDataObj, callback) => {
        $$.loading(true);
        let option = {
            url: '/api/StorageDevice/CreateOrEditStorageDevice',
            method: "Post",
            data: storageDataObj
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == MessageType.Successful) {
                callback(true, storageDataObj);
                showToast.success(RMResx.RM_AR_CP_GSS_SaveStorage_Successful);
            } else {
                showToast.error(result.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onStorageNameChanged = (value) => {
        this.state.storageData.Name = value.trim();
        this.setState({ storageData: RM.deepcopy(this.state.storageData) });
    }

    onDescriptionChange = (des) => {
        this.state.storageData.Description = des.trim();
        this.setState({ storageData: RM.deepcopy(this.state.storageData) });
    }

    onStorageTypeChanged = (args) => {
        args.oldValue.checked = false;
        this.state.storageData.Type = args.newValue.index;
        this.state.storageData.mCurrentXRI.VIM = args.newValue.vim[0];
        this.setState({
            storageData: RM.deepcopy(this.state.storageData),
            retentions: [RM.deepcopy(this.retentionDefaultObj)],
            extend: "",
        }, () => {
            if (this.refStorage) {
                this.state.storageData.mCurrentXRI.Params = {};
                this.refStorage.setData(this.state.storageData);
            }
        });
    }

    onAdvancedChanged = (args) => {
        this.setState({ advanced: args });
    }

    onSelectCustomizedRegion = (extendValue) => {
        this.setState({
            advanced: true,
            extend: extendValue,
        });
    }

    onExtendChange = (args) => {
        this.setState({ extend: args });
    }

    onEnableRetentionChanged(index, args) {
        let indexRetention = this.state.retentions[index];

        // RECO-24997 兼容storage默认显示years
        if (!indexRetention.SetupDataRetention && args) {
            indexRetention.ArchiveDateUnit = DateUnit.Year;
        }

        indexRetention.SetupDataRetention = args;
        if (!args) {
            indexRetention.StorageList.forEach(item => {
                item.checked = false;
            });

            indexRetention.RetentionDataTimeType = RetentionDataTimeRadioValue.ArchivedTime;
            indexRetention.KeepValue = "";
            indexRetention.ArchiveDateUnit = DateUnit.Year;
            indexRetention.DeleteTheData = true;
            indexRetention.RemoveOrphanedStub = false;
            indexRetention.RemoveTheJob = false;
            indexRetention.IsSoftDelete = false;
            indexRetention.SoftDeleteKeepValue = "";
            indexRetention.SoftDeleteDateUnit = DateUnit.Week;
            indexRetention.IsMarkDataTier = false;
            indexRetention.TierType = TierValue.ColdTier;
            indexRetention.IsMove = false;
            indexRetention.MoveDeviceId = "";
        } else {
            indexRetention.RemoveOrphanedStub = true;
        }
        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
    }

    onKeepValueChange(index, value) {
        let indexRetention = this.state.retentions[index];
        indexRetention.KeepValue = value;
        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
        this.setState({ radioChange: true });
    }

    onSoftDeleteKeepValueChange(index, value) {
        const indexRetention = this.state.retentions[index];
        indexRetention.SoftDeleteKeepValue = value;
        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
    }

    onDateSelChange(index, args) {
        let indexRetention = this.state.retentions[index];
        indexRetention.ArchiveDateUnit = args.newValue.value;
        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
    }

    onSoftDeleteDateUnitChange(index, args) {
        const indexRetention = this.state.retentions[index];
        indexRetention.SoftDeleteDateUnit = args.newValue.value;
        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
    }

    onTierRadioChanged(index, args) {
        let indexRetention = this.state.retentions[index];
        indexRetention.TierType = args;
        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
    }

    onRetentionDataTimeRadioChanged(index, args) {
        let indexRetention = this.state.retentions[index];
        indexRetention.RetentionDataTimeType = args;
        indexRetention.RemoveTheJob = false;
        if (args === RetentionDataTimeRadioValue.ModifiedTime) {
            indexRetention.DeleteTheData = true;
            indexRetention.IsMarkDataTier = false;
            indexRetention.IsMove = false;

            if (this.state.retentions[index + 1] && !this.state.retentions[index + 1].SetupDataRetention) {
                this.state.retentions.splice(index + 1, 1);
            }
        }
        this.setState({
            retentions: RM.deepcopy(this.state.retentions)
        }, () => {
            $$.verify(this.keepDataValidation);
        });
    }

    onDataRadioChanged(index, args) {
        let indexRetention = this.state.retentions[index];
        indexRetention.RemoveOrphanedStub = true;
        indexRetention.RemoveTheJob = false;
        indexRetention.IsSoftDelete = false;
        if (args == DataRadioValue.DelData) {
            indexRetention.DeleteTheData = true;
            indexRetention.IsMarkDataTier = false;
            indexRetention.IsMove = false;
            indexRetention.MoveDeviceId = "";
            indexRetention.StorageList.forEach(item => {
                item.checked = false;
            });
            if (this.state.retentions[index + 1] && !this.state.retentions[index + 1].SetupDataRetention) {
                this.state.retentions.splice(index + 1, 1);
            }
        } else if (args == DataRadioValue.MarkDataTier) {
            indexRetention.DeleteTheData = false;
            indexRetention.IsMarkDataTier = true;
            indexRetention.TierType = TierValue.ColdTier;
            indexRetention.IsMove = false;
            indexRetention.MoveDeviceId = "";
            indexRetention.StorageList.forEach(item => {
                item.checked = false;
            });

            let add = RM.deepcopy(this.retentionDefaultObj);
            this.state.retentions.push(add);
        } else {
            indexRetention.DeleteTheData = false;
            indexRetention.IsMarkDataTier = false;
            indexRetention.IsMove = true;

            if (indexRetention.MoveDeviceId == "" && this.state.retentions[index + 1] && !this.state.retentions[index + 1].SetupDataRetention) {
                this.state.retentions.splice(index + 1, 1);
            }
        }
        this.updateMoveStorageCom();
    }

    onRemoveStubChanged(index, args) {
        let indexRetention = this.state.retentions[index];
        indexRetention.RemoveOrphanedStub = args;
        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
    }

    onRemoveJobChanged(index, args) {
        let indexRetention = this.state.retentions[index];
        indexRetention.RemoveTheJob = args;
        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
    }

    onSoftDeleteChanged(index, args) {
        let indexRetention = this.state.retentions[index];

        if (!args) {
            indexRetention.SoftDeleteKeepValue = "";
            indexRetention.SoftDeleteDateUnit = DateUnit.Week;
        }

        indexRetention.IsSoftDelete = args;
        this.setState({ retentions: RM.deepcopy(this.state.retentions) });
    }

    onMoveDeviceChanged(index, args) {
        let indexRetention = this.state.retentions[index];
        indexRetention.MoveDeviceId = args.newValue.Id;
        if (args.newValue.Id && args.oldValue == null) {
            let add = RM.deepcopy(this.retentionDefaultObj);
            this.state.retentions.push(add);
        }

        this.updateMoveStorageCom();
    }

    getNumberStr(index) {
        let numberStr = RMResx.RM_AR_CP_GSS_EnableRetention;
        if (index > 0) {
            let number = index + 1;
            let remainder = number % 10;
            if (remainder == 1) {
                number = number + RMResx.RM_AR_CP_GSS_Retention_Number_ST;
            } else if (remainder == 2) {
                number = number + RMResx.RM_AR_CP_GSS_Retention_Number_ND;
            } else if (remainder == 3) {
                number = number + RMResx.RM_AR_CP_GSS_Retention_Number_RD;
            } else {
                number = number + RMResx.RM_AR_CP_GSS_Retention_Number_TH;
            }
            numberStr = <$g.I18NProvider msg={RMResx.RM_AR_CP_GSS_EnableRetention_Others}>{number}</$g.I18NProvider>;
        }
        return numberStr;
    }

    setDisabled(index) {
        if (!this.state.isShowMoveDataRadio && index === this.firstMoveDataRuleIndex) {
            this.firstMoveDataRuleIndex = -1;
            return false;
        }
        if (this.state.retentions[index + 1] && this.state.retentions[index + 1].SetupDataRetention) {
            return true;
        } 
        if (this.state.retentions[index + 1] && !this.state.retentions[index + 1].SetupDataRetention) {
            if (!this.state.retentions[index].SetupDataRetention) {
                this.state.retentions.splice(index + 1, 1);
            }
            return false;
        }
    }

    isAzureStorage(device) {
        let isAzure = false;
        let deviceType = null;
        device.StorageList && device.StorageList.forEach(item => {
            if (item.Id == device.MoveDeviceId) {
                deviceType = item.Type;
            }
        });
        if (deviceType == StorageTypeIndex.AzureBlob) {
            isAzure = true;
        }
        return isAzure;
    }

    customVerify(index, value) {
        if (!this.state.radioChange && !this.state.isSystemStorage && value) { return true; }
        let indexRetention = this.state.retentions[index];
        let archiveDateUnit = indexRetention.ArchiveDateUnit;
        let isCheckedArchivedTime = indexRetention.RetentionDataTimeType === RetentionDataTimeRadioValue.ArchivedTime;
        if (!value) {
            return RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"];
        } else if (this.state.isSystemStorage && index == 0 && !RM.gData.disableRetentionPeriodLimitation && isCheckedArchivedTime &&
            ((archiveDateUnit == DateUnit.Day && value < 91) || (archiveDateUnit == DateUnit.Week && value < 13) || (archiveDateUnit == DateUnit.Month && value < 4))
        ) {
            return RMResx.RM_JS_RetentionRule_ValueError;
        }
        return true;
    }

    customVerifySoftDelete(retention, value) {
        if (!retention.IsSoftDelete || value) {
            return true;
        }
        return RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"];
    }

    renderRetentionDataTime(retention, index) {
        if (RM.gData.enableFilelevelBackup && this.state.storageData.Type === StorageTypeIndex.AzureBlob && index === 0) {
            return <div className="storagePanel-radio">
                <div>
                    <R.Radio
                        name={"radioTime" + index}
                        text={RMResx.RM_AR_CP_GSS_Retention_ArchivedTime}
                        value={RetentionDataTimeRadioValue.ArchivedTime}
                        checked={retention.RetentionDataTimeType === RetentionDataTimeRadioValue.ArchivedTime}
                        onChange={this.onRetentionDataTimeRadioChanged.bind(this, index)}
                        disabled={this.setDisabled(index)}
                    />
                </div>
                <div className="margin-top-s">
                    <R.Radio
                        name={"radioTime" + index}
                        text={RMResx.RM_AR_CP_GSS_Retention_ModifiedTime}
                        value={RetentionDataTimeRadioValue.ModifiedTime}
                        checked={retention.RetentionDataTimeType === RetentionDataTimeRadioValue.ModifiedTime}
                        onChange={this.onRetentionDataTimeRadioChanged.bind(this, index)}
                        disabled={this.setDisabled(index)}
                    />
                </div>
            </div>
        } else {
            return <div className="ra-storagePanel-title" tabIndex="0">{RMResx.RM_AR_CP_GSS_Retention_ArchivedTime}</div>
        }
    }

    renderRetentions(retention, index) {
        let supportOption = !RM.gData.enableFilelevelBackup || (RM.gData.enableFilelevelBackup && retention.RetentionDataTimeType === RetentionDataTimeRadioValue.ArchivedTime);
        return <div className="ra-storagePanel-content-popbottom" key={`retention_${index}`}>
            <div id="raRetentions">
                <R.Checkbox
                    id="raRetentionRuleChk"
                    text={this.getNumberStr(index)}
                    title={this.getNumberStr(index)}
                    disabled={this.setDisabled(index)}
                    checked={retention.SetupDataRetention}
                    onChange={this.onEnableRetentionChanged.bind(this, index)}
                />
            </div>
            <$g.Popover>{RMResx["Gui.Common_3bfd5492-2d4f-4c09-87fb-12018bcea07d"]}</$g.Popover>
            {retention.SetupDataRetention && <div className="ra-storagePanel-retention">
                {this.renderRetentionDataTime(retention, index)}
                <div className="storagePanel-line-top">
                    <div className="storagePanel-label" >{RMResx["Gui.Common_Keep the last"]}</div>
                    <R.Validation
                        element="Input"
                        rules={{
                            customVerify: this.customVerify.bind(this, index),
                        }}
                    >
                        <span ref={r => this.keepDataValidation = r}>
                            <R.Input
                                id="raCPLastNumIpt"
                                type="number"
                                hasControl
                                width={100}
                                min={1}
                                value={retention.KeepValue}
                                onChange={this.onKeepValueChange.bind(this, index)}
                                aria={{ ariaLabel: RMResx["Gui.Common_Keep the last"] }} />
                            <div className="inline-block margin-left-8">
                                <R.Combobox
                                    id="raCPLastCom"
                                    width={170}
                                    searchable={false}
                                    textField='text'
                                    valueField='value'
                                    checkedField='checked'
                                    items={this.getDateOptions(index, "ArchiveDateUnit")}
                                    onChange={this.onDateSelChange.bind(this, index)}
                                />
                            </div>
                        </span>
                    </R.Validation>
                </div>
                <div className="storagePanel-radio">
                    <div className="ra-storagePanel-title margin-bottom-s" tabIndex="0">{RMResx.RM_AR_CP_GSS_OperateDataTitle}</div>
                    <div>
                        <R.Radio
                            name={"radioDeleteData" + index}
                            text={RMResx["Gui.Common_Delete the data"]}
                            value={DataRadioValue.DelData}
                            checked={retention.DeleteTheData}
                            onChange={this.onDataRadioChanged.bind(this, index)}
                            disabled={this.setDisabled(index)}
                        />
                    </div>
                    {retention.DeleteTheData && <div>
                        <div className="storagePanel-radio storagePanel-checkbox">
                            <div className="storagePanel-checkbox-remove">
                                <R.Checkbox
                                    id="raStorageRemoveStub"
                                    text={RMResx.RM_AR_CP_GSS_Retention_RemoveStub}
                                    title={RMResx.RM_AR_CP_GSS_Retention_RemoveStub}
                                    checked={retention.RemoveOrphanedStub}
                                    onChange={this.onRemoveStubChanged.bind(this, index)}
                                />
                            </div>
                            {supportOption && <div>
                                <R.Checkbox
                                    id="raStorageRemoveJob"
                                    text={RMResx.RM_AR_CP_GSS_Retention_RemoveJob}
                                    title={RMResx.RM_AR_CP_GSS_Retention_RemoveJob}
                                    checked={retention.RemoveTheJob}
                                    onChange={this.onRemoveJobChanged.bind(this, index)}
                                />
                            </div>}
                            {RM.gData.enableSoftDelete && !this.state.isSystemStorage && <>
                                <div className="margin-top-xs">
                                    <R.Checkbox
                                        id="raStorageSoftDelete"
                                        text={RMResx.RM_AR_CP_GSS_Retention_SoftDelete}
                                        title={RMResx.RM_AR_CP_GSS_Retention_SoftDelete}
                                        checked={retention.IsSoftDelete}
                                        onChange={this.onSoftDeleteChanged.bind(this, index)}
                                    />
                                    <$g.Popover>{RMResx.RM_AR_CP_GSS_Retention_SoftDeleteDes}</$g.Popover>
                                </div>
                                {retention.IsSoftDelete && (
                                    <div className="storagePanel-line-top">
                                        <R.Validation
                                            element="Input"
                                            rules={{
                                                customVerify: this.customVerifySoftDelete.bind(this, retention),
                                            }}
                                        >
                                            <$g.I18NProvider msg={RMResx.RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast}>
                                                <span ref={r => this.keepDataValidation = r} style={{ margin: "0 6px" }}>
                                                    <R.Input
                                                        id="raCPSoftDeleteLastNumIpt"
                                                        type="number"
                                                        hasControl
                                                        width={100}
                                                        min={1}
                                                        value={retention.SoftDeleteKeepValue}
                                                        onChange={this.onSoftDeleteKeepValueChange.bind(this, index)}
                                                        aria={{ ariaLabel: RMResx.RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast }} />
                                                    <div className="inline-block margin-left-8">
                                                         <R.Combobox
                                                            id="raCPSoftDeleteLastCom"
                                                            width={170}
                                                            searchable={false}
                                                            textField='text'
                                                            valueField='value'
                                                            checkedField='checked'
                                                            items={this.getDateOptions(index, "SoftDeleteDateUnit")}
                                                            onChange={this.onSoftDeleteDateUnitChange.bind(this, index)}
                                                        />
                                                    </div>
                                                </span>
                                            </$g.I18NProvider>
                                        </R.Validation>
                                    </div>
                                )}
                            </>}
                        </div>
                    </div>}
                </div>
                {!this.state.isSystemStorage && this.state.storageData.Type == StorageTypeIndex.AzureBlob && supportOption && <div className="storagePanel-radio">
                    <R.Radio
                        name={"radioMarkDataTier" + index}
                        text={RMResx.RM_AR_CP_GSS_Retention_MarkDataTier}
                        value={DataRadioValue.MarkDataTier}
                        checked={retention.IsMarkDataTier}
                        onChange={this.onDataRadioChanged.bind(this, index)}
                        disabled={this.setDisabled(index)}
                    />
                    {retention.IsMarkDataTier && <div className="ra-storagePanel-moveCom">
                        <div>
                            <R.Radio
                                name={"radioTier" + index}
                                text={RMResx.RM_AR_CP_GSS_Retention_ColdTier}
                                value={TierValue.ColdTier}
                                checked={retention.TierType === TierValue.ColdTier}
                                onChange={this.onTierRadioChanged.bind(this, index)}
                                disabled={this.setDisabled(index)}
                            />
                        </div>
                        <div className="margin-top-s">
                            <R.Radio
                                name={"radioTier" + index}
                                text={RMResx.RM_AR_CP_GSS_Retention_ArchiveTier}
                                value={TierValue.ArchivedTier}
                                checked={retention.TierType === TierValue.ArchivedTier}
                                onChange={this.onTierRadioChanged.bind(this, index)}
                                disabled={this.setDisabled(index)}
                            />
                        </div>
                    </div>}
                </div>}
                {this.state.storageData.Type != StorageTypeIndex.Box && supportOption && this.state.isShowMoveDataRadio && <div className="storagePanel-radio">
                    <div className="ra-storagePanel-moveRadio">
                        <R.Radio
                            name={"radioDeleteData" + index}
                            text={RMResx.RM_AR_CP_GSS_Retention_MoveDataRadio}
                            value={DataRadioValue.MoveData}
                            checked={retention.IsMove}
                            onChange={this.onDataRadioChanged.bind(this, index)}
                            disabled={this.setDisabled(index)}
                        />
                        <$g.Popover>{RMResx.RM_AR_CP_GSS_Retention_MoveDataRadioMsg}</$g.Popover>
                    </div>
                    {retention.IsMove && <div className="ra-storagePanel-moveCom1">
                        <R.Validation
                            element="Combobox"
                            require={RMResx.RM_AR_CP_Common_SelEmpty}
                        >
                            <R.Combobox
                                id="raStorageMoveData"
                                tooltipField="Name"
                                width='100%'
                                textField="Name"
                                valueField="Id"
                                checkedField="checked"
                                noneText={RMResx["Gui.Common_Select One"]}
                                linkMode={false}
                                searchable={false}
                                items={retention.StorageList}
                                onChange={this.onMoveDeviceChanged.bind(this, index)} />
                        </R.Validation>
                    </div>}
                </div>}
            </div>}
        </div>;
    }

    render() {
        let userGuideLink = RM.gData.enviromentName == Enviroments.ChinaNorth ? "https://cdn.avepoint.com/pdfs/cn/user_guides/AvePoint_Opus_User_Guide.pdf" : getUserGuildTagPage(storageKeys.storageConfiguration);
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className={this.state.isShowWarnMsg.show ? "margin-bottom-m" : ""}>
                        <R.Messagebar
                            classify="warn"
                            message={this.state.tipMsg}
                            status={this.state.isShowWarnMsg}
                            hasClose={false}
                        />
                    </div>
                    <div className="ra-storagePanel-content">
                        <div className="ra-storagePanel-title require">{RMResx.RM_AR_CP_GSS_Name}</div>
                        <R.Validation
                            element="Input"
                            require={RMResx.RM_AR_CP_Common_NameEmpty} >
                            <R.Input
                                id="raStorageSettingsNameIpt"
                                type="text"
                                disabled={this.state.isSystemStorage}
                                value={this.state.storageData.Name}
                                onChange={this.onStorageNameChanged}
                                aria={{ ariaLabel: RMResx.RM_AR_CP_GSS_Name }}
                            />
                        </R.Validation>
                    </div>
                    <div className="ra-storagePanel-content">
                        <div className="ra-storagePanel-title">{RMResx.RM_AR_CP_GSS_Description}</div>
                        <R.Input
                            id="raStorageSettingsDesIpt"
                            type="textarea"
                            className="resizable"
                            disabled={this.state.isSystemStorage}
                            value={this.state.storageData.Description}
                            onChange={this.onDescriptionChange}
                            aria={{ ariaLabel: RMResx.RM_AR_CP_GSS_Description }}
                        />
                    </div>
                    {!this.state.isSystemStorage && <div>
                        <div className="ra-storagePanel-content">
                            <div tabIndex={0} className="ra-storagePanel-title require">{RMResx["Gui.Common_Storage Type"]}</div>
                            <$g.Popover>{RMResx["Gui.Common_Specify a storage type for the physical device you are about to create."]}</$g.Popover>
                            <R.Combobox
                                id="raStorageTypeCom"
                                tooltipField="name"
                                width='100%'
                                textField="name"
                                valueField="value"
                                checkedField="checked"
                                linkMode={false}
                                searchable={false}
                                disabled={this.state.storageTypeDisable}
                                items={this.state.storageTypeList}
                                onChange={this.onStorageTypeChanged}
                            />
                        </div>
                        <div className="ra-storagePanel-content">
                            <div tabIndex={0} className="ra-storagePanel-title require">{RMResx.RM_AR_CP_GSS_StorageConfigure}</div>
                            {this.state.storageData.Type != StorageTypeIndex.AzureBlob && <$g.Popover>
                                <$g.I18NProvider msg={RMResx["Gui.Common_Configure the specific required configurations for the storage type specified above."] + " " + RMResx.RM_AR_CP_GSS_StorageConfigure_FirewallTips}>
                                    <a className="ra-link-a" href={userGuideLink} target="_blank">
                                        {RMResx.RM_AR_CP_Stub_Type_AspxGuide}
                                    </a>
                                </$g.I18NProvider>
                            </$g.Popover>}
                            {this.state.storageData.Type == StorageTypeIndex.AzureBlob && <$g.Popover>
                                <$g.I18NProvider msg={RMResx["Gui.Common_Configure the specific required configurations for the storage type specified above."] + RMResx["Gui.Common_0cbff17b-0c23-4a81-8f59-a8d838aee2ab"] + " " + RMResx.RM_AR_CP_GSS_StorageConfigure_FirewallTips}>
                                    <a className="ra-link-a" href={userGuideLink} target="_blank">
                                        {RMResx.RM_AR_CP_Stub_Type_AspxGuide}
                                    </a>
                                </$g.I18NProvider>
                            </$g.Popover>}

                            {this.state.storageData.Type == StorageTypeIndex.Amazon && <Amazon ref={r => this.refStorage = r} onSelectCustomizedRegion={this.onSelectCustomizedRegion}></Amazon>}
                            {[StorageTypeIndex.S3Compatible, StorageTypeIndex.WasabiS3Compatible].includes(this.state.storageData.Type) && <CompatibleStorage ref={r => this.refStorage = r}></CompatibleStorage>}
                            {this.state.storageData.Type == StorageTypeIndex.Box && <Box ref={r => this.refStorage = r}></Box>}
                            {this.state.storageData.Type == StorageTypeIndex.Dropbox && <Dropbox ref={r => this.refStorage = r}></Dropbox>}
                            {this.state.storageData.Type == StorageTypeIndex.FTP && <FTP ref={r => this.refStorage = r}></FTP>}
                            {this.state.storageData.Type == StorageTypeIndex.AzureBlob && <AzureBlob ref={r => this.refStorage = r}></AzureBlob>}
                            {this.state.storageData.Type == StorageTypeIndex.NetApp_Alta_Vault && <NetApp ref={r => this.refStorage = r}></NetApp>}
                            {this.state.storageData.Type == StorageTypeIndex.Rackspace && <Rackspace ref={r => this.refStorage = r}></Rackspace>}
                            {this.state.storageData.Type == StorageTypeIndex.SFTP && <SFTP ref={r => this.refStorage = r}></SFTP>}
                            {this.state.storageData.Type == StorageTypeIndex.Google && <Google ref={r => this.refStorage = r}></Google>}
                        </div>
                        <div className="ra-storagePanel-content">
                            <R.Checkbox
                                id="raAdvancedChk"
                                text={RMResx["Gui.Common_Advanced"]}
                                title={RMResx["Gui.Common_Advanced"]}
                                disabled={this.state.isSystemStorage}
                                checked={this.state.advanced}
                                onChange={this.onAdvancedChanged}
                            />
                            {this.state.advanced && <div className="ra-storagePanel-extend">
                                <div className="ra-storagePanel-title">{RMResx["Gui.Common_5514307E-E936-44C9-811D-7D1DDA6667A4"]}</div>
                                <R.Validation
                                    element="Input"
                                    require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                    <R.Input
                                        id="raStorageSettingsExtendIpt"
                                        type="textarea"
                                        value={this.state.extend}
                                        onChange={this.onExtendChange}
                                        aria={{ ariaLabel: RMResx["Gui.Common_5514307E-E936-44C9-811D-7D1DDA6667A4"] }}
                                    />
                                </R.Validation>
                            </div>}
                        </div>
                    </div>}

                    {/* retention map */}
                    {this.state.retentions.map((retention, index) => {
                        return this.renderRetentions(retention, index);
                    })}
                </div>
            </R.Validation>
        </div>;
    }
}