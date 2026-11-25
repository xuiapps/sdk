# Three assemblies, one WASM module

`Application.exe` calls `Reports.dll` and `Numbers.dll`; `Reports.dll` also calls `Numbers.dll`.
Each DLL is compiled separately. Only `Application.Main` is a root.

## Source

### Numbers.cs

Source: [Numbers.cs](Fixtures/MultiAssembly/Numbers.cs)

```csharp
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]

namespace Numbers;

public static class Arithmetic
{
    public static int Add(int left, int right) => left + right;

    // This overload and the unused class must not enter the WASM module.
    public static long Add(long left, long right) => left + right;
}

public static class Unused
{
    public static int Fail() => throw new System.NotSupportedException();
}
```

### Reports.cs

Source: [Reports.cs](Fixtures/MultiAssembly/Reports.cs)

```csharp
namespace Reports;

public static class Report
{
    public static int Run(int value)
    {
        if (value < 0) return 0;
        System.Console.WriteLine("From Reports.dll");
        int result = Numbers.Arithmetic.Add(value, 2);
        System.Console.WriteLine(result);
        return Numbers.Arithmetic.Add(result, 1);
    }
}
```

### Program.cs

Source: [Program.cs](Fixtures/MultiAssembly/Program.cs)

```csharp
internal static class Program
{
    public static int Main()
    {
        System.Console.WriteLine("From Application.exe");
        int input = Numbers.Arithmetic.Add(38, 1);
        int result = Reports.Report.Run(input);
        System.Console.WriteLine(result);
        return result;
    }
}
```

## Retained methods

- `Application.exe`: `Program.Main`
- `Numbers.dll`: `Numbers.Arithmetic.Add`
- `Reports.dll`: `Reports.Report.Run`

The unused class and the `long` overload are omitted. Console calls in both assemblies use the test host rewrite.

## Output

```text
From Application.exe
From Reports.dll
41
42
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
  (type (;4;) (func (result i32)))
  (type (;5;) (func (param i32 i32) (result i32)))
  (type (;6;) (func (param i32) (result i32)))
  (type (;7;) (func))
  (import "xui" "console-log" (func (;0;) (type 2) (param externref)))
  (import "xui" "console-log" (func (;1;) (type 3) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (export "main" (func 3))
  (func (;3;) (type 4) (result i32)
    (local i32 i64 f64 externref)
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 20
      array.new_data 0 0
      i32.const 0
      i32.const 20
      call 2
      global.set 0
    end
    global.get 0
    call 0
    i32.const 38
    i32.const 1
    call 4
    call 5
    local.tee 0
    local.get 0
    call 1
    return
  )
  (func (;4;) (type 5) (param i32 i32) (result i32)
    local.get 0
    local.get 1
    i32.add
    return
  )
  (func (;5;) (type 6) (param i32) (result i32)
    (local i32 i64 f64 externref)
    local.get 0
    i32.const 0
    i32.lt_s
    i32.eqz
    if (type 7) ;; label = @1
      global.get 1
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 16
        array.new_data 0 1
        i32.const 0
        i32.const 16
        call 2
        global.set 1
      end
      global.get 1
      call 0
      local.get 0
      i32.const 2
      call 4
      local.tee 1
      local.get 1
      call 1
      i32.const 1
      call 4
      return
    else
      i32.const 0
      return
    end
    unreachable
  )
  (data (;0;) "F\00r\00o\00m\00 \00A\00p\00p\00l\00i\00c\00a\00t\00i\00o\00n\00.\00e\00x\00e\00")
  (data (;1;) "F\00r\00o\00m\00 \00R\00e\00p\00o\00r\00t\00s\00.\00d\00l\00l\00")
)
```
