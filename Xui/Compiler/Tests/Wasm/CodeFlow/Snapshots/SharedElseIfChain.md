# SharedElseIfChain WASM

Source: [SharedElseIfChain.cs](Fixtures/CodeFlow/SharedElseIfChain.cs)

## Source

```csharp
namespace Samples.CodeFlow;

public static class SharedElseIfChain
{
    private static int checks;
    private static bool Check(int value) { checks++; return (value & 1) != 0; }

    public static int Run(int value)
    {
        checks = 0;
        int result;
        if (value > 10 && Check(value)) result = 1;
        else if (value > 8 && Check(value + 1)) result = 2;
        else if (value > 6 && Check(value)) result = 3;
        else if (value > 4 && Check(value + 1)) result = 4;
        else if (value > 2 && Check(value)) result = 5;
        else if (value > 0 && Check(value + 1)) result = 6;
        else result = value < 0 ? 7 : 8;
        int other = 12;
        ref int selected = ref (value < 0 ? ref result : ref other);
        selected += 1;
        int extra = value switch { 0 => 10, 1 => 20, 2 => 30, 3 => 40, 4 => 50, _ => 60 };
        return checks * 100 + (result > 4 ? result : result + 10) + other + extra;
    }
}

public static class EntryPoint
{
    public static int Main()
    {
        System.Console.WriteLine(SharedElseIfChain.Run(-3));
        System.Console.WriteLine(SharedElseIfChain.Run(0));
        System.Console.WriteLine(SharedElseIfChain.Run(1));
        System.Console.WriteLine(SharedElseIfChain.Run(2));
        System.Console.WriteLine(SharedElseIfChain.Run(3));
        System.Console.WriteLine(SharedElseIfChain.Run(4));
        System.Console.WriteLine(SharedElseIfChain.Run(5));
        System.Console.WriteLine(SharedElseIfChain.Run(6));
        System.Console.WriteLine(SharedElseIfChain.Run(7));
        System.Console.WriteLine(SharedElseIfChain.Run(8));
        System.Console.WriteLine(SharedElseIfChain.Run(9));
        System.Console.WriteLine(SharedElseIfChain.Run(10));
        System.Console.WriteLine(SharedElseIfChain.Run(11));
        System.Console.WriteLine(SharedElseIfChain.Run(12));
        return 0;
    }
}
```

## Output

```text
80
31
141
149
158
269
278
187
186
287
286
185
184
285
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
  (type (;3;) (struct (field i32) (field externref)))
  (type (;4;) (func (param eqref)))
  (type (;5;) (func (result eqref)))
  (type (;6;) (func (result eqref exnref)))
  (type (;7;) (func (result exnref)))
  (type (;8;) (func))
  (type (;9;) (struct (field (mut i32))))
  (type (;10;) (func (result i32)))
  (type (;11;) (func (param eqref i32) (result eqref)))
  (type (;12;) (func (param eqref i32 eqref)))
  (type (;13;) (func (param eqref i32) (result externref)))
  (type (;14;) (func (param eqref i32 externref)))
  (type (;15;) (func (param eqref i32) (result i32)))
  (type (;16;) (func (param eqref i32 i32)))
  (type (;17;) (func (result i32)))
  (type (;18;) (func (param i32) (result i32)))
  (type (;19;) (func))
  (type (;20;) (func))
  (type (;21;) (func))
  (type (;22;) (func (result i32)))
  (type (;23;) (func (result i32)))
  (type (;24;) (func))
  (type (;25;) (func (result i32 i32)))
  (type (;26;) (func))
  (type (;27;) (func))
  (type (;28;) (func))
  (type (;29;) (func (result i32)))
  (type (;30;) (func (result i32)))
  (type (;31;) (func))
  (type (;32;) (func (result i32 i32)))
  (type (;33;) (func))
  (type (;34;) (func))
  (type (;35;) (func))
  (type (;36;) (func (result i32)))
  (type (;37;) (func (result i32)))
  (type (;38;) (func))
  (type (;39;) (func (result i32 i32)))
  (type (;40;) (func))
  (type (;41;) (func))
  (type (;42;) (func))
  (type (;43;) (func (result i32)))
  (type (;44;) (func (result i32)))
  (type (;45;) (func))
  (type (;46;) (func (result i32 i32)))
  (type (;47;) (func))
  (type (;48;) (func))
  (type (;49;) (func))
  (type (;50;) (func (result i32)))
  (type (;51;) (func (result i32)))
  (type (;52;) (func))
  (type (;53;) (func (result i32 i32)))
  (type (;54;) (func))
  (type (;55;) (func))
  (type (;56;) (func))
  (type (;57;) (func (result i32)))
  (type (;58;) (func (result i32)))
  (type (;59;) (func))
  (type (;60;) (func (result i32 i32)))
  (type (;61;) (func))
  (type (;62;) (func))
  (type (;63;) (func (result i32)))
  (type (;64;) (func (result i32)))
  (type (;65;) (func))
  (type (;66;) (func))
  (type (;67;) (func))
  (type (;68;) (func (result eqref i32)))
  (type (;69;) (func (result eqref i32)))
  (type (;70;) (func))
  (type (;71;) (func))
  (type (;72;) (func))
  (type (;73;) (func))
  (type (;74;) (func))
  (type (;75;) (func))
  (type (;76;) (func))
  (type (;77;) (func (result i32)))
  (type (;78;) (func (result i32)))
  (type (;79;) (func (result i32)))
  (type (;80;) (func (result i32)))
  (type (;81;) (func (result i32)))
  (type (;82;) (func (result i32)))
  (type (;83;) (func))
  (type (;84;) (func))
  (type (;85;) (func))
  (type (;86;) (func))
  (type (;87;) (func))
  (type (;88;) (func))
  (type (;89;) (func (result i32)))
  (type (;90;) (func (result i32)))
  (type (;91;) (func (result i32 i32)))
  (type (;92;) (func (result i32 i32)))
  (type (;93;) (func (param i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 32768)
  (tag (;0;) (type 4) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut i32) i32.const 16)
  (global (;7;) eqref struct.new_default 9)
  (export "main" (func 2))
  (func (;2;) (type 17) (result i32)
    i32.const -3
    call 3
    call 0
    i32.const 0
    call 3
    call 0
    i32.const 1
    call 3
    call 0
    i32.const 2
    call 3
    call 0
    i32.const 3
    call 3
    call 0
    i32.const 4
    call 3
    call 0
    i32.const 5
    call 3
    call 0
    i32.const 6
    call 3
    call 0
    i32.const 7
    call 3
    call 0
    i32.const 8
    call 3
    call 0
    i32.const 9
    call 3
    call 0
    i32.const 10
    call 3
    call 0
    i32.const 11
    call 3
    call 0
    i32.const 12
    call 3
    call 0
    i32.const 0
    return
  )
  (func (;3;) (type 18) (param i32) (result i32)
    (local i32 i32 i32 i32 eqref i32 eqref i32 i32 i32 i64 f64 externref exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 15
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 16
    i32.add
    global.set 6
    local.get 15
    i32.const 0
    i32.add
    i32.const 0
    i32.const 4
    memory.fill
    local.get 15
    i32.const 4
    i32.add
    i32.const 0
    i32.const 4
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        block (type 92) (result i32 i32) ;; label = @3
          block (type 91) (result i32 i32) ;; label = @4
            block (type 90) (result i32) ;; label = @5
              block (type 89) (result i32) ;; label = @6
                block (type 88) ;; label = @7
                  block (type 87) ;; label = @8
                    block (type 86) ;; label = @9
                      block (type 85) ;; label = @10
                        block (type 84) ;; label = @11
                          block (type 83) ;; label = @12
                            block (type 82) (result i32) ;; label = @13
                              block (type 81) (result i32) ;; label = @14
                                block (type 80) (result i32) ;; label = @15
                                  block (type 79) (result i32) ;; label = @16
                                    block (type 78) (result i32) ;; label = @17
                                      block (type 77) (result i32) ;; label = @18
                                        block (type 76) ;; label = @19
                                          block (type 75) ;; label = @20
                                            block (type 74) ;; label = @21
                                              block (type 73) ;; label = @22
                                                block (type 72) ;; label = @23
                                                  block (type 71) ;; label = @24
                                                    block (type 70) ;; label = @25
                                                      block (type 69) (result eqref i32) ;; label = @26
                                                        block (type 68) (result eqref i32) ;; label = @27
                                                          block (type 67) ;; label = @28
                                                            block (type 66) ;; label = @29
                                                              block (type 65) ;; label = @30
                                                                block (type 64) (result i32) ;; label = @31
                                                                  block (type 63) (result i32) ;; label = @32
                                                                    block (type 62) ;; label = @33
                                                                      block (type 61) ;; label = @34
                                                                        block (type 60) (result i32 i32) ;; label = @35
                                                                          block (type 59) ;; label = @36
                                                                            block (type 58) (result i32) ;; label = @37
                                                                              block (type 57) (result i32) ;; label = @38
                                                                                block (type 56) ;; label = @39
                                                                                  block (type 55) ;; label = @40
                                                                                    block (type 54) ;; label = @41
                                                                                      block (type 53) (result i32 i32) ;; label = @42
                                                                                        block (type 52) ;; label = @43
                                                                                          block (type 51) (result i32) ;; label = @44
                                                                                            block (type 50) (result i32) ;; label = @45
                                                                                              block (type 49) ;; label = @46
                                                                                                block (type 48) ;; label = @47
                                                                                                  block (type 47) ;; label = @48
                                                                                                    block (type 46) (result i32 i32) ;; label = @49
                                                                                                    block (type 45) ;; label = @50
                                                                                                    block (type 44) (result i32) ;; label = @51
                                                                                                    block (type 43) (result i32) ;; label = @52
                                                                                                    block (type 42) ;; label = @53
                                                                                                    block (type 41) ;; label = @54
                                                                                                    block (type 40) ;; label = @55
                                                                                                    block (type 39) (result i32 i32) ;; label = @56
                                                                                                    block (type 38) ;; label = @57
                                                                                                    block (type 37) (result i32) ;; label = @58
                                                                                                    block (type 36) (result i32) ;; label = @59
                                                                                                    block (type 35) ;; label = @60
                                                                                                    block (type 34) ;; label = @61
                                                                                                    block (type 33) ;; label = @62
                                                                                                    block (type 32) (result i32 i32) ;; label = @63
                                                                                                    block (type 31) ;; label = @64
                                                                                                    block (type 30) (result i32) ;; label = @65
                                                                                                    block (type 29) (result i32) ;; label = @66
                                                                                                    block (type 28) ;; label = @67
                                                                                                    block (type 27) ;; label = @68
                                                                                                    block (type 26) ;; label = @69
                                                                                                    block (type 25) (result i32 i32) ;; label = @70
                                                                                                    block (type 24) ;; label = @71
                                                                                                    block (type 23) (result i32) ;; label = @72
                                                                                                    block (type 22) (result i32) ;; label = @73
                                                                                                    block (type 21) ;; label = @74
                                                                                                    block (type 20) ;; label = @75
                                                                                                    block (type 19) ;; label = @76
                                                                                                    i32.const 0
                                                                                                    local.set 22
                                                                                                    global.get 7
                                                                                                    local.set 21
                                                                                                    local.get 21
                                                                                                    ref.cast (ref 9)
                                                                                                    local.get 22
                                                                                                    struct.set 9 0
                                                                                                    local.get 0
                                                                                                    i32.const 10
                                                                                                    i32.gt_s
                                                                                                    i32.eqz
                                                                                                    br_if 2 (;@74;)
                                                                                                    br 0 (;@76;)
                                                                                                    end
                                                                                                    local.get 0
                                                                                                    call 4
                                                                                                    i32.eqz
                                                                                                    br_if 1 (;@74;)
                                                                                                    br 0 (;@75;)
                                                                                                    end
                                                                                                    i32.const 1
                                                                                                    br 1 (;@73;)
                                                                                                    end
                                                                                                    local.get 0
                                                                                                    br 1 (;@72;)
                                                                                                    end
                                                                                                    local.set 17
                                                                                                    local.get 15
                                                                                                    i32.const 4
                                                                                                    i32.add
                                                                                                    local.get 17
                                                                                                    i32.store align=1
                                                                                                    br 1 (;@71;)
                                                                                                    end
                                                                                                    i32.const 8
                                                                                                    br 1 (;@70;)
                                                                                                    end
                                                                                                    br 40 (;@30;)
                                                                                                    end
                                                                                                    i32.gt_s
                                                                                                    i32.eqz
                                                                                                    br_if 2 (;@67;)
                                                                                                    br 0 (;@69;)
                                                                                                    end
                                                                                                    local.get 0
                                                                                                    i32.const 1
                                                                                                    i32.add
                                                                                                    call 4
                                                                                                    i32.eqz
                                                                                                    br_if 1 (;@67;)
                                                                                                    br 0 (;@68;)
                                                                                                    end
                                                                                                    i32.const 2
                                                                                                    br 1 (;@66;)
                                                                                                    end
                                                                                                    local.get 0
                                                                                                    br 1 (;@65;)
                                                                                                    end
                                                                                                    local.set 17
                                                                                                    local.get 15
                                                                                                    i32.const 4
                                                                                                    i32.add
                                                                                                    local.get 17
                                                                                                    i32.store align=1
                                                                                                    br 1 (;@64;)
                                                                                                    end
                                                                                                    i32.const 6
                                                                                                    br 1 (;@63;)
                                                                                                    end
                                                                                                    br 33 (;@30;)
                                                                                                    end
                                                                                                    i32.gt_s
                                                                                                    i32.eqz
                                                                                                    br_if 2 (;@60;)
                                                                                                    br 0 (;@62;)
                                                                                                    end
                                                                                                    local.get 0
                                                                                                    call 4
                                                                                                    i32.eqz
                                                                                                    br_if 1 (;@60;)
                                                                                                    br 0 (;@61;)
                                                                                                    end
                                                                                                    i32.const 3
                                                                                                    br 1 (;@59;)
                                                                                                    end
                                                                                                    local.get 0
                                                                                                    br 1 (;@58;)
                                                                                                    end
                                                                                                    local.set 17
                                                                                                    local.get 15
                                                                                                    i32.const 4
                                                                                                    i32.add
                                                                                                    local.get 17
                                                                                                    i32.store align=1
                                                                                                    br 1 (;@57;)
                                                                                                    end
                                                                                                    i32.const 4
                                                                                                    br 1 (;@56;)
                                                                                                    end
                                                                                                    br 26 (;@30;)
                                                                                                    end
                                                                                                    i32.gt_s
                                                                                                    i32.eqz
                                                                                                    br_if 2 (;@53;)
                                                                                                    br 0 (;@55;)
                                                                                                    end
                                                                                                    local.get 0
                                                                                                    i32.const 1
                                                                                                    i32.add
                                                                                                    call 4
                                                                                                    i32.eqz
                                                                                                    br_if 1 (;@53;)
                                                                                                    br 0 (;@54;)
                                                                                                    end
                                                                                                    i32.const 4
                                                                                                    br 1 (;@52;)
                                                                                                    end
                                                                                                    local.get 0
                                                                                                    br 1 (;@51;)
                                                                                                    end
                                                                                                    local.set 17
                                                                                                    local.get 15
                                                                                                    i32.const 4
                                                                                                    i32.add
                                                                                                    local.get 17
                                                                                                    i32.store align=1
                                                                                                    br 1 (;@50;)
                                                                                                    end
                                                                                                    i32.const 2
                                                                                                    br 1 (;@49;)
                                                                                                    end
                                                                                                    br 19 (;@30;)
                                                                                                    end
                                                                                                    i32.gt_s
                                                                                                    i32.eqz
                                                                                                    br_if 2 (;@46;)
                                                                                                    br 0 (;@48;)
                                                                                                  end
                                                                                                  local.get 0
                                                                                                  call 4
                                                                                                  i32.eqz
                                                                                                  br_if 1 (;@46;)
                                                                                                  br 0 (;@47;)
                                                                                                end
                                                                                                i32.const 5
                                                                                                br 1 (;@45;)
                                                                                              end
                                                                                              local.get 0
                                                                                              br 1 (;@44;)
                                                                                            end
                                                                                            local.set 17
                                                                                            local.get 15
                                                                                            i32.const 4
                                                                                            i32.add
                                                                                            local.get 17
                                                                                            i32.store align=1
                                                                                            br 1 (;@43;)
                                                                                          end
                                                                                          i32.const 0
                                                                                          br 1 (;@42;)
                                                                                        end
                                                                                        br 12 (;@30;)
                                                                                      end
                                                                                      i32.gt_s
                                                                                      i32.eqz
                                                                                      br_if 2 (;@39;)
                                                                                      br 0 (;@41;)
                                                                                    end
                                                                                    local.get 0
                                                                                    i32.const 1
                                                                                    i32.add
                                                                                    call 4
                                                                                    i32.eqz
                                                                                    br_if 1 (;@39;)
                                                                                    br 0 (;@40;)
                                                                                  end
                                                                                  i32.const 6
                                                                                  br 1 (;@38;)
                                                                                end
                                                                                local.get 0
                                                                                br 1 (;@37;)
                                                                              end
                                                                              local.set 17
                                                                              local.get 15
                                                                              i32.const 4
                                                                              i32.add
                                                                              local.get 17
                                                                              i32.store align=1
                                                                              br 1 (;@36;)
                                                                            end
                                                                            i32.const 0
                                                                            br 1 (;@35;)
                                                                          end
                                                                          br 5 (;@30;)
                                                                        end
                                                                        i32.lt_s
                                                                        br_if 1 (;@33;)
                                                                        br 0 (;@34;)
                                                                      end
                                                                      i32.const 8
                                                                      br 1 (;@32;)
                                                                    end
                                                                    i32.const 7
                                                                    br 1 (;@31;)
                                                                  end
                                                                  br 0 (;@31;)
                                                                end
                                                                local.set 17
                                                                local.get 15
                                                                i32.const 4
                                                                i32.add
                                                                local.get 17
                                                                i32.store align=1
                                                                br 0 (;@30;)
                                                              end
                                                              i32.const 12
                                                              local.set 17
                                                              local.get 15
                                                              i32.const 0
                                                              i32.add
                                                              local.get 17
                                                              i32.store align=1
                                                              local.get 0
                                                              i32.const 0
                                                              i32.lt_s
                                                              br_if 1 (;@28;)
                                                              br 0 (;@29;)
                                                            end
                                                            ref.null eq
                                                            local.get 15
                                                            i32.const 0
                                                            i32.add
                                                            br 1 (;@27;)
                                                          end
                                                          ref.null eq
                                                          local.get 15
                                                          i32.const 4
                                                          i32.add
                                                          br 1 (;@26;)
                                                        end
                                                        br 0 (;@26;)
                                                      end
                                                      local.set 22
                                                      local.set 21
                                                      local.get 21
                                                      local.get 22
                                                      local.get 21
                                                      local.get 22
                                                      call 9
                                                      i32.const 1
                                                      i32.add
                                                      local.set 22
                                                      local.get 22
                                                      call 10
                                                      local.get 0
                                                      br_table 1 (;@24;) 2 (;@23;) 3 (;@22;) 4 (;@21;) 5 (;@20;) 0 (;@25;)
                                                    end
                                                    br 5 (;@19;)
                                                  end
                                                  i32.const 10
                                                  br 5 (;@18;)
                                                end
                                                i32.const 20
                                                br 5 (;@17;)
                                              end
                                              i32.const 30
                                              br 5 (;@16;)
                                            end
                                            i32.const 40
                                            br 5 (;@15;)
                                          end
                                          i32.const 50
                                          br 5 (;@14;)
                                        end
                                        i32.const 60
                                        br 5 (;@13;)
                                      end
                                      local.set 4
                                      br 5 (;@12;)
                                    end
                                    local.set 4
                                    br 5 (;@11;)
                                  end
                                  local.set 4
                                  br 5 (;@10;)
                                end
                                local.set 4
                                br 5 (;@9;)
                              end
                              local.set 4
                              br 5 (;@8;)
                            end
                            local.set 4
                            br 5 (;@7;)
                          end
                          br 4 (;@7;)
                        end
                        br 3 (;@7;)
                      end
                      br 2 (;@7;)
                    end
                    br 1 (;@7;)
                  end
                  br 0 (;@7;)
                end
                local.get 4
                local.set 3
                global.get 7
                ref.cast (ref 9)
                struct.get 9 0
                i32.const 100
                i32.mul
                local.get 15
                i32.const 4
                i32.add
                i32.load align=1
                i32.const 4
                i32.gt_s
                br_if 1 (;@5;)
                br 0 (;@6;)
              end
              local.get 15
              i32.const 4
              i32.add
              i32.load align=1
              br 1 (;@4;)
            end
            local.get 15
            i32.const 4
            i32.add
            i32.load align=1
            br 1 (;@3;)
          end
          i32.const 10
          i32.add
          br 0 (;@3;)
        end
        i32.add
        local.get 15
        i32.const 0
        i32.add
        i32.load align=1
        i32.add
        local.get 3
        i32.add
        local.get 15
        global.set 6
        return
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 14
    local.get 15
    global.set 6
    local.get 14
    throw_ref
  )
  (func (;4;) (type 93) (param i32) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 7
    ref.cast (ref 9)
    struct.get 9 0
    i32.const 1
    i32.add
    local.set 3
    global.get 7
    local.set 2
    local.get 2
    ref.cast (ref 9)
    local.get 3
    struct.set 9 0
    local.get 0
    i32.const 1
    i32.and
    i32.const 0
    i32.gt_u
    i32.const 255
    i32.and
    return
  )
  (func (;5;) (type 11) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    unreachable
  )
  (func (;6;) (type 12) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    unreachable
  )
  (func (;7;) (type 13) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    unreachable
  )
  (func (;8;) (type 14) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    unreachable
  )
  (func (;9;) (type 15) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
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
  (func (;10;) (type 16) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
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
  (data (;0;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;1;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;2;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;3;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;4;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
