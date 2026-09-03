import {Component} from "react";
import { bindEvents } from "../../../Utilities/CommonUtil";
import { getRequestVerificationToken } from "../../../Utilities/CommonUtil";
import Upload from "../../../Components/CP/Upload";
import "../../../Less/PRM/ImportTrim.less";

const UploadFileType = {
    MetaFile: 1,
    Location: 2,
    RecordsFile: 3,
    Relationship:4,
    Deletion:5
};
const {log} = console;

export default class ImportHRTRIM extends Component {
    constructor(props) {
        super(props);
        this.state = {
            fileTypes: "xlsx;csv",
            mFileSize: 5,                      //Meta文件大小要求
            lFileSize: 5,                      //Location文件大小要求
            rFileSize: 20,                     //Records文件大小要求
            showTip: false,
            tipType: "success",
            tipMsg: "",                        //提示信息
            fileMessage: "",
            //判断文件是否有变动                （true 变更    false 未变更）
            metaChooseState: false,
            locationChooseState: false,
            recordsChooseState: false,
            relatedChooseState: false,
            deletionChooseState: false
        };
        
        bindEvents(this, "onImport", "StartImportData", "showMessageTip", "hideMessageTip", "onDeleteSureClick","onDeleteCancleClick");
    }

    chooseFileSuccess(file) {
        if (file.element_name == "metaFileUp" ) {
            this.setState({
                metaChooseState: true
            });
        }else
        if (file.element_name == "locationFileUp" ) {
            this.setState({
                locationChooseState: true
            });
        } else
        if (file.element_name == "recordsFileUp" ) {
            this.setState({
                recordsChooseState: true
            });
        } else
        if (file.element_name == "relationFileUp" ) {
            this.setState({
                relatedChooseState: true
            });
        } else if (file.element_name == "deletionFileUp") {
            this.setState({
                deletionChooseState: true
            });
        }
        if (file.fileMessage) {
            this.showMessageTip("error", file.fileMessage);
        } else {
            this.hideMessageTip();
        }
    }

    metaDeleteFileSuccess() {
        this.setState({
            metaChooseState: false
        });
    }

    locationDeleteFileSuccess() {
        this.setState({
            locationChooseState: false
        });
    }

    recordsDeleteFileSuccess() {
        this.setState({
            recordsChooseState: false
        });
    }

    relatedDeleteFileSuccess() {
        this.setState({
            relatedChooseState: false
        });
    }
    deletionDeleteFileSuccess() {
        this.setState({
            deletionChooseState: false
        });
    }
    onImport(type){
        let reqInfo = {};
        switch (type) {
            case UploadFileType.MetaFile:
                reqInfo.url = "/api/ImportHPRMPhysicalApi/ImportMetaData";
                reqInfo.type = type;
                reqInfo.formId = "form-import-meta";
                reqInfo.successMsg = ".Success Import Meta Data.";
                reqInfo.errorMsg = ".Failed to Import Meta Data.";
                this.StartImportData(reqInfo);
                log("start import MetaFile");
                break;
            case UploadFileType.Location:
                reqInfo.url = "/api/LocationManagementApi/ImportData";
                reqInfo.type = type;
                reqInfo.formId = "form-import-location";
                reqInfo.successMsg = ".Success Import location Data.";
                reqInfo.errorMsg = ".Failed to Import location Data.";
                this.StartImportData(reqInfo);
                break;
            case UploadFileType.RecordsFile:
                reqInfo.url = "/api/ImportHPRMPhysicalApi/ImportData";
                reqInfo.type = type;
                reqInfo.formId = "form-import-records";
                reqInfo.errorMsg = ".Failed to Import Records Data.";
                this.StartImportData(reqInfo);
                log("start import RecordsFile");
                break;
            case UploadFileType.Relationship:
                reqInfo.url = "/api/ImportHPRMPhysicalApi/ImportRelated";
                reqInfo.type = type;
                reqInfo.formId = "form-import-relationship";
                reqInfo.successMsg = ".Success Import Relationship Data.";
                reqInfo.errorMsg = ".Failed to Import Migration Relationship.";
                this.StartImportData(reqInfo);
                log("start import RecordsFile");
                break;
            case UploadFileType.Deletion:
                reqInfo.url = "/api/ImportHPRMPhysicalApi/ImportDeletionData";
                reqInfo.type = type;
                reqInfo.formId = "form-import-deletion";
                reqInfo.successMsg = ".Success uploading number list file for deletion.";
                reqInfo.errorMsg = ".Failed to upload number list file for deletion.";
                this.StartImportData(reqInfo);
                log("start Records deletion");
                break;
        }
    }

    StartImportData(reqInfo) {
        var ajaxFormOption = {
            type: "POST",
            url: reqInfo.url,
            beforeSubmit: function () {
                $$.loading(true);
            },
            success: (data) => {
                $$.loading(false);
                if (data == "ok") {
                    if (reqInfo.type == UploadFileType.RecordsFile || reqInfo.type == UploadFileType.Deletion) {
                        this.showMessageTip(
                            "success",
                            <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                                <a style={{ color: "#0072d0" }} className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                            </$g.I18NProvider>);
                    }else{
                        this.showMessageTip("success", reqInfo.successMsg);
                    }
                }else{
                    this.showMessageTip("error", reqInfo.errorMsg);
                }
            },
            error: (msg) => {
                $$.loading(false);
                this.showMessageTip("error", reqInfo.errorMsg);
            },
        };
        $(`#${reqInfo.formId}`).ajaxSubmit(ajaxFormOption);
    }


    onRelated(baseOn) {
        $$.loading(true);
        let urlData = "/api/ImportHPRMPhysicalApi/RelatedBaseOnPhysical";
        if (baseOn != 0) { urlData = "/api/ImportHPRMPhysicalApi/RelatedBaseOnElectronic"; }
        let option = {
            url: urlData,
            method: "post"
        };
        fetchUtility(option).then((data) => {
            $$.loading(false);
            if (data == "ok") { 
                this.showMessageTip(
                    "success",
                    <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a style={{ color: "#0072d0" }} className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>); 
            } else {
                this.showMessageTip("error", data);
            }
        }).catch((e) => {
            $$.loading(false);
            this.showMessageTip("error", e.errorMsg);
        });
    }
    
    onDownloadSubFolder() {
        $$.loading(true);
        let divElement = document.getElementById("downloadDiv");
        let downloadUrl = "/api/ImportHPRMPhysicalApi/DownloadSubFolderList";
        let requestVerificationToken = getRequestVerificationToken();
        ReactDOM.render(
            <form action={downloadUrl} method='post'> 
                <input name='RequestVerificationToken' type='text' value={requestVerificationToken} readOnly />
            </form>,
            divElement
        );
        divElement.querySelector("form").submit();
        ReactDOM.unmountComponentAtNode(divElement);
        $$.loading(false);
    }

    onDeleteSubFolder() { 
        this.args = {
            // classify: "warn",
            width: "550px",
            height: "360px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div>
                    {"Are you sure you want to delete the imported sub folders?"}
                </div> 
            </div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.onDeleteCancleClick },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onDeleteSureClick },  
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onDeleteSureClick() {
        $$.loading(true);
        let urlData = "/api/ImportHPRMPhysicalApi/ClearSubFolder"; 
        let option = {
            url: urlData,
            method: "post"
        };
        fetchUtility(option).then((data) => {
            $$.loading(false);
            $$.messagedialog(false);
            if (data == "ok") {
                this.showMessageTip(
                    "success",
                    "Delete all the sub folders successfully.");
            } else {
                this.showMessageTip("error", data);
            }
        }).catch((e) => {
            $$.loading(false);
            $$.messagedialog(false);
            this.showMessageTip("error", e.errorMsg);
        });
    }
    onDeleteCancleClick() { 
        $$.messagedialog(false);
    }
    showMessageTip(type, msg) {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }
    
    hideMessageTip() {
        this.setState({showTip: false});
    }

    getAcceptFileTypes(){
        return [".csv","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"];
    }

    render() {
        return <div id="raImportPhysicalData">
            <div className="margin-bottom-l">
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{show: this.state.showTip}}
                    onClose={this.hideMessageTip}
                />
            </div>
            <div className="ra-page-main">
                {/*Step1 Import meta file*/}
                <div className="ra-section">
                    <div className="ra-section-head">
                        <span tabIndex='0'>{".Step 1"}</span>
                    </div>
                    <div className="ra-page-form">
                        <div className="ra-form-label ra-require">
                            <span tabIndex='0'>{".Import Meta File"}</span>
                        </div>
                        <form id="form-import-meta" encType="multipart/form-data" action="" method="post">

                            <div className="ra-form-content">
                                <Upload
                                    fileTypes={this.state.fileTypes}
                                    fileSize={this.state.mFileSize}
                                    multiple={false}
                                    chooseFileInputName='metaFileUp'
                                    chooseFileSuccess={this.chooseFileSuccess.bind(this)}
                                    deleteFileSuccess={this.metaDeleteFileSuccess.bind(this)}
                                    hideDownloadTemplate={true}
                                    acceptFileTypes={this.getAcceptFileTypes()}
                                >
                                </Upload>
                                <R.Button
                                    primary={true}
                                    classify="theme"
                                    text={".Import"}
                                    onClick={this.onImport.bind(this, UploadFileType.MetaFile)}/>
                            </div>
                        </form>
                    </div>
                </div>

                {/*Step 2 Import location*/}
                <div className="ra-section">
                    <div className="ra-section-head">
                        <span tabIndex='0'>{".Step 2"}</span>
                    </div>
                    <div className="ra-page-form">
                        <div className="ra-form-label ra-require">
                            <span tabIndex='0'>{".Import Physical Locations"}</span>
                        </div>
                        <form id="form-import-location" encType="multipart/form-data" action="" method="post">
                            <div className="ra-form-content">
                                <Upload
                                    fileTypes={this.state.fileTypes}
                                    fileSize={this.state.lFileSize}
                                    multiple={false}
                                    chooseFileInputName='locationFileUp'
                                    chooseFileSuccess={this.chooseFileSuccess.bind(this)}
                                    deleteFileSuccess={this.locationDeleteFileSuccess.bind(this)}
                                    hideDownloadTemplate={true}
                                    acceptFileTypes={this.getAcceptFileTypes()}
                                >
                                    {RMResx.RM_CP_GSS_StoragePolicy_Tip}
                                </Upload>
                                <R.Button
                                    primary={true}
                                    classify="theme"
                                    text={".Import"}
                                    onClick={this.onImport.bind(this, UploadFileType.Location)}/>
                            </div>
                        </form>
                    </div>
                </div>

                {/*Step 3 Import Records File*/}
                <div className="ra-section">
                    <div className="ra-section-head">
                        <span tabIndex='0'>{".Step 3"}</span>
                    </div>
                    <div className="ra-page-form">
                        <div className="ra-form-label ra-require">
                            <span tabIndex='0'>{".Import Physical Records"}</span>
                        </div>
                        <form id="form-import-records" encType="multipart/form-data" action="" method="post">
                            <div className="ra-form-content">
                                <Upload
                                    fileTypes={this.state.fileTypes}
                                    fileSize={this.state.rFileSize}
                                    multiple={false}
                                    chooseFileInputName='recordsFileUp'
                                    chooseFileSuccess={this.chooseFileSuccess.bind(this)}
                                    deleteFileSuccess={this.recordsDeleteFileSuccess.bind(this)}
                                    hideDownloadTemplate={true}
                                    acceptFileTypes={this.getAcceptFileTypes()}
                                >{RMResx.RM_CP_GSS_StoragePolicy_Tip}
                                </Upload>
                                <R.Button
                                    primary={true}
                                    classify="theme"
                                    text={".Import"}
                                    onClick={this.onImport.bind(this, UploadFileType.RecordsFile)}/>
                            </div>
                        </form>
                    </div> 

                    <div className="ra-form-label">
                        <span tabIndex='0'>{"---------------------------------------------------------------"}</span>
                    </div>
                    <div className="ra-form-label ra-require">
                        <span tabIndex='0'>{".Download or delete sub folders"}</span>
                    </div>
                    <div className="ra-page-form">
                        <R.Button
                            type="link" color="blue"
                            text={".Download"}
                            onClick={this.onDownloadSubFolder.bind(this)} />
                        <R.Button
                            type="link" color="red"
                            text={".Delete"}
                            onClick={this.onDeleteSubFolder.bind(this)} />

                        <div id='downloadDiv' style={{ display: "none" }} />
                    </div>
                </div>

                {/*Step 4 Import Records Relationship*/}
                <div className="ra-section">
                    <div className="ra-section-head">
                        <span tabIndex='0'>{".Step 4"}</span>
                    </div>
                    <div className="ra-page-form">
                        <div className="ra-form-label ra-require">
                            <span tabIndex='0'>{".Related Records Base on TRIM Physical Import"}</span>
                        </div> 
                        <div className="ra-form-content"> 
                            <R.Button
                                primary={true}
                                classify="theme"
                                text={".Related"}
                                onClick={this.onRelated.bind(this, 0)} />
                        </div> 
                    </div>
                </div>
                {/*Step 5 Import Records Relationship*/}
                <div className="ra-section">
                    <div className="ra-section-head">
                        <span tabIndex='0'>{".Step 5"}</span>
                    </div>
                    <div className="ra-page-form">
                        <div className="ra-form-label ra-require">
                            <span tabIndex='0'>{".Import Migration Relationships"}</span>
                        </div>
                        <form id="form-import-relationship" encType="multipart/form-data" action="" method="post">
                            <div className="ra-form-content">
                                <Upload
                                    fileTypes={this.state.fileTypes}
                                    fileSize={30}
                                    multiple={false}
                                    chooseFileInputName='relationFileUp'
                                    chooseFileSuccess={this.chooseFileSuccess.bind(this)}
                                    deleteFileSuccess={this.relatedDeleteFileSuccess.bind(this)}
                                    hideDownloadTemplate={true}
                                    acceptFileTypes={this.getAcceptFileTypes()}
                                >{RMResx.RM_CP_GSS_StoragePolicy_Tip}
                                </Upload>
                                <R.Button
                                    primary={true}
                                    classify="theme"
                                    text={".Import"}
                                    onClick={this.onImport.bind(this, UploadFileType.Relationship)} />
                            </div>
                        </form>
                    </div>
                    <div className="ra-page-form">
                        <div className="ra-form-label ra-require">
                            <span tabIndex='0'>{".Related Base on Migration Relationships"}</span>
                        </div> 
                        <div className="ra-form-content"> 
                            <R.Button
                                primary={true}
                                classify="theme"
                                text={".Related"}
                                onClick={this.onRelated.bind(this, 1)} />
                        </div> 
                    </div>
                </div>
                
                {/* Combined deletion */}
                <div className="ra-section">
                    <div className="ra-section-head">
                        <span tabIndex='0'>{"Imported Physical Records deletion"}</span>
                    </div>
                    <div className="ra-form-content">
                        <div className="ra-page-form">
                            <div className="ra-form-label ra-require">
                                <span tabIndex='0'>{".Upload number list and start deletion"}</span>
                            </div>
                            <form id="form-import-deletion" encType="multipart/form-data" action="" method="post">
                                <div className="ra-form-content">
                                    <Upload
                                        fileTypes={"txt"}
                                        fileSize={30}
                                        multiple={false}
                                        chooseFileInputName='deletionFileUp'
                                        chooseFileSuccess={this.chooseFileSuccess.bind(this)}
                                        deleteFileSuccess={this.deletionDeleteFileSuccess.bind(this)}
                                        hideDownloadTemplate={true}
                                        acceptFileTypes={[".txt"]}
                                    >{RMResx.RM_CP_GSS_StoragePolicy_Tip}
                                    </Upload>
                                    <R.Button
                                        primary={true}
                                        classify="theme"
                                        text={".Upload and Start Job"}
                                        onClick={this.onImport.bind(this, UploadFileType.Deletion)} />
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>;
    }
}