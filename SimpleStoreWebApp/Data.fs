namespace SimpleStoreWebApp.Data

open System.Data.Linq
open Microsoft.FSharp.Data.TypeProviders
open System.ComponentModel.DataAnnotations
open FSharp.Data
open FSharp.Net

type OrderItem() = 
    let mutable productId = 0
    let mutable quantity = 0

    member this.ProductId with get() = productId and set(value) = productId <- value
    member this.Quantity with get() = quantity and set(value) = quantity <- value

type Order() =
    let mutable emailAddress = ""
    let mutable orderName = ""
    let mutable orderAddress1 = ""
    let mutable orderAddress2 = ""
    let mutable orderAddress3 = ""
    let mutable orderPostcode = ""
    let mutable orderCountry = ""
    let mutable items = new System.Collections.Generic.List<OrderItem>()

    [<Required>]
    member this.EmailAddress with get() = emailAddress and set(value) = emailAddress <- value
    [<Required>]
    member this.OrderName with get() = orderName and set(value) = orderName <- value
    [<Required>]
    member this.OrderAddress1 with get() = orderAddress1 and set(value) = orderAddress1 <- value
    member this.OrderAddress2 with get() = orderAddress2 and set(value) = orderAddress2 <- value
    member this.OrderAddress3 with get() = orderAddress3 and set(value) = orderAddress3 <- value
    [<Required>]
    member this.OrderPostcode with get() = orderPostcode and set(value) = orderPostcode <- value
    [<Required>]
    member this.OrderCountry with get() = orderCountry and set(value) = orderCountry <- value
    member this.Items with get() = items and set(value) = items <- value

type internal dbSchema = SqlEntityConnection<ConnectionStringName = "StoreConnection", Pluralize = true, ConfigFile = "web.config">

type menuItems = XmlProvider<"MenuSample.xml">

module internal Data =
    let addOrder(order:Order, goodsCostFun, postageCostFun) =
        let db = dbSchema.GetDataContext()
        let newOrder = dbSchema.ServiceTypes.Order.CreateOrder(
                                                            0, 
                                                            System.DateTime.Now, 
                                                            order.EmailAddress, 
                                                            order.OrderName, 
                                                            order.OrderAddress1, 
                                                            order.OrderAddress3,
                                                            order.OrderPostcode, 
                                                            0.0, 
                                                            0.0)
        newOrder.CustomerAddress2 <- order.OrderAddress2
        newOrder.CustomerCountry <- order.OrderCountry

        db.Orders.AddObject(newOrder)
        order.Items
        |> Seq.map(fun i -> new dbSchema.ServiceTypes.OrderItem(ProductId = i.ProductId, Quantity = i.Quantity, Cost = 0.0))
        |> Seq.iter(fun o -> newOrder.OrderItems.Add(o)) 

        newOrder.GoodsCost <- goodsCostFun newOrder
        newOrder.Postage <- postageCostFun newOrder

        db.DataContext.SaveChanges() |> ignore
        newOrder

    let updateOrder id updateFunc =
        let db = dbSchema.GetDataContext()
        let orders = query{ for order in db.Orders.Include("OrderItems").Include("OrderItems.Product") do where(order.Id = id) }
        orders |> Seq.iter(fun o -> updateFunc o)
        db.DataContext.SaveChanges() |> ignore
        orders        