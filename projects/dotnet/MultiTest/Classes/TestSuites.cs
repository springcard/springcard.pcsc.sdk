/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/09/2014
 * Time: 11:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;

namespace MultiTest.Classes
{
    public class TestSuites
    {
        List<TestSuite> content = new List<TestSuite>();

        public TestSuites()
        {
            Load();
        }

        public TestSuite this[int index]
        {
            get
            {
                return content[index];
            }
        }

        public string[] Titles
        {
            get
            {
                List<string> result = new List<string>();
                foreach (TestSuite suite in content)
                {
                    result.Add(suite.Title);
                }
                return result.ToArray();
            }
        }

        public TestSuite Get(string Title)
        {
            foreach (TestSuite suite in content)
            {
                if (suite.Title.Equals(Title))
                    return suite;
            }
            return null;
        }

        private void Load()
        {
            string directory = AppUtils.BaseDirectory + @"\conf";
            Logger.Debug("Listing XML files in {0}", directory);
            string[] files = Directory.GetFiles(directory, "*.xml");
            foreach (string file in files)
            {
                Logger.Debug("- {0}", file);
            }
            content.Clear();
            foreach (string file in files)
            {
                Logger.Debug("Loading test suite {0}", file);
                try
                {
                    TestSuite suite = new TestSuite(file);
                    content.Add(suite);
                    Logger.Debug("Added under title '{0}'", suite.Title);
                }
                catch
                {
                    Logger.Warning("File {0} is not a valid test suite", file);
                }
            }
        }
    }

	public class TestSuite
	{
		string FileName;		
		private XmlDocument xmlDocument;

        public string Title { get; private set; }
		
		public TestSuite(string FileName)
		{
			this.FileName = FileName;
			Load();
		}
		
		private void Load()
		{
			xmlDocument = new XmlDocument();
			xmlDocument.Load(FileName);
            try
            {
                XmlNode xmlNode = xmlDocument.SelectSingleNode("sprincard-multitest/title");
                Title = xmlNode.InnerText;
            }
            catch
            {
                Title = Path.GetFileNameWithoutExtension(FileName);
            }
        }
		
		public void LoadTree(ref TreeView treeView)
		{
			treeView.Nodes.Clear();
			XmlNode xmlRoot = xmlDocument.SelectSingleNode("sprincard-multitest/tree");
			AddNodesToTree(xmlRoot, ref treeView, null);
			treeView.ExpandAll();
		}
		
		private void AddNodesToTree(XmlNode xmlRoot, ref TreeView treeView, TreeNode parentTreeNode)
		{				
			if (xmlRoot.HasChildNodes)
			{
				XmlNode xmlNode = xmlRoot.FirstChild;
				while (xmlNode != null)
				{
					if (xmlNode.Name == "node")
					{
						string xmlTitle = xmlNode.Attributes["title"].Value;
						Console.WriteLine(xmlNode.Name + " " + xmlTitle);
						
						TreeNode treeNode = new TreeNode(xmlTitle);
						
						if (xmlNode.HasChildNodes)
						{
							AddNodesToTree(xmlNode, ref treeView, treeNode);
						}
						
						if (parentTreeNode == null)
							treeView.Nodes.Add(treeNode);
						else
							parentTreeNode.Nodes.Add(treeNode);
					}
					else if (xmlNode.Name == "script")
					{
						string xmlTitle = xmlNode.Attributes["title"].Value;
						Console.WriteLine(xmlNode.Name + " " + xmlTitle);
						
						TreeNode treeNode = new TreeNode(xmlTitle);
						
						treeNode.Tag = xmlNode;

						if (parentTreeNode == null)
							treeView.Nodes.Add(treeNode);
						else
							parentTreeNode.Nodes.Add(treeNode);						
					}
					
					xmlNode = xmlNode.NextSibling;
				}				
			}
		}		
	}
}
