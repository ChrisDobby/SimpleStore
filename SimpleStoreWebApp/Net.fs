namespace SimpleStoreWebApp

module Net =
    open System.Net
    open System
    open System.IO

    let postForm (data:string) callback url =        
        let dataBytes = System.Text.Encoding.ASCII.GetBytes(data)
        let req = WebRequest.Create(Uri(url)) 
        req.Method <- "POST"
        req.ContentType <- "Content-Type: application/x-www-form-urlencoded"
        req.ContentLength <- int64 dataBytes.Length
        use dataStream = req.GetRequestStream()
        dataStream.Write(dataBytes, 0, dataBytes.Length)
        dataStream.Close()
        use resp = req.GetResponse() 
        use stream = resp.GetResponseStream() 
        use reader = new IO.StreamReader(stream) 
        let data = reader.ReadToEnd()
        callback data


