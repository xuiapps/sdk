# Managed arrays and struct elements

Source: [Vectors.cs](Fixtures/Arrays/Vectors.cs)

## Source

```csharp
using System;

public struct Point { public int X, Y; }
public class Scene { public Point[] Points; public string[] Labels; }

public static class Program
{
    private static int Sum(Point[] points)
    {
        int total = 0;
        for (int i = 0; i < points.Length; i++) total += points[i].X + points[i].Y;
        return total;
    }

    public static int Main()
    {
        var scene = new Scene { Points = new Point[2], Labels = new string[2] };
        scene.Points[0].X = 10;
        scene.Points[0].Y = 20;
        scene.Points[1] = scene.Points[0];
        scene.Points[1].X = 5;
        scene.Points[1].Y = 7;
        scene.Labels[0] = "two independent points";
        Console.WriteLine(scene.Labels[0]);
        Console.WriteLine(scene.Points.Length);
        Console.WriteLine(scene.Points[0].X);
        Console.WriteLine(scene.Points[1].X);
        return Sum(scene.Points);
    }
}
```

## Output

```text
two independent points
2
10
5
```

## Exit Code

```text
42
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param externref i32)))
  (type (;3;) (func (param externref i32) (result i32)))
  (type (;4;) (func (param externref)))
  (type (;5;) (func (param i32)))
  (type (;6;) (struct (field i32) (field externref)))
  (type (;7;) (func (param eqref)))
  (type (;8;) (func (result eqref)))
  (type (;9;) (func (result eqref exnref)))
  (type (;10;) (func (result exnref)))
  (type (;11;) (func))
  (type (;12;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;13;) (sub (struct (field (mut eqref)))))
  (type (;14;) (func (param eqref eqref) (result eqref)))
  (type (;15;) (sub 13 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)))))
  (type (;16;) (struct (field (mut i32)) (field (mut i32))))
  (type (;17;) (array (mut eqref)))
  (type (;18;) (array (mut externref)))
  (type (;19;) (func (result i32)))
  (type (;20;) (func (param eqref i32) (result eqref)))
  (type (;21;) (func (param eqref i32 eqref)))
  (type (;22;) (func (param eqref i32) (result externref)))
  (type (;23;) (func (param eqref i32 externref)))
  (type (;24;) (func (param eqref i32) (result i32)))
  (type (;25;) (func (param eqref i32 i32)))
  (type (;26;) (func (result i32)))
  (type (;27;) (func (param eqref)))
  (type (;28;) (func (param eqref) (result i32)))
  (type (;29;) (func))
  (type (;30;) (func))
  (import "xui" "console-log" (func (;0;) (type 4) (param externref)))
  (import "xui" "console-log" (func (;1;) (type 5) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (import "xui-object" "register-array" (func (;3;) (type 2) (param externref i32)))
  (import "xui-object" "is-array" (func (;4;) (type 3) (param externref i32) (result i32)))
  (memory (;0;) 16 32768)
  (tag (;0;) (type 7) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut externref) ref.null extern)
  (global (;8;) (mut externref) ref.null extern)
  (global (;9;) (mut i32) i32.const 16)
  (global (;10;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 83 i32.const 99 i32.const 101 i32.const 110 i32.const 101 array.new_fixed 0 5 i32.const 0 i32.const 3 struct.new 12)
  (export "main" (func 5))
  (func (;5;) (type 26) (result i32)
    (local eqref eqref i32 eqref i32 i32 i64 f64 externref eqref exnref i32 i32 i32 i64 f64 eqref i32 eqref eqref i32 i64 f64 externref)
    global.get 9
    local.tee 11
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 16
    i32.add
    global.set 9
    block (type 10) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        struct.new_default 15
        local.set 9
        local.get 9
        ref.cast (ref 15)
        global.get 10
        struct.set 15 0
        local.get 9
        call 6
        local.get 9
        local.tee 18
        local.get 18
        i32.const 2
        local.tee 17
        i32.const 0
        i32.lt_s
        if ;; label = @3
          i32.const 8
          global.get 8
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 45
            array.new_data 0 8
            i32.const 0
            i32.const 45
            call 2
            global.set 8
          end
          global.get 8
          struct.new 6
          throw 0
        end
        local.get 17
        array.new_default 17
        local.set 16
        i32.const 0
        local.set 17
        block (type 11) ;; label = @3
          loop (type 11) ;; label = @4
            local.get 17
            local.get 16
            ref.cast (ref 17)
            array.len
            i32.lt_u
            i32.eqz
            if ;; label = @5
              br 2 (;@3;)
            end
            local.get 16
            ref.cast (ref 17)
            local.get 17
            struct.new_default 16
            array.set 17
            local.get 17
            i32.const 1
            i32.add
            local.set 17
            br 0 (;@4;)
          end
        end
        local.get 16
        local.tee 16
        extern.convert_any
        i32.const 17
        call 3
        local.get 16
        ref.cast (ref 17)
        local.set 18
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        local.set 19
        local.get 19
        ref.cast (ref 15)
        local.get 18
        struct.set 15 1
        local.tee 18
        local.get 18
        i32.const 2
        local.tee 17
        i32.const 0
        i32.lt_s
        if ;; label = @3
          i32.const 8
          global.get 8
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 45
            array.new_data 0 8
            i32.const 0
            i32.const 45
            call 2
            global.set 8
          end
          global.get 8
          struct.new 6
          throw 0
        end
        local.get 17
        array.new_default 18
        local.tee 16
        extern.convert_any
        i32.const 18
        call 3
        local.get 16
        ref.cast (ref 18)
        local.set 18
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        local.set 19
        local.get 19
        ref.cast (ref 15)
        local.get 18
        struct.set 15 2
        local.set 0
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        i32.const 0
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 17)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        local.get 17
        i32.const 2
        i32.mul
        i32.const 10
        local.set 20
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        local.get 20
        call 13
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        i32.const 0
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 17)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        local.get 17
        i32.const 2
        i32.mul
        i32.const 20
        local.set 20
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 2
          i32.const 4
          i32.add
        else
          local.get 2
          i32.const 1
          i32.add
        end
        local.get 20
        call 13
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        i32.const 1
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        i32.const 0
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 17)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        ref.cast (ref 17)
        local.get 17
        array.get 17
        ref.cast (ref 16)
        local.set 19
        local.get 11
        i32.const 0
        i32.add
        local.get 19
        ref.cast (ref 16)
        struct.get 16 0
        i32.store align=1
        local.get 11
        i32.const 4
        i32.add
        local.get 19
        ref.cast (ref 16)
        struct.get 16 1
        i32.store align=1
        local.get 11
        i32.const 0
        i32.add
        local.set 20
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 17)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        ref.cast (ref 17)
        local.get 17
        array.get 17
        local.set 19
        local.get 19
        ref.cast (ref 16)
        local.get 20
        i32.const 0
        i32.add
        i32.load align=1
        struct.set 16 0
        local.get 19
        ref.cast (ref 16)
        local.get 20
        i32.const 4
        i32.add
        i32.load align=1
        struct.set 16 1
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        i32.const 1
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 17)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        local.get 17
        i32.const 2
        i32.mul
        i32.const 5
        local.set 20
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        local.get 20
        call 13
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        i32.const 1
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 17)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        local.get 17
        i32.const 2
        i32.mul
        i32.const 7
        local.set 20
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 2
          i32.const 4
          i32.add
        else
          local.get 2
          i32.const 1
          i32.add
        end
        local.get 20
        call 13
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 2
        i32.const 0
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 22
          array.new_data 0 0
          i32.const 0
          i32.const 22
          call 2
          global.set 0
        end
        global.get 0
        local.set 23
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 18)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        ref.cast (ref 18)
        local.get 17
        local.get 23
        array.set 18
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 2
        i32.const 0
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 18)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        ref.cast (ref 18)
        local.get 17
        array.get 18
        call 0
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 17)
        array.len
        call 1
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        i32.const 0
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 17)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        local.get 17
        i32.const 2
        i32.mul
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        call 12
        call 1
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        i32.const 1
        local.set 17
        local.set 16
        local.get 16
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        drop
        local.get 17
        local.get 16
        ref.cast (ref 17)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 7
            i32.const 0
            i32.const 42
            call 2
            global.set 7
          end
          global.get 7
          struct.new 6
          throw 0
        end
        local.get 16
        local.get 17
        i32.const 2
        i32.mul
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        call 12
        call 1
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 19
        ref.cast (ref 15)
        struct.get 15 1
        call 7
        local.get 11
        global.set 9
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 11
    global.set 9
    local.get 10
    throw_ref
  )
  (func (;6;) (type 27) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;7;) (type 28) (param eqref) (result i32)
    (local i32 i32 eqref i32 eqref i32 eqref i32 eqref eqref i32 i64 f64 externref)
    i32.const 0
    local.set 1
    i32.const 0
    local.set 2
    block (type 29) ;; label = @1
      loop (type 29) ;; label = @2
        local.get 2
        local.get 0
        local.tee 10
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 2
            global.set 4
          end
          global.get 4
          struct.new 6
          throw 0
        end
        local.get 10
        ref.cast (ref 17)
        array.len
        i32.lt_s
        if (type 30) ;; label = @3
          local.get 1
          local.get 0
          local.get 2
          local.set 8
          local.set 7
          local.get 7
          local.tee 10
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 4
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 4
              i32.const 0
              i32.const 53
              call 2
              global.set 4
            end
            global.get 4
            struct.new 6
            throw 0
          end
          local.get 10
          drop
          local.get 8
          local.get 7
          ref.cast (ref 17)
          array.len
          i32.lt_u
          i32.eqz
          if ;; label = @4
            i32.const 7
            global.get 7
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 42
              array.new_data 0 7
              i32.const 0
              i32.const 42
              call 2
              global.set 7
            end
            global.get 7
            struct.new 6
            throw 0
          end
          local.get 7
          local.get 8
          i32.const 2
          i32.mul
          local.set 4
          local.set 3
          local.get 3
          ref.is_null
          local.get 4
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 4
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 4
              i32.const 0
              i32.const 53
              call 2
              global.set 4
            end
            global.get 4
            struct.new 6
            throw 0
          end
          local.get 3
          local.get 3
          ref.is_null
          if (type 19) (result i32) ;; label = @4
            local.get 4
            i32.const 0
            i32.add
          else
            local.get 4
            i32.const 0
            i32.add
          end
          call 12
          local.get 0
          local.get 2
          local.set 8
          local.set 7
          local.get 7
          local.tee 10
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 4
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 4
              i32.const 0
              i32.const 53
              call 2
              global.set 4
            end
            global.get 4
            struct.new 6
            throw 0
          end
          local.get 10
          drop
          local.get 8
          local.get 7
          ref.cast (ref 17)
          array.len
          i32.lt_u
          i32.eqz
          if ;; label = @4
            i32.const 7
            global.get 7
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 42
              array.new_data 0 7
              i32.const 0
              i32.const 42
              call 2
              global.set 7
            end
            global.get 7
            struct.new 6
            throw 0
          end
          local.get 7
          local.get 8
          i32.const 2
          i32.mul
          local.set 4
          local.set 3
          local.get 3
          ref.is_null
          local.get 4
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 4
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 4
              i32.const 0
              i32.const 53
              call 2
              global.set 4
            end
            global.get 4
            struct.new 6
            throw 0
          end
          local.get 3
          local.get 3
          ref.is_null
          if (type 19) (result i32) ;; label = @4
            local.get 4
            i32.const 4
            i32.add
          else
            local.get 4
            i32.const 1
            i32.add
          end
          call 12
          i32.add
          i32.add
          local.set 1
          local.get 2
          i32.const 1
          i32.add
          local.set 2
          br 1 (;@2;)
        else
          br 2 (;@1;)
        end
        unreachable
      end
    end
    local.get 1
    return
  )
  (func (;8;) (type 20) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 2
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 6
      throw 0
    end
    unreachable
  )
  (func (;9;) (type 21) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 2
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 6
      throw 0
    end
    unreachable
  )
  (func (;10;) (type 22) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 2
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 6
      throw 0
    end
    unreachable
  )
  (func (;11;) (type 23) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 2
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 6
      throw 0
    end
    unreachable
  )
  (func (;12;) (type 24) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 2
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 6
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load align=1
      return
    end
    local.get 0
    extern.convert_any
    i32.const 17
    call 4
    local.get 1
    i32.const 2
    i32.rem_u
    i32.const 0
    i32.eq
    i32.and
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      local.get 1
      i32.const 2
      i32.div_u
      array.get 17
      ref.cast (ref 16)
      struct.get 16 0
      return
    end
    local.get 0
    extern.convert_any
    i32.const 17
    call 4
    local.get 1
    i32.const 2
    i32.rem_u
    i32.const 1
    i32.eq
    i32.and
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      local.get 1
      i32.const 2
      i32.div_u
      array.get 17
      ref.cast (ref 16)
      struct.get 16 1
      return
    end
    unreachable
  )
  (func (;13;) (type 25) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 2
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 6
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      i32.store align=1
      return
    end
    local.get 0
    extern.convert_any
    i32.const 17
    call 4
    local.get 1
    i32.const 2
    i32.rem_u
    i32.const 0
    i32.eq
    i32.and
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      local.get 1
      i32.const 2
      i32.div_u
      array.get 17
      ref.cast (ref 16)
      local.get 2
      struct.set 16 0
      return
    end
    local.get 0
    extern.convert_any
    i32.const 17
    call 4
    local.get 1
    i32.const 2
    i32.rem_u
    i32.const 1
    i32.eq
    i32.and
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      local.get 1
      i32.const 2
      i32.div_u
      array.get 17
      ref.cast (ref 16)
      local.get 2
      struct.set 16 1
      return
    end
    unreachable
  )
  (func (;14;) (type 14) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 13)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 13)
    struct.get 13 0
    local.set 2
    loop ;; label = @1
      local.get 2
      local.get 1
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      ref.cast (ref 12)
      struct.get 12 1
      local.tee 2
      ref.is_null
      if ;; label = @2
        ref.null eq
        return
      end
      br 0 (;@1;)
    end
    unreachable
  )
  (data (;0;) "t\00w\00o\00 \00i\00n\00d\00e\00p\00e\00n\00d\00e\00n\00t\00 \00p\00o\00i\00n\00t\00s\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
  (data (;7;) "I\00n\00d\00e\00x\00 \00w\00a\00s\00 \00o\00u\00t\00s\00i\00d\00e\00 \00t\00h\00e\00 \00b\00o\00u\00n\00d\00s\00 \00o\00f\00 \00t\00h\00e\00 \00a\00r\00r\00a\00y\00.\00")
  (data (;8;) "A\00r\00i\00t\00h\00m\00e\00t\00i\00c\00 \00o\00p\00e\00r\00a\00t\00i\00o\00n\00 \00r\00e\00s\00u\00l\00t\00e\00d\00 \00i\00n\00 \00a\00n\00 \00o\00v\00e\00r\00f\00l\00o\00w\00.\00")
)
```
