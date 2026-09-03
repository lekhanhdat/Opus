class TextUtil {
    static calculateTextWidth = (text, options) => {
        const { size, family } = options;
        const canvas = document.createElement("canvas");
        const ctx = canvas.getContext("2d");
        ctx.font = `${size}px ${family}`;
        const metrics = ctx.measureText(text);
        const actual =
            Math.abs(metrics.actualBoundingBoxLeft) +
            Math.abs(metrics.actualBoundingBoxRight);
        return Math.max(metrics.width, actual);
    };
}

export default TextUtil;
