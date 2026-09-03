# Related Records Command Set Extension for SharePoint 2019

## Summary
This solution provides a Command Set extension for SharePoint 2019 that allows users to view and create related records from a list view.

## Used SharePoint Framework Version
SPFx 1.4.1 (compatible with SharePoint 2019)

## Solution

Solution|Author(s)
--------|---------
Related Records Command Set|Your Name

## Version history

Version|Date|Comments
-------|----|--------
1.0|YYYY-MM-DD|Initial release

## Disclaimer
**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**

## Features
This extension illustrates the following concepts:

- Using SPFx Command Set extensions with SharePoint 2019
- Integration with Office UI Fabric React v5
- Interacting with list items

## Building and Installing the solution

### Build the solution
1. Run `npm install` to install all dependencies
2. Run `gulp build` to build the solution
3. Run `gulp bundle --ship` to create a production bundle
4. Run `gulp package-solution --ship` to create the .sppkg file

### Deploy the solution
1. Upload the .sppkg file from the `sharepoint/solution` folder to your App Catalog
2. Add the app to your site
3. The commands will now appear in your list views
