import { LightningElement, api, track } from 'lwc';
import { UnitConvertingUtil, I18N } from 'c/utils';
export default class DefinitionOfInactive extends LightningElement {
    @api timeRangeOption;
    @api queryParam;

    @track fromOptions = [];
    @track toOptions = [];
    @track fromValue = "";
    @track toValue = "";
    
    translation = {
      inactiveDataTitle: I18N.get("OpusApp.Tab.InactiveData.InactiveDataTitle"),
      timeRange: I18N.get("OpusApp.Tab.InactiveData.ModifiedTitle"),
      from: I18N.get("OpusApp.Tab.InactiveData.ModifiedFrom"),
      to: I18N.get("OpusApp.Tab.InactiveData.ModifiedTo"),
    };

    _inactiveData;

    @api 
    get inactiveData() {
      return this._inactiveData
    };
    set inactiveData(value) {
      this._inactiveData = value;
    }

    get totalData() {
        return [
            { text: I18N.get("OpusApp.Tab.InactiveData.RecordsCount"), value: this._inactiveData.RecordsTotalCount },
            { text: I18N.get("OpusApp.Tab.InactiveData.DataSizeTitle"), value: UnitConvertingUtil.DecimalConvert(this._inactiveData.DataTotalSize, 2) },
            { text: I18N.get("OpusApp.Tab.InactiveData.FileSizeTitle"), value: UnitConvertingUtil.DecimalConvert(this._inactiveData.FileTotalSize, 2) },
        ]
    }

    connectedCallback() {
      this.resetFromToOptions(this.queryParam);
      this.fromValue = this.getFromToValue(this.fromOptions, this.queryParam.withoutDateQueryParameter.from);
      this.toValue = this.getFromToValue(this.toOptions, this.queryParam.withoutDateQueryParameter.to);
    }

    resetFromToOptions = (queryParam) => {
      const withoutDateQueryParameter = queryParam.withoutDateQueryParameter;
      const fromOptions = this.getFromToOptions(
          false,
          withoutDateQueryParameter.to,
      );
      const toOptions = this.getFromToOptions(
          true,
          withoutDateQueryParameter.from,
      );

      this.fromOptions = fromOptions;
      this.toOptions = toOptions;
  };
  
    getFromToOptions = (isGreaterThan, value) => {
      return this.timeRangeOption.filter((item) =>
          isGreaterThan
              ? item.value > value && item.value !== -1
              : item.value < value && item.value !== 999
      )
    };

    getFromToValue = (options, selectedValue) => {
      let result = options.find(item => item.value === selectedValue);
      return result.value;
    }

    handleFromOptionChange(event) {
      this.toOptions = this.getFromToOptions(true, event.detail.value);
      const rangChange = new CustomEvent('rangechange', {
          detail: { query: event.detail.value, filterType: 'fromOptionChange' }
      });
      this.dispatchEvent(rangChange);
    }  

    handleToOptionChange(event) {
      this.fromOptions = this.getFromToOptions(false, event.detail.value);
      const rangChange = new CustomEvent('rangechange', {
        detail: { query: event.detail.value, filterType: 'toOptionChange' }
      });
      this.dispatchEvent(rangChange);
    }  
}