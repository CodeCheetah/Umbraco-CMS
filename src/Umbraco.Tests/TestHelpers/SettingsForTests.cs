﻿using System.Collections.Generic;
using System.IO;
using System.Configuration;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;

namespace Umbraco.Tests.TestHelpers
{
    public class SettingsForTests
    {
        public static void ConfigureSettings(IGlobalSettings settings)
        {
            UmbracoConfig.For.SetGlobalConfig(settings);
        }

        // umbracoSettings

        /// <summary>
        /// Sets the umbraco settings singleton to the object specified
        /// </summary>
        /// <param name="settings"></param>
        public static void ConfigureSettings(IUmbracoSettingsSection settings)
        {
            UmbracoConfig.For.SetUmbracoSettings(settings);
        }

        public static IGlobalSettings GenerateMockGlobalSettings()
        {
            var config = Mock.Of<IGlobalSettings>(
                settings =>
                    settings.ConfigurationStatus == UmbracoVersion.SemanticVersion.ToSemanticString() &&
                    settings.UseHttps == false &&
                    settings.HideTopLevelNodeFromPath == false &&
                    settings.Path == IOHelper.ResolveUrl("~/umbraco") &&
                    settings.UseDirectoryUrls == true &&
                    settings.TimeOutInMinutes == 20 &&
                    settings.DefaultUILanguage == "en" &&
                    settings.LocalTempStorageLocation == LocalTempStorage.Default &&
                    settings.ReservedPaths == (GlobalSettings.StaticReservedPaths + "~/umbraco") &&
                    settings.ReservedUrls == GlobalSettings.StaticReservedUrls);
            return config;
        }

        /// <summary>
        /// Returns generated settings which can be stubbed to return whatever values necessary
        /// </summary>
        /// <returns></returns>
        public static IUmbracoSettingsSection GenerateMockUmbracoSettings()
        {
            var settings = new Mock<IUmbracoSettingsSection>();

            var content = new Mock<IContentSection>();
            var security = new Mock<ISecuritySection>();
            var requestHandler = new Mock<IRequestHandlerSection>();
            var templates = new Mock<ITemplatesSection>();
            var logging = new Mock<ILoggingSection>();
            var tasks = new Mock<IScheduledTasksSection>();
            var providers = new Mock<IProvidersSection>();
            var routing = new Mock<IWebRoutingSection>();

            settings.Setup(x => x.Content).Returns(content.Object);
            settings.Setup(x => x.Security).Returns(security.Object);
            settings.Setup(x => x.RequestHandler).Returns(requestHandler.Object);
            settings.Setup(x => x.Templates).Returns(templates.Object);
            settings.Setup(x => x.Logging).Returns(logging.Object);
            settings.Setup(x => x.ScheduledTasks).Returns(tasks.Object);
            settings.Setup(x => x.Providers).Returns(providers.Object);
            settings.Setup(x => x.WebRouting).Returns(routing.Object);

            //Now configure some defaults - the defaults in the config section classes do NOT pertain to the mocked data!!
            settings.Setup(x => x.Content.ForceSafeAliases).Returns(true);
            settings.Setup(x => x.Content.ImageAutoFillProperties).Returns(ContentImagingElement.GetDefaultImageAutoFillProperties());
            settings.Setup(x => x.Content.ImageFileTypes).Returns(ContentImagingElement.GetDefaultImageFileTypes());
            settings.Setup(x => x.RequestHandler.AddTrailingSlash).Returns(true);
            settings.Setup(x => x.RequestHandler.UseDomainPrefixes).Returns(false);
            settings.Setup(x => x.RequestHandler.CharCollection).Returns(RequestHandlerElement.GetDefaultCharReplacements());
            settings.Setup(x => x.Content.UmbracoLibraryCacheDuration).Returns(1800);
            settings.Setup(x => x.WebRouting.UrlProviderMode).Returns("AutoLegacy");
            settings.Setup(x => x.Templates.DefaultRenderingEngine).Returns(RenderingEngine.Mvc);
            settings.Setup(x => x.Providers.DefaultBackOfficeUserProvider).Returns("UsersMembershipProvider");

            return settings.Object;
        }

        //// from appSettings

        //private static readonly IDictionary<string, string> SavedAppSettings = new Dictionary<string, string>();

        //static void SaveSetting(string key)
        //{
        //    SavedAppSettings[key] = ConfigurationManager.AppSettings[key];
        //}

        //static void SaveSettings()
        //{
        //    SaveSetting("umbracoHideTopLevelNodeFromPath");
        //    SaveSetting("umbracoUseDirectoryUrls");
        //    SaveSetting("umbracoPath");
        //    SaveSetting("umbracoReservedPaths");
        //    SaveSetting("umbracoReservedUrls");
        //    SaveSetting("umbracoConfigurationStatus");
        //}

      

        // reset & defaults

        //static SettingsForTests()
        //{
        //    //SaveSettings();
        //}

        public static void Reset()
        {
            ResetSettings();
            GlobalSettings.Reset();

            //foreach (var kvp in SavedAppSettings)
            //    ConfigurationManager.AppSettings.Set(kvp.Key, kvp.Value);

            //// set some defaults that are wrong in the config file?!
            //// this is annoying, really
            //HideTopLevelNodeFromPath = false;
        }

        /// <summary>
        /// This sets all settings back to default settings
        /// </summary>
        private static void ResetSettings()
        {
            _defaultGlobalSettings = null;
            ConfigureSettings(GetDefaultUmbracoSettings());
            ConfigureSettings(GetDefaultGlobalSettings());
        }

        private static IUmbracoSettingsSection _defaultUmbracoSettings;
        private static IGlobalSettings _defaultGlobalSettings;

        internal static IGlobalSettings GetDefaultGlobalSettings()
        {
            if (_defaultGlobalSettings == null)
            {
                _defaultGlobalSettings = GenerateMockGlobalSettings();
            }
            return _defaultGlobalSettings;
        }

        internal static IUmbracoSettingsSection GetDefaultUmbracoSettings()
        {
            if (_defaultUmbracoSettings == null)
            {
                //TODO: Just make this mocks instead of reading from the config

                var config = new FileInfo(TestHelper.MapPathForTest("~/Configurations/UmbracoSettings/web.config"));

                var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = config.FullName };
                var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                _defaultUmbracoSettings = configuration.GetSection("umbracoConfiguration/defaultSettings") as UmbracoSettingsSection;
            }

            return _defaultUmbracoSettings;
        }
    }
}
