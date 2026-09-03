import { LightningElement, track, api } from 'lwc';
import { UnitConvertingUtil, I18N } from 'c/utils';

const fontSizeLabel = {
    fontSize: 14,
    fontWeight: 400,
    color: '#293037',
}

export default class FileAnalysis extends LightningElement {
    barChart;
    treemapChart;

    @track selectedObjectTypes = [];
    @track objectTypeOptions = undefined;

    _tableData;
    _tableTotal;
    _sizeRangeInfo;
    _fileTypesData;
    _queryParam;
    isRendered = false;

    translation = {
        fileAnalysis: I18N.get("OpusApp.Tab.FileAnalysis.FileAnalysisTitle"),
        byFileSize: I18N.get("OpusApp.Tab.FileAnalysis.FileSizeTitle"),
        topFileType: I18N.get("OpusApp.Tab.FileAnalysis.FileTypeTitle"),
    }

    get columns() {
        return [
            {
                label: I18N.get("OpusApp.Table.Column.Object"),
                fieldName: "displayName",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: {fieldName: 'rowClass'}
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.InactiveFileCount"),
                fieldName: "inactiveSumCount",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: {fieldName: 'rowClass'}
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.TotalFileCount"),
                fieldName: "totalItemCount",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: {fieldName: 'rowClass'}
                },
                hideDefaultActions: true
            },
            {
                label:  I18N.get("OpusApp.Table.Column.InactiveOfTotalCount"),
                fieldName: "inactiveCountOfTotal",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: {fieldName: 'rowClass'}
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.InactiveFileSize"),
                fieldName: "inactiveTotalSize",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: {fieldName: 'rowClass'}
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.TotalFileSize"),
                fieldName: "totalSize",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: {fieldName: 'rowClass'}
                },
                hideDefaultActions: true
            },
            {
                label: I18N.get("OpusApp.Table.Column.InactiveOfTotalSize"),
                fieldName: "inactiveSizeOfTotal",
                cellAttributes: {
                    style: 'height: 35px; line-height: 35px;',
                    class: {fieldName: 'rowClass'}
                },
                hideDefaultActions: true
            }
        ];
    }

    @api
    get tableData() {
        return this._tableData;
    }
    set tableData(value) {
        this._tableData = value;
    }
    
    @api
    get tableTotal() {
        return this._tableTotal;
    }
    set tableTotal(value) {
        this._tableTotal = value;
    }

    @api 
    get sizeRangeInfo() {
        return this._sizeRangeInfo;
    }
    set sizeRangeInfo(value) {
        if (JSON.stringify(this._sizeRangeInfo) !== JSON.stringify(value)) {
            this._sizeRangeInfo = value;
            this.renderBarChart(value);
            this.isRerender = false;
        }
    }

    @api 
    get fileTypesData() {
        return this._fileTypesData;
    }
    set fileTypesData(value) {
        if (JSON.stringify(this._fileTypesData) !== JSON.stringify(value)) {
            this._fileTypesData = value;
            this.renderTreemapChart(value);
            this.isRerender = false;
        }
    }

    @api 
    get queryParam() {
        return this._queryParam;
    }
    set queryParam(value) {
        this._queryParam = value;
    }

    get tableDataWithTotal() {
        const tableDataTemp = JSON.parse(this._tableData);
        const tableTotalTemp = JSON.parse(this._tableTotal);

        const formattedTotal = {
            ...tableTotalTemp,
            displayName: I18N.get("OpusApp.Table.Footer.Total"),
            rowClass: 'custom-footer-row'

        };
        
        const combinedData = [...tableDataTemp.items, formattedTotal];
        combinedData.forEach(item => {
            item.inactiveTotalSize = UnitConvertingUtil.DecimalConvert(item.inactiveTotalSize, 2);
            item.totalSize = UnitConvertingUtil.DecimalConvert(item.totalSize, 2);
            item.inactiveCountOfTotal = `${Number(item.inactiveCountOfTotal || 0).toFixed(0)}%`;
            item.inactiveSizeOfTotal = `${Number(item.inactiveSizeOfTotal || 0).toFixed(0)}%`;

            return item;
        })

        return combinedData;
    }

    renderedCallback() {
        if (this.isRendered) {
            return;
        }
        if (window.ApexCharts) {
            this.renderBarChart(this._sizeRangeInfo);
            this.renderTreemapChart(this._fileTypesData);
            this.isRendered = true;
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

    renderBarChart = (sizeRangeInfo) => {
        if (!sizeRangeInfo) {
            return;
        }
        const container = this.template.querySelector('.file-bar-chart-container');
        if (!container) {
            return
        }

        if (this.barChart) {
            this.barChart.destroy();
            this.barChart = null;
        }

        const barOptions = this.getBarOptions(sizeRangeInfo);
        this.barChart = new window.ApexCharts(this.template.querySelector('.file-bar-chart-container'), barOptions);

        this.barChart.render();
    }

    renderTreemapChart = (fileTypesData) => {
        if (!fileTypesData) {
            return;
        }
        const container = this.template.querySelector('.file-treemap-chart-container');
        if (!container) {
            return
        }
        
        if (this.treemapChart) {
            this.treemapChart.destroy();
            this.treemapChart = null;
        }

        const treemapOptions = this.getTreemapOptions(fileTypesData);
        this.treemapChart = new window.ApexCharts(this.template.querySelector('.file-treemap-chart-container'), treemapOptions);
        this.treemapChart.render();
    }

    getBarOptions = (sizeRangeInfo) => {
        return {
            chart: {
                type: 'bar',
                height: 350,
                zoom: { enabled: true },
                toolbar: {
                    show: false
                },
                events: {
                    dataPointSelection: this.onSizeRangeSelect,
                },
            },
            title: {
                text: I18N.get("OpusApp.Chart.SizeRange.Unit"),
                align: 'left',
                offsetX: 6.5,
                offsetY: 12,
                style: fontSizeLabel,
            },
            plotOptions: {
                bar: {
                    borderRadius: 4,
                    columnWidth: 18,
                    borderRadiusApplication: "end",
                },
            },
            dataLabels: {
                enabled: false,
            },
            series: [{
                name: I18N.get("OpusApp.Chart.SizeRange.Tooltip"),
                data: sizeRangeInfo.map((item) => ({
                    x: item.name,
                    y: UnitConvertingUtil.DecimalConvert(item.fileTotalSize, 2, 3),
                    meta: item.id,
                })),
            }],
            xaxis: {
                labels: { style: fontSizeLabel },
            },
            yaxis: {
                labels: { style: fontSizeLabel },
            },
        };
    }

    getTreemapOptions = (fileTypesData) => {
        return {
            chart: {
                type: 'treemap',
                height: 350,
                zoom: { enabled: false },
                toolbar:
                {
                    show: false
                },
                events: {
                    dataPointSelection: this.onFileTypeSelect,
                }
            },
            series: [
                {
                    name: 'FileExtension',
                    data: fileTypesData.map(item => ({
                        x: item.name,
                        y: UnitConvertingUtil.DecimalConvert(item.fileTotalSize, 2, 3),
                        meta: item.name
                    })),
                }
            ],
            colors: ['#28a745', '#dc3545', '#ffc107', '#fd7e14', '#007bff', '#6f42c1', '#e83e8c', '#20c997', '#17a2b8', '#6610f2'],
            tooltip: {
                y: {
                    formatter: (val) => `${val} MB` 
                }
            },
            plotOptions: {
                treemap: {
                    distributed: true, 
                }
            }
        };
    }

    onSizeRangeSelect = (e, chart, options) => {
        const selectedIndex = options.selectedDataPoints[0][0];
        let sizeRangeQueryParameter = {};
        if (selectedIndex !== undefined) {
            const item = this._sizeRangeInfo[selectedIndex];
            sizeRangeQueryParameter = {
                queryMode: 1,
                sizeRange: item.id
            };
        } else {
            sizeRangeQueryParameter = {};
        }

        this._queryParam = {
            ...this._queryParam,
            sizeRangeQueryParameter
        }
        const filterEvent = new CustomEvent('filterchange', {
            detail: { query: this._queryParam, filterType: 'sizeRangeSelected' }
        });
        this.dispatchEvent(filterEvent);
    }

    onFileTypeSelect = (e, chart, options) => {
        const selectedIndex = options.selectedDataPoints[0][0];
        let fileExtensionQueryParameter = {};
        if(selectedIndex !== undefined) {
            const item = this._fileTypesData[selectedIndex];
            fileExtensionQueryParameter = {
                fileExtensions: [item.name],
            };
        } else {
            fileExtensionQueryParameter = {};
        }

        this._queryParam = {
            ...this._queryParam,
            fileExtensionQueryParameter
        }
        const filterEvent = new CustomEvent('filterchange', {
            detail: { query: this._queryParam, filterType: 'fileTypeSelected' }
        });
        this.dispatchEvent(filterEvent);
    };

    handleTableSelection = (event) => {
        const nodeQueryParameter = {
            viewMode: 2,
            objectIds: event.detail.selectedRows.reduce((list, item) => item.id ? [...list, item.id] : list, []),
            pageSize: 5,
            pageIndex: 0,
        };

        this._queryParam = {
            ...this._queryParam,
            nodeQueryParameter
        }
        const filterEvent = new CustomEvent('filterchange', {
            detail: { query: this._queryParam, filterType: 'tableSelected' }
        });
        this.dispatchEvent(filterEvent);
    }

    disconnectedCallback() {
        if (this.barChart) {
            this.barChart.destroy();
        }
        if (this.treemapChart) {
            this.treemapChart.destroy();
        }
    }
}
