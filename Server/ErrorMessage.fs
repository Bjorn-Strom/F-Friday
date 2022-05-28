module ErrorMessage

open Giraffe

type UserMessage = { UserMessage: string }
type HttpStatus =
    | NotFound of string
    | BadRequest of string
    | Forbidden of string
    | InternalError of exn
    
let notFound message = RequestErrors.NOT_FOUND message
let badRequest message = RequestErrors.BAD_REQUEST message
let forbidden message = RequestErrors.FORBIDDEN message
let internalError exn = ServerErrors.INTERNAL_ERROR exn
let httpStatusResult result next context =
    task {
        match! result with
        | Ok result -> return! json result next context
        | Error (BadRequest e) -> return! badRequest { UserMessage = e } next context
        | Error (NotFound e) -> return! notFound { UserMessage = e } next context
        | Error (Forbidden e) -> return! forbidden { UserMessage = e } next context
        | Error (InternalError e) ->
            printfn "%A" (e.ToString())
            return! internalError { UserMessage = $"Det har skjedd en feil i backenden :(" } next context
    }