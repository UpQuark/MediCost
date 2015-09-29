﻿using System.Web;
using System.Web.Optimization;

namespace MediCostUI
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/medicost.js",
                      "~/Scripts/typeahead.bundle.js",
                      "~/Scripts/jquery.tablesorter.combined.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/medicost.css"));                      
        }
    }
}
