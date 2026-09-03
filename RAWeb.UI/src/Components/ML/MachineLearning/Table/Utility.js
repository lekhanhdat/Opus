const modifyItemsCheckStatus = (items, checkStatus) => {
    for(let key in items){
        items[key].checked = checkStatus;
    }
    return items;
};

export {modifyItemsCheckStatus};
    
    
