"use strict";

const gulp = require("gulp");
const build = require("@microsoft/sp-build-web");
const path = require("path");
const fs = require("fs");
const constantsFilepath = "./src/config/PackagedInfo.ts";

build.addSuppression(
  `Warning - [sass] The local CSS class 'ms-Grid' is not camelCase and will not be type-safe.`
);

// Configure packaged solution information
build.task("package-info", {
  execute: async (config) => {
    const packageTime = new Date().toISOString();
    const filePath = path.resolve(constantsFilepath);
    
    fs.writeFile(filePath, `export default "${packageTime}";`, (err) => {
      if (err) {
        console.error("Failed to update package info:", err);
        throw err;
      }
      console.log("Update package info success!");
      console.log("Packaging SPFx solution for SharePoint 2019...");
    });
  },
});

// Register clean task
build.initialize(gulp);

// Add custom task to create trusted development certificate
gulp.task("trust-dev-cert", function () {
  const cert = require("@microsoft/sp-build-web/lib/core-tasks/dev-cert");
  return cert.configureTrustCert();
});
