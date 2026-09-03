import { LightningElement, track } from 'lwc';
import apexChart from '@salesforce/resourceUrl/apexcharts';  // Import static resource
import styles from '@salesforce/resourceUrl/styles';
import { loadScript, loadStyle } from 'lightning/platformResourceLoader';
import { I18N, publish } from 'c/utils';
import GetInactiveSummaryData from "@salesforce/apex/DashboardController.GetInactiveSummaryData";
import QueryInactiveAggregateInfo from "@salesforce/apex/DashboardController.QueryInactiveAggregateInfo";
import QueryInactiveSizeRanges from "@salesforce/apex/DashboardController.QueryInactiveSizeRanges";
import QueryInactiveFileExtensions from "@salesforce/apex/DashboardController.QueryInactiveFileExtensions";
import QueryInactiveSummaryObjectTotalInfo from "@salesforce/apex/DashboardController.QueryInactiveSummaryObjectTotalInfo";
import QueryAnalysis from "@salesforce/apex/DashboardController.QueryAnalysis";
import SearchObject from "@salesforce/apex/DashboardController.SearchObject";
import QueryFigureDataInfo from "@salesforce/apex/DashboardController.QueryFigureDataInfo";
import GetWithoutInDateList from "@salesforce/apex/DashboardController.GetWithoutInDateList";
import GetLicense from "@salesforce/apex/DashboardController.GetLicense";

export default class OpusApp extends LightningElement {
    @track translation = {};
    @track readyToDisplay = false;

    @track inactiveData = "";
    @track summaryData = "";
    @track sizeRangeData = "";
    @track fileExtensionsData = "";
    @track searchObjectData = "";
    @track figuredDataInfo = "";
    @track dataAnalysisTable = "";
    @track dataAnalysisTableTotal = "";
    @track fileAnalysisTable = "";
    @track fileAnalysisTableTotal = "";
    @track timeRangeOption = "";
    @track hasLicense = true;

    @track hasError = false;
    @track showErrorPage = false;
    @track errorStatusCode = 200;

    @track inactiveDataQueryParameter = {
        dataType: 1,
        withoutDateQueryParameter: {
            from: -1,
            to: 999,
        },
    };
    @track dataAnalysisQueryParameter = {
        ...this.inactiveDataQueryParameter,
        sizeRangeQueryParameter: {},
        nodeQueryParameter: {
            viewMode: 1,
            objectIds: [],
            pageSize: 5,
            pageIndex: 0,
        },
        selectedObjectIds: [],
        fileExtensionQueryParameter: {},
    };
    @track fileAnalysisQueryParameter = {
        ...this.inactiveDataQueryParameter,
        sizeRangeQueryParameter: {},
        nodeQueryParameter: {
            viewMode: 2,
            objectIds: [],
            pageSize: 5,
        },
        fileExtensionQueryParameter: {},
    };

    async connectedCallback() {
        await this.appConstructor();
        this.showErrorPage = this.hasError || !this.hasLicense;
    }

    async getSummaryData() {
        try {
            const res = await GetInactiveSummaryData();
            this.summaryData = JSON.parse(res);
        } catch (e) {
            console.error("Get summary gadget data failed", e);
        }
    }

    async getInactiveAggregateInfo(payload) {
        try {
            const res = await QueryInactiveAggregateInfo({payload});
            this.inactiveData = JSON.parse(res);
        } catch (error) {
            console.error("Get summary gadget data failed", error);
        }
    }

    async QueryInactiveSizeRanges(payload) {
        try {
            const res = await QueryInactiveSizeRanges({payload});
            return JSON.parse(res);
        } catch (error) {
            console.error(error);
        }
    }

    async QueryInactiveFileExtensions(payload) {
        try {
            const res = await QueryInactiveFileExtensions({payload});
            return JSON.parse(res);
        } catch (error) {
            console.error(error);
        }
    }

    async SearchObject(payload) {
        try {
            const res = await SearchObject({payload})
            this.searchObjectData = JSON.parse(res);
        } catch (error) {
            console.error(error);
        }
    }

    async QueryFigureDataInfo(payload) {
        try {
            const res = await QueryFigureDataInfo({payload})
            this.figuredDataInfo = JSON.parse(res);
        } catch (error) {
            console.error(error);
        }
    }

    async QueryInactiveSummaryObjectTotalInfo(payload) {
        let result = "";
        try {
            result = await QueryInactiveSummaryObjectTotalInfo({payload})
        } catch (error) {
            console.error(error);
        }
        return result;
    }

    async getAnalysis(payload) {
        let result = "";
        try {
            result = await QueryAnalysis({payload});
        } catch (error) {
            console.error(error);
        }
        return result;
    }

    async getWithoutInDateList() {
        let result = "";
        try {
            result = await GetWithoutInDateList();
            result = JSON.parse(result)
            const dateList = [
                {
                    value: -1,
                    label: `0 ${this.translation.month}`,
                },
            ];
            for (let dateInfo of result) {
                dateList.push({
                    value: dateInfo.id,
                    label: dateInfo.unit > 1 ? `${dateInfo.unit} ${this.translation.months}` : `${dateInfo.unit} ${this.translation.month}`
                });
            }
            dateList.push({
            value: 999,
                label: this.translation.indefinite,
            });

            return dateList;
        } catch (error) {
            console.error(error);
        }
        return result;
    }

    async getLicense() {
        let result;
        try {
            result = await GetLicense();
            result = JSON.parse(result);
            this.errorStatusCode = result.statusCode || 500;
            
            if (result.statusCode == 200) {
                this.hasLicense = result.response == 'true';
                this.hasError = false;
            } else {
                this.hasLicense = false;
                this.hasError = true;
            }
        } catch (error) {
            this.hasLicense = false;
            this.hasError = true;
            this.errorStatusCode = error.status || 500
            console.error('get license error::', error);
        }
    }

    async appConstructor() {
        if (this.readyToDisplay) {
            return;
        }
        try {
            publish('loadingEvent', true);
            await I18N.initialize();
            this.translation = {
                appName: I18N.get("OpusApp.AppName"),
                inactiveDataTitle: I18N.get("OpusApp.InactiveDataSummary"),
                month: I18N.get("OpusApp.Unit.Time.Month"),
                months: I18N.get("OpusApp.Unit.Time.Months"),
                indefinite: I18N.get("OpusApp.Unit.Time.Indefinite")
            };
            await this.getLicense();

            if (this.hasLicense) {
                await loadStyle(this, styles);
                await loadScript(this, apexChart);

                const result = await Promise.all([
                    this.getWithoutInDateList(),
                    this.QueryInactiveSizeRanges(JSON.stringify(this.fileAnalysisQueryParameter)),
                    this.QueryInactiveFileExtensions(JSON.stringify(this.fileAnalysisQueryParameter)),
                    this.getAnalysis(JSON.stringify(this.dataAnalysisQueryParameter)),
                    this.QueryInactiveSummaryObjectTotalInfo(JSON.stringify(this.dataAnalysisQueryParameter)),
                    this.getAnalysis(JSON.stringify(this.fileAnalysisQueryParameter)),
                    this.QueryInactiveSummaryObjectTotalInfo(JSON.stringify(this.fileAnalysisQueryParameter)),
                    this.getSummaryData(),
                    this.getInactiveAggregateInfo(JSON.stringify(this.inactiveDataQueryParameter)),
                    this.SearchObject(JSON.stringify(this.dataAnalysisQueryParameter)),
                    this.QueryFigureDataInfo(JSON.stringify(this.dataAnalysisQueryParameter))
                ])

                const [timeRangeOption, sizeRangeData, fileExtensionsData, dataAnalysisTable, dataAnalysisTableTotal, fileAnalysisTable, fileAnalysisTableTotal] = result;
                this.timeRangeOption = timeRangeOption;
                this.sizeRangeData = sizeRangeData;
                this.fileExtensionsData = fileExtensionsData;
                this.dataAnalysisTable = dataAnalysisTable;
                this.dataAnalysisTableTotal = dataAnalysisTableTotal;
                this.fileAnalysisTable = fileAnalysisTable;
                this.fileAnalysisTableTotal = fileAnalysisTableTotal;
            } else {
                this.showErrorPage = true;
            }

            this.readyToDisplay = true;
        }
        catch (error) {
            this.readyToDisplay = false;
            this.hasError = true;
            this.errorStatusCode = error.statusCode || 500
            console.debug("Failed to load required resource" + error);
        }
        finally{
            publish('loadingEvent', false);
        }
    }

    updateChartData  = async (selectedOptions) => {
        this.dataAnalysisQueryParameter.nodeQueryParameter.objectIds = selectedOptions.detail.value
        publish('loadingEvent', true);
        await this.QueryFigureDataInfo(JSON.stringify(this.dataAnalysisQueryParameter));
        publish('loadingEvent', false);
    }

    onSelectionChange = async (selectedRows) => {
        this.dataAnalysisQueryParameter.selectedObjectIds = selectedRows.detail.value;
        this.dataAnalysisQueryParameter.nodeQueryParameter.pageIndex = 0;
        publish('loadingEvent', true);

        //update table
        this.dataAnalysisTable = await this.getAnalysis(JSON.stringify(this.dataAnalysisQueryParameter));

        // update chart 
        await this.updateChartData(selectedRows);
        this.dataAnalysisTableTotal = await this.QueryInactiveSummaryObjectTotalInfo(JSON.stringify(this.dataAnalysisQueryParameter));
        publish('loadingEvent', false);
    }

    handleNextPage = async (data) => {
        this.dataAnalysisQueryParameter.nodeQueryParameter.pageIndex = data.detail.value
        publish('loadingEvent', true);
        this.dataAnalysisTable = await this.getAnalysis(JSON.stringify(this.dataAnalysisQueryParameter));
        publish('loadingEvent', false);
    }

    handleFileAnalysisFilterChange = async (event) => {
        const {query, filterType} = event.detail;
        this.fileAnalysisQueryParameter = {...query}
        
        const promises = [
            this.getAnalysis(JSON.stringify(query)),
            this.QueryInactiveSummaryObjectTotalInfo(JSON.stringify(query))
        ]
        publish('loadingEvent', true);
        try {
            switch (filterType) {
                case "sizeRangeSelected":
                    if (Object.keys(query.fileExtensionQueryParameter).length == 0) {
                        this.fileExtensionsData = await this.QueryInactiveFileExtensions(JSON.stringify(query))
                    }
                    break;
                case "fileTypeSelected": 
                    if (Object.keys(query.sizeRangeQueryParameter).length == 0) {
                        this.sizeRangeData = await this.QueryInactiveSizeRanges(JSON.stringify(query));
                    }
                    break;
                case "tableSelected":
                    if (Object.keys(query.fileExtensionQueryParameter).length == 0) {
                        this.fileExtensionsData = await this.QueryInactiveFileExtensions(JSON.stringify(query));
                    }
                    if (Object.keys(query.sizeRangeQueryParameter).length == 0) {
                        this.sizeRangeData = await this.QueryInactiveSizeRanges(JSON.stringify(query));
                    }
            }
            const result = await Promise.all(promises);
            this.fileAnalysisTable = result[0];
            this.fileAnalysisTableTotal = result[1];
        } catch (error) {
            this.hasError = true;
            this.errorStatusCode = error.statusCode || 500
            console.debug(error);
        }
        publish('loadingEvent', false);
    }

    handleTimeRangeChange = async (event) => {
        const {query, filterType} = event.detail;
        if (filterType === "fromOptionChange") {
            this.inactiveDataQueryParameter.withoutDateQueryParameter.from = query;
        } else if (filterType === "toOptionChange") {
            this.inactiveDataQueryParameter.withoutDateQueryParameter.to = query;
        }

        publish('loadingEvent', true);
        try {
            const promises = [
                this.QueryInactiveSizeRanges(JSON.stringify(this.fileAnalysisQueryParameter)),
                this.QueryInactiveFileExtensions(JSON.stringify(this.fileAnalysisQueryParameter)),
                this.getAnalysis(JSON.stringify(this.dataAnalysisQueryParameter)),
                this.QueryInactiveSummaryObjectTotalInfo(JSON.stringify(this.dataAnalysisQueryParameter)),
                this.getAnalysis(JSON.stringify(this.fileAnalysisQueryParameter)),
                this.QueryInactiveSummaryObjectTotalInfo(JSON.stringify(this.fileAnalysisQueryParameter)),
                this.getInactiveAggregateInfo(JSON.stringify(this.inactiveDataQueryParameter)),
                this.SearchObject(JSON.stringify(this.dataAnalysisQueryParameter)),
                this.QueryFigureDataInfo(JSON.stringify(this.dataAnalysisQueryParameter))
            ]
            const result = await Promise.all(promises);

            const [sizeRangeData, fileExtensionsData, dataAnalysisTable, dataAnalysisTableTotal, fileAnalysisTable, fileAnalysisTableTotal] = result;
            this.sizeRangeData = sizeRangeData;
            this.fileExtensionsData = fileExtensionsData;
            this.dataAnalysisTable = dataAnalysisTable;
            this.dataAnalysisTableTotal = dataAnalysisTableTotal;
            this.fileAnalysisTable = fileAnalysisTable;
            this.fileAnalysisTableTotal = fileAnalysisTableTotal;
        } catch (error) {
            this.hasError = true;
            this.errorStatusCode = error.statusCode || 500
            console.debug(error);
        }
        publish('loadingEvent', false);
    }
}
