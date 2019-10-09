/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2019 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace SpringCard.LibCs
{
    /**
	 * \brief Application Updater
	 */
    public class AppUpdater
    {
        public const string PublishedVersionRootUtl = "https://files.springcard.com/versions";

        public static bool GetPublishedVersion(string ApplicationName, out string VersionData)
        {
            string url = string.Format("{0}/{1}.xml", PublishedVersionRootUtl, ApplicationName.ToLower());
            try
            {
                RestClient rest = new RestClient();
                VersionData = rest.GET(url);
                if (!string.IsNullOrEmpty(VersionData))
                    return true;
            }
            catch { }
            VersionData = null;
            return false;
        }

        private static string extractVersion(string What, string VersionData)
        {
            if (VersionData.StartsWith("<?"))
                VersionData = VersionData.Substring(VersionData.IndexOf("?>") + 2);

            string lookup = string.Format("<{0} version=\"", What);
            if (VersionData.Contains(lookup))
            {
                string str = VersionData;
                str = str.Substring(str.IndexOf(lookup));
                str = str.Substring(str.IndexOf('"') + 1);
                str = str.Substring(0, str.IndexOf('"'));
                return str;
            }
            return "";
        }

        public static string GetApplicationVersion(string VersionData)
        {
            return extractVersion("appversion", VersionData);
        }

        public static string GetConfigurationVersion(string VersionData)
        {
            return extractVersion("confversion", VersionData);
        }



        private string ApplicationName;
        private bool VersionDataOk;
        private string VersionData;
        private Thread BackgroundThread;
        private AppUpdaterReadyCallback OnUpdaterReady;

        public AppUpdater(string ApplicationName)
        {
            this.ApplicationName = ApplicationName;
            VersionDataOk = GetPublishedVersion(ApplicationName, out VersionData);
        }

        public delegate void AppUpdaterReadyCallback();

        public AppUpdater(string ApplicationName, AppUpdaterReadyCallback OnUpdaterReady)
        {
            this.ApplicationName = ApplicationName;
            this.OnUpdaterReady = OnUpdaterReady;
            BackgroundThread = new Thread(BackgroundProc);
            BackgroundThread.Start();
        }

        private void BackgroundProc()
        {
            VersionDataOk = GetPublishedVersion(ApplicationName, out VersionData);
            if (VersionDataOk)
                if (OnUpdaterReady != null)
                    OnUpdaterReady();
        }

        public string PublishedApplicationVersion
        {
            get
            {
                if (!VersionDataOk)
                    return "";
                return GetApplicationVersion(VersionData);
            }
        }

        public bool HasNewApplicationVersion(string CurrentVersion)
        {
            if (!VersionDataOk)
                return false;

            AppInfo.VersionDelta delta = AppInfo.CompareVersions(CurrentVersion, PublishedApplicationVersion);
            if (delta > 0)
                return true;
            return false;
        }

        public bool HasNewApplicationVersion()
        {
            return HasNewApplicationVersion(AppInfo.FullVersion);
        }

        public string PublishedConfigurationVersion
        {
            get
            {
                if (!VersionDataOk)
                    return "";
                return GetConfigurationVersion(VersionData);
            }
        }

        public bool HasNewConfigurationVersion(string CurrentVersion)
        {
            if (!VersionDataOk)
                return false;

            AppInfo.VersionDelta delta = AppInfo.CompareVersions(CurrentVersion, PublishedConfigurationVersion);
            if (delta > 0)
                return true;
            return false;
        }
    }
}

