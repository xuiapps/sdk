# Class inheritance and virtual dispatch

Source: [Units.cs](Fixtures/Polymorphism/Units.cs)

## Source

```csharp
using System;

public struct Point { public int X; public int Y; }

public class Unit
{
    public string Name;
    public Point Position;
    public Unit(string name) { Name = name; }
    public virtual int Power() => 1;
    public virtual string Kind() => "unit";
}

public class Scout : Unit
{
    public int Bonus;
    public Scout(string name, int bonus) : base(name) { Bonus = bonus; }
    public override int Power() => base.Power() + Bonus;
    public override string Kind() => "scout";
}

public class Veteran : Scout
{
    public Veteran(string name, int bonus) : base(name, bonus) { }
    public override int Power() => base.Power() + 10;
}

public class Medic : Unit
{
    public Medic(string name) : base(name) { }
}

public static class Program
{
    static int Inspect(Unit unit)
    {
        Console.WriteLine(unit.Name);
        Console.WriteLine(unit.Kind());
        Console.WriteLine(unit.Power());
        Console.WriteLine(unit.Position.X);
        Console.WriteLine(unit is Scout);
        var scout = unit as Scout;
        if (scout != null) Console.WriteLine(scout.Bonus);
        Console.WriteLine(unit is Medic);
        Console.WriteLine((unit as Medic) == null);
        return unit.Power();
    }

    public static int Main()
    {
        var unit = new Veteran("Ada", 3);
        unit.Position.X = 4;
        unit.Position.Y = 6;
        return Inspect(unit);
    }
}
```

## Output

```text
Ada
scout
14
4
True
3
False
True
```

## Exit Code

```text
14
```

## Runtime Types

### Unit

- ID: `1`
- Base: `System.Object`
- WASM type: `15`
- Table start: `0`

- Field `Unit.Name`: first `1`, count `1`
- Field `Unit.Position`: first `2`, count `2`

- Slot 0: `Unit.Power`
- Slot 1: `Unit.Kind`

### Scout

- ID: `2`
- Base: `Unit`
- WASM type: `16`
- Table start: `2`

- Field `Unit.Name`: first `1`, count `1`
- Field `Unit.Position`: first `2`, count `2`
- Field `Scout.Bonus`: first `4`, count `1`

- Slot 0: `Scout.Power`
- Slot 1: `Scout.Kind`

### Veteran

- ID: `3`
- Base: `Scout`
- WASM type: `17`
- Table start: `4`

- Field `Unit.Name`: first `1`, count `1`
- Field `Unit.Position`: first `2`, count `2`
- Field `Scout.Bonus`: first `4`, count `1`

- Slot 0: `Veteran.Power`
- Slot 1: `Scout.Kind`

### Medic

- ID: `4`
- Base: `Unit`
- WASM type: `18`
- Table start: `6`

- Field `Unit.Name`: first `1`, count `1`
- Field `Unit.Position`: first `2`, count `2`

- Slot 0: `Unit.Power`
- Slot 1: `Unit.Kind`

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param externref)))
  (type (;3;) (func (param i32)))
  (type (;4;) (func (param i32)))
  (type (;5;) (struct (field i32) (field externref)))
  (type (;6;) (func (param eqref)))
  (type (;7;) (func (result eqref)))
  (type (;8;) (func (result eqref exnref)))
  (type (;9;) (func (result exnref)))
  (type (;10;) (func))
  (type (;11;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;12;) (sub (struct (field (mut eqref)))))
  (type (;13;) (func (param eqref eqref) (result eqref)))
  (type (;14;) (struct (field (mut i32)) (field (mut i32))))
  (type (;15;) (sub 12 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)) (field (mut i32)))))
  (type (;16;) (sub 15 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)) (field (mut i32)) (field (mut i32)))))
  (type (;17;) (sub 16 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)) (field (mut i32)) (field (mut i32)))))
  (type (;18;) (sub 15 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)) (field (mut i32)))))
  (type (;19;) (func (result i32)))
  (type (;20;) (func (param eqref i32) (result eqref)))
  (type (;21;) (func (param eqref i32 eqref)))
  (type (;22;) (func (param eqref i32) (result externref)))
  (type (;23;) (func (param eqref i32 externref)))
  (type (;24;) (func (param eqref i32) (result i32)))
  (type (;25;) (func (param eqref i32 i32)))
  (type (;26;) (func (result i32)))
  (type (;27;) (func (param eqref externref i32)))
  (type (;28;) (func (param eqref) (result i32)))
  (type (;29;) (func))
  (type (;30;) (func (param eqref externref i32)))
  (type (;31;) (func (param eqref externref)))
  (type (;32;) (func (param eqref) (result externref)))
  (type (;33;) (func (param eqref) (result externref)))
  (type (;34;) (func (param eqref) (result i32)))
  (type (;35;) (func (param eqref) (result i32)))
  (type (;36;) (func (param eqref) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param externref)))
  (import "xui" "console-log" (func (;1;) (type 3) (param i32)))
  (import "xui" "console-log-boolean" (func (;2;) (type 4) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 8 8 funcref)
  (memory (;0;) 16 16)
  (tag (;0;) (type 6) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut externref) ref.null extern)
  (global (;8;) (mut externref) ref.null extern)
  (global (;9;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 85 i32.const 110 i32.const 105 i32.const 116 array.new_fixed 0 4 i32.const 2 i32.const 4 struct.new 11)
  (global (;10;) eqref i32.const 2 global.get 9 i32.const 2 i32.const 83 i32.const 99 i32.const 111 i32.const 117 i32.const 116 array.new_fixed 0 5 i32.const 2 i32.const 5 struct.new 11)
  (global (;11;) eqref i32.const 3 global.get 10 i32.const 4 i32.const 86 i32.const 101 i32.const 116 i32.const 101 i32.const 114 i32.const 97 i32.const 110 array.new_fixed 0 7 i32.const 2 i32.const 5 struct.new 11)
  (global (;12;) eqref i32.const 4 global.get 9 i32.const 6 i32.const 77 i32.const 101 i32.const 100 i32.const 105 i32.const 99 array.new_fixed 0 5 i32.const 2 i32.const 4 struct.new 11)
  (export "main" (func 4))
  (elem (;0;) (i32.const 0) func 11)
  (elem (;1;) (i32.const 1) func 9)
  (elem (;2;) (i32.const 2) func 13)
  (elem (;3;) (i32.const 3) func 10)
  (elem (;4;) (i32.const 4) func 12)
  (elem (;5;) (i32.const 5) func 10)
  (elem (;6;) (i32.const 6) func 11)
  (elem (;7;) (i32.const 7) func 9)
  (func (;4;) (type 26) (result i32)
    (local eqref i32 eqref i32 i32 i64 f64 externref eqref externref i32 eqref eqref i32 i64 f64 externref)
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 3
      array.new_data 0 0
      i32.const 0
      i32.const 3
      call 3
      global.set 0
    end
    global.get 0
    i32.const 3
    local.set 10
    local.set 9
    struct.new_default 17
    local.set 8
    local.get 8
    ref.cast (ref 17)
    global.get 11
    struct.set 17 0
    local.get 8
    local.get 9
    local.get 10
    call 5
    local.get 8
    local.tee 11
    local.get 11
    local.tee 12
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 12
    ref.cast (ref 17)
    i32.const 2
    i32.const 4
    local.set 13
    local.set 1
    local.set 0
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 0
    local.get 0
    ref.is_null
    if (type 19) (result i32) ;; label = @1
      local.get 1
      i32.const 0
      i32.add
    else
      local.get 1
      i32.const 0
      i32.add
    end
    local.get 13
    call 19
    local.tee 11
    local.get 11
    local.tee 12
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 12
    ref.cast (ref 17)
    i32.const 2
    i32.const 6
    local.set 13
    local.set 1
    local.set 0
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 0
    local.get 0
    ref.is_null
    if (type 19) (result i32) ;; label = @1
      local.get 1
      i32.const 4
      i32.add
    else
      local.get 1
      i32.const 1
      i32.add
    end
    local.get 13
    call 19
    call 6
    return
  )
  (func (;5;) (type 27) (param eqref externref i32)
    (local eqref externref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.get 2
    local.set 5
    local.set 4
    local.set 3
    local.get 3
    local.get 4
    local.get 5
    call 7
    return
  )
  (func (;6;) (type 28) (param eqref) (result i32)
    (local eqref eqref i32 eqref i32 eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    struct.get 15 1
    call 0
    local.get 0
    local.set 6
    local.get 6
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    local.get 6
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 1
    i32.add
    call_indirect (type 32)
    call 0
    local.get 0
    local.set 7
    local.get 7
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    local.get 7
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 0
    i32.add
    call_indirect (type 34)
    call 1
    local.get 0
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    i32.const 7
    local.set 3
    local.set 2
    local.get 2
    ref.is_null
    local.get 3
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 2
    local.get 2
    ref.is_null
    if (type 19) (result i32) ;; label = @1
      local.get 3
      i32.const 0
      i32.add
    else
      local.get 3
      i32.const 0
      i32.add
    end
    call 18
    call 1
    local.get 0
    global.get 10
    call 20
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 2
    local.get 0
    global.get 10
    call 20
    local.set 1
    local.get 1
    ref.is_null
    if (type 29) ;; label = @1
    else
      local.get 1
      local.tee 9
      ref.is_null
      if ;; label = @2
        i32.const 4
        global.get 6
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 53
          array.new_data 0 6
          i32.const 0
          i32.const 53
          call 3
          global.set 6
        end
        global.get 6
        struct.new 5
        throw 0
      end
      local.get 9
      ref.cast (ref 16)
      struct.get 16 4
      call 1
    end
    local.get 0
    global.get 12
    call 20
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 2
    local.get 0
    global.get 12
    call 20
    ref.null eq
    drop
    ref.is_null
    call 2
    local.get 0
    local.set 7
    local.get 7
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    local.get 7
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 0
    i32.add
    call_indirect (type 34)
    return
  )
  (func (;7;) (type 30) (param eqref externref i32)
    (local eqref externref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 4
    local.set 3
    local.get 3
    local.get 4
    call 8
    local.get 0
    local.get 2
    local.set 7
    local.tee 6
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 6
    ref.cast (ref 16)
    local.set 6
    local.get 6
    ref.cast (ref 16)
    local.get 7
    struct.set 16 4
    return
  )
  (func (;8;) (type 31) (param eqref externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 7
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 3
    ref.cast (ref 15)
    local.set 3
    local.get 3
    ref.cast (ref 15)
    local.get 7
    struct.set 15 1
    return
  )
  (func (;9;) (type 32) (param eqref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 4
      array.new_data 0 1
      i32.const 0
      i32.const 4
      call 3
      global.set 1
    end
    global.get 1
    return
  )
  (func (;10;) (type 33) (param eqref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 5
      array.new_data 0 2
      i32.const 0
      i32.const 5
      call 3
      global.set 2
    end
    global.get 2
    return
  )
  (func (;11;) (type 34) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    i32.const 1
    return
  )
  (func (;12;) (type 35) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 13
    i32.const 10
    i32.add
    return
  )
  (func (;13;) (type 36) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 11
    local.get 0
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 3
    ref.cast (ref 16)
    struct.get 16 4
    i32.add
    return
  )
  (func (;14;) (type 20) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      i32.const 4
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 1
    i32.const 0
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      struct.get 17 0
      return
    end
    local.get 1
    i32.const 5
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 15)
      struct.get 15 0
      return
    end
    unreachable
  )
  (func (;15;) (type 21) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      i32.const 4
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 1
    i32.const 0
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      local.get 2
      struct.set 17 0
      return
    end
    local.get 1
    i32.const 5
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 15)
      local.get 2
      struct.set 15 0
      return
    end
    unreachable
  )
  (func (;16;) (type 22) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      i32.const 4
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 1
    i32.const 1
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      struct.get 17 1
      return
    end
    local.get 1
    i32.const 6
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 15)
      struct.get 15 1
      return
    end
    unreachable
  )
  (func (;17;) (type 23) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      i32.const 4
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 1
    i32.const 1
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      local.get 2
      struct.set 17 1
      return
    end
    local.get 1
    i32.const 6
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 15)
      local.get 2
      struct.set 15 1
      return
    end
    unreachable
  )
  (func (;18;) (type 24) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      i32.const 4
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load align=1
      return
    end
    local.get 1
    i32.const 2
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      struct.get 17 2
      return
    end
    local.get 1
    i32.const 3
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      struct.get 17 3
      return
    end
    local.get 1
    i32.const 4
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      struct.get 17 4
      return
    end
    local.get 1
    i32.const 7
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 15)
      struct.get 15 2
      return
    end
    local.get 1
    i32.const 8
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 15)
      struct.get 15 3
      return
    end
    unreachable
  )
  (func (;19;) (type 25) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      i32.const 4
      global.get 6
      struct.new 5
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
    local.get 1
    i32.const 2
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      local.get 2
      struct.set 17 2
      return
    end
    local.get 1
    i32.const 3
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      local.get 2
      struct.set 17 3
      return
    end
    local.get 1
    i32.const 4
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 17)
      local.get 2
      struct.set 17 4
      return
    end
    local.get 1
    i32.const 7
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 15)
      local.get 2
      struct.set 15 2
      return
    end
    local.get 1
    i32.const 8
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 15)
      local.get 2
      struct.set 15 3
      return
    end
    unreachable
  )
  (func (;20;) (type 13) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 12)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 12)
    struct.get 12 0
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
      ref.cast (ref 11)
      struct.get 11 1
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
  (data (;0;) "A\00d\00a\00")
  (data (;1;) "u\00n\00i\00t\00")
  (data (;2;) "s\00c\00o\00u\00t\00")
  (data (;3;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;4;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;5;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;6;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;7;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;8;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
