import PropTypes from 'prop-types';
import {getRequestVerificationToken, showToast} from '../../Utilities/CommonUtil';
import StringUtil from "../../Utilities/StringUtil";

class Upload extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            fileTypes: this.props.fileTypes,      //限定文件的格式
            fileSize: this.props.fileSize,        //限定文件大小
            saveContentFlag: true,               //显示上传内容表示
            FileName: '',                         //上传文件名
            isNoChangeDirectSaveValue: true,    //表单提交时判断是否文件改变
            uploadLists: [],
            acceptFileTypes: this.props.acceptFileTypes,
            uploadV3Lists: [],
            isShowUpgradeVEOV3Dialog: false,
        };
        this.uploaderRef = React.createRef();
    }
    UNSAFE_componentWillReceiveProps(props) {
        if (props.uploadLists != this.props.uploadLists) {
            this.setState({
                uploadLists: props.uploadLists,
            });
        }
    }
    //点击上传按钮
    importFileHandler() {
        this.fileInput.click();
    }
    //选择图片调用
    chooseFileHandler() {
        //获取file对象
        let file = this.fileInput.files[0];
       
        //上传为空时
        if (!file) {
            return;
        }
        
        file.fileMessage = '';
        file.element_name = this.props.chooseFileInputName;

        //限制文件格式
        let allowFileExtNames = this.state.fileTypes.split(";");
        let isValidFileExtName = allowFileExtNames.some(t => {return file.name.toLowerCase().endsWith(t.toLowerCase());});
    
        if (!isValidFileExtName) {
            file.fileMessage = this.props.fileTypes.toLowerCase() == 'zip' ? RMResx.RM_JS_ES_FileTypeError : RMResx.RM_JS_Uploader_FileTypeError;
            if(allowFileExtNames.length == 2){
                file.fileMessage = ".The type of the file that you are trying to upload is invalid. Only the xlsx or csv file is supported. Upload a xlsx or csv file to proceed.";
            }
            this.props.chooseFileSuccess(file);
            return;
        }
        //限制文件大小
        if (file.size > this.state.fileSize * 1024 * 1024) {
            file.fileMessage = RMResx.RM_JS_ES_FileSizeError;
            this.props.chooseFileSuccess(file);
            return;
        }
        //显示上传的内容(单个)
        if (!this.props.multiple) {
            this.setState({
                saveContentFlag: true,     //文件内容显示显示隐藏
                uploadLists: [{
                    FileName: file.name,      //文件名
                    FileSize: (file.size / 1024).toFixed(2)  //文件大小
                }]

            });
        }
        //显示上传的内容(多个)
        if (this.props.multiple) {
            this.state.uploadLists.push(
                {
                    FileName: file.name,      //文件名
                    FileSize: (file.size / 1024).toFixed(2)  //文件大小
                }
            );
            this.setState({
                saveContentFlag: true,     //文件内容显示显示隐藏
                uploadLists: this.state.uploadLists
            });

        }
        this.props.chooseFileSuccess(file);
        
        if (!this.state.isShowUpgradeVEOV3Dialog) {
            // Auto set isUploadVEOV3 is false when clicked VEO v2 upload button
            if (this.props.changeUploadVEOV3) {
                this.props.changeUploadVEOV3(false);
            }
        }

        this.setState({
            isNoChangeDirectSaveValue: false,
            isShowUpgradeVEOV3Dialog: false,
        });
    }
    //删除文件
    deleteFileHandler() {
        this.setState({
            isNoChangeDirectSaveValue: false,   //当文件改变时（false）
            saveContentFlag: false,     //文件内容显示显示隐藏
            FileName: '',              //文件名
            FileSize: '',
            uploadLists: []
        });
        this.props.deleteFileSuccess('');
        this.fileInput.value = '';
    }
    // 下载
    onDownLoad() {
        this.downLoadFile(this.props.hasUpgradeVEOV3 ? this.props.downLoadTemplateV3Url : this.props.downLoadUrl);//下载地址
    }
    savedFile(){
        this.downLoadFile(this.props.savedFileUrl);//下载地址
    }

    handleFormDownloadAction = (url, requestVerificationToken) => {
        const divElement = document.getElementById("downloadDiv");
        ReactDOM.render(
            <form action={url} method='post'>
                <input name='RequestVerificationToken' type='text' value={requestVerificationToken} readOnly/>
            </form>,
            divElement
        );
        divElement.querySelector("form").submit();
        ReactDOM.unmountComponentAtNode(divElement);
    }

    downLoadFile(url){
        const requestVerificationToken = getRequestVerificationToken();
        const self = this;
        if (this.props.hasSupportShowErrorWhenDownloadFile) {
            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `RequestVerificationToken=${encodeURIComponent(requestVerificationToken)}`,
            })
                .then(function (response) {
                    return response.text().then(function (dataString) {
                        if (response.status === 400) {
                            showToast.error(dataString);
                        } else {
                            self.handleFormDownloadAction(url, requestVerificationToken);
                        }
                    });
                })
                .catch((error) => {
                    console.error("Dowload file error: ", JSON.stringify(error));
                })
        } else {
            self.handleFormDownloadAction(url, requestVerificationToken);
        }
    }
    getAcceptFileTypes(){
        return  this.props.acceptFileTypes.join(",");
    }

    onOpenUpgradeVEOV3Dialog(){
        this.setState({ isShowUpgradeVEOV3Dialog: true });
    }

    onCloseUpgradeVEOV3Dialog(){
        this.setState({ isShowUpgradeVEOV3Dialog: false });
        //TODO: call api to update template and hide upload file v3 btn
    }

    onDownLoadVEOV3(){
        this.downLoadFile(this.props.downLoadTemplateV3Url);
    }

    onUploadVEOV3(args) {
        if (args.isSucceed) {
            this.uploaderRef.current.file = args.file;
            this.uploaderRef.current.file.element_name = this.props.chooseFileInputName;
            this.uploaderRef.current.file.fileMessage = "";
        }
    };

    onDeleteVEOV3(args) {
        if (args.isSucceed) {
            this.setState({ uploadV3Lists: [] });
        }
    }

    onVEOV3Save() {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        // Auto set isUploadVEOV3 is true when clicked VEO v3 upload button
        this.props.changeUploadVEOV3(true);
        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(this.uploaderRef.current.file);
        this.fileInput.files = dataTransfer.files;
        this.chooseFileHandler();
    }

    isNeedShowVEOV3Btn(){
        if(this.props.hasVEOV3Permission && this.props.noChangeStatusHiddenInputName == 'veoIsNoChangeDirectSave') {
            return !this.props.hasUpgradeVEOV3;    
        }else{
            return false;
        }
    }

    onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    getUploadedTips = () => {
        switch (this.props.fileTypes.toLowerCase()) {
            case 'zip':
                return RMResx.RM_ES_Attachment;
            case 'csv':
                return RMResx.RM_CP_Attachment_CSV;
            default:
                return RMResx["ReportCenter.Common_0441e1aa-a422-4448-a065-560344fcd12b"];
        }
    }

    renderUpgradeVEOV3Dialog(){
        return (
            <R.Dialog
                id="UpgradeVEOV3"
                header={RMResx.RM_CP_ES_UpgradeVEOV3_Btn}
                width={745}
                height={440}
                status={{ show: this.state.isShowUpgradeVEOV3Dialog }}
                struct={{ foot: true }}
                destroy={true}
                closeable={true}
                onHide={this.onCloseUpgradeVEOV3Dialog.bind(this)}
            >
                <R.Validation>
                    <div id='esUpgradeVEOV3' ref={r => this.allValidation = r}>
                        <span className='es-veov3-desc'>{RMResx.RM_CP_ES_UpgradeVEOV3_Description}</span>
                        <span className="ra-inline-middle ra-cursor-pointer ra-link-a es-veov3-download-btn" tabIndex="0" onKeyDown={this.onKeyDown}  onClick={this.onDownLoadVEOV3.bind(this)}>
                            {RMResx.RM_ES_DownloadTemplate}
                        </span>
                        <div className='es-veov3-import-section'>
                            <span className='es-veov3-upload-title'>{StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)}</span>
                            <R.Validation element="Uploader" require={RMResx.RM_SPS_Location_SelectZipFile}>
                                <R.Uploader
                                    ref={this.uploaderRef}
                                    files={this.state.uploadV3Lists}
                                    fileTypes={["ZIP"]}
                                    onUpload={this.onUploadVEOV3.bind(this)}
                                    onDelete={this.onDeleteVEOV3.bind(this)}
                                    multiple={false}
                                />
                            </R.Validation>
                        </div>
                    </div>
                </R.Validation>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onVEOV3Save.bind(this)} />
            </R.Dialog>
        );
    }

    //view
    render() {
        let showDownloadTemplate = !this.props.hideDownloadTemplate;
        let requireZipFile = this.props.fileTypes.toLowerCase() == 'zip';
        let uploadedTip = this.getUploadedTips();
        let fileMaxSizeTip = requireZipFile ? RMResx.RM_ES_UploadFileMaxSize : (<$g.I18NProvider msg={RMResx.RM_JS_FileUploader_TipMsg}>
            <>{this.state.fileSize}</>
            <>{this.state.fileTypes}</>
        </$g.I18NProvider>);
        return <React.Fragment>
            {showDownloadTemplate && <a className="ra-inline-middle ra-cursor-pointer ra-link-a" tabIndex="0" onKeyDown={this.onKeyDown} onClick={this.onDownLoad.bind(this)}>
                    <i className="fia-download template-down-icon"></i>
                    {RMResx.RM_ES_DownloadTemplate}
                </a>
            }
            <div className="margin-top-16">
                <div className="upload-title">
                    <span tabIndex='0'>{uploadedTip}</span>
                </div>
                <div className={this.state.saveContentFlag && this.state.uploadLists.length > 0 ? 'block' : 'none'}>
                    {(this.state.uploadLists).map((uploadList, index) =>
                        <div key={index}>
                            <a className="ra-link-a ra-cursor-pointer" onClick={this.savedFile.bind(this)} tabIndex='0'>{uploadList.FileName}</a>
                            <span className="upload-filesize" tabIndex='0'>
                                {`${uploadList.FileSize} ${RMResx.RM_JS_ES_FileSizeUnitKB}`}
                            </span>
                            <R.Button
                                tooltip={RMResx.RM_JS_Common_Delete}
                                type="bald"
                                icon="fia-delete"
                                onClick={this.deleteFileHandler.bind(this)}
                            />
                        </div>
                    )}
                </div>
                <div className="ra-inline-middle margin-top-16 margin-bottom-16 ">
                    <R.Button
                        text={RMResx.RM_ES_UploadFile}
                        onClick={this.importFileHandler.bind(this)}/>

                    {this.isNeedShowVEOV3Btn() && (
                        <R.Button
                            className="margin-left-8"
                            text={RMResx.RM_CP_ES_UpgradeVEOV3_Btn}
                            onClick={this.onOpenUpgradeVEOV3Dialog.bind(this)}
                        />
                    )}
 
                    <span className="margin-left-8" tabIndex='0'>{fileMaxSizeTip}</span>

                    <div>
                        <input 
                            type="file" className="none" name={this.props.chooseFileInputName} 
                            ref={r => this.fileInput = r} onChange={this.chooseFileHandler.bind(this)} 
                            accept={this.getAcceptFileTypes()} />  
                        <input 
                            type="hidden" name={this.props.noChangeStatusHiddenInputName} 
                            defaultValue={this.state.isNoChangeDirectSaveValue} />
                    </div>
                </div>
            </div>
            <div id='downloadDiv' style={{display: "none"}}/>
            {this.renderUpgradeVEOV3Dialog()}
        </React.Fragment>;
    }
}
const propTypes = {
    fileTypes: PropTypes.string,
    downLoadUrl: PropTypes.string,
    uploadLists: PropTypes.array,
    multiple: PropTypes.bool,
    chooseFileInputName: PropTypes.string,
    noChangeStatusHiddenInputName: PropTypes.string,
    hideDownloadTemplate: PropTypes.bool,
    acceptFileTypes: PropTypes.array
};

const defaultProps = {
    fileTypes: '',
    downLoadUrl: '',
    uploadLists: [],
    multiple: false,
    chooseFileInputName: '',
    noChangeStatusHiddenInputName: '',
    hideDownloadTemplate: false,
    acceptFileTypes: [""]
};

Upload.propTypes = propTypes;
Upload.defaultProps = defaultProps;

export default Upload;