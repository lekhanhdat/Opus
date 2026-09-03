export const formatNumber = (value) => {
    const normalized = Number(value ?? 0);
    if (Number.isNaN(normalized)) {
        return '0';
    }

    return normalized.toFixed(2);
};

export const renderTooltipRow = (label, value, indicatorColor) => {
    const marker = indicatorColor
        ? `<span style="display:inline-block;width:12px;height:12px;background:${indicatorColor};border:1px solid #FFFFFF;box-sizing:border-box;flex-shrink:0;"></span>`
        : '';

    return `
        <div style="display:flex;align-items:center;gap:8px;font-family:Open Sans, sans-serif;font-size:14px;line-height:20px;color:#FFFFFF;">
            ${marker}
            <span>${label}: ${formatNumber(value)}</span>
        </div>
    `;
};

export const renderTooltipContainer = (title, rows) => {
    return `
        <div style="background:#323E4D;border-radius:8px;padding:8px;display:flex;flex-direction:column;gap:4px;">
            <div style="font-family:Open Sans, sans-serif;font-weight:600;font-size:14px;line-height:20px;color:#FFFFFF;">${title}</div>
            ${rows.join('')}
        </div>
    `;
};