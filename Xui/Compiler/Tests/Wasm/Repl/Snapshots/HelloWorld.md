# Hello World WASM

## Source

```csharp
System.Console.WriteLine("Hello World!");
```

## Output

```text
Hello World!
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
  (type (;2;) (func (param externref)))
  (type (;3;) (func))
  (import "xui" "console-log" (func (;0;) (type 2) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (global (;0;) (mut externref) ref.null extern)
  (export "main" (func 2))
  (func (;2;) (type 3)
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 12
      array.new_data 0 0
      i32.const 0
      i32.const 12
      call 1
      global.set 0
    end
    global.get 0
    call 0
    return
  )
  (data (;0;) "H\00e\00l\00l\00o\00 \00W\00o\00r\00l\00d\00!\00")
)
```
