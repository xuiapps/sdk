# UTF-16 interpolation

## Source

```csharp
using System;
public static class Program {
    public static int Main() {
        for (int rating = 1; rating <= 5; rating++)
            Console.WriteLine($"Rating{rating}: {rating} of 5 stars");
        return 42;
    }
}
```

## Output

```text
Rating1: 1 of 5 stars
Rating2: 2 of 5 stars
Rating3: 3 of 5 stars
Rating4: 4 of 5 stars
Rating5: 5 of 5 stars
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
  (type (;2;) (func (param externref) (result i32)))
  (type (;3;) (func (param i32 i32)))
  (type (;4;) (func (param i32 externref)))
  (type (;5;) (func (param i32 i64 i32)))
  (type (;6;) (func (param i32 i32) (result externref)))
  (type (;7;) (func (param externref i32) (result i32)))
  (type (;8;) (func (param externref)))
  (type (;9;) (struct (field i32) (field externref)))
  (type (;10;) (func (param eqref)))
  (type (;11;) (func (result eqref)))
  (type (;12;) (func (result eqref exnref)))
  (type (;13;) (func (result exnref)))
  (type (;14;) (func))
  (type (;15;) (struct (field (mut eqref)) (field (mut i32))))
  (type (;16;) (func (result i32)))
  (type (;17;) (func (param eqref i32) (result eqref)))
  (type (;18;) (func (param eqref i32 eqref)))
  (type (;19;) (func (param eqref i32) (result externref)))
  (type (;20;) (func (param eqref i32 externref)))
  (type (;21;) (func (param eqref i32) (result i32)))
  (type (;22;) (func (param eqref i32 i32)))
  (type (;23;) (func (result i32)))
  (type (;24;) (func))
  (type (;25;) (func))
  (import "xui" "console-log" (func (;0;) (type 8) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (import "wasm:js-string" "length" (func (;2;) (type 2) (param externref) (result i32)))
  (import "wasm:js-string" "charCodeAt" (func (;3;) (type 7) (param externref i32) (result i32)))
  (table (;0;) 65536 65536 eqref)
  (table (;1;) 65536 65536 externref)
  (memory (;0;) 16 32768)
  (tag (;0;) (type 10) (param eqref))
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
  (global (;11;) (mut i32) i32.const 16)
  (global (;12;) (mut i32) i32.const 0)
  (export "main" (func 4))
  (func (;4;) (type 23) (result i32)
    (local i32 i32 eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 11
    local.tee 11
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 16
    i32.add
    global.set 11
    local.get 11
    i32.const 0
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 8
    memory.fill
    global.get 12
    local.tee 8
    i32.const 65535
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 1
    i32.add
    global.set 12
    local.get 11
    i32.const 0
    i32.add
    local.get 8
    i32.const 0
    i32.add
    i32.store align=1
    block (type 13) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 1
        local.set 0
        block (type 24) ;; label = @3
          loop (type 24) ;; label = @4
            local.get 0
            i32.const 5
            i32.gt_s
            i32.eqz
            if (type 25) ;; label = @5
              local.get 1
              local.set 3
              ref.null eq
              local.get 3
              i32.const 19
              i32.const 2
              drop
              drop
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 8
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 8
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 8
                end
                global.get 8
                struct.new 9
                throw 0
              end
              local.get 2
              ref.is_null
              i32.eqz
              if ;; label = @6
                unreachable
              end
              local.get 3
              local.set 12
              local.get 12
              i32.load align=1
              ref.null eq
              table.set 0
              local.get 12
              i32.const 4
              i32.add
              i32.const 0
              i32.store align=1
              local.get 1
              local.set 3
              ref.null eq
              local.get 3
              global.get 2
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 6
                array.new_data 0 2
                i32.const 0
                i32.const 6
                call 1
                global.set 2
              end
              global.get 2
              local.set 21
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 8
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 8
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 8
                end
                global.get 8
                struct.new 9
                throw 0
              end
              local.get 2
              ref.is_null
              i32.eqz
              if ;; label = @6
                unreachable
              end
              local.get 3
              local.set 12
              local.get 12
              local.get 21
              call 12
              local.get 1
              local.set 3
              ref.null eq
              local.get 3
              local.get 0
              local.set 18
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 8
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 8
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 8
                end
                global.get 8
                struct.new 9
                throw 0
              end
              local.get 2
              ref.is_null
              i32.eqz
              if ;; label = @6
                unreachable
              end
              local.get 3
              local.set 12
              local.get 12
              local.get 18
              i64.extend_i32_s
              i32.const 1
              call 14
              local.get 1
              local.set 3
              ref.null eq
              local.get 3
              global.get 3
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 2
                array.new_data 0 3
                i32.const 0
                i32.const 2
                call 1
                global.set 3
              end
              global.get 3
              local.set 21
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 8
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 8
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 8
                end
                global.get 8
                struct.new 9
                throw 0
              end
              local.get 2
              ref.is_null
              i32.eqz
              if ;; label = @6
                unreachable
              end
              local.get 3
              local.set 12
              local.get 12
              local.get 21
              call 12
              local.get 1
              local.set 3
              ref.null eq
              local.get 3
              local.get 0
              local.set 18
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 8
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 8
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 8
                end
                global.get 8
                struct.new 9
                throw 0
              end
              local.get 2
              ref.is_null
              i32.eqz
              if ;; label = @6
                unreachable
              end
              local.get 3
              local.set 12
              local.get 12
              local.get 18
              i64.extend_i32_s
              i32.const 1
              call 14
              local.get 1
              local.set 3
              ref.null eq
              local.get 3
              global.get 4
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 11
                array.new_data 0 4
                i32.const 0
                i32.const 11
                call 1
                global.set 4
              end
              global.get 4
              local.set 21
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 8
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 8
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 8
                end
                global.get 8
                struct.new 9
                throw 0
              end
              local.get 2
              ref.is_null
              i32.eqz
              if ;; label = @6
                unreachable
              end
              local.get 3
              local.set 12
              local.get 12
              local.get 21
              call 12
              local.get 1
              local.set 3
              ref.null eq
              local.get 3
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 8
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 8
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 8
                end
                global.get 8
                struct.new 9
                throw 0
              end
              local.get 2
              ref.is_null
              i32.eqz
              if ;; label = @6
                unreachable
              end
              local.get 3
              local.set 12
              local.get 12
              i32.const 1
              call 15
              call 0
              local.get 0
              i32.const 1
              i32.add
              local.set 0
              br 1 (;@4;)
            else
              br 2 (;@3;)
            end
            unreachable
          end
        end
        i32.const 42
        local.get 8
        ref.null eq
        i32.const 1
        table.fill 0
        local.get 8
        ref.null extern
        i32.const 1
        table.fill 1
        local.get 8
        global.set 12
        local.get 11
        global.set 11
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 8
    ref.null eq
    i32.const 1
    table.fill 0
    local.get 8
    ref.null extern
    i32.const 1
    table.fill 1
    local.get 8
    global.set 12
    local.get 11
    global.set 11
    local.get 6
    throw_ref
  )
  (func (;5;) (type 17) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 8
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 8
        i32.const 0
        i32.const 53
        call 1
        global.set 8
      end
      i32.const 4
      global.get 8
      struct.new 9
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      table.get 0
      return
    end
    unreachable
  )
  (func (;6;) (type 18) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 8
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 8
        i32.const 0
        i32.const 53
        call 1
        global.set 8
      end
      i32.const 4
      global.get 8
      struct.new 9
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      table.set 0
      return
    end
    unreachable
  )
  (func (;7;) (type 19) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 8
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 8
        i32.const 0
        i32.const 53
        call 1
        global.set 8
      end
      i32.const 4
      global.get 8
      struct.new 9
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      table.get 1
      return
    end
    unreachable
  )
  (func (;8;) (type 20) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 8
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 8
        i32.const 0
        i32.const 53
        call 1
        global.set 8
      end
      i32.const 4
      global.get 8
      struct.new 9
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      table.set 1
      return
    end
    unreachable
  )
  (func (;9;) (type 21) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 8
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 8
        i32.const 0
        i32.const 53
        call 1
        global.set 8
      end
      i32.const 4
      global.get 8
      struct.new 9
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load align=1
      return
    end
    unreachable
  )
  (func (;10;) (type 22) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 8
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 8
        i32.const 0
        i32.const 53
        call 1
        global.set 8
      end
      i32.const 4
      global.get 8
      struct.new 9
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
    unreachable
  )
  (func (;11;) (type 3) (param i32 i32)
    (local eqref eqref i32 i32 i32)
    local.get 0
    i32.const 4
    i32.add
    i32.load align=1
    local.set 6
    local.get 1
    i32.const 2147483647
    local.get 6
    i32.sub
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    local.get 1
    i32.add
    local.set 4
    local.get 0
    i32.load align=1
    table.get 0
    local.tee 2
    ref.is_null
    i32.eqz
    if ;; label = @1
      local.get 2
      ref.cast (ref 0)
      array.len
      local.set 5
      local.get 4
      local.get 5
      i32.gt_u
      i32.eqz
      if ;; label = @2
        return
      end
    end
    local.get 5
    i32.const 1073741823
    i32.gt_u
    if ;; label = @1
      local.get 4
      local.set 5
    else
      local.get 5
      i32.const 2
      i32.mul
      local.set 5
    end
    local.get 5
    i32.const 16
    i32.lt_u
    if ;; label = @1
      i32.const 16
      local.set 5
    end
    local.get 5
    local.get 4
    i32.lt_u
    if ;; label = @1
      local.get 4
      local.set 5
    end
    local.get 5
    array.new_default 0
    local.set 3
    local.get 6
    if ;; label = @1
      local.get 3
      ref.cast (ref 0)
      i32.const 0
      local.get 2
      ref.cast (ref 0)
      i32.const 0
      local.get 6
      array.copy 0 0
    end
    local.get 0
    i32.load align=1
    local.get 3
    table.set 0
  )
  (func (;12;) (type 4) (param i32 externref)
    (local eqref i32 i32 i32)
    local.get 1
    ref.is_null
    if ;; label = @1
      return
    end
    local.get 1
    call 2
    local.tee 3
    i32.eqz
    if ;; label = @1
      return
    end
    local.get 0
    local.get 3
    call 11
    local.get 0
    i32.load align=1
    table.get 0
    ref.cast (ref 0)
    local.set 2
    local.get 0
    i32.const 4
    i32.add
    i32.load align=1
    local.set 4
    loop ;; label = @1
      local.get 2
      ref.cast (ref 0)
      local.get 4
      local.get 5
      i32.add
      local.get 1
      local.get 5
      call 3
      array.set 0
      local.get 5
      i32.const 1
      i32.add
      local.tee 5
      local.get 3
      i32.lt_u
      br_if 0 (;@1;)
    end
    local.get 0
    i32.const 4
    i32.add
    local.get 4
    local.get 3
    i32.add
    i32.store align=1
  )
  (func (;13;) (type 3) (param i32 i32)
    local.get 0
    i32.const 1
    call 11
    local.get 0
    i32.load align=1
    table.get 0
    ref.cast (ref 0)
    local.get 0
    i32.const 4
    i32.add
    i32.load align=1
    local.get 1
    array.set 0
    local.get 0
    i32.const 4
    i32.add
    local.get 0
    i32.const 4
    i32.add
    i32.load align=1
    i32.const 1
    i32.add
    i32.store align=1
  )
  (func (;14;) (type 5) (param i32 i64 i32)
    (local eqref i64 i32 i32 i32)
    local.get 0
    i32.const 21
    call 11
    local.get 0
    i32.load align=1
    table.get 0
    ref.cast (ref 0)
    local.set 3
    local.get 0
    i32.const 4
    i32.add
    i32.load align=1
    local.set 5
    local.get 2
    local.get 1
    i64.const 0
    i64.lt_s
    i32.and
    if ;; label = @1
      local.get 3
      ref.cast (ref 0)
      local.get 5
      i32.const 45
      array.set 0
      local.get 5
      i32.const 1
      i32.add
      local.set 5
      i64.const 0
      local.get 1
      i64.sub
      local.set 1
    end
    local.get 1
    local.set 4
    loop ;; label = @1
      local.get 6
      i32.const 1
      i32.add
      local.set 6
      local.get 4
      i64.const 10
      i64.div_u
      local.tee 4
      i64.eqz
      i32.eqz
      br_if 0 (;@1;)
    end
    local.get 5
    local.get 6
    i32.add
    local.set 7
    local.get 0
    i32.const 4
    i32.add
    local.get 7
    i32.store align=1
    loop ;; label = @1
      local.get 7
      i32.const 1
      i32.sub
      local.set 7
      local.get 3
      ref.cast (ref 0)
      local.get 7
      local.get 1
      i64.const 10
      i64.rem_u
      i32.wrap_i64
      i32.const 48
      i32.add
      array.set 0
      local.get 1
      i64.const 10
      i64.div_u
      local.set 1
      local.get 7
      local.get 5
      i32.gt_u
      br_if 0 (;@1;)
    end
  )
  (func (;15;) (type 6) (param i32 i32) (result externref)
    (local externref)
    local.get 0
    i32.const 0
    call 11
    local.get 0
    i32.load align=1
    table.get 0
    ref.cast (ref 0)
    i32.const 0
    local.get 0
    i32.const 4
    i32.add
    i32.load align=1
    call 1
    local.set 2
    local.get 1
    if ;; label = @1
      local.get 0
      i32.load align=1
      ref.null eq
      table.set 0
      local.get 0
      i32.const 4
      i32.add
      i32.const 0
      i32.store align=1
    end
    local.get 2
  )
  (data (;0;) "T\00r\00u\00e\00")
  (data (;1;) "F\00a\00l\00s\00e\00")
  (data (;2;) "R\00a\00t\00i\00n\00g\00")
  (data (;3;) ":\00 \00")
  (data (;4;) " \00o\00f\00 \005\00 \00s\00t\00a\00r\00s\00")
  (data (;5;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;6;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;7;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;8;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;9;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;10;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
