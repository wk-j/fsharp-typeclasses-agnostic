﻿module Control.Monad.Trans

open Prelude
open Control.Applicative

let singleton x = [x]
let concat (x:'a list list) :'a list = List.concat x

module Maybe =

    type MaybeT< ^ma > = MaybeT of (^ma ) with

        static member inline ( ? ) (MaybeT x, _Functor:Fmap) = fun f -> MaybeT (fmap (Option.map f) x)

        static member inline (?<-) (_:Return, _Monad:Return, t:MaybeT<_>) = MaybeT << return' << Some
        static member inline ( ? ) (MaybeT x, _Monad:Bind) =
            let runMaybeT (MaybeT m) = m
            fun f ->
                MaybeT <| do' {
                    let! maybe_value = x
                    return! match maybe_value with
                            | None       -> return' None
                            | Some value -> runMaybeT <| f value}

        static member inline (?<-) (f:MaybeT<_> ,_Applicative:Apply ,x:MaybeT<_> ) = ap f x

        static member inline (?<-) (_       , _MonadPlus:Mzero, t:MaybeT<_>)   = MaybeT (return' None)
        static member inline (?<-) (MaybeT x, _MonadPlus:Mplus,MaybeT y    ) =
            MaybeT <| do' {
                let! maybe_value = x
                return! match maybe_value with
                        | None -> y
                        | Some value -> x}

    let inline mapMaybeT f (MaybeT m) = MaybeT (f m)
    let inline runMaybeT   (MaybeT m) = m


module List =

    type ListT< ^ma > = ListT of (^ma ) with

        static member inline ( ? ) (ListT x     , _Functor:Fmap) = fun f -> ListT (fmap (List.map f) x)

        static member inline (?<-) (_:Return    , _Monad:Return, t:ListT<_>) = ListT << return' << singleton
        static member inline ( ? ) (ListT (x:^A), _Monad:Bind) =
            let inline runListT (ListT m) = m
            fun k -> ListT ( x >>= mapM (  (runListT) << k)  >>= (concat >> return') )

        static member inline (?<-) (f:ListT<_->_>, _Applicative:Apply, x:ListT<_> ) = ap f x

        static member inline (?<-) (_      , _MonadPlus:Mzero, t:ListT<_>) = ListT (return' [])
        static member inline (?<-) (ListT x, _MonadPlus:Mplus, ListT y   ) = ListT <| do' {
            let! a = x
            let! b = y
            return (a @ b)}

    let inline mapListT f (ListT  m) = ListT (f m)
    let inline runListT   (ListT  m) = m


type Lift = Lift with
    static member inline (?<-) (x, _MonadTrans:Lift, t:Maybe.MaybeT<_>) = Maybe.MaybeT << (liftM Some)      <| x
    static member inline (?<-) (x, _MonadTrans:Lift, t: List. ListT<_>) = List .ListT  << (liftM singleton) <| x

let inline lift x : ^R = (x ? (Lift) <- Unchecked.defaultof< ^R>)