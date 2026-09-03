/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */



namespace AvePoint.Hybrid.AgentService
{
    using Castle.Core.Resource;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    internal class AgentIocConfigurationResource : AbstractResource
    {
        private readonly string configPath;
        private readonly string sectionName;
        private XmlNode configSectionNode;

        internal AgentIocConfigurationResource(string configPath)
            : this(configPath, "castle") { }

        internal AgentIocConfigurationResource(string configPath, string sectionName)
        {
            this.configPath = configPath;
            this.sectionName = sectionName;
            if (!File.Exists(this.configPath))
            {
                throw new FileNotFoundException("Path: " + configPath);
            }
            LoadSectionNode();
        }

        private void LoadSectionNode()
        {
            var config = new XmlDocument();
            config.Load(configPath);
            var nodes = config.GetElementsByTagName(this.sectionName);
            if (nodes.Count > 1)
            {
                throw new Exception(string.Format("There should not be more than one section {0} in file {1}", sectionName, configPath));
            }
            if (nodes.Count == 1)
            {
                configSectionNode = nodes[0];
            }
        }

        public bool ContainsCastleSection()
        {
            return (configSectionNode != null);
        }

        public override IResource CreateRelative(string relativePath)
        {
            throw new NotImplementedException();
        }

        public override System.IO.TextReader GetStreamReader(Encoding encoding)
        {
            throw new NotSupportedException("Encoding is not supported");
        }

        public override System.IO.TextReader GetStreamReader()
        {
            return new StringReader(this.configSectionNode.OuterXml);
        }
    }
}
