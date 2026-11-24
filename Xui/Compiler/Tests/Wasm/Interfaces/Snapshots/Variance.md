# Covariant sources and contravariant sinks

Source: [Variance.cs](Fixtures/Interfaces/Variance.cs)

## Source

```csharp
using System;

public class Animal { public int Value; }
public class Dog : Animal { }
public interface ISource<out T> { T Get(); T Read() => Get(); }
public interface ISink<in T> { int Put(T value); }

public class Source<T> : ISource<T>
{
    public T Item;
    public Source(T item) { Item = item; }
    public T Get() => Item;
}

public class Sink : ISink<Animal>
{
    public int Put(Animal value) => value.Value;
}

public static class Program
{
    static Animal Read(ISource<Animal> source) => source.Read();
    static int Write(ISink<Dog> sink, Dog dog) => sink.Put(dog);
    static void Inspect(object value)
    {
        Console.WriteLine(value is ISource<Animal>);
        Console.WriteLine(value is ISource<object>);
        Console.WriteLine(value is ISource<Dog>);
    }

    public static int Main()
    {
        var dog = new Dog();
        dog.Value = 42;
        var source = new Source<Dog>(dog);
        Console.WriteLine(Read(source) == dog);
        Inspect(source);
        Inspect(new Source<Animal>(new Animal()));
        Inspect(new Source<int>(7));
        Console.WriteLine(Write(new Sink(), dog));
        return Read(source).Value;
    }
}
```

## Output

```text
True
True
True
True
True
True
False
False
False
False
42
```

## Exit Code

```text
42
```

## Interface Dispatch

- `Source<Dog>`: `ISource<Animal>.Read` → `ISource<Dog>.Read`
- `Source<Dog>`: `ISource<Dog>.Get` → `Source<Dog>.Get`
- `Source<Dog>`: `ISource<Animal>.Get` → `Source<Dog>.Get`
- `Source<Animal>`: `ISource<Animal>.Read` → `ISource<Animal>.Read`
- `Source<Animal>`: `ISource<Animal>.Get` → `Source<Animal>.Get`
- `Sink`: `ISink<Dog>.Put` → `Sink.Put`

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
  (type (;13;) (sub 11 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;14;) (sub 13 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;15;) (sub 11 (struct (field (mut eqref)) (field (mut eqref)))))
  (type (;16;) (sub 11 (struct (field (mut eqref)) (field (mut eqref)))))
  (type (;17;) (sub 11 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;18;) (sub 11 (struct (field (mut eqref)))))
  (type (;19;) (func (result i32)))
  (type (;20;) (func (param eqref)))
  (type (;21;) (func (param eqref eqref)))
  (type (;22;) (func (param eqref) (result eqref)))
  (type (;23;) (func (param eqref)))
  (type (;24;) (func (param eqref)))
  (type (;25;) (func (param eqref eqref)))
  (type (;26;) (func (param eqref i32)))
  (type (;27;) (func (param eqref)))
  (type (;28;) (func (param eqref eqref) (result i32)))
  (type (;29;) (func (param eqref) (result eqref)))
  (type (;30;) (func (param eqref) (result eqref)))
  (type (;31;) (func (param eqref eqref) (result i32)))
  (type (;32;) (func (param eqref eqref) (result i32)))
  (type (;33;) (func (param eqref) (result eqref)))
  (type (;34;) (func (param eqref) (result eqref)))
  (type (;35;) (func (param eqref) (result eqref)))
  (type (;36;) (func (param eqref) (result eqref)))
  (import "xui" "console-log-boolean" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 28 28 funcref)
  (tag (;0;) (type 5) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 65 i32.const 110 i32.const 105 i32.const 109 i32.const 97 i32.const 108 array.new_fixed 0 6 i32.const 0 i32.const 2 struct.new 10)
  (global (;7;) eqref i32.const 2 global.get 6 i32.const 4 i32.const 68 i32.const 111 i32.const 103 array.new_fixed 0 3 i32.const 0 i32.const 2 struct.new 10)
  (global (;8;) eqref i32.const 3 ref.null eq i32.const 8 i32.const 83 i32.const 111 i32.const 117 i32.const 114 i32.const 99 i32.const 101 i32.const 60 i32.const 68 i32.const 111 i32.const 103 i32.const 62 array.new_fixed 0 11 i32.const 1 i32.const 2 struct.new 10)
  (global (;9;) eqref i32.const 4 ref.null eq i32.const 0 i32.const 73 i32.const 83 i32.const 111 i32.const 117 i32.const 114 i32.const 99 i32.const 101 i32.const 60 i32.const 68 i32.const 111 i32.const 103 i32.const 62 array.new_fixed 0 12 i32.const 0 i32.const 0 struct.new 10)
  (global (;10;) eqref i32.const 5 ref.null eq i32.const 13 i32.const 83 i32.const 111 i32.const 117 i32.const 114 i32.const 99 i32.const 101 i32.const 60 i32.const 65 i32.const 110 i32.const 105 i32.const 109 i32.const 97 i32.const 108 i32.const 62 array.new_fixed 0 14 i32.const 1 i32.const 2 struct.new 10)
  (global (;11;) eqref i32.const 6 ref.null eq i32.const 0 i32.const 73 i32.const 83 i32.const 111 i32.const 117 i32.const 114 i32.const 99 i32.const 101 i32.const 60 i32.const 65 i32.const 110 i32.const 105 i32.const 109 i32.const 97 i32.const 108 i32.const 62 array.new_fixed 0 15 i32.const 0 i32.const 0 struct.new 10)
  (global (;12;) eqref i32.const 7 ref.null eq i32.const 18 i32.const 83 i32.const 111 i32.const 117 i32.const 114 i32.const 99 i32.const 101 i32.const 60 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 i32.const 62 array.new_fixed 0 13 i32.const 1 i32.const 2 struct.new 10)
  (global (;13;) eqref i32.const 8 ref.null eq i32.const 0 i32.const 73 i32.const 83 i32.const 111 i32.const 117 i32.const 114 i32.const 99 i32.const 101 i32.const 60 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 i32.const 62 array.new_fixed 0 14 i32.const 0 i32.const 0 struct.new 10)
  (global (;14;) eqref i32.const 9 ref.null eq i32.const 23 i32.const 83 i32.const 105 i32.const 110 i32.const 107 array.new_fixed 0 4 i32.const 1 i32.const 1 struct.new 10)
  (global (;15;) eqref i32.const 10 ref.null eq i32.const 0 i32.const 73 i32.const 83 i32.const 105 i32.const 110 i32.const 107 i32.const 60 i32.const 65 i32.const 110 i32.const 105 i32.const 109 i32.const 97 i32.const 108 i32.const 62 array.new_fixed 0 13 i32.const 0 i32.const 0 struct.new 10)
  (global (;16;) eqref i32.const 11 ref.null eq i32.const 0 i32.const 73 i32.const 83 i32.const 111 i32.const 117 i32.const 114 i32.const 99 i32.const 101 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 15 i32.const 0 i32.const 0 struct.new 10)
  (global (;17;) eqref i32.const 12 ref.null eq i32.const 0 i32.const 73 i32.const 83 i32.const 105 i32.const 110 i32.const 107 i32.const 60 i32.const 68 i32.const 111 i32.const 103 i32.const 62 array.new_fixed 0 10 i32.const 0 i32.const 0 struct.new 10)
  (export "main" (func 3))
  (elem (;0;) (i32.const 12) func 16)
  (elem (;1;) (i32.const 8) func 13)
  (elem (;2;) (i32.const 10) func 16)
  (elem (;3;) (i32.const 11) func 16)
  (elem (;4;) (i32.const 17) func 17)
  (elem (;5;) (i32.const 13) func 14)
  (elem (;6;) (i32.const 16) func 17)
  (elem (;7;) (i32.const 27) func 15)
  (elem (;8;) (i32.const 24) func 15)
  (func (;3;) (type 19) (result i32)
    (local eqref i32 i64 f64 externref eqref eqref eqref eqref eqref eqref eqref i32 eqref eqref eqref i32 i64 f64 externref)
    struct.new_default 14
    local.set 5
    local.get 5
    ref.cast (ref 14)
    global.get 7
    struct.set 14 0
    local.get 5
    call 4
    local.get 5
    local.set 0
    local.get 0
    i32.const 42
    local.set 16
    local.tee 15
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 15
    ref.cast (ref 14)
    local.set 15
    local.get 15
    ref.cast (ref 14)
    local.get 16
    struct.set 14 1
    local.get 0
    local.set 7
    struct.new_default 15
    local.set 6
    local.get 6
    ref.cast (ref 15)
    global.get 8
    struct.set 15 0
    local.get 6
    local.get 7
    call 5
    local.get 6
    local.tee 14
    local.get 14
    call 6
    local.get 0
    ref.eq
    call 0
    local.tee 14
    local.get 14
    call 7
    struct.new_default 13
    local.set 8
    local.get 8
    ref.cast (ref 13)
    global.get 6
    struct.set 13 0
    local.get 8
    call 8
    local.get 8
    local.set 10
    struct.new_default 16
    local.set 9
    local.get 9
    ref.cast (ref 16)
    global.get 10
    struct.set 16 0
    local.get 9
    local.get 10
    call 9
    local.get 9
    call 7
    i32.const 7
    local.set 12
    struct.new_default 17
    local.set 11
    local.get 11
    ref.cast (ref 17)
    global.get 12
    struct.set 17 0
    local.get 11
    local.get 12
    call 10
    local.get 11
    call 7
    struct.new_default 18
    local.set 13
    local.get 13
    ref.cast (ref 18)
    global.get 14
    struct.set 18 0
    local.get 13
    call 11
    local.get 13
    local.get 0
    call 12
    call 1
    call 6
    local.tee 15
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 15
    ref.cast (ref 13)
    struct.get 13 1
    return
  )
  (func (;4;) (type 20) (param eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 8
    return
  )
  (func (;5;) (type 21) (param eqref eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 2
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 3
    ref.cast (ref 15)
    local.set 3
    local.get 3
    ref.cast (ref 15)
    local.get 2
    struct.set 15 1
    return
  )
  (func (;6;) (type 22) (param eqref) (result eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 3
    ref.cast (ref 11)
    local.get 1
    ref.cast (ref 11)
    struct.get 11 0
    ref.cast (ref 10)
    struct.get 10 2
    i32.const 0
    i32.add
    call_indirect (type 30)
    return
  )
  (func (;7;) (type 23) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    global.get 11
    call 18
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 0
    local.get 0
    global.get 16
    call 18
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 0
    local.get 0
    global.get 9
    call 18
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 0
    return
  )
  (func (;8;) (type 24) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;9;) (type 25) (param eqref eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 2
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 3
    ref.cast (ref 16)
    local.set 3
    local.get 3
    ref.cast (ref 16)
    local.get 2
    struct.set 16 1
    return
  )
  (func (;10;) (type 26) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 4
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 3
    ref.cast (ref 17)
    local.set 3
    local.get 3
    ref.cast (ref 17)
    local.get 4
    struct.set 17 1
    return
  )
  (func (;11;) (type 27) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;12;) (type 28) (param eqref eqref) (result i32)
    (local eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 3
    local.set 2
    local.get 2
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 5
    ref.cast (ref 11)
    local.get 3
    local.get 2
    ref.cast (ref 11)
    struct.get 11 0
    ref.cast (ref 10)
    struct.get 10 2
    i32.const 1
    i32.add
    call_indirect (type 31)
    return
  )
  (func (;13;) (type 29) (param eqref) (result eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 3
    ref.cast (ref 11)
    local.get 1
    ref.cast (ref 11)
    struct.get 11 0
    ref.cast (ref 10)
    struct.get 10 2
    i32.const 2
    i32.add
    call_indirect (type 33)
    return
  )
  (func (;14;) (type 30) (param eqref) (result eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 3
    ref.cast (ref 11)
    local.get 1
    ref.cast (ref 11)
    struct.get 11 0
    ref.cast (ref 10)
    struct.get 10 2
    i32.const 3
    i32.add
    call_indirect (type 35)
    return
  )
  (func (;15;) (type 32) (param eqref eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 1
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 3
    ref.cast (ref 13)
    struct.get 13 1
    return
  )
  (func (;16;) (type 34) (param eqref) (result eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 2
    ref.cast (ref 15)
    struct.get 15 1
    return
  )
  (func (;17;) (type 36) (param eqref) (result eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 3
      struct.new 4
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 1
    return
  )
  (func (;18;) (type 12) (param eqref eqref) (result eqref)
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
    local.get 1
    global.get 9
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 8
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 11
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 8
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 10
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
      local.get 2
      global.get 12
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 15
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 14
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 16
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 8
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 10
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 17
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 14
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
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
)
```
