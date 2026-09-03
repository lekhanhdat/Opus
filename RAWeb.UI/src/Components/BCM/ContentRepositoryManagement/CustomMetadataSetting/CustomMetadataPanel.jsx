import { createRef } from "react";
import _ from "lodash";

import CustomMetadataTable from "./CustomMetadataTable";
import ManageMetadataTable from "./ManageMetadataTable";
import { SourceFlag } from "../../../Common/Constants";
import { showToast } from "../../../../Utilities/CommonUtil";
import { RAMessageType } from "../Common/CRMCommonUtil";

export default class CustomMetadataPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            isChecked: false,
            isShowManageMetadataPanel: { show: false },
            metadataList: [],
            manageMetadataList: [],
            idColumnSelectedList: [],
        };
        this.manageMetadataTableRef = createRef();
        this.customMetadataPanelRef = createRef();
    }

    componentReceive(type, args) {
        switch (type) {
            case "isOpenPanel": // Used for this panel
                this.setState({
                    isChecked: args.indexData.IsEnableCustomIndexMetadata,
                    metadataList: args.indexData.CustomIndexMetadataDtos,
                    manageMetadataList: args.manageData,
                    idColumnSelectedList: args.inUsedColumnData.map((item) => item.UniqueId),
                }, () => {
                    if (this.state.isChecked) {
                        this.dispatch("raCrmCustomMetadataTable", "didUpdate");
                    }
                });
                break;
            case "isOpenManageMetadataPanel":
                this.setState({
                    isShowManageMetadataPanel: { show: args },
                });
                break;
            case "save":
                this.handleSaveCustomMetadata();
                break;
            default:
                break;
        }
    }

    onSyncMetadataForSearchChange = (checked) => {
        this.setState({
            isChecked: checked,
        });
    };

    changeMetadataList = (arr) => {
        this.setState({ metadataList: arr });
    }

    onCancelManageMetadata = () => {
        this.setState({
            isShowManageMetadataPanel: { show: false },
        });
    };

    onSaveManageMetadata = async () => {
        const manageMetadataPanel = this.manageMetadataTableRef.current;
        if (manageMetadataPanel) {
            if (!manageMetadataPanel.isValid()) return false;
            const manageMetadataList = manageMetadataPanel.getManageMetadataList();
            const newManageMetadataList = manageMetadataList.map((item) => {
                delete item.CanAction;
                delete item.IsDefault;
                return item;
            });
            const option = {
                url: "/api/SPSettingApi/SaveCustomMetadataColumns",
                method: "POST",
                data: newManageMetadataList,
            };
            $$.loading(true);
            const res = await fetchUtility(option);
            $$.loading(false);
            if (res.MessageType === RAMessageType.Successful) {
                const res = await this.props.getCustomMetadataColumns();
                this.setState({
                    manageMetadataList: res,
                }, () => {
                    this.dispatch("raCrmCustomMetadataTable", "didUpdate");
                    this.onCancelManageMetadata();
                });
                showToast.success(RMResx.RM_JS_SP_CustomMetadata_SaveSuccess);
            } else {
                showToast.error(res.ErrorMessage);
            }
        }
    };

    onSaveCustomMetadata = async () => {
        const isExoContentSource = this.props.sourceFlag === SourceFlag.Exchange;
        if (this.state.isChecked) {
            const customMetadataPanel = this.customMetadataPanelRef.current;
            if (customMetadataPanel && !customMetadataPanel.isValid()) return false;
        }
        const newMetadataList = _.cloneDeep(this.state.metadataList).map((item) => {
            delete item.ColumnTypeList;
            if (item.Id) {
                return item;
            }
            return {
                SourceColumnName: item.SourceColumnName,
                TargetColumnName: item.TargetColumnName,
                TargetColumnId: item.TargetColumnId,
                ColumnType: item.ColumnType,
                ContentSource: isExoContentSource ? SourceFlag.Exchange : SourceFlag.SharePoint,
                IsUsedInLocation: false,
            }
        });
        const url = isExoContentSource ? "/api/EXOSettingApi/SaveCustomIndexMetadatas" : "/api/SPSettingApi/SaveCustomIndexMetadatas";
        const option = {
            url,
            method: "POST",
            data: {
                IsEnableCustomIndexMetadata: this.state.isChecked,
                CustomIndexMetadataDtos: newMetadataList,
            },
        };
        $$.loading(true);
        const res = await fetchUtility(option);
        $$.loading(false);
        if (res.MessageType === RAMessageType.Successful) {
            this.props.onClose();
            showToast.success(RMResx.RM_JS_SP_CustomMetadata_SaveSuccess);
        } else {
            showToast.error(res.ErrorMessage);
        }
    }

    handleSaveCustomMetadata = () => {
        if (this.state.isChecked && this.state.metadataList.length === 0) {
            const args = {
                width: "550px",
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_SP_CustomMetadata_SaveConfirmMsg,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_Cancel, onClick: () => $$.messagedialog(false),
                    },
                    {
                        id: "raCrmSaveCustomMetadata",
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: this.onSaveCustomMetadata.bind(this),
                    },
                ]
            }
            $$.messagedialog(true, args);
        } else {
            this.onSaveCustomMetadata();
        }
    }

    renderManageMetadataPanel = () => {
        return (
            <R.Panel
                header={RMResx.RM_JS_SP_CustomMetadata_ManageBtn}
                size={670}
                actionType="back"
                status={this.state.isShowManageMetadataPanel}
                destroy={true}
            >
                <ManageMetadataTable
                    ref={this.manageMetadataTableRef}
                    metadataListProps={this.state.metadataList}
                    manageMetadataListProps={this.state.manageMetadataList}
                    idColumnSelectedList={this.state.idColumnSelectedList}
                />
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.onCancelManageMetadata}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onSaveManageMetadata}
                    />
                </>
            </R.Panel>
        );
    };

    render() {
        return (
            <div>
                <div className="flex align-center gap-s" id={this.props.id}>
                    <span>
                        <R.Switch
                            id="raCrmSyncMetadataForSearch"
                            checked={this.state.isChecked}
                            onChange={this.onSyncMetadataForSearchChange}
                        />
                    </span>
                    <span tabIndex="0">{RMResx.RM_JS_SP_CustomMetadata_SwitchSync}</span>
                </div>
                {this.state.isChecked && (
                    <div className="margin-top-l">
                        <CustomMetadataTable
                            id="raCrmCustomMetadataTable"
                            ref={this.customMetadataPanelRef}
                            metadataList={this.state.metadataList}
                            manageMetadataList={this.state.manageMetadataList}
                            setMetadataList={this.changeMetadataList}
                            sourceFlag={this.props.sourceFlag}
                        />
                    </div>
                )}
                {this.renderManageMetadataPanel()}
            </div>
        );
    }
}