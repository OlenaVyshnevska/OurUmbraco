﻿using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Examine;
using our.Examine.DocumentationIndexDataService.Helper;
using our.ExamineServices;
using uDocumentation.Busineslogic;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;

namespace our.CustomHandlers
{
    /// <summary>
    /// Main Application startup handler
    /// </summary>
    public class OurApplicationStartupHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            CreateRoutes();
            BindExamineEvents();
            uDocumentation.Busineslogic.GithubSourcePull.ZipDownloader.OnFinish += ZipDownloader_OnFinish;
        }

        private void CreateRoutes()
        {
            RouteTable.Routes.MapUmbracoRoute("Search", "search2/{term}",
                new
                {
                    Controller = "Search",
                    Action = "Search",
                    Term = UrlParameter.Optional
                },
                //NOTE: This virtual page will be routed as if it were the root 'community' page
                new UmbracoVirtualNodeByIdRouteHandler(1052));
        }

        private void BindExamineEvents()
        {
            ExamineManager.Instance.IndexProviderCollection["projectIndexer"].GatheringNodeData += ProjectNodeIndexDataService.ProjectIndexer_GatheringNodeData;
            ExamineManager.Instance.IndexProviderCollection["projectIndexer"].IndexingError += ExamineHelper.LogErrors;
            ExamineManager.Instance.IndexProviderCollection["documentationIndexer"].IndexingError += ExamineHelper.LogErrors;
            ExamineManager.Instance.IndexProviderCollection["ForumIndexer"].IndexingError += ExamineHelper.LogErrors;
        }

        /// <summary>
        /// Whenever the github zip downloader completes and docs index is rebuilt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ZipDownloader_OnFinish(object sender, FinishEventArgs e)
        {
            var indexer = ExamineManager.Instance.IndexProviderCollection[ExamineHelper.DocumentationIndexer];

            //TODO: Fix this - we cannot "Rebuild" on a live site, because the entire index will be taken down/deleted and then recreated, if people
            // are searching during this operation, YSODs will occur.
            indexer.RebuildIndex();
        }
    }
}
