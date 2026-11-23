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
- WASM type: `8`
- Table start: `0`

- Field `Unit.Name`: first `1`, count `1`
- Field `Unit.Position`: first `2`, count `2`

- Slot 0: `Unit.Power`
- Slot 1: `Unit.Kind`

### Scout

- ID: `2`
- Base: `Unit`
- WASM type: `10`
- Table start: `2`

- Field `Unit.Name`: first `1`, count `1`
- Field `Unit.Position`: first `2`, count `2`
- Field `Scout.Bonus`: first `4`, count `1`

- Slot 0: `Scout.Power`
- Slot 1: `Scout.Kind`

### Veteran

- ID: `3`
- Base: `Scout`
- WASM type: `11`
- Table start: `4`

- Field `Unit.Name`: first `1`, count `1`
- Field `Unit.Position`: first `2`, count `2`
- Field `Scout.Bonus`: first `4`, count `1`

- Slot 0: `Veteran.Power`
- Slot 1: `Scout.Kind`

### Medic

- ID: `4`
- Base: `Unit`
- WASM type: `12`
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
  (type (;5;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;6;) (sub (struct (field (mut eqref)))))
  (type (;7;) (func (param eqref eqref) (result eqref)))
  (type (;8;) (sub 6 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)) (field (mut i32)))))
  (type (;9;) (struct (field (mut i32)) (field (mut i32))))
  (type (;10;) (sub 8 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)) (field (mut i32)) (field (mut i32)))))
  (type (;11;) (sub 10 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)) (field (mut i32)) (field (mut i32)))))
  (type (;12;) (sub 8 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)) (field (mut i32)))))
  (type (;13;) (func (result i32)))
  (type (;14;) (func (param eqref externref i32)))
  (type (;15;) (func (param eqref) (result i32)))
  (type (;16;) (func))
  (type (;17;) (func (param eqref externref i32)))
  (type (;18;) (func (param eqref) (result externref)))
  (type (;19;) (func (param eqref) (result i32)))
  (type (;20;) (func (param eqref) (result externref)))
  (type (;21;) (func (param eqref) (result i32)))
  (type (;22;) (func (param eqref externref)))
  (type (;23;) (func (param eqref) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param externref)))
  (import "xui" "console-log" (func (;1;) (type 3) (param i32)))
  (import "xui" "console-log-boolean" (func (;2;) (type 4) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 8 8 funcref)
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 85 i32.const 110 i32.const 105 i32.const 116 array.new_fixed 0 4 i32.const 2 i32.const 4 struct.new 5)
  (global (;4;) eqref i32.const 2 global.get 3 i32.const 2 i32.const 83 i32.const 99 i32.const 111 i32.const 117 i32.const 116 array.new_fixed 0 5 i32.const 2 i32.const 5 struct.new 5)
  (global (;5;) eqref i32.const 3 global.get 4 i32.const 4 i32.const 86 i32.const 101 i32.const 116 i32.const 101 i32.const 114 i32.const 97 i32.const 110 array.new_fixed 0 7 i32.const 2 i32.const 5 struct.new 5)
  (global (;6;) eqref i32.const 4 global.get 3 i32.const 6 i32.const 77 i32.const 101 i32.const 100 i32.const 105 i32.const 99 array.new_fixed 0 5 i32.const 2 i32.const 4 struct.new 5)
  (export "main" (func 4))
  (elem (;0;) (i32.const 0) func 9)
  (elem (;1;) (i32.const 1) func 8)
  (elem (;2;) (i32.const 2) func 13)
  (elem (;3;) (i32.const 3) func 10)
  (elem (;4;) (i32.const 4) func 11)
  (elem (;5;) (i32.const 5) func 10)
  (elem (;6;) (i32.const 6) func 9)
  (elem (;7;) (i32.const 7) func 8)
  (func (;4;) (type 13) (result i32)
    (local i32 i64 f64 externref eqref externref i32 eqref eqref i32 i64 f64 externref)
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
    local.set 6
    local.set 5
    struct.new_default 11
    local.set 4
    local.get 4
    ref.cast (ref 11)
    global.get 5
    struct.set 11 0
    local.get 4
    local.get 5
    local.get 6
    call 5
    local.get 4
    local.tee 7
    local.get 7
    ref.cast (ref 11)
    i32.const 4
    local.set 9
    ref.cast (ref 11)
    local.set 8
    local.get 8
    ref.cast (ref 11)
    local.get 9
    struct.set 11 2
    local.tee 7
    local.get 7
    ref.cast (ref 11)
    i32.const 6
    local.set 9
    ref.cast (ref 11)
    local.set 8
    local.get 8
    ref.cast (ref 11)
    local.get 9
    struct.set 11 3
    call 6
    return
  )
  (func (;5;) (type 14) (param eqref externref i32)
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
  (func (;6;) (type 15) (param eqref) (result i32)
    (local eqref eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 8)
    struct.get 8 1
    call 0
    local.get 0
    local.set 2
    local.get 2
    ref.cast (ref 8)
    local.get 2
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 1
    i32.add
    call_indirect (type 18)
    call 0
    local.get 0
    local.set 3
    local.get 3
    ref.cast (ref 8)
    local.get 3
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 0
    i32.add
    call_indirect (type 19)
    call 1
    local.get 0
    ref.cast (ref 8)
    ref.cast (ref 8)
    struct.get 8 2
    call 1
    local.get 0
    global.get 4
    call 14
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 2
    local.get 0
    global.get 4
    call 14
    local.set 1
    local.get 1
    ref.is_null
    if (type 16) ;; label = @1
    else
      local.get 1
      ref.cast (ref 10)
      struct.get 10 4
      call 1
    end
    local.get 0
    global.get 6
    call 14
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 2
    local.get 0
    global.get 6
    call 14
    ref.null eq
    drop
    ref.is_null
    call 2
    local.get 0
    local.set 3
    local.get 3
    ref.cast (ref 8)
    local.get 3
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 0
    i32.add
    call_indirect (type 19)
    return
  )
  (func (;7;) (type 17) (param eqref externref i32)
    (local eqref externref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 4
    local.set 3
    local.get 3
    local.get 4
    call 12
    local.get 0
    local.get 2
    local.set 7
    ref.cast (ref 10)
    local.set 6
    local.get 6
    ref.cast (ref 10)
    local.get 7
    struct.set 10 4
    return
  )
  (func (;8;) (type 18) (param eqref) (result externref)
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
  (func (;9;) (type 19) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    i32.const 1
    return
  )
  (func (;10;) (type 20) (param eqref) (result externref)
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
  (func (;11;) (type 21) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 13
    i32.const 10
    i32.add
    return
  )
  (func (;12;) (type 22) (param eqref externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 7
    ref.cast (ref 8)
    local.set 3
    local.get 3
    ref.cast (ref 8)
    local.get 7
    struct.set 8 1
    return
  )
  (func (;13;) (type 23) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 9
    local.get 0
    ref.cast (ref 10)
    struct.get 10 4
    i32.add
    return
  )
  (func (;14;) (type 7) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 6)
    struct.get 6 0
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
      ref.cast (ref 5)
      struct.get 5 1
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
)
```
