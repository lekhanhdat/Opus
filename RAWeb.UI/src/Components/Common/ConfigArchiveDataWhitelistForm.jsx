import Upload from "../CP/Upload";

import "../../Less/Common/ConfigArchiveDataWhitelistForm.less";
import { RAMessageType } from "../CP/CompliantExports/Constants";

class ConfigArchiveDataWhitelistForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            fileTypes: "CSV",
            fileSize: 5,
            uploadLists: [],
            whitelistChooseState: false,
        };
    }

    componentReceive(type, args) {
        switch (type) {
            case "init":
                this.handleLoadData();
                break;
            case "save":
                this.handleSave(args);
                break;
            default:
                break;
        }
    }

    handleLoadData = () => {
        const option = {
            url: "/api/RetentionApi/GetRetentionSettings",
            method: "GET",
        };
        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                if (res.FileName || res.FileSize) {
                    this.setState({
                        uploadLists: [res],
                    });
                }
            })
            .catch(() => {
                console.error("Failed to get retention settings");
            })
            .finally(() => $$.loading(false));
    };

    handleSave = (callback) => {
        const self = this;
        $$.loading(true);
        const formData = new FormData();
        const chooseFileInputName = this.props.chooseFileInputName;
        const noChangeStatusHiddenInputName =
            this.props.noChangeStatusHiddenInputName;
        const fileUp = $(`[name=${chooseFileInputName}]`)[0].files[0];

        formData.append(chooseFileInputName, fileUp ?? "");
        formData.append(
            noChangeStatusHiddenInputName,
            $(`[name=IsNoChangeDirectSave]`).val(),
        );

        fetch("/api/RetentionApi/SaveRetentionSettings", {
            method: "POST",
            body: formData,
        })
            .then(function (response) {
                return response.text().then(function (dataString) {
                    return {
                        responseStatus: response.status,
                        responseString: JSON.parse(dataString),
                    };
                });
            })
            .then(function (data) {
                $$.loading(false);
                if (data.responseString.MessageType === RAMessageType.Successful) {
                    $$.toast({
                        classify: "success",
                        content: RMResx.RM_CP_Retention_Settings_SaveSuccess,
                    });
                    callback(true);
                } else {
                    self.handleShowMessageTip("error", data.responseString.ErrorMessage);
                }
            });
    };

    handleShowMessageTip = (type, msg) => {
        const tipsOption = {
            classify: type,
            content: msg,
        };
        $$.toast(tipsOption);
    };

    handleChooseFileSuccess = (file) => {
        if (file.element_name === "RetentionSettingsFileUp") {
            this.setState({
                whitelistChooseState: true,
            });
        }

        if (file.fileMessage) {
            this.handleShowMessageTip("error", file.fileMessage);
        }
    };

    handleDeleteFileSuccess = () => {
        switch (this.props.chooseFileInputName) {
            case "RetentionSettingsFileUp":
                this.setState({
                    whitelistChooseState: false,
                });
                break;
            default:
                break;
        }
    };

    render() {
        return (
            <div className="ra-retention-whitelist" id={this.props.id}>
                <form
                    id={this.props.id}
                    encType="multipart/form-data"
                    action=""
                    method="POST"
                >
                    <div>
                        <Upload
                            fileTypes={this.state.fileTypes}
                            fileSize={this.state.fileSize}
                            uploadLists={this.state.uploadLists}
                            multiple={false}
                            savedFileUrl="/api/RetentionApi/DownloadCurrentRetentionSettings"
                            noChangeStatusHiddenInputName={
                                this.props.noChangeStatusHiddenInputName
                            }
                            chooseFileInputName={this.props.chooseFileInputName}
                            chooseFileSuccess={this.handleChooseFileSuccess}
                            deleteFileSuccess={this.handleDeleteFileSuccess}
                            hideDownloadTemplate
                            hasSupportShowErrorWhenDownloadFile
                            acceptFileTypes={[".csv"]}
                        />
                    </div>
                </form>
            </div>
        );
    }
}

export { ConfigArchiveDataWhitelistForm };
