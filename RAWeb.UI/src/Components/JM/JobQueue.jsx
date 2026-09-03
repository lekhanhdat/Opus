import { JobQueueTemplate } from "./JMTableTemplate";
import JMTable from "./JMTable";
import * as JMConstants from "./JMConstants";

export default class JobQueue extends R.Component {
    constructor(props) {
        super(props);
        this.filterData = this.getDefaultFilterData();
        this.totalNumber = 0;
        this.state = {
            jobsChecked: [],
            jobsCount: 0,             //分页数据总数
            jobsPagerIndex: 0,         //分页每页的条数
            jobsPagerSize: 15,         //分页每页条数
            allColumns: this.getColumns(),
			items: [],
			editPriorityShow: false,
			priorityValue: 0
        };
        this.bind(['onPagerChange', 'onSelectChange', 'onDeleteSureClick', 'initData', 'priorityValueChange', 'hidePriorityPanel', 'onSavePriority']);
    }

    componentInit() {
        this.initData(true);
    }

    showMsgToast(content, type) {
        let option = { content: content, classify: type };
        $$.toast(option);
    }

    getDefaultFilterData() {
        let filterData = {
            PageSize: 15,
            TotalNumber: 0,
            JumpPage: 1,
            CurrentPage: 0,
            IsSort: true,
            IsDesc: true,
            SortBy: 'CreateTime',
            SearchValue: '',
            SearcheKeys: '',
            Filters: []
        };
        return filterData;
    }

    initData(isResetPagerIndex) {
        if (isResetPagerIndex) {
            this.filterData.JumpPage = 1;
            this.filterData.CurrentPage = 0;
            this.setState({ jobsPagerIndex: 0 });
        }
        $$.loading(true);
        let urlData = '/api/JMApi/QueueQueryPager';
        let option = {
            url: urlData,
            method: 'POST',
            data: this.filterData
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            //分页重置回到第一页，分页总数（TotalNumber）也重置
            if (isResetPagerIndex) {
                this.totalNumber = data.TotalNumber;
            }
            if (data.Result) {
                this.setState({ items: data.Result, jobsCount: this.totalNumber });
            }
            this.dispatch("JobMonitorTable", { columns: this.state.allColumns, items: data.Result, isReset: isResetPagerIndex });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onRefreshClick() {
        this.initData(true);
	}

	onPriorityClick() {
		this.setState({ editPriorityShow: true });
    }

    onDeleteClick() {
        this.args = {
            // classify: "warn",
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_JM_ConfirmDeleteQueue,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.onDeleteCancleClick },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onDeleteSureClick },
            ],
        };
        $$.messagedialog(true, this.args);
    }

    onDeleteSureClick() {
        $$.messagedialog(false);
        $$.loading(true);
        let urlData = '/api/JMApi/DeletaJobQueue';
        let idList = [];
        for (let key of this.state.jobsChecked) {
            idList.push(key.MessageId);
        }
        let option = {
            url: urlData,
            method: 'POST',
            data: idList
        };
        fetchUtility(option).then((res) => {
            if (res > 0) {
                this.initData(true);
                this.showMsgToast(RMResx.RM_JS_JM_DeleteQueueSuccess, 'success');
            } else {
                this.showMsgToast(RMResx.RM_JS_JM_DeleteQueueFailed, 'error');
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onDeleteCancleClick() {
        $$.messagedialog(false);
    }

    getColumns() {
        return [{
            header: RMResx.RM_JS_JM_Module,
            width: [200],
            resizeable: true,
        },
        {
            header: RMResx.RM_JS_RC_ReportColumn_CreatedBy,
            width: [200],
            resizeable: true,
            visible: true,
        },
        {
            header: RMResx.RM_JS_RC_ReportColumn_CreatedTime,
            resizeable: true,
            width: [200],
			sortable: true,
			valuePath: "CreateTime",
		},
		{
            header: RMResx.RM_JS_JM_Priority,
            resizeable: true,
            width: [200],
			sortable: true,
			valuePath: "JobPriority",
        }];
    }

    onPagerChange(pagerIndex, pagerSize, callback) {
        this.filterData.CurrentPage = JSON.parse(JSON.stringify(this.filterData.JumpPage));
        this.filterData.JumpPage = pagerIndex + 1;
        this.filterData.PageSize = pagerSize;
        this.setState({
            jobsPagerIndex: pagerIndex,
            jobsPagerSize: pagerSize
        });
        this.initData(false);
        callback(true);
    }

    onSort = (isAsc, columnName) => {
		this.filterData.IsDesc = !isAsc;
		this.filterData.SortBy = columnName;
        this.initData(false);
    }

    onSelectChange(items) {
        this.setState({ jobsChecked: items });
	}

	priorityValueChange(value) {
        this.setState({priorityValue: value?.newValue?.value});
    }
	
	hidePriorityPanel() {
        this.setState({ editPriorityShow: false });
	}
	
	onSavePriority() {
		if (!$$.verify('#queue-combobox-value')) {
			return;
		}
		const JobIds = this.state.jobsChecked.map(item => item.MessageId);
		let setting = { 
			JobIds: JobIds,
			JobPriority: this.state.priorityValue
		};
        $$.loading(true);
        let urlData = "/api/JMApi/UpdateJobQueuePriority";
        let option = {
            url: urlData,
            method: "POST",
            data: setting
		};
        fetchUtility(option).then((res) => {           
			
            $$.loading(false);
            $$.messagedialog(false);

			this.setState({ editPriorityShow: false });
			if (res) {
				this.initData(true);
                this.showMsgToast(RMResx.RM_JS_JM_SavePrioritySuccess, 'success');
            } else {
                this.showMsgToast(RMResx.RM_JS_JM_SavePriorityFailed, 'error');
            }
		}).catch((e) => {
            $$.loading(false);
        }).finally(() => {
            $$.loading(false);
			this.state.priorityValue = 0;
		});
    }

    renderNavBar() {
        let jobsCheckedCount = this.state.jobsChecked.length;
        let jobsCount = this.state.jobsCount;
        let selectJobItemsCount = RMResx.RM_Common_SelectTableItemsCounter.format(jobsCheckedCount, jobsCount);
        return < div className="ra-main-navbar ra-border-none">
            <div className="flex" style={{ columnGap: "8px" }}>
                <R.Button
                    text={RMResx.RM_JS_JM_Refresh_Btn}
                    primary={true}
                    classify="theme"
                    onClick={this.onRefreshClick.bind(this)}
                />
                {
                    jobsCheckedCount > 0 && <R.Button
                        text={RMResx.RM_JS_JM_Priority_Btn}
                        icon="fia-edit"
                        onClick={this.onPriorityClick.bind(this)}
                    />
                }
                {
                    jobsCheckedCount > 0 && <R.Button
                        text={RMResx.RM_JS_Common_Delete}
                        icon="fia-delete"
                        onClick={this.onDeleteClick.bind(this)}
                    />
                }
            </div>
            <div className="ra-main-selected-counter">{selectJobItemsCount}</div>
        </div >;
    }

    renderTable() {
        return <div className="ra-main-table">
            <JMTable
                id="JobMonitorTable"
                template={JobQueueTemplate}
                uniqueKey={"MessageId"}
                checkable={true}
                flexible={true}
                onChange={this.onSelectChange}
                onSort={this.onSort}
            />
        </div>;
    }

    renderFooter(){
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.jobsCount}
                pagerIndex={this.state.jobsPagerIndex}
                pagerSize={this.state.jobsPagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.onPagerChange}
            />
        </div>;
	}

	renderEditPriorityForm() {
		const items = JMConstants.JobPriority;
        const labelWithPopover = (
            <span>
                {RMResx.RM_EL_EditPriorityTitle}
                <$g.Popover>{RMResx.RM_JS_JM_Priority_Tooltip}</$g.Popover>
            </span>
        );
		return <div>
			<$g.FormRow label={labelWithPopover}>
				<R.Validation id="queue-combobox-value">
					<R.Validation
						element="Combobox"
						require={RMResx.RM_JS_JM_Priority_ErrorMsg}
					>
						<R.Combobox
							id="queuePriorityCombobox"
							textField='name'
							valueField='value'
							checkedField='checked'
							waterMark='Select a Location'
							items={items}
							width={"100%"}
							searchable={false}
							onChange={this.priorityValueChange}
							triggerBySource={true}
							aria="tooltip_demo_labelledby"
						/>
					</R.Validation>
				</R.Validation>
			</$g.FormRow>
		</div>;
	}
	
	renderEditPriorityPanel() {
        return <R.Panel
            id="editqueuePriorityContainer"
            header={RMResx.RM_JS_EL_EditPriority}
            size={664}
            onHide={this.hidePriorityPanel}
            status={{ show: this.state.editPriorityShow }}
            destroy={true}
        >
            {this.renderEditPriorityForm()}
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.hidePriorityPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSavePriority} />
            </>
        </R.Panel>;
    }

    render() {
        return <section>
            {this.renderNavBar()}
            {this.renderTable()}
            {this.renderFooter()}
            {this.renderEditPriorityPanel()}
        </section>;
    }
}