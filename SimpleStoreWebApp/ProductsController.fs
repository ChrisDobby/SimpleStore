namespace SimpleStoreWebApp.Controllers

open System.Net
open System.Net.Http
open System.Web
open System.Web.Http
open System.Data.EntityClient
open System.Data.Linq
open Microsoft.FSharp.Data.TypeProviders

[<CLIMutable>]
type Product =
    {
        Id : string;
        Name : string;
        Image : string;
        Description : string;
        Price : float;
        Postage : float;
        PreOrder : bool;
        AvailabilityText : string;
        InStock : bool;
    }

type ProductsController() =
    inherit ApiController()

    member this.Get() =
        let db = SimpleStoreWebApp.Data.dbSchema.GetDataContext()
        let products = db.Products |> Seq.map(fun p -> 
                                        {
                                            Id = p.Id.ToString();
                                            Name = p.Name;
                                            Image = p.Image;
                                            Description = p.Description;
                                            Price = p.Price;
                                            Postage = p.Postage;
                                            PreOrder = p.PreOrder;
                                            AvailabilityText = p.AvailabilityText;
                                            InStock = p.InStock;
                                        })
                                   |> Seq.sortBy(fun p -> p.Price)

        this.Request.CreateResponse(HttpStatusCode.OK, products)
