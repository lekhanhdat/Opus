export default class RouteConfig {
    constructor(navId, url, text, group, icon, expand) {
        this.exact = true;
        this.component = null;
        this.navId = navId;
        this.url = url;
        this.text = text;
        this.tooltip = text;
        this.icon = !icon ? ".placeholder .fia-arrow-right-nav" : icon;
        this.showInNav = true;
        this.isStandardNav = false;
        this.isInternal = true;
        this.children = [];
        this.expand = expand || false;
        this.group = group;
    }

    setComponent(component) {
        this.component = component;
        return this;
    }

    setShowInNav(showInNav) {
        this.showInNav = showInNav;
        return this;
    }

    setIsStandardNav(isStandardNav) {
        this.showInNav = true;
        this.isStandardNav = isStandardNav;
        return this;
    }

    setIsInternal(isInternal) {
        this.isInternal = isInternal;
        return this;
    }

    setTooltip(tooltip) {
        this.tooltip = tooltip;
        return this;
    }

    setExact(exact) {
        this.exact = exact;
        return this;
    }

    addChildren(...children) {
        for (const child of children) {
            this.children.push(child);
            if(child.showInNav) {
                this.showInNav = true;
            }
            if(child.isStandardNav) {
                this.isStandardNav = true;
            }
        }
        return this;
    }

    cloneNavObject(asStandardNav) {
        if(!this.showInNav || (asStandardNav && !this.isStandardNav)) {
            return null;
        }
        return {
            id: this.navId,
            content: this.text,
            url: this.url,
            icon: this.icon,
            expanded: this.expand,
            isInternal: this.isInternal,
            group: this.group,
            children: this.children
                .filter(child => child.showInNav && (!asStandardNav || child.isStandardNav))  
                .map(child => child.cloneNavObject())
        };
    }
}