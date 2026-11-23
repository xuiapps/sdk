# Locals WASM

## Source

```csharp
bool boolean = true;
char character = 'A';
sbyte signedByte = -12;
byte unsignedByte = 250;
short signedShort = -1234;
ushort unsignedShort = 60000;
int signedInt = -42;
uint unsignedInt = 4000000000;
long signedLong = -5000000000;
ulong unsignedLong = 10000000000;
float single = 1.25f;
double number = 2.5;
nint signedNative = -7;
nuint unsignedNative = 8;
string text = "Local strings";

System.Console.WriteLine(boolean);
System.Console.WriteLine((int)character);
System.Console.WriteLine((int)signedByte);
System.Console.WriteLine((int)unsignedByte);
System.Console.WriteLine((int)signedShort);
System.Console.WriteLine((int)unsignedShort);
System.Console.WriteLine(signedInt);
System.Console.WriteLine((long)unsignedInt);
System.Console.WriteLine(signedLong);
System.Console.WriteLine((long)unsignedLong);
System.Console.WriteLine(single);
System.Console.WriteLine(number);
System.Console.WriteLine((int)signedNative);
System.Console.WriteLine((int)unsignedNative);
System.Console.WriteLine(text);
```

## Output

```text
True
65
-12
250
-1234
60000
-42
4000000000
-5000000000
10000000000
1.25
2.5
-7
8
Local strings
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
  (type (;3;) (func (param i32)))
  (type (;4;) (func (param i64)))
  (type (;5;) (func (param f32)))
  (type (;6;) (func (param f64)))
  (type (;7;) (func (param externref)))
  (type (;8;) (func))
  (import "xui" "console-log-boolean" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param i32)))
  (import "xui" "console-log" (func (;2;) (type 4) (param i64)))
  (import "xui" "console-log" (func (;3;) (type 5) (param f32)))
  (import "xui" "console-log" (func (;4;) (type 6) (param f64)))
  (import "xui" "console-log" (func (;5;) (type 7) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;6;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (global (;0;) (mut externref) ref.null extern)
  (export "main" (func 7))
  (func (;7;) (type 8)
    (local i32 i32 i32 i32 i32 i32 i32 i32 i64 i64 f32 f64 i32 i32 externref)
    i32.const 1
    i32.const 255
    i32.and
    local.set 0
    i32.const 65
    i32.const 65535
    i32.and
    local.set 1
    i32.const -12
    i32.extend8_s
    local.set 2
    i32.const 250
    i32.const 255
    i32.and
    local.set 3
    i32.const -1234
    i32.extend16_s
    local.set 4
    i32.const 60000
    i32.const 65535
    i32.and
    local.set 5
    i32.const -42
    local.set 6
    i32.const -294967296
    local.set 7
    i64.const -5000000000
    local.set 8
    i64.const 10000000000
    local.set 9
    f32.const 0x1.4p+0 (;=1.25;)
    f64.promote_f32
    f32.demote_f64
    local.set 10
    f64.const 0x1.4p+1 (;=2.5;)
    local.set 11
    i32.const -7
    local.set 12
    i32.const 8
    local.set 13
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 13
      array.new_data 0 0
      i32.const 0
      i32.const 13
      call 6
      global.set 0
    end
    global.get 0
    local.set 14
    local.get 0
    call 0
    local.get 1
    call 1
    local.get 2
    call 1
    local.get 3
    call 1
    local.get 4
    call 1
    local.get 5
    call 1
    local.get 6
    call 1
    local.get 7
    i64.extend_i32_u
    call 2
    local.get 8
    call 2
    local.get 9
    call 2
    local.get 10
    f64.promote_f32
    f32.demote_f64
    call 3
    local.get 11
    call 4
    local.get 12
    call 1
    local.get 13
    call 1
    local.get 14
    call 5
    return
  )
  (data (;0;) "L\00o\00c\00a\00l\00 \00s\00t\00r\00i\00n\00g\00s\00")
)
```
