import { getRequestVerificationToken, showToast } from "../../../Utilities/CommonUtil";
import StringUtil from "../../../Utilities/StringUtil";
import "../../../Less/BCM/ContentRepositoryManagement/importSetting.less";
import { SourceFlags } from "../../../Constants/Constants";

export default class ImportSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            files: [],
            // uniqueIdData: {},
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
        let url = this.props.downloadTemplateUrl;
        $downloadStatusKey.val(downloadTemplate);

        $("#crm-import-download")
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
        $$.loading(true);
        const formData = new FormData();
        const url = this.props.saveSettingUrl;
        formData.append('fileUp', this.files.file, this.files.fileName);
        fetch(url, {
            method: 'POST',
            body: formData,
        })
            .then(function (response) {
                return response.text().then(function (dataString) {
                    return {
                        responseStatus: response.status,
                        responseString: dataString
                    };
                });
            })
            .then(function (data) {
                $$.loading(false);
                if (data.responseString) {
                    callback(true, formData);
                    let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(content);
                }
                // return result;
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
                        <form id="crm-import-download" method="POST" action="">
                            <input type="hidden" id="importDownloadFlag" name="importDownloadFlag" value="" />
                            <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                        </form>
                        <span className="ra-import-download-span" onClick={this.handleDownloadTemplate} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_JS_TM_DownLoadTemplate}</span>
                    </div>
                    <div>
                        <div className="ra-import-title" tabIndex="0">
                            <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)} />
                        </div>
                        <div>
                            <R.Validation
                                element="Uploader"
                                require={(this.source == SourceFlags.SP || this.source == SourceFlags.Teams) ? RMResx.RM_JS_BCM_ImportSetting_selectCSVFile : RMResx.RM_SPS_Location_NoImportFile}>
                                <R.Uploader
                                    ref={this.uploaderRef}
                                    files={this.state.files}
                                    fileTypes={(this.source == SourceFlags.SP || this.source == SourceFlags.Teams) ? ["CSV"] : ["XLSX"]}
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