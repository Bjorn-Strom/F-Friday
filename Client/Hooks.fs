module Hooks

open Feliz

[<Hook>]
let useEffectOnce callback =
    let (hasRun, setHasRun) = React.useState(false)

    React.useEffect((fun () ->
       if not hasRun then
           callback()
           setHasRun(true)
    ), [| hasRun :> obj; callback :> obj |])