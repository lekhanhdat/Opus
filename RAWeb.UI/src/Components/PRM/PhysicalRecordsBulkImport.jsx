import { Component } from 'react';
import SiteMapLinks from "../../Constants/SiteMapLinks";
import RouterUrls from '../../Constants/RouterUrls';
import { bindEvents, setCheckedStatus } from '../../Utilities/CommonUtil';
import '../../Less/PRM/PhysicalRecordsBulkImport.less';

export default class PhysicalRecordsBulkImport extends Component {
    constructor(props) {
        super(props);
        this.initBindings();
        this.state = {
            showTip: false,
            tipType: 'error',
            tipMsg: "",
            physicalLibraryInfos: [],
            selectedPhysicalLibrary: null,
            importFileName: null
        };

        this.getPhysicalLibraryInfos();
    }

    initBindings() {
        bindEvents(this, "onPhysicalLibraryChange", "onSaveClick", "onCancelClick", "onImportFileBtnClick",
            "onDownloadClick", "onChooseFileChange", "onDelSelectedFileClick", "hideTopMessage", "onSelPhysiLibChange");
    }

    getPhysicalLibraryInfos() {
        $$.loading(true);
        $.ajax({
            type: "GET",
            url: "/api/PhysicalRecordsBulkImportApi/GetPhysicalLibraryInfos",
            data: [],
            success: (data) => {
                $$.loading(false);
                this.setState({
                    physicalLibraryInfos: $.parseJSON(data) // Fortify Issue Type: JSON Injection; Sink Details: phy data; Ignore Reason: 前后台对象存在对应关系
                });
            },
            error: (msg) => {
            },
            dataType: "json"
        });
    }

    onChooseFileChange(e) {
        let file = this.chooseFileInput.files[0];
        if (file) {
            this.setState({ importFileName: file.name });
        }
    }

    onPhysicalLibraryChange(e, args) {
        let newItem = args.newValue;
        this.setState({ selectedPhysicalLibrary: newItem });
    }

    onSelPhysiLibChange(e) {
    }

    onImportFileBtnClick(e) {
        this.chooseFileInput.click();
    }

    onDownloadClick(e) {
        this.downloadForm.submit();
    }

    onDelSelectedFileClick(e) {
        this.chooseFileInput.value = "";
        this.setState({ importFileName: null });
    }

    onSaveClick(e) {
        if (!this.state.selectedPhysicalLibrary) {
            this.showTopMessage("error", RMResx.RM_JS_ImportPhsicalRecord_NoSelectLibrary);
            return;
        }
        let filePath = this.chooseFileInput.value;
        if (!filePath) {
            return;
        }
        let fileType,
            fileSize = this.chooseFileInput.files[0].size;
        if (filePath.lastIndexOf(".") != -1) {
            fileType = (
                filePath.substring(
                    filePath.lastIndexOf(".") + 1,
                    filePath.length)
            ).toLowerCase();
        }
        if (fileType != 'csv' || parseInt(fileSize / (1024 * 1024), 10) > 20) {
            this.showTopMessage("error", RMResx.RM_JS_ImportPhsicalRecord_FileTypeOrSizeError);
            return;
        }
        this.hideTopMessage();
        var ajaxFormOption = {
            type: "POST",
            url: "/PRM/ImportData",
            beforeSubmit: function () {
                $$.loading(true);
            },
            success: (data) => {
                $$.loading(false);
                var dataObj = JSON.parse(data);
                if (dataObj.id != null) {
                    this.showTopMessage(
                        'success',
                        <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                }
                else {
                    this.showTopMessage("error", RMResx.RM_JS_ImportPhsicalRecord_FileTypeOrSizeError);
                }
            },
            error: (msg) => {
                $$.loading(false);
            }
        };
        $(this.importFileForm).ajaxSubmit(ajaxFormOption);
    }

    onCancelClick(e) {
        this.props.history.push({
            pathname: RouterUrls.Home
        });
    }

    showTopMessage(type, msg) {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideTopMessage() {
        this.setState({
            showTip: false
        });
    }

    renderTopMessage() {
        if (this.state.showTip) {
            return <R.Messagebar message={this.state.tipMsg} classify={this.state.tipType}
                onClose={this.hideTopMessage} status={{ show: true }} />;
        }
        return null;
    }

    render() {
        let selPhysiLib = this.state.selectedPhysicalLibrary,
            selPhysiLibId = !selPhysiLib ? " " : selPhysiLib.value;
        return <div id="rmPhysicalRecordsBulkImport">
            <$g.SiteMap data={[SiteMapLinks.PRM_PhysicalRecordsBulkImport]} />
            {this.renderTopMessage()}
            <div id="container">
                <div
                    tabIndex="0"
                    style={{ marginLeft: "0" }}
                    className="ra-page-content1">
                    {RMResx.RM_JS_ImportPhsicalRecord_Desc}
                </div>
                <div className="ra-page-title1 ra-require" tabIndex="0">{RMResx.RM_JS_ImportPhsicalRecord_ChooseLibrary}</div>
                <div className="ra-page-content1">
                    <R.Combobox
                        width='550px'
                        disabled={this.state.physicalLibraryInfos.length == 0}
                        textField='name'
                        checkedField='checked'
                        searchPlaceholder={RMResx.RM_JS_ImportPhsicalRecord_Water}
                        items={this.state.physicalLibraryInfos}
                        onChange={this.onPhysicalLibraryChange}
                    />
                </div>
                <div className="ra-page-title1 ra-require" tabIndex="0">{RMResx.RM_JS_ImportPhsicalRecord_Tip}</div>
                <div className="ra-page-content1">
                    <div id="import">
                        <R.Button
                            text={RMResx.RM_JS_ImportPhsicalRecord_Import}
                            onClick={this.onImportFileBtnClick} />
                        <form id="importFileForm" encType="multipart/form-data" action="" method="post"
                            ref={r => this.importFileForm = r}>
                            <input type="file" ref={r => this.chooseFileInput = r} id="fileUp" name="fileUp" tabIndex="-1"
                                onChange={this.onChooseFileChange} />
                            <input type="hidden" id="hSettingId" name="hSettingId"
                                value={selPhysiLibId} onChange={this.onSelPhysiLibChange} />
                        </form>
                    </div>
                </div>
                <div id="downloadTemplate">
                    <$g.IconButton iconClass="ra-iconbtn-icon-download" text={RMResx.RM_JS_ImportPhsicalRecord_DownLoad}
                        onClick={this.onDownloadClick} />
                </div>
                {this.state.importFileName &&
                    <div id="selectedImportFile">
                        <span tabIndex="0">{this.state.importFileName}</span>
                        <span id="delSelectedFile" tabIndex="0" className="ra-iconbtn-icon-del"
                            onClick={this.onDelSelectedFileClick}></span>
                    </div>
                }
            </div>
            <div id="btnGroups">
                <R.Button
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.onCancelClick} />
                <R.Button
                    primary={true}
                    classify="theme"
                    disabled={!this.state.importFileName}
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.onSaveClick} />
            </div>
            <form id="downloadForm" method="get" ref={r => this.downloadForm = r}
                action="/api/PhysicalRecordsBulkImportApi/DownloadFile"></form>
        </div>;
    }
}