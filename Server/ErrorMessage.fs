module ErrorMessage

open Giraffe

type Message = { Message: string }
type HttpStatus =
    | NotFound of string
    | BadRequest of string
    | InternalError of exn
    
let notFound message = RequestErrors.NOT_FOUND message
let badRequest message = RequestErrors.BAD_REQUEST message
let internalError exn = ServerErrors.INTERNAL_ERROR exn
let httpStatusResult result next context =
    task {
        match! result with
        | Ok result -> return! json result next context
        | Error (BadRequest e) -> return! badRequest { Message = e } next context
        | Error (NotFound e) -> return! notFound { Message = e } next context
        | Error (InternalError e) ->
            printfn "%A" (e.ToString())
            return! internalError { Message = "Det har skjedd en feil i backenden :(" } next context
    }