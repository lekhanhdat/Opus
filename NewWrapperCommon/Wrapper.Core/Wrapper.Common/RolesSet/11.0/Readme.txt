This Document is used for support fxcop custom rule in visual studio 2012.
1.Copy CustomDictionary_1.xml to visual studio install path, C:\Program Files (x86)\Microsoft Visual Studio 11.0\Team Tools\Static Analysis Tools\FxCop
2.Copy FxCopCustomRules.dll to ..\client\Wrapper\Wrapper.Common\RolesSet, replace the original one.
3.If there are some errors in analysis report with the ID "C100007", but you did not change the code, please update CustomDictionary_1.xml and repeat step#2. If it does not work, contact Oliver Luo(qinglong.luo@avepoint.com).