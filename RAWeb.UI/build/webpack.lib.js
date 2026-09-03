/*Covered by AvePoint copyright and license agreement*/
var webpack = require('webpack');
var path = require("path");
const fs = require("fs");

const outputPath = '../RAWeb/wwwroot/dist/bundle';
const resolvedOutputPath = path.resolve(outputPath);

if (!fs.existsSync(resolvedOutputPath)) {
    fs.mkdirSync(resolvedOutputPath, { recursive: true });
}

module.exports = {
    entry: {
        lib: "./lib.entry.js"
    },
    output: {
        path: resolvedOutputPath,
        filename: "lib.js"
    },
    context: __dirname,
    devtool: "source-map",
    resolve: {
        extensions: [".js"]
    },
    optimization: {
        minimize: true
    },
    plugins: [
        new webpack.DefinePlugin({
            'process.env': {
                NODE_ENV: JSON.stringify('production')
            }
        })
    ]
};
