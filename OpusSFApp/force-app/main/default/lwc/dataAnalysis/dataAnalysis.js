import { LightningElement, track, api } from 'lwc';
import { UnitConvertingUtil, I18N } from 'c/utils'

export default class DataAnalysis extends LightningElement {
    chart;
    intervalId;
    isRerendered = false;
    @api searchObject;
    @track selectedObjectTypes = [];
    @track objectTypeOptions = undefined;
    @track data = [];
    @track visibleData = [];
    @track _tableData;
    @track _tableTotal;
    _chartData;
    translation = {
        dataAnalysisTitle: I18N.get("OpusApp.Tab.DataAnalysis.DataAnalysisTitle"),
        growthByCreatedTime: I18N.get("OpusApp.Tab.DataAnalysis.GrowthByCreatedTime"),
        firstPage: I18N.get("OpusApp.Pagination.FirstPage"),
        nextPage: I18N.get("OpusApp.Pagination.NextPage"),
        previousPage: I18N.get("OpusApp.Pagination.PreviousPage")
    }

    @track selectedRows = [];
    get columns() {
        return [
            {
                label: I18N.get("OpusApp.Table.Column.Object"),
                fieldName: "displayName",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: { fieldName: 'rowClass' }
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.InactiveRecordsCount"),
                fieldName: "inactiveSumCount",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: { fieldName: 'rowClass' }
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.TotalRecordsCount"),
                fieldName: "totalItemCount",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: { fieldName: 'rowClass' }
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.InactiveOfTotalCount"),
                fieldName: "inactiveCountOfTotal",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: { fieldName: 'rowClass' }
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.InactiveDataSize"),
                fieldName: "inactiveTotalSize",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: { fieldName: 'rowClass' }
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.TotalDataSize"),
                fieldName: "totalSize",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: { fieldName: 'rowClass' }
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.InactiveOfTotalSize"),
                fieldName: "inactiveSizeOfTotal",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: { fieldName: 'rowClass' }
                },
                hideDefaultActions: true
            }
        ];
    } 
    
    //pagination
    @track currentPage = 1;
    @track pageSize = 5; 
    @track totalRecords = 0;

    @api
    set tableTotal(value) {
        if (value) {
            try {
                this._tableTotal = typeof value === 'string' ? JSON.parse(value) : value;
                this.updateVisibleData();
            } catch (error) {
                console.error('Error parsing tableTotal:', error);
            }
        }
    }

    get tableTotal() {
        return this._tableTotal;
    }

    @api
    set tableData(value) {
        if (value) {
            try {
                this._tableData = typeof value === 'string' ? JSON.parse(value) : value;
                if (this.totalRecords !== this._tableData.count) {
                    this.totalRecords = this._tableData.count;
                    this.currentPage = 1;
                }
                this.processTableData();
            } catch (error) {
                console.error('Error parsing tableData:', error);
            }
        }
    }

    get tableData() {
        return this._tableData;
    }

    @api
    set chartData(value) {
        if (value) {
            this._chartData = value;
            this.updateChart();
        }
    }

    get chartData() {
        return this._chartData;
    }

    handleRowSelection(event) {
        this.selectedRows = event.detail.selectedRows.reduce((list, item) => item.id ? [...list, item.id] : list, []);
        this.dispatchEvent(new CustomEvent('rowselection', { detail: { value: this.selectedRows } }));
    }

    get totalPages() {
        return this.totalRecords > 0 ? Math.ceil(this.totalRecords / this.pageSize) : 1;
    }

    get disablePrevious() {
        return this.currentPage <= 1;
    }

    get disableNext() {
        return this.currentPage >= this.totalPages;
    }

    processTableData() {
        if (!this._tableData || !this._tableData.items) return;

        this.data = this._tableData.items.map(item => ({
            ...item,
            inactiveTotalSize: UnitConvertingUtil.DecimalConvert(item.inactiveTotalSize, 2),
            totalSize: UnitConvertingUtil.DecimalConvert(item.totalSize, 2),
            inactiveCountOfTotal: `${Number(item.inactiveCountOfTotal || 0).toFixed(0)}%`,
            inactiveSizeOfTotal: `${Number(item.inactiveSizeOfTotal || 0).toFixed(0)}%`
        }));

        this.updateVisibleData();
    }

    updateVisibleData() {
        if (!this._tableTotal) {
            console.warn("tableTotal is undefined, skipping updateVisibleData.");
            return;
        }

        const formattedTotal = {
            ...this._tableTotal,
            inactiveTotalSize: UnitConvertingUtil.DecimalConvert(this._tableTotal.inactiveTotalSize, 2),
            totalSize: UnitConvertingUtil.DecimalConvert(this._tableTotal.totalSize, 2),
            inactiveCountOfTotal: `${Number(this._tableTotal.inactiveCountOfTotal || 0).toFixed(0)}%`,
            inactiveSizeOfTotal: `${Number(this._tableTotal.inactiveSizeOfTotal || 0).toFixed(0)}%`,
            displayName: I18N.get("OpusApp.Table.Footer.Total"),
            rowClass: 'custom-footer-row'
        };

        this.visibleData = [...this.data, formattedTotal];
    }

    callBackFetchNextPage(pageIndex) {
        this.dispatchEvent(
            new CustomEvent('nextpage', {
                detail: { value: pageIndex },
                bubbles: true,
                composed: true
            })
        );
    }

    handleFirstPage() {
        if (this.currentPage > 1) {
            this.currentPage = 1;
            this.callBackFetchNextPage(0);
        }
    }

    handlePrevious() {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.callBackFetchNextPage(this.currentPage - 1);
        }
    }

    handleNext() {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.callBackFetchNextPage(this.currentPage - 1);

        }
    }

    get currentDataRange() {
        if (this.totalRecords === 0) return "";

        const startIndex = (this.currentPage - 1) * this.pageSize + 1;
        const endIndex = Math.min(startIndex + this.pageSize - 1, this.totalRecords);

        return `${startIndex} - ${endIndex}`;
    }

    renderedCallback() {
        if (this.isRerendered) return;

        if (window.ApexCharts) {
            this.initializeChart();
            this.isRerendered = true;
        } else {
            console.error('ApexCharts JS is not loaded.');
        }

        // set font weight for total row
        let customFooterStyles = document.createElement('style');
        customFooterStyles.innerText = '.file-analysis-table .slds-table .custom-footer-row{font-weight: 700 !important;}';
        this.template.querySelector('.file-analysis-table').appendChild(customFooterStyles);
    
        // Hide checkbox for total row
        let hideCheckboxStyles = document.createElement('style');
        hideCheckboxStyles.innerText = '.file-analysis-table .slds-table tr:last-child td .slds-checkbox{display: none !important;}';
        this.template.querySelector('.file-analysis-table').appendChild(hideCheckboxStyles);
    }

    updateChart() {
        if (!this.chart) return;

        const categories = this._chartData.map(item => item.Year);
        const recordCount = this._chartData.map(item => item.DataCreatedCount);
        const storageUsed = this._chartData.map(item => UnitConvertingUtil.DecimalConvert(item.TotalStorageUsed, 2, 3)); // Convert B to MB
        const countForecast = this._chartData.filter(item => item.IsDashLine).length;

        this.chart.updateOptions({
            xaxis: { categories },
            series: [
                { name: 'Record count', data: recordCount },
                { name: 'Storage used', data: storageUsed }
            ],
            forecastDataPoints: {
                count: countForecast,
            },
        });
    }

    initializeChart() {
        if (!this._chartData.length) return;
        const categories = this._chartData.map(item => item.Year);
        const recordCount = this._chartData.map(item => item.DataCreatedCount);
        const storageUsed = this._chartData.map(item => UnitConvertingUtil.DecimalConvert(item.TotalStorageUsed, 2, 3));
        const countForecast = this._chartData.filter(item => item.IsDashLine).length;

        const fontSizeLabel = {
            fontSize: 14,
            fontWeight: 400,
            color: '#293037',
        }
        const options = {
            chart: {
                type: 'line',
                height: 350,
                zoom: { enabled: false },
                toolbar: { show: false }
            },
            grid: {
                borderColor: "#E5EBEF",
                padding: {
                    top: 0,
                    right: 36,
                    bottom: 0,
                    left: 36,
                },
            },
            legend: {
                show: false
            },
            title: {
                text: I18N.get("OpusApp.Chart.GrowthChart.CountUnit"),
                align: 'left',
                offsetX: 32,
                offsetY: 32,
                style: fontSizeLabel,
            },
            subtitle: {
                text: I18N.get("OpusApp.Chart.GrowthChart.StorageUnit"),
                align: 'right',
                offsetX: -41.5,
                offsetY: 32,
                style: fontSizeLabel
            },
            forecastDataPoints: {
                count: countForecast,
            },
            series: [
                {
                    name: I18N.get("OpusApp.Chart.GrowthChart.Count"),
                    data: recordCount,
                    yaxis: 0,
                    color: '#F5BD24'
                },
                {
                    name: I18N.get("OpusApp.Chart.GrowthChart.StorageUsed"),
                    data: storageUsed,
                    yaxis: 1,
                    color: '#01B8AA'
                }
            ],
            xaxis: {
                categories: categories,
                labels: { style: fontSizeLabel },
                axisBorder: {
                    show: false,
                },
                axisTicks: {
                    show: false,
                },
            },
            yaxis: [
                {
                    title: { 
                        text: I18N.get("OpusApp.Chart.GrowthChart.Count"),
                        offsetX: -10,
                        style: fontSizeLabel,
                     },
                    min: 0,
                    labels: { 
                        formatter: val => Math.round(val),
                        style: fontSizeLabel
                    },
                    axisBorder: {
                        show: true,
                        width: 2,
                        color: "#F5BD24",
                        offsetX: 0,
                        offsetY: 0
                    },
                    axisTicks: {
                        show: true,
                        width: 6,
                        color: "#F5BD24",
                    },
                },
                {
                    opposite: true,
                    title: { 
                        text: I18N.get("OpusApp.Chart.GrowthChart.StorageUsed"),
                        offsetX: 10,
                        style: fontSizeLabel,
                     },
                    min: 0,
                    labels: { style: fontSizeLabel, },
                    axisBorder: {
                        show: true,
                        width: 2,
                        color: "#24BCA4",
                        offsetX: 0,
                        offsetY: 0
                    },
                    axisTicks: {
                        show: true,
                        width: 6,
                        color: "#24BCA4",
                    },
                }
            ],
            stroke: {
                width: [3,3],
                curve: 'smooth',
            },
            markers: {
                size: 4,
                colors: ["#fff", "#fff"],
                strokeColors: ['#F5BD24', '#24BCA4'],
                strokeWidth: 2,
                hover: { size: 4 }
            },
            tooltip: {
                shared: true,
            },
        };

        this.chart = new window.ApexCharts(this.template.querySelector('.line-chart-container'), options);
        this.chart.render();
    }

    disconnectedCallback() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
        }
        if (this.chart) {
            this.chart.destroy();
        }
    }
}
