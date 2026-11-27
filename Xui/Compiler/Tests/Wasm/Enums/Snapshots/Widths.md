# Enum widths and signedness

Source: [Widths.cs](Fixtures/Enums/Widths.cs)

## Source

```csharp
using System;
using System.Runtime.InteropServices;

[Flags] public enum Options : byte { None, Read = 1, Write = 128 }
public enum Delta : sbyte { Back = -7 }
public enum Wide : ulong { High = 0x8000000000000001UL }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Packet
{
    public byte Prefix;
    public Options Options;
    public Delta Delta;
    public Wide Wide;
    public byte Suffix;
}

public class Box { public Packet Value; }

public static class Program
{
    static Packet Copy(Packet packet) => packet;
    static Options Choose(bool write) => write ? Options.Read | Options.Write : Options.Read;

    public static int Main()
    {
        var packet = new Packet();
        packet.Prefix = 17;
        packet.Options = Choose(true);
        packet.Delta = Delta.Back;
        packet.Wide = Wide.High;
        packet.Suffix = 29;
        var box = new Box();
        box.Value = Copy(packet);
        var copy = box.Value;
        var values = new Options[2];
        values[1] = copy.Options;
        Console.WriteLine(copy.Prefix);
        Console.WriteLine((int)values[1]);
        Console.WriteLine((int)copy.Delta);
        Console.WriteLine(copy.Wide == Wide.High);
        Console.WriteLine(copy.Wide > (Wide)long.MaxValue);
        Console.WriteLine(copy.Suffix);
        return (values[1] & Options.Write) != 0 ? 42 : 1;
    }
}
```

## Output

```text
17
129
-7
True
True
29
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
  (type (;2;) (func (param i32)))
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
  (type (;13;) (struct (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i64)) (field (mut i32))))
  (type (;14;) (struct (field (mut i32))))
  (type (;15;) (struct (field (mut i32))))
  (type (;16;) (struct (field (mut i64))))
  (type (;17;) (sub 11 (struct (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i64)) (field (mut i32)))))
  (type (;18;) (array (mut i32)))
  (type (;19;) (func (result i32)))
  (type (;20;) (func (param eqref i32) (result eqref)))
  (type (;21;) (func (param eqref i32 eqref)))
  (type (;22;) (func (param eqref i32) (result externref)))
  (type (;23;) (func (param eqref i32 externref)))
  (type (;24;) (func (param eqref i32) (result i32)))
  (type (;25;) (func (param eqref i32 i32)))
  (type (;26;) (func (param eqref i32) (result i32)))
  (type (;27;) (func (param eqref i32 i32)))
  (type (;28;) (func (param eqref i32) (result i64)))
  (type (;29;) (func (param eqref i32 i64)))
  (type (;30;) (func (result i32)))
  (type (;31;) (func))
  (type (;32;) (func (param i32) (result i32)))
  (type (;33;) (func))
  (type (;34;) (func (param eqref)))
  (type (;35;) (func (param i32 i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log-boolean" (func (;1;) (type 3) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 32768)
  (tag (;0;) (type 5) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut externref) ref.null extern)
  (global (;8;) (mut i32) i32.const 16)
  (global (;9;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 66 i32.const 111 i32.const 120 array.new_fixed 0 3 i32.const 0 i32.const 6 struct.new 10)
  (export "main" (func 3))
  (func (;3;) (type 30) (result i32)
    (local i32 i32 eqref i32 eqref i32 i32 i64 f64 externref i32 eqref exnref i32 i32 i32 i64 f64 eqref i32 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 13
    i32.const 1048432
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 144
    i32.add
    global.set 8
    local.get 13
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 12
    memory.fill
    local.get 13
    i32.const 12
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 12
    memory.fill
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 3
        ref.null eq
        local.get 3
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        i32.const 0
        call 12
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 1
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        i32.const 0
        call 12
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 2
          i32.add
        else
          local.get 3
          i32.const 2
          i32.add
        end
        i32.const 0
        call 14
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 3
          i32.add
        else
          local.get 3
          i32.const 3
          i32.add
        end
        i64.const 0
        call 16
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 11
          i32.add
        else
          local.get 3
          i32.const 4
          i32.add
        end
        i32.const 0
        call 12
        local.get 0
        local.set 3
        ref.null eq
        local.get 3
        i32.const 17
        local.set 22
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        local.get 22
        i32.const 255
        i32.and
        call 12
        local.get 0
        local.set 3
        ref.null eq
        local.get 3
        i32.const 1
        i32.const 255
        i32.and
        local.set 10
        local.get 10
        call 4
        local.set 22
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 1
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        local.get 22
        i32.const 255
        i32.and
        call 12
        local.get 0
        local.set 3
        ref.null eq
        local.get 3
        i32.const -7
        local.set 22
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 2
          i32.add
        else
          local.get 3
          i32.const 2
          i32.add
        end
        local.get 22
        i32.extend8_s
        call 14
        local.get 0
        local.set 3
        ref.null eq
        local.get 3
        i64.const -9223372036854775807
        local.set 23
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 3
          i32.add
        else
          local.get 3
          i32.const 3
          i32.add
        end
        local.get 23
        call 16
        local.get 0
        local.set 3
        ref.null eq
        local.get 3
        i32.const 29
        local.set 22
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 19) (result i32) ;; label = @3
          local.get 3
          i32.const 11
          i32.add
        else
          local.get 3
          i32.const 4
          i32.add
        end
        local.get 22
        i32.const 255
        i32.and
        call 12
        struct.new_default 17
        local.set 11
        local.get 11
        ref.cast (ref 17)
        global.get 9
        struct.set 17 0
        local.get 11
        call 5
        local.get 11
        local.tee 20
        local.get 20
        local.get 0
        local.set 14
        local.get 13
        i32.const 24
        i32.add
        local.get 14
        i32.const 12
        memory.copy
        local.get 13
        i32.const 24
        i32.add
        local.get 13
        i32.const 36
        i32.add
        call 6
        local.set 22
        local.tee 21
        ref.is_null
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 21
        ref.cast (ref 17)
        local.set 21
        local.get 21
        ref.cast (ref 17)
        local.get 22
        i32.const 0
        i32.add
        i32.load8_u
        struct.set 17 1
        local.get 21
        ref.cast (ref 17)
        local.get 22
        i32.const 1
        i32.add
        i32.load8_u
        struct.set 17 2
        local.get 21
        ref.cast (ref 17)
        local.get 22
        i32.const 2
        i32.add
        i32.load8_s
        struct.set 17 3
        local.get 21
        ref.cast (ref 17)
        local.get 22
        i32.const 3
        i32.add
        i64.load align=1
        struct.set 17 4
        local.get 21
        ref.cast (ref 17)
        local.get 22
        i32.const 11
        i32.add
        i32.load8_u
        struct.set 17 5
        local.tee 21
        ref.is_null
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 21
        ref.cast (ref 17)
        local.set 21
        local.get 13
        i32.const 48
        i32.add
        local.get 21
        ref.cast (ref 17)
        struct.get 17 1
        i32.store8
        local.get 13
        i32.const 49
        i32.add
        local.get 21
        ref.cast (ref 17)
        struct.get 17 2
        i32.store8
        local.get 13
        i32.const 50
        i32.add
        local.get 21
        ref.cast (ref 17)
        struct.get 17 3
        i32.store8
        local.get 13
        i32.const 51
        i32.add
        local.get 21
        ref.cast (ref 17)
        struct.get 17 4
        i64.store align=1
        local.get 13
        i32.const 59
        i32.add
        local.get 21
        ref.cast (ref 17)
        struct.get 17 5
        i32.store8
        local.get 13
        i32.const 48
        i32.add
        local.set 14
        local.get 1
        local.get 14
        i32.const 12
        memory.copy
        i32.const 2
        local.tee 19
        i32.const 0
        i32.lt_s
        if ;; label = @3
          i32.const 8
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 45
            array.new_data 0 7
            i32.const 0
            i32.const 45
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 19
        array.new_default 18
        local.tee 20
        local.get 20
        i32.const 1
        local.get 1
        local.set 14
        local.get 13
        i32.const 60
        i32.add
        local.get 14
        i32.const 12
        memory.copy
        local.get 13
        i32.const 60
        i32.add
        i32.const 1
        i32.add
        i32.load8_u
        local.set 22
        local.set 19
        local.set 18
        local.get 18
        local.tee 21
        ref.is_null
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 21
        drop
        local.get 19
        local.get 18
        ref.cast (ref 18)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 6
            i32.const 0
            i32.const 42
            call 2
            global.set 6
          end
          global.get 6
          struct.new 4
          throw 0
        end
        local.get 18
        ref.cast (ref 18)
        local.get 19
        local.get 22
        i32.const 255
        i32.and
        array.set 18
        local.get 1
        local.set 14
        local.get 13
        i32.const 72
        i32.add
        local.get 14
        i32.const 12
        memory.copy
        local.get 13
        i32.const 72
        i32.add
        i32.const 0
        i32.add
        i32.load8_u
        call 0
        local.tee 20
        local.get 20
        i32.const 1
        local.set 19
        local.set 18
        local.get 18
        local.tee 21
        ref.is_null
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 21
        drop
        local.get 19
        local.get 18
        ref.cast (ref 18)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 6
            i32.const 0
            i32.const 42
            call 2
            global.set 6
          end
          global.get 6
          struct.new 4
          throw 0
        end
        local.get 18
        ref.cast (ref 18)
        local.get 19
        array.get 18
        i32.const 255
        i32.and
        call 0
        local.get 1
        local.set 14
        local.get 13
        i32.const 84
        i32.add
        local.get 14
        i32.const 12
        memory.copy
        local.get 13
        i32.const 84
        i32.add
        i32.const 2
        i32.add
        i32.load8_s
        call 0
        local.get 1
        local.set 14
        local.get 13
        i32.const 96
        i32.add
        local.get 14
        i32.const 12
        memory.copy
        local.get 13
        i32.const 96
        i32.add
        i32.const 3
        i32.add
        i64.load align=1
        i64.const -9223372036854775807
        i64.eq
        call 1
        local.get 1
        local.set 14
        local.get 13
        i32.const 108
        i32.add
        local.get 14
        i32.const 12
        memory.copy
        local.get 13
        i32.const 108
        i32.add
        i32.const 3
        i32.add
        i64.load align=1
        i64.const 9223372036854775807
        i64.gt_u
        call 1
        local.get 1
        local.set 14
        local.get 13
        i32.const 120
        i32.add
        local.get 14
        i32.const 12
        memory.copy
        local.get 13
        i32.const 120
        i32.add
        i32.const 11
        i32.add
        i32.load8_u
        call 0
        i32.const 1
        local.set 19
        local.set 18
        local.get 18
        local.tee 21
        ref.is_null
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
            call 2
            global.set 3
          end
          global.get 3
          struct.new 4
          throw 0
        end
        local.get 21
        drop
        local.get 19
        local.get 18
        ref.cast (ref 18)
        array.len
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 6
            i32.const 0
            i32.const 42
            call 2
            global.set 6
          end
          global.get 6
          struct.new 4
          throw 0
        end
        local.get 18
        ref.cast (ref 18)
        local.get 19
        array.get 18
        i32.const 255
        i32.and
        i32.const 128
        i32.and
        if (type 31) ;; label = @3
          i32.const 42
          local.get 13
          global.set 8
          return
        else
          i32.const 1
          local.get 13
          global.set 8
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 13
    global.set 8
    local.get 12
    throw_ref
  )
  (func (;4;) (type 32) (param i32) (result i32)
    local.get 0
    if (type 33) ;; label = @1
      i32.const 129
      i32.const 255
      i32.and
      return
    else
      i32.const 1
      i32.const 255
      i32.and
      return
    end
    unreachable
  )
  (func (;5;) (type 34) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;6;) (type 35) (param i32 i32) (result i32)
    (local exnref i32 i32 i32 i64 f64)
    global.get 8
    local.tee 3
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 3
    i32.const 32
    i32.add
    global.set 8
    local.get 3
    i32.const 0
    i32.add
    local.get 0
    i32.const 12
    memory.copy
    local.get 3
    i32.const 0
    i32.add
    local.set 0
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 4
        local.get 3
        i32.const 12
        i32.add
        local.get 4
        i32.const 12
        memory.copy
        local.get 3
        i32.const 12
        i32.add
        local.set 4
        local.get 1
        local.get 4
        i32.const 12
        memory.copy
        local.get 1
        local.get 3
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 3
    global.set 8
    local.get 2
    throw_ref
  )
  (func (;7;) (type 20) (param eqref i32) (result eqref)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    unreachable
  )
  (func (;8;) (type 21) (param eqref i32 eqref)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    unreachable
  )
  (func (;9;) (type 22) (param eqref i32) (result externref)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    unreachable
  )
  (func (;10;) (type 23) (param eqref i32 externref)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    unreachable
  )
  (func (;11;) (type 24) (param eqref i32) (result i32)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load8_u
      return
    end
    unreachable
  )
  (func (;12;) (type 25) (param eqref i32 i32)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      i32.store8
      return
    end
    unreachable
  )
  (func (;13;) (type 26) (param eqref i32) (result i32)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load8_s
      return
    end
    unreachable
  )
  (func (;14;) (type 27) (param eqref i32 i32)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      i32.store8
      return
    end
    unreachable
  )
  (func (;15;) (type 28) (param eqref i32) (result i64)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i64.load align=1
      return
    end
    unreachable
  )
  (func (;16;) (type 29) (param eqref i32 i64)
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
        call 2
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      i64.store align=1
      return
    end
    unreachable
  )
  (func (;17;) (type 12) (param eqref eqref) (result eqref)
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
  (data (;0;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;1;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;2;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;3;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;4;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
  (data (;6;) "I\00n\00d\00e\00x\00 \00w\00a\00s\00 \00o\00u\00t\00s\00i\00d\00e\00 \00t\00h\00e\00 \00b\00o\00u\00n\00d\00s\00 \00o\00f\00 \00t\00h\00e\00 \00a\00r\00r\00a\00y\00.\00")
  (data (;7;) "A\00r\00i\00t\00h\00m\00e\00t\00i\00c\00 \00o\00p\00e\00r\00a\00t\00i\00o\00n\00 \00r\00e\00s\00u\00l\00t\00e\00d\00 \00i\00n\00 \00a\00n\00 \00o\00v\00e\00r\00f\00l\00o\00w\00.\00")
)
```
