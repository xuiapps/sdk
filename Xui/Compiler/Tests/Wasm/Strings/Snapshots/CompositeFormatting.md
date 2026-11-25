# Composite formatting

## Source

```csharp
public static class Program {
    public static int Main() {
        System.Console.WriteLine(string.Format("{{key {0:D4}}}: {1,-5} 0x{2:X}", 42, true, 18446744073709551615UL));
        return 42;
    }
}
```

## Output

```text
{key 0042}: True  0xFFFFFFFFFFFFFFFF
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
  (type (;2;) (func (param externref externref) (result (ref extern))))
  (type (;3;) (func (param externref) (result i32)))
  (type (;4;) (func (param i32) (result externref)))
  (type (;5;) (func (param externref i32 externref)))
  (type (;6;) (func (param externref externref) (result externref i32)))
  (type (;7;) (func (param i32 i32) (result externref)))
  (type (;8;) (func (param i32 i64) (result externref)))
  (type (;9;) (func (param i32 f64) (result externref)))
  (type (;10;) (func (param externref) (result externref)))
  (type (;11;) (func (param eqref) (result externref)))
  (type (;12;) (func (param externref)))
  (type (;13;) (struct (field i32) (field externref) (field externref)))
  (type (;14;) (func (param eqref)))
  (type (;15;) (func (result eqref)))
  (type (;16;) (func (result eqref exnref)))
  (type (;17;) (func (result exnref)))
  (type (;18;) (func))
  (type (;19;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;20;) (sub (struct (field (mut eqref)))))
  (type (;21;) (func (param eqref eqref) (result eqref)))
  (type (;22;) (sub final 20 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;23;) (sub final 20 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;24;) (sub final 20 (struct (field (mut eqref)) (field (mut i64)))))
  (type (;25;) (func (result i32)))
  (import "xui" "console-log" (func (;0;) (type 12) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (import "wasm:js-string" "concat" (func (;2;) (type 2) (param externref externref) (result (ref extern))))
  (import "wasm:js-string" "length" (func (;3;) (type 3) (param externref) (result i32)))
  (import "xui-string" "format-text" (func (;4;) (type 10) (param externref) (result externref)))
  (import "xui-string" "format-start" (func (;5;) (type 4) (param i32) (result externref)))
  (import "xui-string" "format-set" (func (;6;) (type 5) (param externref i32 externref)))
  (import "xui-string" "format-finish" (func (;7;) (type 6) (param externref externref) (result externref i32)))
  (import "xui-string" "format-i32" (func (;8;) (type 7) (param i32 i32) (result externref)))
  (import "xui-string" "format-i64" (func (;9;) (type 8) (param i32 i64) (result externref)))
  (import "xui-string" "format-f64" (func (;10;) (type 9) (param i32 f64) (result externref)))
  (tag (;0;) (type 14) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut externref) ref.null extern)
  (global (;8;) (mut externref) ref.null extern)
  (global (;9;) (mut externref) ref.null extern)
  (global (;10;) (mut externref) ref.null extern)
  (global (;11;) (mut externref) ref.null extern)
  (global (;12;) (mut externref) ref.null extern)
  (global (;13;) (mut externref) ref.null extern)
  (global (;14;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 84 i32.const 121 i32.const 112 i32.const 101 array.new_fixed 0 16 i32.const 0 i32.const 0 struct.new 19)
  (global (;15;) eqref i32.const 2 global.get 14 i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 array.new_fixed 0 12 i32.const 0 i32.const 2 struct.new 19)
  (global (;16;) eqref i32.const 3 global.get 14 i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 66 i32.const 111 i32.const 111 i32.const 108 i32.const 101 i32.const 97 i32.const 110 array.new_fixed 0 14 i32.const 0 i32.const 2 struct.new 19)
  (global (;17;) eqref i32.const 4 global.get 14 i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 85 i32.const 73 i32.const 110 i32.const 116 i32.const 54 i32.const 52 array.new_fixed 0 13 i32.const 0 i32.const 2 struct.new 19)
  (export "main" (func 11))
  (func (;11;) (type 25) (result i32)
    (local externref externref externref externref externref eqref eqref i32 i64 f64 externref externref)
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 30
      array.new_data 0 0
      i32.const 0
      i32.const 30
      call 1
      global.set 0
    end
    global.get 0
    i32.const 42
    local.set 7
    global.get 15
    local.get 7
    struct.new 22
    i32.const 1
    i32.const 255
    i32.and
    local.set 7
    global.get 16
    local.get 7
    struct.new 23
    i32.const -1
    i64.extend_i32_s
    local.set 8
    global.get 17
    local.get 8
    struct.new 24
    call 13
    local.set 4
    call 13
    local.set 3
    call 13
    local.set 2
    local.set 0
    local.get 0
    ref.is_null
    if ;; label = @1
      global.get 1
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 6
        array.new_data 0 1
        i32.const 0
        i32.const 6
        call 1
        global.set 1
      end
      global.get 1
      local.set 11
      global.get 9
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 21
        array.new_data 0 9
        i32.const 0
        i32.const 21
        call 1
        global.set 9
      end
      global.get 9
      local.set 10
      local.get 10
      ref.is_null
      if ;; label = @2
        global.get 9
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 21
          array.new_data 0 9
          i32.const 0
          i32.const 21
          call 1
          global.set 9
        end
        global.get 9
        local.set 10
      end
      local.get 11
      ref.is_null
      i32.eqz
      if ;; label = @2
        local.get 11
        call 3
        if ;; label = @3
          local.get 10
          global.get 12
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 13
            array.new_data 0 12
            i32.const 0
            i32.const 13
            call 1
            global.set 12
          end
          global.get 12
          call 2
          local.get 11
          call 2
          global.get 13
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 2
            array.new_data 0 13
            i32.const 0
            i32.const 2
            call 1
            global.set 13
          end
          global.get 13
          call 2
          local.set 10
        end
      end
      i32.const 9
      local.get 10
      local.get 11
      struct.new 13
      throw 0
    end
    i32.const 3
    call 5
    local.set 1
    local.get 1
    i32.const 0
    local.get 2
    call 6
    local.get 1
    i32.const 1
    local.get 3
    call 6
    local.get 1
    i32.const 2
    local.get 4
    call 6
    local.get 0
    local.get 1
    call 7
    local.set 7
    local.set 10
    local.get 7
    i32.const 1
    i32.eq
    if ;; label = @1
      i32.const 13
      global.get 10
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 41
        array.new_data 0 10
        i32.const 0
        i32.const 41
        call 1
        global.set 10
      end
      global.get 10
      ref.null extern
      struct.new 13
      throw 0
    end
    local.get 7
    i32.const 2
    i32.eq
    if ;; label = @1
      i32.const 14
      global.get 11
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 34
        array.new_data 0 11
        i32.const 0
        i32.const 34
        call 1
        global.set 11
      end
      global.get 11
      ref.null extern
      struct.new 13
      throw 0
    end
    local.get 10
    call 0
    i32.const 42
    return
  )
  (func (;12;) (type 21) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 20)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 20)
    struct.get 20 0
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
      ref.cast (ref 19)
      struct.get 19 1
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
  (func (;13;) (type 11) (param eqref) (result externref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null extern
      return
    end
    local.get 0
    ref.test (ref 22)
    if ;; label = @1
      local.get 0
      ref.cast (ref 20)
      struct.get 20 0
      global.get 15
      ref.eq
      if ;; label = @2
        i32.const 3
        local.get 0
        ref.cast (ref 22)
        struct.get 22 1
        call 8
        return
      end
    end
    local.get 0
    ref.test (ref 23)
    if ;; label = @1
      local.get 0
      ref.cast (ref 20)
      struct.get 20 0
      global.get 16
      ref.eq
      if ;; label = @2
        i32.const 1
        local.get 0
        ref.cast (ref 23)
        struct.get 23 1
        call 8
        return
      end
    end
    local.get 0
    ref.test (ref 24)
    if ;; label = @1
      local.get 0
      ref.cast (ref 20)
      struct.get 20 0
      global.get 17
      ref.eq
      if ;; label = @2
        i32.const 6
        local.get 0
        ref.cast (ref 24)
        struct.get 24 1
        call 9
        return
      end
    end
    i32.const 0
    i32.const 0
    call 8
  )
  (data (;0;) "{\00{\00k\00e\00y\00 \00{\000\00:\00D\004\00}\00}\00}\00:\00 \00{\001\00,\00-\005\00}\00 \000\00x\00{\002\00:\00X\00}\00")
  (data (;1;) "f\00o\00r\00m\00a\00t\00")
  (data (;2;) "a\00r\00g\00s\00")
  (data (;3;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;4;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;5;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;6;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;7;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;8;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
  (data (;9;) "V\00a\00l\00u\00e\00 \00c\00a\00n\00n\00o\00t\00 \00b\00e\00 \00n\00u\00l\00l\00.\00")
  (data (;10;) "I\00n\00p\00u\00t\00 \00s\00t\00r\00i\00n\00g\00 \00w\00a\00s\00 \00n\00o\00t\00 \00i\00n\00 \00a\00 \00c\00o\00r\00r\00e\00c\00t\00 \00f\00o\00r\00m\00a\00t\00.\00")
  (data (;11;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00m\00e\00t\00h\00o\00d\00 \00i\00s\00 \00n\00o\00t\00 \00s\00u\00p\00p\00o\00r\00t\00e\00d\00.\00")
  (data (;12;) " \00(\00P\00a\00r\00a\00m\00e\00t\00e\00r\00 \00'\00")
  (data (;13;) "'\00)\00")
)
```
