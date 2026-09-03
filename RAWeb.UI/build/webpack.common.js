/*Covered by AvePoint copyright and license agreement*/
const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const TerserPlugin = require("terser-webpack-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const fs = require("fs");

const outputPath = '../RAWeb/wwwroot/dist/bundle';
const resolvedOutputPath = path.resolve(outputPath);

if (!fs.existsSync(resolvedOutputPath)) {
    fs.mkdirSync(resolvedOutputPath, { recursive: true });
}

module.exports = {
    mode: "development",
    entry: {
        common: "./src/Components/Common/_common.pack.jsx"
    },
    output: {
        path: resolvedOutputPath,
        filename: "common.js",
        library: "$g",
    },
    watchOptions: {
        aggregateTimeout: 800,
    },
    target: 'node',
    devtool: "source-map",
    module: {
        rules: [
            {
                test: /\.css$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader',
                        options: {
                            sourceMap: true,
                            url: false
                        }
                    }
                ]
            },
            {
                test: /\.less$/,
                exclude: /node_modules/,
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader',
                        options: {
                            sourceMap: true,
                            url: false
                        }
                    }, 
                    {
                        loader: 'less-loader',
                        options: {
                            sourceMap: true,
                        
                        }
                    }
                ]
            },
            {
                test: /\.(png|jpg|jpeg|gif|svg)$/,
                exclude: /node_modules/,
                use: [
                    {
                        loader: 'url-loader',
                        options: {
                            limit: 10240,
                            name: "[name].[ext]",
                            mimetype: 'image/png',
                            outputPath: 'images/'
                        },
                    }
                ]
            },
            {
                test: /\.jsx$/,
                exclude: /(node_modules|bower_components)/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        cacheDirectory: path.join(outputPath, 'babel_cache'),
                        presets: ['@babel/preset-env', '@babel/preset-react'],
                    }
                }
            }
        ]
    },
    resolve: {
        modules: ["./node_modules"],
        extensions: [".js", ".json", ".jsx", ".css"]
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: "common.css"
        }),
        new webpack.DefinePlugin({
            'process.env': {
                'NODE_ENV': '"development"'
            }
        })
    ],
    optimization: {
        minimize: false,
        minimizer: [
            new TerserPlugin({
                extractComments: false
            }),
            new CssMinimizerPlugin()
        ]
    }, 
    externals: {
        react: 'window.React',
        'react-dom': 'window.ReactDOM',
    }
};
