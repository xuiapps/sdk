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
  (type (;5;) (struct (field i32) (field externref)))
  (type (;6;) (func (param eqref)))
  (type (;7;) (func (result eqref)))
  (type (;8;) (func (result eqref exnref)))
  (type (;9;) (func (result exnref)))
  (type (;10;) (func))
  (type (;11;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;12;) (sub (struct (field (mut eqref)))))
  (type (;13;) (func (param eqref eqref) (result eqref)))
  (type (;14;) (sub 12 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;15;) (sub 14 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;16;) (func (result i32)))
  (type (;17;) (func (param eqref)))
  (type (;18;) (func (param eqref) (result i32)))
  (type (;19;) (func (param eqref)))
  (type (;20;) (func (param eqref) (result externref)))
  (type (;21;) (func (param eqref) (result externref)))
  (type (;22;) (func (param eqref i32) (result i32)))
  (type (;23;) (func (param eqref i32) (result i32)))
  (type (;24;) (func (param eqref) (result i32)))
  (type (;25;) (func (param eqref) (result i32)))
  (type (;26;) (func (param eqref) (result i32)))
  (import "xui" "console-log-boolean" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param externref)))
  (import "xui" "console-log" (func (;2;) (type 4) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 12 12 funcref)
  (tag (;0;) (type 6) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 85 i32.const 110 i32.const 105 i32.const 116 array.new_fixed 0 4 i32.const 3 i32.const 2 struct.new 11)
  (global (;8;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 73 i32.const 77 i32.const 111 i32.const 118 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 8 i32.const 0 i32.const 0 struct.new 11)
  (global (;9;) eqref i32.const 3 ref.null eq i32.const 0 i32.const 73 i32.const 80 i32.const 111 i32.const 115 i32.const 105 i32.const 116 i32.const 105 i32.const 111 i32.const 110 array.new_fixed 0 9 i32.const 0 i32.const 0 struct.new 11)
  (global (;10;) eqref i32.const 4 ref.null eq i32.const 0 i32.const 73 i32.const 78 i32.const 97 i32.const 109 i32.const 101 i32.const 100 array.new_fixed 0 6 i32.const 0 i32.const 0 struct.new 11)
  (global (;11;) eqref i32.const 5 global.get 7 i32.const 6 i32.const 83 i32.const 99 i32.const 111 i32.const 117 i32.const 116 array.new_fixed 0 5 i32.const 3 i32.const 2 struct.new 11)
  (global (;12;) eqref i32.const 6 ref.null eq i32.const 0 i32.const 73 i32.const 77 i32.const 97 i32.const 114 i32.const 107 i32.const 101 i32.const 114 array.new_fixed 0 7 i32.const 0 i32.const 0 struct.new 11)
  (global (;13;) eqref i32.const 7 ref.null eq i32.const 0 i32.const 73 i32.const 77 i32.const 105 i32.const 115 i32.const 115 i32.const 105 i32.const 110 i32.const 103 array.new_fixed 0 8 i32.const 0 i32.const 0 struct.new 11)
  (export "main" (func 4))
  (elem (;0;) (i32.const 3) func 11)
  (elem (;1;) (i32.const 4) func 9)
  (elem (;2;) (i32.const 5) func 8)
  (elem (;3;) (i32.const 9) func 10)
  (elem (;4;) (i32.const 10) func 9)
  (elem (;5;) (i32.const 11) func 8)
  (elem (;6;) (i32.const 6) func 8)
  (elem (;7;) (i32.const 7) func 9)
  (elem (;8;) (i32.const 8) func 10)
  (func (;4;) (type 16) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    struct.new_default 15
    local.set 0
    local.get 0
    ref.cast (ref 15)
    global.get 11
    struct.set 15 0
    local.get 0
    call 5
    local.get 0
    call 6
    return
  )
  (func (;5;) (type 17) (param eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 7
    return
  )
  (func (;6;) (type 18) (param eqref) (result i32)
    (local eqref eqref i32 eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    global.get 8
    call 12
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 0
    local.get 0
    global.get 12
    call 12
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 0
    local.get 0
    global.get 13
    call 12
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 0
    local.get 0
    global.get 13
    call 12
    ref.null eq
    drop
    ref.is_null
    call 0
    local.get 0
    global.get 10
    call 12
    local.set 1
    local.get 1
    local.tee 6
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 6
    ref.cast (ref 12)
    local.get 1
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 0
    i32.add
    call_indirect (type 20)
    call 1
    local.get 0
    local.tee 5
    global.get 8
    call 12
    local.set 6
    local.get 5
    ref.is_null
    i32.eqz
    local.get 6
    ref.is_null
    i32.and
    if ;; label = @1
      i32.const 3
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 28
        array.new_data 0 3
        i32.const 0
        i32.const 28
        call 3
        global.set 3
      end
      global.get 3
      struct.new 5
      throw 0
    end
    local.get 6
    i32.const 4
    local.set 3
    local.set 2
    local.get 2
    local.tee 6
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 6
    ref.cast (ref 12)
    local.get 3
    local.get 2
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 1
    i32.add
    call_indirect (type 22)
    local.get 0
    local.set 4
    local.get 4
    local.tee 6
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 6
    ref.cast (ref 12)
    local.get 4
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 2
    i32.add
    call_indirect (type 24)
    call 2
    local.get 0
    local.tee 5
    global.get 11
    call 12
    local.set 6
    local.get 5
    ref.is_null
    i32.eqz
    local.get 6
    ref.is_null
    i32.and
    if ;; label = @1
      i32.const 3
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 28
        array.new_data 0 3
        i32.const 0
        i32.const 28
        call 3
        global.set 3
      end
      global.get 3
      struct.new 5
      throw 0
    end
    local.get 6
    local.tee 6
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 6
    ref.cast (ref 15)
    struct.get 15 1
    call 2
    return
  )
  (func (;7;) (type 19) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;8;) (type 21) (param eqref) (result externref)
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
  (func (;9;) (type 23) (param eqref i32) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 0
    local.tee 4
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 4
    ref.cast (ref 14)
    struct.get 14 1
    local.get 1
    i32.add
    local.set 5
    local.tee 4
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 4
    ref.cast (ref 14)
    local.set 4
    local.get 4
    ref.cast (ref 14)
    local.get 5
    struct.set 14 1
    local.get 0
    local.set 2
    local.get 2
    local.tee 4
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 4
    ref.cast (ref 14)
    local.get 2
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 3
    i32.add
    call_indirect (type 26)
    return
  )
  (func (;10;) (type 25) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 2
    ref.cast (ref 15)
    struct.get 15 1
    i32.const 10
    i32.add
    return
  )
  (func (;11;) (type 26) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 2
    ref.cast (ref 14)
    struct.get 14 1
    return
  )
  (func (;12;) (type 13) (param eqref eqref) (result eqref)
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
    local.get 1
    global.get 8
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 7
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 11
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 9
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 7
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 11
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 10
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 7
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 11
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 12
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 11
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 13
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
  (data (;0;) "S\00c\00o\00u\00t\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
