import { getRequestVerificationToken, showToast } from "../../../../Utilities/CommonUtil";
import StringUtil from "../../../../Utilities/StringUtil";
import "../../../../Less/BCM/ContentRepositoryManagement/importSetting.less";
import { SourceFlags } from "../../../../Constants/Constants";

export default class ImportSCPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            files: [],
        };
        this.uploaderRef = React.createRef();
        this.source = this.props.source;
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    handleDownloadTemplate = (e) => {
        let downloadTemplate = StringUtil.newGuid();
        var $downloadStatusKey = $("#importDownloadFlag");
        let url = "/api/BCMAdminSettingApi/DownloadArchiverImportTemplate";
        if (this.source == SourceFlags.Teams) {
            url = "/api/BCMAdminSettingApi/DownloadTeamsArchiverImportTemplate";
        }
        $downloadStatusKey.val(downloadTemplate);

        $("#crm-import-sc-download")
            .attr("action", url)
            .submit();
    }

    handleUpload(args) {
        const isSucceed = args.isSucceed;
        $$.log(isSucceed ? 'uploadSuccess:' : 'uploadError', args);
        if (isSucceed) {
            args.files[0].fileId = StringUtil.newGuid();
            this.files = args.files[0];
        }
    }

    handleDelete(args) {
        const isSucceed = args.isSucceed;
        if (isSucceed) {
            this.files = null;
        }
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        let content = RMResx.RM_AR_SPS_RunImportSCJobMsg;
        if (this.source == SourceFlags.Teams) {
            content = RMResx.RM_AR_Teams_RunImportSCJobMsg;
        }
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmRunImportSCJobDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.runImportSCJobDoAction.bind(this, callback)
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    runImportSCJobDoAction(callback) {
        $$.loading(true);
        const formData = new FormData();
        let url ='/api/SPSettingApi/RunArchiverJobForImport';
        if (this.source == SourceFlags.OneDrive) {
            url = '/api/OneDriveSettingApi/RunArchiverJobForImport';
        } else if (this.source == SourceFlags.Teams) {
            url = '/api/TeamsSettingApi/RunArchiverJobForImport';
        }
        formData.append('fileUp', this.files.file, this.files.fileName);
        formData.append('selectedTree', JSON.stringify(this.props.treeData));
        fetch(url, {
            method: 'POST',
            body: formData,
        })
            .then(function (response) {
                return response.text().then(function (dataString) {
                    return {
                        responseStatus: response.status,
                        responseString: JSON.parse(dataString)
                    };
                });
            })
            .then(function (data) {
                $$.loading(false);
                if (data.responseString.MessageType == 0) {
                    callback(true, formData);
                    let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(content);
                } else {
                    showToast.error(data.responseString.ErrorMessage);
                }
            });
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    render() {
        let requestVerificationToken = getRequestVerificationToken();
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-import-download">
                        <form id="crm-import-sc-download" method="POST" action="">
                            <input type="hidden" id="importDownloadFlag" name="importDownloadFlag" value="" />
                            <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                        </form>
                        <span className="ra-import-download-span" onClick={this.handleDownloadTemplate} tabIndex="0" onKeyDown={this.onKeyDown}>
                            {RMResx.RM_AR_SPS_DownLoadSC}
                        </span>
                    </div>
                    <div>
                        <div className="ra-import-title" tabIndex="0">
                            <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)} />
                        </div>
                        <div>
                            <R.Validation
                                element="Uploader"
                                require={RMResx.RM_JS_BCM_ImportSetting_selectCSVFile}
                            >
                                <R.Uploader
                                    ref={this.uploaderRef}
                                    files={this.state.files}
                                    fileTypes={["CSV"]}
                                    onUpload={this.handleUpload.bind(this)}
                                    onDelete={this.handleDelete.bind(this)}
                                    multiple={false}
                                />
                            </R.Validation>
                        </div>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}