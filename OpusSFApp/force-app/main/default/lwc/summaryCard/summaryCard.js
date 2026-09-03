import { UnitConvertingUtil, I18N } from 'c/utils';
import { LightningElement, api, track } from 'lwc';

export default class SummaryCard extends LightningElement {
    @api summaryData;
    @track summaries = [];

    translation = {
      summaryTitle: I18N.get("OpusApp.Tab.Summary.SummaryTitle")
    };
    
    getSummaryList() {
      return [
        {
          id:'summary-object',
          title: I18N.get("OpusApp.Tab.Summary.ObjectSection"),
          items: [
            {
              id: 1001,
              name: I18N.get("OpusApp.Tab.Summary.TotalObject"),
              value: this.summaryData.ObjectTotalCount,
              unit: ""
            },
            {
              id: 1002,
              name: I18N.get("OpusApp.Tab.Summary.RecordsCount"),
              value: this.summaryData.RecordsTotalCount,
              unit: ""
            },
            {
              id: 1003,
              name: I18N.get("OpusApp.Tab.Summary.MaxObject"),
              value: this.summaryData.BiggestObjectByRecordCount,
              unit: ""
            },
            {
              id: 1004,
              name: I18N.get("OpusApp.Tab.Summary.OldestRecords"),
              value: this.summaryData.OldestRecords,
              unit: I18N.get("OpusApp.Unit.Time.Months")
            }
          ]
        },
        {
          id:'summary-dataa',
          title: I18N.get("OpusApp.Tab.Summary.DataSection"),
          items: [
            {
              id: 2001,
              name: I18N.get("OpusApp.Tab.Summary.DataSize"),
              value: UnitConvertingUtil.DynamicConvert(this.summaryData.DataTotalSize, 2),
              unit: UnitConvertingUtil.GetUnitI18N(this.summaryData.DataTotalSize)
            },
            {
              id: 2002,
              name: I18N.get("OpusApp.Tab.Summary.StorageUsage"),
              value: this.summaryData.DataStorageUsage + "%",
              unit: ""
            },
            {
              id: 2003,
              name: I18N.get("OpusApp.Tab.Summary.BiggestObject"),
              value: this.summaryData.BiggestObjectByDataSize,
              unit: ""
            }
          ]
        },
        {
          id:'summary-file',
          title: I18N.get("OpusApp.Tab.Summary.FileSection"),
          items: [
            {
              id: 3001,
              name: I18N.get("OpusApp.Tab.Summary.FileSize"),
              value: UnitConvertingUtil.DynamicConvert(this.summaryData.FileTotalSize, 2),
              unit: UnitConvertingUtil.GetUnitI18N(this.summaryData.FileTotalSize)
            },
            {
              id: 3002,
              name: I18N.get("OpusApp.Tab.Summary.StorageUsage"),
              value: this.summaryData.FileStorageUsage + "%",
              unit: ""
            },
            {
              id: 3003,
              name: I18N.get("OpusApp.Tab.Summary.BiggestObject"),
              value: this.summaryData.BiggestObjectByFileSize,
              unit: ""
            }
          ]
        }
      ]
    }

    getListClass(item) {
      return `slds-grid custom-grid-gap slds-p-right_${item.length === 4 ? 'xx-large' : 'x-large'}`;
    }

    getItemClass(item) {
      return `lds-col slds-p-top_xx-small slds-size_${item.length === 4 ? '1-of-4' : '1-of-3'}`;
    }

    connectedCallback() {
      const summaryList = this.getSummaryList();
      this.summaries = summaryList.map(summary => {
        return {
          ...summary,
          listClass: this.getListClass(summary.items),
          itemClass: this.getItemClass(summary.items)
        }
      });
    }

    renderedCallback() {
      this.checkOverflow();
    }

    checkOverflow() {
      this.summaries.forEach(summary => {
        summary.items.forEach(item => {
          const container = this.template.querySelector(`[data-id="${item.id}"]`);
          if (container) {
            if (container.scrollWidth > container.clientWidth) {
              item.tooltipText = `${item.value} ${item.unit}`;
            } else {
              item.tooltipText = '';
            }
          }
        });
      });
    }
}