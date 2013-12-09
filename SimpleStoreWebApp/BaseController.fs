namespace SimpleStoreWebApp.Controllers

open System.Web
open System.Web.Mvc

type MenuItem(title, link) = 
    member this.Title with get() = title
    member this.Link with get() = link

[<HandleError>]
type BaseController() as this =
    inherit Controller()

    do
        let items = SimpleStoreWebApp.Data.menuItems.Load("App_Data\Menu.xml").GetItems()
        this.ViewData.["MenuItems"] <- items |> Seq.map(fun i -> new MenuItem(i.Title, i.Link))

