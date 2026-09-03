export default class TableDataCache {
    cacheItems = [];

    clearCacheItems() {
        this.cacheItems = [];
    }

    getSelectedItems() {
        return this.cacheItems.filter(t => t.isChecked);
    }

    getSelectedItemIds() {
        return this.cacheItems.filter(t => t.isChecked).map(c => c.Id);
    }

    addCacheItem(item) {
        let isExits = this.cacheItems.find(r => r.Id == item.Id);
        if (item.Id != '') {
            if (isExits == undefined) {
                item.isChecked = false;
                this.cacheItems.push(item);
            } else {
                this.updateItemCheckedStatus(item);
            }
        }
    }

    updateItemCheckedStatus(item) {
        let cacheItem = this.cacheItems.find(r => r.Id == item.Id);
        if (cacheItem !== undefined) {
            item.isChecked = cacheItem.isChecked;
        }
    }

    updateCacheItemsStatus(rowItems) {
        if (rowItems && rowItems.length > 0) {
            this.cacheItems.forEach((item, key) => {
                let rowItem = rowItems.find(t => t.Id == item.Id);
                if (rowItem !== undefined) {
                    item.isChecked = rowItem.isChecked;
                }
            });
        }
    }
}