using System.Web;
using System.Web.Optimization;

namespace MediCostUI
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Script bundles
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-*"));

            bundles.Add(new ScriptBundle("~/bundles/medicosts").Include(
                      "~/Scripts/medicost.js",
                      "~/Scripts/typeahead.bundle.js",
                      "~/Scripts/jquery.tablesorter.combined.js"));

            // Style Bundles
            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                      "~/Content/bootstrap.css"));

            bundles.Add(new StyleBundle("~/Content/jqueryui").Include(
                      "~/Content/themes/base/resizable.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/reset.css",
                      "~/Content/medicost.css"));                      
        }
    }
}
