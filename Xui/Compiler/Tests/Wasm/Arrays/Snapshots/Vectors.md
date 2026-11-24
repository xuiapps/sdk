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
  (type (;2;) (func (param externref)))
  (type (;3;) (func (param i32)))
  (type (;4;) (struct (field i32) (field externref)))
  (type (;5;) (func (param eqref)))
  (type (;6;) (func (result eqref)))
  (type (;7;) (func (result eqref exnref)))
  (type (;8;) (func (result exnref)))
  (type (;9;) (func))
  (type (;10;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;11;) (sub (struct (field (mut eqref)))))
  (type (;12;) (func (param eqref eqref) (result eqref)))
  (type (;13;) (sub 11 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)))))
  (type (;14;) (struct (field (mut i32)) (field (mut i32))))
  (type (;15;) (array (mut eqref)))
  (type (;16;) (array (mut externref)))
  (type (;17;) (func (result i32)))
  (type (;18;) (func (param eqref)))
  (type (;19;) (func (param eqref) (result i32)))
  (type (;20;) (func))
  (type (;21;) (func))
  (import "xui" "console-log" (func (;0;) (type 2) (param externref)))
  (import "xui" "console-log" (func (;1;) (type 3) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (tag (;0;) (type 5) (param eqref))
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
  (global (;10;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 83 i32.const 99 i32.const 101 i32.const 110 i32.const 101 array.new_fixed 0 5 i32.const 0 i32.const 3 struct.new 10)
  (export "main" (func 3))
  (func (;3;) (type 17) (result i32)
    (local eqref i32 i64 f64 externref eqref exnref i32 i32 i32 i64 f64 eqref i32 eqref eqref i32 i64 f64 externref)
    global.get 9
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 9
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        struct.new_default 13
        local.set 5
        local.get 5
        ref.cast (ref 13)
        global.get 10
        struct.set 13 0
        local.get 5
        call 4
        local.get 5
        local.tee 14
        local.get 14
        i32.const 2
        local.tee 13
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
          struct.new 4
          throw 0
        end
        local.get 13
        array.new_default 15
        local.set 12
        i32.const 0
        local.set 13
        block (type 9) ;; label = @3
          loop (type 9) ;; label = @4
            local.get 13
            local.get 12
            ref.cast (ref 15)
            array.len
            i32.lt_u
            i32.eqz
            if ;; label = @5
              br 2 (;@3;)
            end
            local.get 12
            ref.cast (ref 15)
            local.get 13
            struct.new_default 14
            array.set 15
            local.get 13
            i32.const 1
            i32.add
            local.set 13
            br 0 (;@4;)
          end
        end
        local.get 12
        local.set 14
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        local.set 15
        local.get 15
        ref.cast (ref 13)
        local.get 14
        struct.set 13 1
        local.tee 14
        local.get 14
        i32.const 2
        local.tee 13
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
          struct.new 4
          throw 0
        end
        local.get 13
        array.new_default 16
        local.set 14
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        local.set 15
        local.get 15
        ref.cast (ref 13)
        local.get 14
        struct.set 13 2
        local.set 0
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        i32.const 0
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 15)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 15)
        local.get 13
        array.get 15
        i32.const 10
        local.set 16
        ref.cast (ref 14)
        local.set 15
        local.get 15
        ref.cast (ref 14)
        local.get 16
        struct.set 14 0
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        i32.const 0
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 15)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 15)
        local.get 13
        array.get 15
        i32.const 20
        local.set 16
        ref.cast (ref 14)
        local.set 15
        local.get 15
        ref.cast (ref 14)
        local.get 16
        struct.set 14 1
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        i32.const 1
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        i32.const 0
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 15)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 15)
        local.get 13
        array.get 15
        ref.cast (ref 14)
        local.set 15
        local.get 7
        i32.const 0
        i32.add
        local.get 15
        ref.cast (ref 14)
        struct.get 14 0
        i32.store align=1
        local.get 7
        i32.const 4
        i32.add
        local.get 15
        ref.cast (ref 14)
        struct.get 14 1
        i32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 16
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 15)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 15)
        local.get 13
        array.get 15
        local.set 15
        local.get 15
        ref.cast (ref 14)
        local.get 16
        i32.const 0
        i32.add
        i32.load align=1
        struct.set 14 0
        local.get 15
        ref.cast (ref 14)
        local.get 16
        i32.const 4
        i32.add
        i32.load align=1
        struct.set 14 1
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        i32.const 1
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 15)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 15)
        local.get 13
        array.get 15
        i32.const 5
        local.set 16
        ref.cast (ref 14)
        local.set 15
        local.get 15
        ref.cast (ref 14)
        local.get 16
        struct.set 14 0
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        i32.const 1
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 15)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 15)
        local.get 13
        array.get 15
        i32.const 7
        local.set 16
        ref.cast (ref 14)
        local.set 15
        local.get 15
        ref.cast (ref 14)
        local.get 16
        struct.set 14 1
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 2
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
        local.set 19
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 16)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 16)
        local.get 13
        local.get 19
        array.set 16
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 2
        i32.const 0
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 16)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 16)
        local.get 13
        array.get 16
        call 0
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 15)
        array.len
        call 1
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        i32.const 0
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 15)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 15)
        local.get 13
        array.get 15
        ref.cast (ref 14)
        struct.get 14 0
        call 1
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        i32.const 1
        local.set 13
        local.set 12
        local.get 12
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        drop
        local.get 13
        local.get 12
        ref.cast (ref 15)
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
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 15)
        local.get 13
        array.get 15
        ref.cast (ref 14)
        struct.get 14 0
        call 1
        local.get 0
        local.tee 15
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
          struct.new 4
          throw 0
        end
        local.get 15
        ref.cast (ref 13)
        struct.get 13 1
        call 5
        local.get 7
        global.set 9
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 7
    global.set 9
    local.get 6
    throw_ref
  )
  (func (;4;) (type 18) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;5;) (type 19) (param eqref) (result i32)
    (local i32 i32 eqref i32 eqref eqref i32 i64 f64 externref)
    i32.const 0
    local.set 1
    i32.const 0
    local.set 2
    block (type 20) ;; label = @1
      loop (type 20) ;; label = @2
        local.get 2
        local.get 0
        local.tee 6
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
          struct.new 4
          throw 0
        end
        local.get 6
        ref.cast (ref 15)
        array.len
        i32.lt_s
        if (type 21) ;; label = @3
          local.get 1
          local.get 0
          local.get 2
          local.set 4
          local.set 3
          local.get 3
          local.tee 6
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
            struct.new 4
            throw 0
          end
          local.get 6
          drop
          local.get 4
          local.get 3
          ref.cast (ref 15)
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
            struct.new 4
            throw 0
          end
          local.get 3
          ref.cast (ref 15)
          local.get 4
          array.get 15
          ref.cast (ref 14)
          struct.get 14 0
          local.get 0
          local.get 2
          local.set 4
          local.set 3
          local.get 3
          local.tee 6
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
            struct.new 4
            throw 0
          end
          local.get 6
          drop
          local.get 4
          local.get 3
          ref.cast (ref 15)
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
            struct.new 4
            throw 0
          end
          local.get 3
          ref.cast (ref 15)
          local.get 4
          array.get 15
          ref.cast (ref 14)
          struct.get 14 1
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
  (func (;6;) (type 12) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 11)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 11)
    struct.get 11 0
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
      ref.cast (ref 10)
      struct.get 10 1
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
