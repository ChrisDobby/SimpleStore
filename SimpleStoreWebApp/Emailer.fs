namespace SimpleStoreWebApp

module internal Emailer =
    open System.Net
    open System.Net.Mail

    let sendMessage(subject, messageText, address, fromAddress, companyName, mailServer, mailUser:string, mailPassword:string) = async {
        let message = new MailMessage(new MailAddress(fromAddress, companyName), new MailAddress(address), 
                                      IsBodyHtml = true,
                                      Body = messageText,
                                      Subject = subject)
        let client = new SmtpClient(mailServer, UseDefaultCredentials = false, 
                                    EnableSsl = true,
                                    Credentials = new NetworkCredential(mailUser, mailPassword))
        client.SendAsync(message, new obj())
    }
    
    let private customerCompleteMessage(order:SimpleStoreWebApp.Data.dbSchema.ServiceTypes.Order, companyName:string) =
        let message = new System.Text.StringBuilder()
        message.Append(sprintf "Your order from %s has been received and the goods will be sent as soon as possible.  Your order is:<br/><br/>" companyName) |> ignore
        order.OrderItems |> Seq.iter(fun i -> message.Append(sprintf "%d x %s<br/>" i.Quantity i.Product.Name) |> ignore)
        message.Append(sprintf "<br/><br/>Thank you for ordering from %s" companyName) |> ignore
        message.ToString()

    let private adminCompleteMessage(order:SimpleStoreWebApp.Data.dbSchema.ServiceTypes.Order) =
        let message = new System.Text.StringBuilder()
        message.Append(sprintf "New order has been received from<br/><br/>%s</br>%s</br>%s</br>%s</br>%s</br>%s</br>" order.CustomerName order.CustomerAddress1 order.CustomerAddress2 order.CustomerAddress3 order.CustomerPostcode order.CustomerEmail) |> ignore
        message.Append("The order consists of:<br/><br/>") |> ignore
        order.OrderItems |> Seq.iter(fun i -> message.Append(sprintf "%d x %s<br/>" i.Quantity i.Product.Name) |> ignore)
        message.Append(sprintf "<br/><br/>&pound;%0.2f has been received from Paypal." order.AmountPaid.Value) |> ignore
        message.ToString()

    let orderComplete(order:SimpleStoreWebApp.Data.dbSchema.ServiceTypes.Order, adminAddress, companyName, mailServer, mailUser, mailPassword) =
        let customerMessage = customerCompleteMessage(order, companyName)
        let adminMessage = adminCompleteMessage order

        sendMessage("Your Order", customerMessage, order.CustomerEmail, adminAddress, companyName, mailServer, mailUser, mailPassword) |> Async.Start

        order.OrderItems |> 
            Seq.map(fun i -> i.Product.EmailOrdersTo.Split(';')) |>
            Seq.collect(fun x -> seq{ for add in x do yield add }) |>
            Seq.distinct |>
            Seq.iter(fun m -> sendMessage("Order Received", adminMessage, m, adminAddress, companyName, mailServer, mailUser, mailPassword) |> Async.Start)

        SimpleStoreWebApp.Data.Data.updateOrder order.Id (fun o -> o.AcknowledgementSent <- new System.Nullable<System.DateTime>(System.DateTime.Now)) |> ignore