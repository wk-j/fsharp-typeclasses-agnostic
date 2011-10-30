﻿module Control.Monad.Cont

open Prelude
type Cont<'r,'a> = Cont of (('a->'r)->'r) with
    static member ( ? ) ( Cont m , _Functor:Fmap)                 = fun f -> Cont <| fun c -> m (c << f)

    static member (?<-) (_:Return, _Monad  :Return, t:Cont<_,'a>) = fun (n:'a) -> Cont (fun k -> k n)
    static member (?<-) (Cont m  , _Monad  :Bind, t:Cont<_,_>)                 =
        let runCont (Cont x) = x
        fun f -> Cont (fun k -> m (fun a -> runCont (f a) k))

let runCont (Cont x) = x

let callCC f = Cont <| fun k -> runCont (f (fun a -> Cont <| fun _ -> k a)) k