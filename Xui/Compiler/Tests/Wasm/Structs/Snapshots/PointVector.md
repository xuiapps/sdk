# Point + Vector WASM

Source: [PointVector.cs](Fixtures/Structs/PointVector.cs)

## Source

```csharp
using nfloat = System.Runtime.InteropServices.NFloat;

public struct Point
{
    public nfloat X;
    public nfloat Y;
    public Point(nfloat x, nfloat y) { X = x; Y = y; }
    public static Point operator +(Point p, Vector v) => new(p.X + v.X, p.Y + v.Y);
}
public struct Vector
{
    public nfloat X;
    public nfloat Y;
    public Vector(nfloat x, nfloat y) { X = x; Y = y; }
}
public static class Program
{
    public static int Main()
    {
        var p = new Point(1, 2);
        var v = new Vector(3, 4);
        var result = p + v;
        System.Console.WriteLine((double)result.X);
        System.Console.WriteLine((double)result.Y);
        return 0;
    }
}
```

## CLR output

```text
4
6
```

## CLR exit code

```text
0
```

## WASM output

```text
4
6
```

## Exit Code

```text
0
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param f64)))
  (type (;3;) (struct (field i32) (field externref)))
  (type (;4;) (func (param eqref)))
  (type (;5;) (func (result eqref)))
  (type (;6;) (func (result eqref exnref)))
  (type (;7;) (func (result exnref)))
  (type (;8;) (func))
  (type (;9;) (struct (field (mut f32)) (field (mut f32))))
  (type (;10;) (struct (field (mut f32)) (field (mut f32))))
  (type (;11;) (func (result i32)))
  (type (;12;) (func (param eqref i32) (result eqref)))
  (type (;13;) (func (param eqref i32 eqref)))
  (type (;14;) (func (param eqref i32) (result externref)))
  (type (;15;) (func (param eqref i32 externref)))
  (type (;16;) (func (param eqref i32) (result f32)))
  (type (;17;) (func (param eqref i32 f32)))
  (type (;18;) (func (result i32)))
  (type (;19;) (func (param eqref f32 f32 i32)))
  (type (;20;) (func (param eqref f32 f32 i32)))
  (type (;21;) (func (param i32 i32 i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (tag (;0;) (type 4) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut i32) i32.const 16)
  (export "main" (func 2))
  (func (;2;) (type 18) (result i32)
    (local i32 eqref i32 eqref i32 f32 i32 i64 f64 externref eqref f32 f32 i32 eqref f32 f32 i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 19
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 19
    i32.const 48
    i32.add
    global.set 6
    local.get 19
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 8
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        i32.const 2
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        local.get 19
        i32.const 8
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 10
        local.get 19
        i32.const 8
        i32.add
        local.set 13
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        call 3
        local.get 19
        i32.const 8
        i32.add
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        i32.const 3
        f32.convert_i32_s
        f64.promote_f32
        i32.const 4
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 16
        f32.demote_f64
        local.set 15
        local.set 17
        local.set 14
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        call 4
        local.get 0
        local.set 20
        local.get 19
        i32.const 16
        i32.add
        local.get 20
        i32.const 8
        memory.copy
        local.get 19
        i32.const 16
        i32.add
        local.get 19
        i32.const 24
        i32.add
        call 5
        local.tee 21
        local.get 21
        local.set 20
        local.get 19
        i32.const 32
        i32.add
        local.get 20
        i32.const 8
        memory.copy
        local.get 19
        i32.const 32
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        call 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        call 0
        i32.const 0
        local.get 19
        global.set 6
        return
        unreachable
      end
      unreachable
    end
    local.set 18
    local.get 19
    global.set 6
    local.get 18
    throw_ref
  )
  (func (;3;) (type 19) (param eqref f32 f32 i32)
    (local eqref i32 eqref i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 9
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 9
    i32.const 0
    i32.add
    global.set 6
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 3
        local.get 1
        f64.promote_f32
        local.set 18
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 11) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 18
        f32.demote_f64
        call 11
        local.get 0
        local.get 3
        local.get 2
        f64.promote_f32
        local.set 18
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 11) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 18
        f32.demote_f64
        call 11
        local.get 9
        global.set 6
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 9
    global.set 6
    local.get 8
    throw_ref
  )
  (func (;4;) (type 20) (param eqref f32 f32 i32)
    (local eqref i32 eqref i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 9
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 9
    i32.const 0
    i32.add
    global.set 6
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 3
        local.get 1
        f64.promote_f32
        local.set 18
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 11) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 18
        f32.demote_f64
        call 11
        local.get 0
        local.get 3
        local.get 2
        f64.promote_f32
        local.set 18
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 11) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 18
        f32.demote_f64
        call 11
        local.get 9
        global.set 6
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 9
    global.set 6
    local.get 8
    throw_ref
  )
  (func (;5;) (type 21) (param i32 i32 i32) (result i32)
    (local eqref i32 eqref i32 f32 eqref f32 f32 i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 13
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 64
    i32.add
    global.set 6
    local.get 13
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 13
    i32.const 0
    i32.add
    local.set 0
    local.get 13
    i32.const 8
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 13
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 14
        local.get 13
        i32.const 16
        i32.add
        local.get 14
        i32.const 8
        memory.copy
        local.get 13
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 14
        local.get 13
        i32.const 24
        i32.add
        local.get 14
        i32.const 8
        memory.copy
        local.get 13
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.add
        f64.promote_f32
        local.get 0
        local.set 14
        local.get 13
        i32.const 32
        i32.add
        local.get 14
        i32.const 8
        memory.copy
        local.get 13
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 14
        local.get 13
        i32.const 40
        i32.add
        local.get 14
        i32.const 8
        memory.copy
        local.get 13
        i32.const 40
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 13
        i32.const 48
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 8
        local.get 13
        i32.const 48
        i32.add
        local.set 11
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 3
        local.get 13
        i32.const 48
        i32.add
        local.set 14
        local.get 2
        local.get 14
        i32.const 8
        memory.copy
        local.get 2
        local.get 13
        global.set 6
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 13
    global.set 6
    local.get 12
    throw_ref
  )
  (func (;6;) (type 12) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    unreachable
  )
  (func (;7;) (type 13) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    unreachable
  )
  (func (;8;) (type 14) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    unreachable
  )
  (func (;9;) (type 15) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    unreachable
  )
  (func (;10;) (type 16) (param eqref i32) (result f32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      f32.load align=1
      return
    end
    unreachable
  )
  (func (;11;) (type 17) (param eqref i32 f32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      f32.store align=1
      return
    end
    unreachable
  )
  (data (;0;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;1;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;2;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;3;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;4;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```

## Status

Passed.
