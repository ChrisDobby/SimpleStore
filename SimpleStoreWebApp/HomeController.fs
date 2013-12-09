namespace SimpleStoreWebApp.Controllers

open System.Web
open System.Web.Mvc
open System.Data.Linq
open Microsoft.FSharp.Data.TypeProviders
open System.Configuration
open System.Globalization

[<HandleError>]
type HomeController() =
    inherit BaseController()
    member this.Index () =
        let selectList = new System.Collections.Generic.List<SelectListItem>()
        
        CultureInfo.GetCultures(CultureTypes.SpecificCultures) |>
            Seq.map(fun l -> RegionInfo(l.LCID)) |>
            Seq.sortBy(fun r -> r.EnglishName) |>
            Seq.distinct |>
            Seq.iter(fun r -> selectList.Add(new SelectListItem(Value = r.TwoLetterISORegionName, Text = r.EnglishName, Selected = (r.TwoLetterISORegionName = "GB"))))

        this.ViewData.["orderCountry"] <- selectList

        this.View() :> ActionResult

    [<HttpPost>]
    member this.Checkout(order:SimpleStoreWebApp.Data.Order) =
        let goodsCost(order:SimpleStoreWebApp.Data.dbSchema.ServiceTypes.Order) = 
            order.OrderItems |> Seq.map(fun i -> i.Product.Price * float i.Quantity) |> Seq.sum

        let postageCost(order:SimpleStoreWebApp.Data.dbSchema.ServiceTypes.Order) =
            order.OrderItems |> Seq.map(fun i -> i.Product.Postage) |> Seq.max

        let names(fullName:string) =
            let splitNames = fullName.Split(' ')
            match splitNames with
                | [|f;s|] -> f, s
                | [|n|] -> n, ""
                | [||] -> "", ""
                | _ -> splitNames.[0], splitNames |> Seq.fold(fun name s -> name + " " + s) ""

        let checkoutUrl(initialUrl:string, order:SimpleStoreWebApp.Data.dbSchema.ServiceTypes.Order) = 
            let firstName, lastName = names order.CustomerName
            System.String.Format("{0}&first_name={1}&last_name={2}&address1={3}&address2={4}&city={5}&zip={6}&country={7}&email={8}&item_name_1={9}&amount_1={10}&item_number_1={11}&quantity_1=1&shipping_1={12}", initialUrl, firstName, lastName, order.CustomerAddress1, order.CustomerAddress2, order.CustomerAddress3, order.CustomerPostcode, order.CustomerCountry, order.CustomerEmail, order.Id, order.GoodsCost, order.Id, order.Postage)

        let newOrder = SimpleStoreWebApp.Data.Data.addOrder(order, goodsCost, postageCost)

        let staticUrl = System.String.Format("{0}?cmd=_cart&upload=1&custom={1}&tax_cart=0&business={2}&currency_code={3}&return={4}&cancel_return={5}", ConfigurationManager.AppSettings.["payPalPaymentsUrl"], newOrder.Id, ConfigurationManager.AppSettings.["paypalAccount"], ConfigurationManager.AppSettings.["paypalCurrency"], ConfigurationManager.AppSettings.["pdtReturnUrl"], ConfigurationManager.AppSettings.["paypalCancelUrl"])

        this.Redirect(checkoutUrl(staticUrl, newOrder))

    member this.OrderComplete() =
        this.View() :> ActionResult
