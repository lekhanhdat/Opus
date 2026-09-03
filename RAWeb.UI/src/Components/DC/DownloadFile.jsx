import { getRequestVerificationToken } from "../../Utilities/CommonUtil";
import TopButtonsComponent from "../Common/Util/TopButtonsComponent";
import DCTable from "./DCTable";
import { DCTableTemplate } from "./DCTableTemplate";
import * as JMConstants from "../JM/JMConstants";

export default class DownloadFile extends R.Component {
    constructor(props) {
        super(props);
        this.defaultShowActions = {
            showRefresh: true,
            showDownload: false,
            showDelete: false,
        };
        this.state = {
            filesChecked: [],
            filesCount: 0,
            filesPagerIndex: 0,
            filesPagerSize: 10,
            items: [],
            allColumns: this.getColumns(),
            showActions: this.defaultShowActions,
            searchKey: '',
        };
        this.filterData = this.getDefaultPager();
        this.selectedFinished = [];
        this.selectedFailed = [];
        this.selectedFinally = [];
    }

    componentInit() {
        this.initDownloadData(true);
    }

    getColumns() {
        return [
            {
                header: RMResx.RM_JS_DC_FileName,
                width: [350],
                resizeable: true,
                valuePath: "FileName",
            },
            {
                header: RMResx.RM_JS_DC_DownloadTime,
                width: [300],
                resizeable: true,
                valuePath: "DownloadTime",
            },
            {
                header: RMResx.RM_JS_JM_JobID,
                width: [300],
                resizeable: true,
                valuePath: "JobId",
            },
            {
                header: RMResx.RM_FA_Inactive_OptimizationTab_FileSizeRangeTitle,
                width: [300],
                resizeable: true,
                valuePath: "FileSize",
            },

            {
                header: RMResx.RM_JS_DC_DownloadType,
                width: [300],
                resizeable: true,
                valuePath: "DownloadType",
            },

            {
                header: RMResx.RM_JS_DC_DownloadStatus,
                resizeable: true,
                width: [150],
                valuePath: "Status",
            }
        ];
    }

    getDefaultPager() {
        let param = {
            PageIndex: 1,
            PageSize: 10,
            Total: 0,
            HasNextPage: true
        };
        return param;
    }

    initDownloadData = (isResetPagerIndex) => {
        $$.loading(true);
        if (isResetPagerIndex) {
            this.filterData.PageIndex = 1;
            this.setState({ filesPagerIndex: 0 });
        }
        let urlData = "/api/RecordsExplorerApi/LoadArchivedContent";
        let dataObj = {
            searchKey: this.state.searchKey,
            pagingInfo: this.filterData
        };
        let option = {
            url: urlData,
            method: "POST",
            data: dataObj
        };
        fetchUtility(option).then((res) => {
            //刷新列表
            let data = JSON.parse(res);

            this.setState({
                items: data.Datas,
                filesCount: data.PagingInfo.Total,
            });

            this.dispatch("DownloadCenterTable", { columns: this.state.allColumns, items: data.Datas, isReset: isResetPagerIndex });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    selectChange = (items) => {
        let statusCode = JMConstants.StatusCode;
        let refreshBtn = true;
        let downloadBtn = false;
        let deleteBtn = false;
        this.selectedFinished = [];
        this.selectedFailed = [];
        this.selectedFinally = [];
        for (let item of items) {
            if (item.JobStatus == statusCode.Finished) {
                this.selectedFinished.push(item);
            }
            if (item.JobStatus == statusCode.Failed) {
                this.selectedFailed.push(item.RecordId);
            }
            if (item.JobStatus == statusCode.Finished || item.JobStatus == statusCode.Failed) {
                this.selectedFinally.push(item.RecordId);
            }
        }
        if (this.selectedFinished.length > 0 && this.selectedFinished.length == items.length) {
            downloadBtn = true;
            deleteBtn = true;
        }
        if (this.selectedFinished.length > 1) {
            for (let finishedItem of this.selectedFinished) {
                if (finishedItem.SasUri && finishedItem.SasUri.trim() !== "") {
                    downloadBtn = false;
                    break;
                }
            }
        }
        if (
            this.selectedFailed.length > 0 &&
            this.selectedFailed.length == items.length
        ) {
            deleteBtn = true;
        }
        if (this.selectedFinally.length > 0 && this.selectedFinally.length == items.length) {
            deleteBtn = true;
        }
        this.setState({
            showActions: {
                showRefresh: refreshBtn,
                showDownload: downloadBtn,
                showDelete: deleteBtn,
            },
            filesChecked: items
        }, () => {
            let showButtons = this.getShowActions();
            this.refTopButtons.updateButtons(showButtons);
        });
    }

    onRefresh = () => {
        this.setState({ showActions: this.defaultShowActions });
        this.initDownloadData(true);
    }

    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }

    getShowActions() {
        let { showRefresh, showDownload, showDelete } = this.state.showActions;
        let buttonsInfo = [
            { isStatic: true, name: RMResx.RM_JS_JM_Refresh_Btn, onClick: this.onRefresh, isShow: showRefresh },
            { name: RMResx.RM_JS_DC_DownloadBtn, icon: "fia-download", onClick: this.onDownloadFile, isShow: showDownload },
            { name: RMResx.RM_JS_Common_Delete, icon: "fia-delete", onClick: this.onDeleteFile, isShow: showDelete },
        ];
        let showButtons = buttonsInfo.filter((item) => { return item.isShow; });
        return showButtons;
    }

    onSearchStart = (args) => {
        let searchValue = args;
        if (searchValue && searchValue != "") {
            this.setState({ searchKey: searchValue });
            this.initDownloadData(true);
        } else {
            this.setState({ searchKey: '' });
            this.initDownloadData(false);
        }
    }

    onDownloadFile = () => {
        $$.loading(true);
        let filesId = [];
        this.state.filesChecked.forEach((item) => {
            filesId.push(item.RecordId);
        });
        if (filesId.length > 1) {
            let filesSize = [];
            this.state.filesChecked.forEach((item) => {
                filesSize.push(item.FileSize);
            });
            let fileSizeCount = 0;
            let substr;
            let fileSize;
            for (let fileSizeStr of filesSize) {
                if (fileSizeStr == "N/A") {
                    continue;
                }
                else if (fileSizeStr.endsWith("GB")) {
                    $$.loading(false);
                    this.showMsgToast(RMResx.RM_DC_DownloadOverLimitPrompt, "error");
                    return;
                }
                else if (fileSizeStr.endsWith("MB")) {
                    substr = fileSizeStr.slice(0, fileSizeStr.length - 3);
                    fileSize = parseFloat(substr) * 1024 * 1024;
                    fileSizeCount += fileSize;
                }
                else if (fileSizeStr.endsWith("KB")) {
                    substr = fileSizeStr.slice(0, fileSizeStr.length - 3);
                    fileSize = parseFloat(substr) * 1024;
                    fileSizeCount += fileSize;
                }
                else {
                    substr = fileSizeStr.slice(0, fileSizeStr.length - 5);
                    fileSize = parseFloat(substr);
                    fileSizeCount += fileSize;
                }
            }
            if (fileSizeCount > 100 * 1024 * 1024) {
                $$.loading(false);
                this.showMsgToast(RMResx.RM_DC_DownloadOverLimitPrompt, "error");
                return;
            }
        }
        let fileDownloadUrl = [];
        this.state.filesChecked.forEach((item) => {
            fileDownloadUrl.push(item.SasUri);
        });
        if (fileDownloadUrl[0] != null && fileDownloadUrl[0] != "") {
            window.open(fileDownloadUrl[0], "_blank");
        } else {
            let requestVerificationToken = getRequestVerificationToken();
            let divElement = document.getElementById("downloadFile");
            let downloadUrl = "/api/RecordsExplorerApi/DownloadArchivedContent";
            ReactDOM.render(
                <form action={downloadUrl} method="post">
                    <input
                        name="fileIdString"
                        type="text"
                        value={filesId.join(",")}
                        readOnly
                    />
                    <input
                        name="RequestVerificationToken"
                        type="text"
                        value={requestVerificationToken}
                        readOnly
                    />
                </form>,
                divElement
            );
            divElement.querySelector("form").submit();
            ReactDOM.unmountComponentAtNode(divElement);
        }
        $$.loading(false);
    }

    onDeleteFile = () => {
        this.args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>{RMResx.RM_JS_DC_DeleteFileJob}</div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.onDeleteCancelClick },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onDeleteSureClick }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onDeleteSureClick = () => {
        $$.messagedialog(false);
        $$.loading(true);
        let urlData = "/api/RecordsExplorerApi/DeleteArchivedContent";
        let idList = [];
        for (let key of this.state.filesChecked) {
            idList.push(key.RecordId);
        }
        let option = {
            url: urlData,
            method: "POST",
            data: idList
        };
        fetchUtility(option)
            .then((res) => {
                let resultData = JSON.parse(res);
                if (resultData.MessageType == 0) {
                    this.onRefresh();
                    this.showMsgToast(
                        RMResx.RM_JS_DC_DelSuccessMessage,
                        "success",
                        true
                    );
                } else {
                    this.showMsgToast(
                        RMResx.RM_JS_DC_DelFailedMessage,
                        "error",
                        true
                    );
                }
                $$.loading(false);
            })
            .catch((e) => {
                $$.loading(false);
            });
    };

    onDeleteCancelClick = () => {
        $$.messagedialog(false);
    }

    onPagerChange = (pagerIndex, pagerSize, callback) => {
        this.filterData.PageIndex = pagerIndex + 1;
        this.filterData.PageSize = pagerSize;
        this.setState({
            filesPagerIndex: pagerIndex,
            filesPagerSize: pagerSize
        });
        this.initDownloadData(false);
        callback(true);
    };

    renderDCHeader() {
        return <div className="ra-main-header">
            <R.Searchbox
                placeholder={RMResx.RM_JS_DC_SearchKeyWord}
                disabled={false}
                onSearch={this.onSearchStart}
                width={380}
            />
        </div>;
    }

    renderDCNavBar() {
        let selectFileItemsCount = RMResx.RM_Common_SelectTableItemsCounter.format(this.state.filesChecked.length, this.state.filesCount);
        return < div className="ra-main-navbar">
            <div className="flex">
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: this.getShowActions() }}
                    showCount={4}
                ></TopButtonsComponent>
            </div>
            <div className="ra-main-selected-counter">{selectFileItemsCount}</div>
        </div >;
    }

    renderDCTable() {
        return <div className="ra-main-table">
            <DCTable
                id="DownloadCenterTable"
                columns={this.state.allColumns}
                template={DCTableTemplate}
                uniqueKey={"DJobId"}
                checkable={true}
                onChange={this.selectChange}
            />
        </div>;
    }

    renderDCFooter() {
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.filesCount}
                pagerIndex={this.state.filesPagerIndex}
                pagerSize={this.state.filesPagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.onPagerChange} />
        </div>;
    }

    render() {
        return <section>
            {this.renderDCHeader()}
            {this.renderDCNavBar()}
            {this.renderDCTable()}
            {this.renderDCFooter()}
            <div id='downloadFile' style={{ display: "none" }} />
        </section>;
    }
}