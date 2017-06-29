﻿namespace InlineAbstractions.TypeClasses

open InlineAbstractions.Prelude
open InlineAbstractions.Types
open Monoid
open Dual
open Endo

module Foldable =
    type Foldr = Foldr with
        static member instance (Foldr, x:option<_>, _) = fun (f,z) -> match x with Some t -> f t z | _ -> z
        static member instance (Foldr, x:List<_>  , _) = fun (f,z) -> List.foldBack          f x z

    type DefaultImpl =
        static member inline FoldMap f x = Inline.instance (Foldr, x) (mappend << f, mempty())
   
    type FoldMap = FoldMap with
        static member inline instance (FoldMap, x:option<_>, _) = fun f -> DefaultImpl.FoldMap f x
        static member inline instance (FoldMap, x:List<_>  , _) = fun f -> DefaultImpl.FoldMap f x
        static member inline instance (FoldMap, x:array<_> , _) = fun f -> Array.foldBack (mappend << f) x (mempty())

    type DefaultImpl with
        static member inline Foldr f z x = 
            let inline foldMap f x = Inline.instance (FoldMap, x) f
            appEndo (foldMap (Endo << f ) x) z

        static member inline Foldl f z t = 
            let inline foldMap f x = Inline.instance (FoldMap, x) f
            appEndo (getDual (foldMap (Dual << Endo << flip f) t)) z

    type Foldr with
        static member inline instance (Foldr, x:array<_>, _) = fun (f,z) -> DefaultImpl.Foldr f z x

    type Foldl = Foldl with
        static member instance (Foldl, x:option<_>, _) = fun (f,z) -> match x with Some t -> f z t | _ -> z
        static member instance (Foldl, x:List<_>  , _) = fun (f,z) -> List.fold              f z x
        static member instance (Foldl, x:array<_> , _) = fun (f,z) -> DefaultImpl.Foldl      f z x

    let inline internal foldr (f: 'a -> 'b -> 'b) (z:'b) x :'b = Inline.instance (Foldr, x) (f,z)
    let inline internal foldMap f x = Inline.instance (FoldMap, x) f
    let inline internal foldl (f: 'a -> 'b -> 'a) (z:'a) x :'a = Inline.instance (Foldl, x) (f,z)