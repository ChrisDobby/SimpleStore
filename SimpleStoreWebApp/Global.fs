namespace SimpleStoreWebApp

open System
open System.Web
open System.Web.Http
open System.Web.Mvc
open System.Web.Routing
open System.Web.Optimization

type Route = { controller : string
               action : string
               id : UrlParameter }

type HttpRouteDefaults = { Controller : string; Id : obj }

type Global() =
    inherit System.Web.HttpApplication() 
    
    static member RegisterRoutes(routes:RouteCollection) =
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")

        routes.MapRoute("Default", 
                        "{controller}/{action}/{id}", 
                        { controller = "Home"; action = "Index"
                          id = UrlParameter.Optional } )

    static member RegisterApi(config:HttpConfiguration) =
        GlobalConfiguration.Configuration.Formatters.XmlFormatter.UseXmlSerializer <- true
        GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver <-
            Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()        
        config.Routes.MapHttpRoute("DefaultApi",
            "api/{controller}/{id}",
            { Controller = "Home"; Id = RouteParameter.Optional })

    static member RegisterBundles(bundles:BundleCollection) =
        bundles.Add(ScriptBundle("~/bundles/jquery").Include([|"~/Scripts/jquery-{version}.js"|]))
        bundles.Add(ScriptBundle("~/bundles/jqueryval").Include([|"~/Scripts/jquery.validate*"|]))
        bundles.Add(ScriptBundle("~/bundles/modernizr").Include([|"~/Scripts/modernizr-*"|]))
        bundles.Add(ScriptBundle("~/bundles/bootstrap").Include([|"~/Scripts/bootstrap.js";"~/Scripts/respond.js"|]))
        bundles.Add(ScriptBundle("~/bundles/knockout").Include([|"~/Scripts/knockout-{version}.js";"~/Scripts/knockout.validation.js"|]))
        bundles.Add(ScriptBundle("~/bundles/sammy").Include([|"~/Scripts/sammy-latest.min.js"|]))
        bundles.Add(ScriptBundle("~/bundles/app").Include([|
                                                            "~/Scripts/app/ajaxPrefilters.js";
                                                            "~/Scripts/app/app.datamodel.js";
                                                            "~/Scripts/app/app.viewmodel.js";
                                                            "~/Scripts/app/home.viewmodel.js";
                                                            "~/Scripts/app/checkout.viewmodel.js";
                                                            "~/Scripts/app/_run.js"                                
                                                            |]))
        bundles.Add(StyleBundle("~/Content/css").Include([|"~/Content/cosmobootstrap.min.css";"~/Content/site.css"|]))


    member this.Start() =
        AreaRegistration.RegisterAllAreas()
        GlobalFilters.Filters.Add(new HandleErrorAttribute())
        Global.RegisterApi(GlobalConfiguration.Configuration) |> ignore
        Global.RegisterRoutes(RouteTable.Routes) |> ignore
        Global.RegisterBundles(BundleTable.Bundles);

