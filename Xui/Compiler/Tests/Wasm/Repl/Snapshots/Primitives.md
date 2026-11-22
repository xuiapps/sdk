# Primitives WASM

## Source

```csharp
System.Console.WriteLine(1);
System.Console.WriteLine(1 + 1);
System.Console.WriteLine(-42);
System.Console.WriteLine(123456);
System.Console.WriteLine(1.25f);
System.Console.WriteLine(2.5);
System.Console.WriteLine(true);
System.Console.WriteLine(false);
```

## Output

```text
1
2
-42
123456
1.25
2.5
True
False
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
  (type (;2;) (func (param i32)))
  (type (;3;) (func (param f32)))
  (type (;4;) (func (param f64)))
  (type (;5;) (func (param i32)))
  (type (;6;) (func))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param f32)))
  (import "xui" "console-log" (func (;2;) (type 4) (param f64)))
  (import "xui" "console-log-boolean" (func (;3;) (type 5) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;4;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (export "main" (func 5))
  (func (;5;) (type 6)
    i32.const 1
    call 0
    i32.const 2
    call 0
    i32.const -42
    call 0
    i32.const 123456
    call 0
    f32.const 0x1.4p+0 (;=1.25;)
    f64.promote_f32
    f32.demote_f64
    call 1
    f64.const 0x1.4p+1 (;=2.5;)
    call 2
    i32.const 1
    call 3
    i32.const 0
    call 3
    return
  )
)
```
