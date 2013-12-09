namespace SimpleStoreWebApp.Controllers

open System.Web
open System.Web.Mvc
open System.Data.Linq
open Microsoft.FSharp.Data.TypeProviders
open System.Configuration

[<HandleError>]
type PaypalController() =
    inherit BaseController()

    member this.Error() =
        this.View() :> ActionResult

    member this.PaymentCancelled() =
        this.View() :> ActionResult

    member this.PDT() =

        let processPayment orderId transactionId amountPaid orderComplete orderError =
            let ordersFullyPaid(orders:seq<SimpleStoreWebApp.Data.dbSchema.ServiceTypes.Order>) =
                orders |> Seq.where(fun o -> o.AmountPaid.HasValue && (o.GoodsCost + o.Postage) = o.AmountPaid.Value)  |> Seq.length = (orders |> Seq.length)
        
            let updatedOrders = SimpleStoreWebApp.Data.Data.updateOrder orderId (fun o -> o.AmountPaid <- new System.Nullable<float>(amountPaid)
                                                                                          o.PaypalTransactionId <- transactionId)

            if not(updatedOrders |> Seq.isEmpty) && ordersFullyPaid updatedOrders then
                let adminAddress, companyName, mailServer, mailUser, mailPassword =
                    ConfigurationManager.AppSettings.["adminEmail"],
                    ConfigurationManager.AppSettings.["companyName"],
                    ConfigurationManager.AppSettings.["mailServer"],
                    ConfigurationManager.AppSettings.["mailUser"],
                    ConfigurationManager.AppSettings.["mailPassword"]

                updatedOrders |> Seq.iter(fun o -> SimpleStoreWebApp.Emailer.orderComplete(o, adminAddress, companyName, mailServer, mailUser, mailPassword))
                orderComplete
            else
                orderError

        let processPdt orderComplete orderError (data:string) =
            let keyValue (pair:string) =
                match pair.Split('=') with
                    | [| n;v |] -> n, v
                    | [| n |] -> n, ""
                    | _ -> "", ""

            let rec dataValue l (p:string) =
                match l with
                    | i::t -> let k, v = keyValue i
                              if k.ToLower() = p.ToLower() then
                                v
                              else
                                dataValue t p
                    | [] -> ""

            let dataList = data.Split('\n') |> Seq.toList
            let transactionId, orderId, amount = 
                (dataValue dataList "txn_id"),
                int (dataValue dataList "custom"),
                float (dataValue dataList "mc_gross")

            processPayment orderId transactionId amount orderComplete orderError

        try
            let transactionId = this.Request.QueryString.["tx"]
            let processPdtWithResult = processPdt (this.RedirectToAction("OrderComplete", "Home")) (this.RedirectToAction("Error"))
            if transactionId = null then
                this.RedirectToAction("Error")
            else
                SimpleStoreWebApp.Net.postForm (sprintf "cmd=_notify-synch&tx=%s&at=%s" transactionId ConfigurationManager.AppSettings.["paypalPdtToken"]) processPdtWithResult ConfigurationManager.AppSettings.["payPalPaymentsUrl"]
        with
            | _ -> this.RedirectToAction("Error")