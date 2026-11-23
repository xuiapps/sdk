# Interface dispatch and casts

Source: [Units.cs](Fixtures/Interfaces/Units.cs)

## Source

```csharp
public interface IPosition { int X { get; } }
public interface IMovable : IPosition { int Move(int distance); }
public interface INamed { string Name { get; } }
public interface IMarker { }
public interface IMissing { }

public class Unit : IMovable, INamed
{
    public int Position;
    public virtual int X => Position;
    public int Move(int distance) { Position += distance; return X; }
    string INamed.Name => "Scout";
}

public class Scout : Unit, IMarker
{
    public override int X => Position + 10;
}

public static class Program
{
    private static int Inspect(IPosition position)
    {
        System.Console.WriteLine(position is IMovable);
        System.Console.WriteLine(position is IMarker);
        System.Console.WriteLine(position is IMissing);
        System.Console.WriteLine((position as IMissing) == null);
        INamed named = position as INamed;
        System.Console.WriteLine(named.Name);
        int result = ((IMovable)position).Move(4);
        System.Console.WriteLine(position.X);
        System.Console.WriteLine(((Scout)position).Position);
        return result;
    }

    public static int Main() => Inspect(new Scout());
}
```

## Output

```text
True
True
False
True
Scout
14
4
```

## Exit Code

```text
14
```

## Runtime Types

### Unit

- Interface: `False`
- Implements: `IMovable`, `IPosition`, `INamed`
- Slot 0: `INamed.get_Name` → `Unit.INamed.get_Name`
- Slot 1: `IMovable.Move` → `Unit.Move`
- Slot 2: `IPosition.get_X` → `Unit.get_X`

### IMovable

- Interface: `True`
- Implements: `IPosition`


### IPosition

- Interface: `True`
- Implements: —


### INamed

- Interface: `True`
- Implements: —


### Scout

- Interface: `False`
- Implements: `IMarker`, `IMovable`, `IPosition`, `INamed`
- Slot 0: `INamed.get_Name` → `Unit.INamed.get_Name`
- Slot 1: `IMovable.Move` → `Unit.Move`
- Slot 2: `IPosition.get_X` → `Scout.get_X`

### IMarker

- Interface: `True`
- Implements: —


### IMissing

- Interface: `True`
- Implements: —


## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param i32)))
  (type (;3;) (func (param externref)))
  (type (;4;) (func (param i32)))
  (type (;5;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;6;) (sub (struct (field (mut eqref)))))
  (type (;7;) (func (param eqref eqref) (result eqref)))
  (type (;8;) (sub 6 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;9;) (sub 8 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;10;) (func (result i32)))
  (type (;11;) (func (param eqref)))
  (type (;12;) (func (param eqref) (result i32)))
  (type (;13;) (func (param eqref)))
  (type (;14;) (func (param eqref) (result externref)))
  (type (;15;) (func (param eqref) (result externref)))
  (type (;16;) (func (param eqref i32) (result i32)))
  (type (;17;) (func (param eqref i32) (result i32)))
  (type (;18;) (func (param eqref) (result i32)))
  (type (;19;) (func (param eqref) (result i32)))
  (type (;20;) (func (param eqref) (result i32)))
  (import "xui" "console-log-boolean" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param externref)))
  (import "xui" "console-log" (func (;2;) (type 4) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 12 12 funcref)
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 85 i32.const 110 i32.const 105 i32.const 116 array.new_fixed 0 4 i32.const 3 i32.const 2 struct.new 5)
  (global (;2;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 73 i32.const 77 i32.const 111 i32.const 118 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 8 i32.const 0 i32.const 0 struct.new 5)
  (global (;3;) eqref i32.const 3 ref.null eq i32.const 0 i32.const 73 i32.const 80 i32.const 111 i32.const 115 i32.const 105 i32.const 116 i32.const 105 i32.const 111 i32.const 110 array.new_fixed 0 9 i32.const 0 i32.const 0 struct.new 5)
  (global (;4;) eqref i32.const 4 ref.null eq i32.const 0 i32.const 73 i32.const 78 i32.const 97 i32.const 109 i32.const 101 i32.const 100 array.new_fixed 0 6 i32.const 0 i32.const 0 struct.new 5)
  (global (;5;) eqref i32.const 5 global.get 1 i32.const 6 i32.const 83 i32.const 99 i32.const 111 i32.const 117 i32.const 116 array.new_fixed 0 5 i32.const 3 i32.const 2 struct.new 5)
  (global (;6;) eqref i32.const 6 ref.null eq i32.const 0 i32.const 73 i32.const 77 i32.const 97 i32.const 114 i32.const 107 i32.const 101 i32.const 114 array.new_fixed 0 7 i32.const 0 i32.const 0 struct.new 5)
  (global (;7;) eqref i32.const 7 ref.null eq i32.const 0 i32.const 73 i32.const 77 i32.const 105 i32.const 115 i32.const 115 i32.const 105 i32.const 110 i32.const 103 array.new_fixed 0 8 i32.const 0 i32.const 0 struct.new 5)
  (export "main" (func 4))
  (elem (;0;) (i32.const 3) func 11)
  (elem (;1;) (i32.const 4) func 9)
  (elem (;2;) (i32.const 5) func 8)
  (elem (;3;) (i32.const 0) func 8)
  (elem (;4;) (i32.const 1) func 9)
  (elem (;5;) (i32.const 2) func 11)
  (elem (;6;) (i32.const 9) func 10)
  (elem (;7;) (i32.const 10) func 9)
  (elem (;8;) (i32.const 11) func 8)
  (elem (;9;) (i32.const 6) func 8)
  (elem (;10;) (i32.const 7) func 9)
  (elem (;11;) (i32.const 8) func 10)
  (func (;4;) (type 10) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    struct.new_default 9
    local.set 0
    local.get 0
    ref.cast (ref 9)
    global.get 5
    struct.set 9 0
    local.get 0
    call 5
    local.get 0
    call 6
    return
  )
  (func (;5;) (type 11) (param eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 7
    return
  )
  (func (;6;) (type 12) (param eqref) (result i32)
    (local eqref eqref i32 eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    global.get 2
    call 12
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 0
    local.get 0
    global.get 6
    call 12
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 0
    local.get 0
    global.get 7
    call 12
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 0
    local.get 0
    global.get 7
    call 12
    ref.null eq
    drop
    ref.is_null
    call 0
    local.get 0
    global.get 4
    call 12
    local.set 1
    local.get 1
    ref.cast (ref 6)
    local.get 1
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 0
    i32.add
    call_indirect (type 14)
    call 1
    local.get 0
    local.tee 5
    global.get 2
    call 12
    local.set 6
    local.get 5
    ref.is_null
    i32.eqz
    local.get 6
    ref.is_null
    i32.and
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 4
    local.set 3
    local.set 2
    local.get 2
    ref.cast (ref 6)
    local.get 3
    local.get 2
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 1
    i32.add
    call_indirect (type 16)
    local.get 0
    local.set 4
    local.get 4
    ref.cast (ref 6)
    local.get 4
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 2
    i32.add
    call_indirect (type 18)
    call 2
    local.get 0
    local.tee 5
    global.get 5
    call 12
    local.set 6
    local.get 5
    ref.is_null
    i32.eqz
    local.get 6
    ref.is_null
    i32.and
    if ;; label = @1
      unreachable
    end
    local.get 6
    ref.cast (ref 9)
    struct.get 9 1
    call 2
    return
  )
  (func (;7;) (type 13) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;8;) (type 15) (param eqref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 5
      array.new_data 0 0
      i32.const 0
      i32.const 5
      call 3
      global.set 0
    end
    global.get 0
    return
  )
  (func (;9;) (type 17) (param eqref i32) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 0
    ref.cast (ref 8)
    struct.get 8 1
    local.get 1
    i32.add
    local.set 5
    ref.cast (ref 8)
    local.set 4
    local.get 4
    ref.cast (ref 8)
    local.get 5
    struct.set 8 1
    local.get 0
    local.set 2
    local.get 2
    ref.cast (ref 8)
    local.get 2
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 3
    i32.add
    call_indirect (type 20)
    return
  )
  (func (;10;) (type 19) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 9)
    struct.get 9 1
    i32.const 10
    i32.add
    return
  )
  (func (;11;) (type 20) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 8)
    struct.get 8 1
    return
  )
  (func (;12;) (type 7) (param eqref eqref) (result eqref)
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
    local.get 1
    global.get 2
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 1
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 5
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 3
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 1
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 5
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 4
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 1
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 5
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 6
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 5
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 7
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
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
  (data (;0;) "S\00c\00o\00u\00t\00")
)
```
