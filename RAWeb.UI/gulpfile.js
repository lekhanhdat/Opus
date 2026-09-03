/*Covered by AvePoint copyright and license agreement*/
const webpack = require('webpack'),
    WebpackBar = require('webpackbar'),
    MiniCssExtractPlugin = require("mini-css-extract-plugin"),
    clean = require("gulp-clean"),
    gulp = require('gulp'),
    log = require('fancy-log'),
    PluginError = require('plugin-error'),
    minimist = require('minimist'),
    path = require("path"),
    checkFormat = "/**/*.jsx";

var buildPath = "../RAWeb/wwwroot/dist/bundle",
    webpackUI = require("./build/webpack.ui"),
    webpackCommon = require("./build/webpack.common"),
    webpackLib = require("./build/webpack.lib"),
    copyList = require("./build/copyFileList.json"),
    needWatch = false;

function withProgressBar(config) {
    var plugins = (config.plugins || []).slice();
    plugins.push(new WebpackBar({
        name: 'compile',
        color: '#00a1d6'
    }));
    return Object.assign({}, config, { plugins: plugins });
}


gulp.task("clean", function () {
    return gulp.src(buildPath).pipe(clean({force: true}));
});

gulp.task("copy", function (cb) {
    copyList.forEach(function (value, index, array) {
        console.info(path.resolve(value.srcPath, '**'));
        console.info(value.destPath);
        var basePath = "";
        if (value.copyBaseFolder) {
            basePath = value.srcPath.substr(0, value.srcPath.lastIndexOf('/'));
        } else {
            basePath = value.srcPath;
        }
        gulp.src([path.resolve(value.srcPath, value.pattern || '**')], { base: basePath })
            .pipe(gulp.dest(value.destPath));
    });
    cb();
});

// ESLint Check
// gulp.task('check', () => {
//     let options = minimist(process.argv.slice(2), {
//         string: 'p',
//         default: { p: 'src' }
//     });
//     let checkPath = options.p;
//     if (checkPath.length < 4 || checkPath.lastIndexOf('.jsx') != checkPath.length - 4) {
//         //check folder
//         checkPath += checkFormat;
//     }
//     gutil.log(checkPath);
//     return gulp.src([checkPath])
//         .pipe(eslint())
//         .pipe(eslint.format('node_modules/eslint-friendly-formatter'))
//         .pipe(eslint.failAfterError());
// });

// 生产环境Build Lib
gulp.task('build:lib', function (cb) {
    var myConfig = Object.assign({}, webpackLib, {
        mode: "production",
        devtool: false,
        watch: needWatch,
        optimization: {
            minimize: true
        }
    });
    myConfig.output.filename = "lib.min.js";
    webpack(myConfig, function (err, stats) {
        if (err) {
            throw (err);
        }
        stats.hasErrors() && console.info(stats.toString({
            chunks: true,  // 使构建过程更静默无输出
            colors: true    // 在控制台展示颜色
        }));
        cb();
    });
});

// 生产环境Build Common 组件
gulp.task("build:common", function (cb) {
    // modify some webpack config options
    var myConfig = Object.assign({}, webpackCommon, {
        mode: "production",
        devtool: false,
        watch: needWatch,
        plugins: [
            new MiniCssExtractPlugin({
                filename: "common.min.css"
            }),
            new webpack.DefinePlugin({
                'process.env': {
                    'NODE_ENV': '"production"'
                }
            })
        ]
    });
    myConfig.optimization.minimize = true;
    myConfig.output.filename = "common.min.js";
    // run webpack
    webpack(myConfig, function (err, stats) {
        if (err) throw new PluginError("build:common", err);
        log("[build:common]", stats.toString({
            colors: true
        }));
        cb();
    });
});

// 生产环境Build UI 组件
gulp.task("build:ui", function (cb) {
    var myConfig = Object.assign({}, webpackUI(false), {
        watch: needWatch,
    })
    webpack(myConfig, function (err, stats) {
        if (err) throw new PluginError("build:ui", err);
        log("[build:ui]", stats.toString({
            colors: true
        }));
        cb();
    });
});

// 开发环境Build Lib
gulp.task('buildDEV:lib', function (cb) {
    var myConfig = Object.assign({}, webpackLib, {
        mode: "development",
        devtool: "source-map",
        watch: needWatch,
        optimization: {
            minimize: false
        }
    });
    myConfig.output.filename = "lib.js";
    webpack(myConfig, function (err, stats) {
        if (err) {
            throw (err);
        }
        stats.hasErrors() && console.info(stats.toString({
            chunks: true,  // 使构建过程更静默无输出
            colors: true    // 在控制台展示颜色
        }));
        cb();
    });
});

//开发环境Build Common 组件
gulp.task("buildDEV:common", function (cb) {
    // modify some webpack config options
    var myConfig = Object.assign({}, webpackCommon, {
        mode: "development",
        devtool: "source-map",
        watch: needWatch,
        plugins: [
            new MiniCssExtractPlugin({
                filename: "common.css"
            }),
            new webpack.DefinePlugin({
                'process.env': {
                    'NODE_ENV': '"development"'
                }
            })
        ]
    });
    myConfig.output.filename = "common.js";
    // run webpack
    webpack(myConfig, function (err, stats) {
        if (err) throw new PluginError("buildDEV:common", err);
        log("[buildDEV:common]", stats.toString({
            colors: true
        }));
        cb();
    });
});

//开发环境Build UI 组件
gulp.task("buildDEV:ui", function (cb) {
    var myConfig = Object.assign({}, webpackUI(true), {
        watch: needWatch,
    })
    myConfig = withProgressBar(myConfig);
    webpack(myConfig, function (err, stats) {
        if (err) throw new PluginError("buildDEV:ui", err);
        log("[buildDEV:ui]", stats.toString({
            colors: true
        }));
        cb();
    });
});

gulp.task('setDevMode', function (cb) {
    needWatch = true;
    cb();
});

//只是开发调试
gulp.task('DevOnly', gulp.series(
    'clean',
    'setDevMode',
    gulp.parallel('buildDEV:ui', 'buildDEV:common', 'buildDEV:lib'),
    'copy'
));

//只Build环境
gulp.task('BuildOnly', gulp.series(
    'clean',
    gulp.parallel('buildDEV:ui', 'buildDEV:common', 'buildDEV:lib'),
    gulp.parallel('build:ui', 'build:common', 'build:lib'),
    'copy'
));


//只Build环境
gulp.task('PreBuild', gulp.series(
    'clean',
    gulp.parallel('build:ui', 'build:common', 'build:lib'),
    'copy'
));
