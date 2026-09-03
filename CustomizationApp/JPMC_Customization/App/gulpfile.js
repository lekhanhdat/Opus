'use strict';
const build = require('@microsoft/sp-build-web');
const path = require('path');
const fs = require('fs');
const constantsFilepath = './src/config/PackagedInfo.ts';

build.task('package-info', build.subTask('package-info', function(gulp, buildOptions, done){
  let packageTime = new Date().toISOString();
  let filePath = path.resolve(constantsFilepath);
  fs.writeFile(
    filePath,
    `export default "${packageTime}";`,
    function (err) {
      done();
      if (err) throw err;
      console.log('Update package info success!');
    }
  );
}));

build.addSuppression(`Warning - [sass] The local CSS class 'ms-Grid' is not camelCase and will not be type-safe.`);

var getTasks = build.rig.getTasks;
build.rig.getTasks = function () {
  var result = getTasks.call(build.rig);

  result.set('serve', result.get('serve-deprecated'));

  return result;
};

build.initialize(require('gulp'));
