import { I18N } from 'c/utils';
import { api, LightningElement, track } from 'lwc';

export default class MultipleSelect extends LightningElement {
    @api selectedValues = [];
    @api isClearAllFilter;
    @api options = [];
    @api customClass;

    @track searchText = '';
    @track tempSelectedValues = [];
    @track filteredOptions = [];

    @track expandedCustom = true;
    @track expandedStandard = true;
    
    translation = {
        dropdownPlaceholder: I18N.get("OpusApp.Selection.SelectItemPlaceholder"),
        searchPlaceholder: I18N.get("OpusApp.Selection.SearchPlaceholder"),
        customGroup: I18N.get("OpusApp.Selection.CustomGroup"),
        standardGroup: I18N.get("OpusApp.Selection.StandardGroup"),
        items: I18N.get("OpusApp.Selection.Items"),
        apply: I18N.get("OpusApp.Common.Button.Apply"),
        cancel: I18N.get("OpusApp.Common.Button.Cancel")
    };

    connectedCallback() {
        this.formatOptions();
        this.tempSelectedValues = [...this.selectedValues];
        this.updateFilteredOptions();
        document.addEventListener('click', this.handleClickOutside.bind(this));
    }

    disconnectedCallback() {
        document.removeEventListener('click', this.handleClickOutside.bind(this));
    }


    get chevronIconCustom() {
        return this.expandedCustom ? 'utility:chevronup' : 'utility:chevrondown';
    }

    get chevronIconStandard() {
        return this.expandedStandard ? 'utility:chevronup' : 'utility:chevrondown';
    }

    handleClickOutside(event) {
        const dropdownMenu = this.template.querySelector('.selection-zone');
        if (dropdownMenu && !dropdownMenu.contains(event.target)) {
            this.isDropdownOpen = false;
        }
    }

    formatOptions() {
        this.options = this.options.map(item => ({
            value: item.ObjectId,
            label: item.DisplayName,
            selected: this.selectedValues.includes(item.ObjectId) || false,
            type: item.ObjectType,
            computedId: `checkbox-${item.ObjectId.replace(/\s+/g, '-')}`
        }));
    }

    updateFilteredOptions() {
        if (!this.searchText) {
            this.filteredOptions = [...this.options];
        } else {
            this.filteredOptions = this.options.filter(option =>
                option.label.toLowerCase().includes(this.searchText.toLowerCase())
            );
        }
    }

    updateOptionsSelection() {
        this.options = this.options.map(option => ({
            ...option,
            selected: this.selectedValues.includes(option.value)
        }));
    }

    handleClearSelection(event) {
        event.stopPropagation();

        this.selectedValues = [];
        this.tempSelectedValues = [];

        this.updateOptionsSelection();
        this.updateFilteredOptions();

        this.syncWithParent(this.selectedValues);
}

    get showClearIcon() {
        return this.selectedValues.length > 0;
    }

    get customOptions() {
        return this.filteredOptions.filter(option => option.type === 1);
    }

    get standardOptions() {
        return this.filteredOptions.filter(option => option.type === 0);
    }

    toggleCustom() {
        this.expandedCustom = !this.expandedCustom;
    }

    toggleStandard() {
        this.expandedStandard = !this.expandedStandard;
    }

    onCustomKeyDown(e) {
        if (e.keyCode == 13) {
            this.expandedCustom = !this.expandedCustom;
        }
    }

    onStandardKeyDown(e) {
        if (e.keyCode == 13) {
            this.expandedStandard = !this.expandedStandard;
        }
    }

    handleCheckboxChange(event) {
        const value = event.target.value;
        if (event.target.checked) {
            if (!this.tempSelectedValues.includes(value)) {
                this.tempSelectedValues = [...this.tempSelectedValues, value];
            }
        } else {
            this.tempSelectedValues = this.tempSelectedValues.filter(val => val !== value);
        }
    }

    handleSearch(event) {
        this.searchText = event.target.value;
        this.updateFilteredOptions();
    }

    handleApply() {
        this.selectedValues = [...this.tempSelectedValues];
        this.updateOptionsSelection();
        this.updateFilteredOptions();
        this.syncWithParent(this.selectedValues);
        this.isDropdownOpen = false;
        document.dispatchEvent(new MouseEvent('click'));
    }

    handleCancel() {
        this.tempSelectedValues = [...this.selectedValues];
        document.dispatchEvent(new MouseEvent('click'));
    }

    get selectedText() {
        if (this.selectedValues.length > 0) {
            return `${this.selectedValues.length} ${this.translation.items}`;
        }
        return this.translation.dropdownPlaceholder;
    }

    syncWithParent(selected) {
        this.dispatchEvent(new CustomEvent('selectionchange', {
            detail: { value: selected },
            bubbles: true,
            composed: true,
        }));
    }
}
