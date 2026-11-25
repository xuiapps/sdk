# Real TestApp view: construction gate

Repository sources: `Xui/Apps/TestApp/Pages/Canvas/Tests/FillRectTest.cs` and `Xui/Apps/TestApp/Pages/Canvas/ICanvasDrawingExample.cs`.

These unchanged sources form a separate TestApp.Canvas assembly linked with the actual Xui.Core implementation and this driver. This first gate deliberately tests construction before adding the rendering host.

## Driver

```csharp
using Xui.Apps.TestApp.Pages.Canvas.Tests;
public static class Program {
    public static int Main() {
        var view = new FillRectTest();
        if (view == null) return 1;
        return view.WritingMode == Xui.Core.UI.WritingMode.HorizontalTB ? 0 : 2;
    }
}
```

## Status

Construction and the default WritingMode passed. Layout, rendering, input, and the complete TestApp are not exercised by this gate.

## Exit Code

```text
0
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (struct (field i32) (field externref)))
  (type (;3;) (func (param eqref)))
  (type (;4;) (func (result eqref)))
  (type (;5;) (func (result eqref exnref)))
  (type (;6;) (func (result exnref)))
  (type (;7;) (func))
  (type (;8;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;9;) (sub (struct (field (mut eqref)))))
  (type (;10;) (func (param eqref eqref) (result eqref)))
  (type (;11;) (sub 9 (struct (field (mut eqref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref)) (field (mut eqref)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut eqref)))))
  (type (;12;) (struct (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref))))
  (type (;13;) (sub 9 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)) (field (mut i64)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)))))
  (type (;14;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;15;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;16;) (struct (field (mut i32))))
  (type (;17;) (struct (field (mut i32))))
  (type (;18;) (struct (field (mut i32))))
  (type (;19;) (struct (field (mut i32))))
  (type (;20;) (struct (field (mut i32))))
  (type (;21;) (struct (field (mut f32)) (field (mut f32))))
  (type (;22;) (struct (field (mut i32))))
  (type (;23;) (struct (field (mut f32)) (field (mut f32))))
  (type (;24;) (struct (field (mut i32))))
  (type (;25;) (struct (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;26;) (struct (field (mut eqref))))
  (type (;27;) (struct (field (mut i32))))
  (type (;28;) (sub 9 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)) (field (mut i64)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut eqref)) (field (mut eqref)))))
  (type (;29;) (sub 9 (struct (field (mut eqref)) (field (mut eqref)))))
  (type (;30;) (sub 9 (struct (field (mut eqref)) (field (mut eqref)))))
  (type (;31;) (sub 11 (struct (field (mut eqref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref)) (field (mut eqref)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut eqref)))))
  (type (;32;) (struct (field (mut f32)) (field (mut f32))))
  (type (;33;) (sub 9 (struct (field (mut eqref)))))
  (type (;34;) (struct (field (mut eqref)) (field (mut eqref))))
  (type (;35;) (struct (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut eqref))))
  (type (;36;) (struct (field (mut i32)) (field (mut i32)) (field (mut externref))))
  (type (;37;) (array (mut i32)))
  (type (;38;) (array (mut eqref)))
  (type (;39;) (array (mut eqref)))
  (type (;40;) (func (result i32)))
  (type (;41;) (func (param eqref i32) (result eqref)))
  (type (;42;) (func (param eqref i32 eqref)))
  (type (;43;) (func (param eqref i32) (result externref)))
  (type (;44;) (func (param eqref i32 externref)))
  (type (;45;) (func (param eqref i32) (result f32)))
  (type (;46;) (func (param eqref i32 f32)))
  (type (;47;) (func (result i32)))
  (type (;48;) (func))
  (type (;49;) (func))
  (type (;50;) (func (param eqref)))
  (type (;51;) (func (param eqref) (result i32)))
  (type (;52;) (func (param eqref)))
  (type (;53;) (func (param eqref f32 f32 i32)))
  (type (;54;) (func (param i32 i32) (result i32)))
  (type (;55;) (func (param eqref f32 f32 f32 f32 i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;0;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 162 162 funcref)
  (memory (;0;) 16 16)
  (tag (;0;) (type 3) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut i32) i32.const 16)
  (global (;7;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 85 i32.const 73 i32.const 46 i32.const 86 i32.const 105 i32.const 101 i32.const 119 array.new_fixed 0 16 i32.const 40 i32.const 55 struct.new 8)
  (global (;8;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 73 i32.const 83 i32.const 101 i32.const 114 i32.const 118 i32.const 105 i32.const 99 i32.const 101 i32.const 80 i32.const 114 i32.const 111 i32.const 118 i32.const 105 i32.const 100 i32.const 101 i32.const 114 array.new_fixed 0 23 i32.const 0 i32.const 0 struct.new 8)
  (global (;9;) eqref i32.const 3 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 85 i32.const 73 i32.const 46 i32.const 76 i32.const 97 i32.const 121 i32.const 101 i32.const 114 i32.const 46 i32.const 73 i32.const 76 i32.const 97 i32.const 121 i32.const 101 i32.const 114 i32.const 72 i32.const 111 i32.const 115 i32.const 116 array.new_fixed 0 28 i32.const 0 i32.const 0 struct.new 8)
  (global (;10;) eqref i32.const 4 global.get 7 i32.const 120 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 65 i32.const 112 i32.const 112 i32.const 115 i32.const 46 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 65 i32.const 112 i32.const 112 i32.const 46 i32.const 80 i32.const 97 i32.const 103 i32.const 101 i32.const 115 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 115 i32.const 46 i32.const 70 i32.const 105 i32.const 108 i32.const 108 i32.const 82 i32.const 101 i32.const 99 i32.const 116 i32.const 84 i32.const 101 i32.const 115 i32.const 116 array.new_fixed 0 48 i32.const 41 i32.const 55 struct.new 8)
  (global (;11;) eqref i32.const 5 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 65 i32.const 112 i32.const 112 i32.const 115 i32.const 46 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 65 i32.const 112 i32.const 112 i32.const 46 i32.const 80 i32.const 97 i32.const 103 i32.const 101 i32.const 115 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 69 i32.const 120 i32.const 97 i32.const 109 i32.const 112 i32.const 108 i32.const 101 array.new_fixed 0 51 i32.const 0 i32.const 0 struct.new 8)
  (global (;12;) eqref i32.const 6 ref.null eq i32.const 40 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 72 i32.const 97 i32.const 115 i32.const 104 i32.const 83 i32.const 101 i32.const 116 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 42 i32.const 22 i32.const 9 struct.new 8)
  (global (;13;) eqref i32.const 7 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 8)
  (global (;14;) eqref i32.const 8 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 8)
  (global (;15;) eqref i32.const 9 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 8)
  (global (;16;) eqref i32.const 10 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 83 i32.const 101 i32.const 116 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 39 i32.const 0 i32.const 0 struct.new 8)
  (global (;17;) eqref i32.const 11 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 54 i32.const 0 i32.const 0 struct.new 8)
  (global (;18;) eqref i32.const 12 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 83 i32.const 101 i32.const 116 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 47 i32.const 0 i32.const 0 struct.new 8)
  (global (;19;) eqref i32.const 13 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 82 i32.const 117 i32.const 110 i32.const 116 i32.const 105 i32.const 109 i32.const 101 i32.const 46 i32.const 83 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 46 i32.const 73 i32.const 83 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 42 i32.const 0 i32.const 0 struct.new 8)
  (global (;20;) eqref i32.const 14 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 82 i32.const 117 i32.const 110 i32.const 116 i32.const 105 i32.const 109 i32.const 101 i32.const 46 i32.const 83 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 46 i32.const 73 i32.const 68 i32.const 101 i32.const 115 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 67 i32.const 97 i32.const 108 i32.const 108 i32.const 98 i32.const 97 i32.const 99 i32.const 107 array.new_fixed 0 53 i32.const 0 i32.const 0 struct.new 8)
  (global (;21;) eqref i32.const 15 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 113 i32.const 117 i32.const 97 i32.const 108 i32.const 105 i32.const 116 i32.const 121 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 97 i32.const 114 i32.const 101 i32.const 114 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 52 i32.const 0 i32.const 0 struct.new 8)
  (global (;22;) eqref i32.const 16 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 68 i32.const 101 i32.const 98 i32.const 117 i32.const 103 i32.const 46 i32.const 73 i32.const 86 i32.const 105 i32.const 101 i32.const 119 i32.const 73 i32.const 110 i32.const 115 i32.const 116 i32.const 114 i32.const 117 i32.const 109 i32.const 101 i32.const 110 i32.const 116 i32.const 115 i32.const 83 i32.const 105 i32.const 110 i32.const 107 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 8)
  (global (;23;) eqref i32.const 17 ref.null eq i32.const 62 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 52 i32.const 34 i32.const 11 struct.new 8)
  (global (;24;) eqref i32.const 18 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 53 i32.const 0 i32.const 0 struct.new 8)
  (global (;25;) eqref i32.const 19 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 75 i32.const 101 i32.const 121 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 80 i32.const 97 i32.const 105 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 i32.const 62 array.new_fixed 0 94 i32.const 0 i32.const 0 struct.new 8)
  (global (;26;) eqref i32.const 20 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 75 i32.const 101 i32.const 121 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 80 i32.const 97 i32.const 105 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 i32.const 62 array.new_fixed 0 94 i32.const 0 i32.const 0 struct.new 8)
  (global (;27;) eqref i32.const 21 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 73 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 8)
  (global (;28;) eqref i32.const 22 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 8)
  (global (;29;) eqref i32.const 23 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 61 i32.const 0 i32.const 0 struct.new 8)
  (global (;30;) eqref i32.const 24 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 75 i32.const 101 i32.const 121 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 80 i32.const 97 i32.const 105 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 i32.const 62 array.new_fixed 0 102 i32.const 0 i32.const 0 struct.new 8)
  (global (;31;) eqref i32.const 25 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 113 i32.const 117 i32.const 97 i32.const 108 i32.const 105 i32.const 116 i32.const 121 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 97 i32.const 114 i32.const 101 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 51 i32.const 0 i32.const 0 struct.new 8)
  (global (;32;) eqref i32.const 26 ref.null eq i32.const 96 i32.const 75 i32.const 101 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 28 i32.const 12 i32.const 2 struct.new 8)
  (global (;33;) eqref i32.const 27 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 45 i32.const 0 i32.const 0 struct.new 8)
  (global (;34;) eqref i32.const 28 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 45 i32.const 0 i32.const 0 struct.new 8)
  (global (;35;) eqref i32.const 29 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 53 i32.const 0 i32.const 0 struct.new 8)
  (global (;36;) eqref i32.const 30 ref.null eq i32.const 108 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 30 i32.const 12 i32.const 2 struct.new 8)
  (global (;37;) eqref i32.const 31 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 8)
  (global (;38;) eqref i32.const 32 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 8)
  (global (;39;) eqref i32.const 33 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 54 i32.const 0 i32.const 0 struct.new 8)
  (global (;40;) eqref i32.const 34 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 77 i32.const 101 i32.const 97 i32.const 115 i32.const 117 i32.const 114 i32.const 101 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 31 i32.const 0 i32.const 0 struct.new 8)
  (global (;41;) eqref i32.const 35 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 84 i32.const 101 i32.const 120 i32.const 116 i32.const 77 i32.const 101 i32.const 97 i32.const 115 i32.const 117 i32.const 114 i32.const 101 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 8)
  (global (;42;) eqref i32.const 36 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 24 i32.const 0 i32.const 0 struct.new 8)
  (global (;43;) eqref i32.const 37 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 83 i32.const 116 i32.const 97 i32.const 116 i32.const 101 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 29 i32.const 0 i32.const 0 struct.new 8)
  (global (;44;) eqref i32.const 38 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 83 i32.const 121 i32.const 109 i32.const 98 i32.const 111 i32.const 108 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 8)
  (global (;45;) eqref i32.const 39 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 111 i32.const 115 i32.const 105 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 8)
  (global (;46;) eqref i32.const 40 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 70 i32.const 105 i32.const 108 i32.const 116 i32.const 101 i32.const 114 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 8)
  (global (;47;) eqref i32.const 41 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 101 i32.const 110 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 27 i32.const 0 i32.const 0 struct.new 8)
  (global (;48;) eqref i32.const 42 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 8)
  (global (;49;) eqref i32.const 43 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 66 i32.const 117 i32.const 105 i32.const 108 i32.const 100 i32.const 101 i32.const 114 array.new_fixed 0 28 i32.const 0 i32.const 0 struct.new 8)
  (global (;50;) eqref i32.const 44 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 71 i32.const 108 i32.const 121 i32.const 112 i32.const 104 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 66 i32.const 117 i32.const 105 i32.const 108 i32.const 100 i32.const 101 i32.const 114 array.new_fixed 0 33 i32.const 0 i32.const 0 struct.new 8)
  (global (;51;) eqref i32.const 45 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 array.new_fixed 0 28 i32.const 0 i32.const 0 struct.new 8)
  (global (;52;) eqref i32.const 46 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 67 i32.const 108 i32.const 105 i32.const 112 i32.const 112 i32.const 105 i32.const 110 i32.const 103 array.new_fixed 0 29 i32.const 0 i32.const 0 struct.new 8)
  (global (;53;) eqref i32.const 47 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 99 i32.const 116 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 8)
  (global (;54;) eqref i32.const 48 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 84 i32.const 101 i32.const 120 i32.const 116 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 8)
  (global (;55;) eqref i32.const 49 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 73 i32.const 109 i32.const 97 i32.const 103 i32.const 101 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 36 i32.const 0 i32.const 0 struct.new 8)
  (global (;56;) eqref i32.const 50 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 84 i32.const 114 i32.const 97 i32.const 110 i32.const 115 i32.const 102 i32.const 111 i32.const 114 i32.const 109 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 33 i32.const 0 i32.const 0 struct.new 8)
  (global (;57;) eqref i32.const 51 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 73 i32.const 68 i32.const 105 i32.const 115 i32.const 112 i32.const 111 i32.const 115 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 18 i32.const 0 i32.const 0 struct.new 8)
  (global (;58;) eqref i32.const 52 ref.null eq i32.const 161 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 array.new_fixed 0 5 i32.const 1 i32.const 1 struct.new 8)
  (export "main" (func 1))
  (func (;1;) (type 47) (result i32)
    (local eqref eqref eqref eqref eqref i32 i64 f64 externref)
    struct.new_default 31
    local.set 1
    local.get 1
    ref.cast (ref 31)
    global.get 10
    struct.set 31 0
    local.get 1
    call 2
    local.get 1
    local.set 0
    local.get 0
    ref.is_null
    i32.eqz
    if (type 48) ;; label = @1
      local.get 0
      local.set 2
      local.get 2
      local.tee 4
      ref.is_null
      if ;; label = @2
        i32.const 4
        global.get 3
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 53
          array.new_data 0 3
          i32.const 0
          i32.const 53
          call 0
          global.set 3
        end
        global.get 3
        struct.new 2
        throw 0
      end
      local.get 4
      ref.cast (ref 11)
      call 3
      i32.const 1
      i32.eq
      if (type 49) ;; label = @2
        i32.const 0
        return
      else
        i32.const 2
        return
      end
      unreachable
    else
      i32.const 1
      return
    end
    unreachable
  )
  (func (;2;) (type 50) (param eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 4
    return
  )
  (func (;3;) (type 51) (param eqref) (result i32)
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
        call 0
        global.set 3
      end
      global.get 3
      struct.new 2
      throw 0
    end
    local.get 2
    ref.cast (ref 11)
    struct.get 11 18
    i32.const 255
    i32.and
    return
  )
  (func (;4;) (type 52) (param eqref)
    (local eqref i32 eqref i32 f32 eqref f32 f32 i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 11
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 32
    i32.add
    global.set 6
    block (type 6) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        local.get 11
        i32.const 0
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 6
        local.get 11
        i32.const 0
        i32.add
        local.set 9
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        call 5
        local.get 11
        i32.const 0
        i32.add
        local.get 11
        i32.const 8
        i32.add
        call 6
        local.set 18
        local.tee 17
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 17
        ref.cast (ref 11)
        local.set 17
        local.get 17
        ref.cast (ref 11)
        local.get 18
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 11 11
        local.get 17
        ref.cast (ref 11)
        local.get 18
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 11 12
        local.get 17
        ref.cast (ref 11)
        local.get 18
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 11 13
        local.get 17
        ref.cast (ref 11)
        local.get 18
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 11 14
        local.get 0
        i32.const 1
        local.set 18
        local.tee 17
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 17
        ref.cast (ref 11)
        local.set 17
        local.get 17
        ref.cast (ref 11)
        local.get 18
        i32.const 255
        i32.and
        struct.set 11 18
        local.get 0
        i32.const 1
        local.set 18
        local.tee 17
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 17
        ref.cast (ref 11)
        local.set 17
        local.get 17
        ref.cast (ref 11)
        local.get 18
        i32.const 255
        i32.and
        struct.set 11 19
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        local.set 20
        local.tee 17
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 17
        ref.cast (ref 11)
        local.set 17
        local.get 17
        ref.cast (ref 11)
        local.get 20
        f32.demote_f64
        struct.set 11 20
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        local.set 20
        local.tee 17
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 17
        ref.cast (ref 11)
        local.set 17
        local.get 17
        ref.cast (ref 11)
        local.get 20
        f32.demote_f64
        struct.set 11 21
        local.get 0
        f64.const inf (;=inf;)
        local.set 20
        local.tee 17
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 17
        ref.cast (ref 11)
        local.set 17
        local.get 17
        ref.cast (ref 11)
        local.get 20
        f32.demote_f64
        struct.set 11 22
        local.get 0
        f64.const inf (;=inf;)
        local.set 20
        local.tee 17
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 17
        ref.cast (ref 11)
        local.set 17
        local.get 17
        ref.cast (ref 11)
        local.get 20
        f32.demote_f64
        struct.set 11 23
        local.get 0
        drop
        local.get 11
        global.set 6
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 11
    global.set 6
    local.get 10
    throw_ref
  )
  (func (;5;) (type 53) (param eqref f32 f32 i32)
    (local eqref i32 eqref i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 9
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 9
    i32.const 0
    i32.add
    global.set 6
    block (type 6) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 3
        local.get 1
        f64.promote_f32
        local.set 18
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 40) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 18
        f32.demote_f64
        call 13
        local.get 0
        local.get 3
        local.get 2
        f64.promote_f32
        local.set 18
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 40) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 18
        f32.demote_f64
        call 13
        local.get 9
        global.set 6
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 9
    global.set 6
    local.get 8
    throw_ref
  )
  (func (;6;) (type 54) (param i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref f32 f32 f32 f32 i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 13
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 64
    i32.add
    global.set 6
    local.get 13
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 13
    i32.const 0
    i32.add
    local.set 0
    block (type 6) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 14
        local.get 13
        i32.const 8
        i32.add
        local.get 14
        i32.const 8
        memory.copy
        local.get 13
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 14
        local.get 13
        i32.const 16
        i32.add
        local.get 14
        i32.const 8
        memory.copy
        local.get 13
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 14
        local.get 13
        i32.const 24
        i32.add
        local.get 14
        i32.const 8
        memory.copy
        local.get 13
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 14
        local.get 13
        i32.const 32
        i32.add
        local.get 14
        i32.const 8
        memory.copy
        local.get 13
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        local.get 13
        i32.const 40
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 6
        local.get 13
        i32.const 40
        i32.add
        local.set 11
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 7
        local.get 13
        i32.const 40
        i32.add
        local.set 14
        local.get 1
        local.get 14
        i32.const 16
        memory.copy
        local.get 1
        local.get 13
        global.set 6
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 13
    global.set 6
    local.get 12
    throw_ref
  )
  (func (;7;) (type 55) (param eqref f32 f32 f32 f32 i32)
    (local eqref i32 eqref i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 6
    block (type 6) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 5
        local.get 1
        f64.promote_f32
        local.set 20
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 40) (result i32) ;; label = @3
          local.get 7
          i32.const 0
          i32.add
        else
          local.get 7
          i32.const 0
          i32.add
        end
        local.get 20
        f32.demote_f64
        call 13
        local.get 0
        local.get 5
        local.get 2
        f64.promote_f32
        local.set 20
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 40) (result i32) ;; label = @3
          local.get 7
          i32.const 4
          i32.add
        else
          local.get 7
          i32.const 1
          i32.add
        end
        local.get 20
        f32.demote_f64
        call 13
        local.get 0
        local.get 5
        local.get 3
        f64.promote_f32
        local.set 20
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 40) (result i32) ;; label = @3
          local.get 7
          i32.const 8
          i32.add
        else
          local.get 7
          i32.const 2
          i32.add
        end
        local.get 20
        f32.demote_f64
        call 13
        local.get 0
        local.get 5
        local.get 4
        f64.promote_f32
        local.set 20
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 0
            global.set 3
          end
          global.get 3
          struct.new 2
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 40) (result i32) ;; label = @3
          local.get 7
          i32.const 12
          i32.add
        else
          local.get 7
          i32.const 3
          i32.add
        end
        local.get 20
        f32.demote_f64
        call 13
        local.get 11
        global.set 6
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 11
    global.set 6
    local.get 10
    throw_ref
  )
  (func (;8;) (type 41) (param eqref i32) (result eqref)
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
        call 0
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 2
      throw 0
    end
    unreachable
  )
  (func (;9;) (type 42) (param eqref i32 eqref)
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
        call 0
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 2
      throw 0
    end
    unreachable
  )
  (func (;10;) (type 43) (param eqref i32) (result externref)
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
        call 0
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 2
      throw 0
    end
    unreachable
  )
  (func (;11;) (type 44) (param eqref i32 externref)
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
        call 0
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 2
      throw 0
    end
    unreachable
  )
  (func (;12;) (type 45) (param eqref i32) (result f32)
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
        call 0
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 2
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      f32.load align=1
      return
    end
    unreachable
  )
  (func (;13;) (type 46) (param eqref i32 f32)
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
        call 0
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 2
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      f32.store align=1
      return
    end
    unreachable
  )
  (func (;14;) (type 10) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 9)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 9)
    struct.get 9 0
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
    global.get 11
    ref.eq
    if ;; label = @1
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
    global.get 14
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
      global.get 12
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 32
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 36
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
    global.get 17
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
    global.get 18
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
    global.get 19
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 12
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 20
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 12
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 21
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 22
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 24
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 25
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 26
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 27
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 28
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 32
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 36
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 29
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 30
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 23
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 31
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 33
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 32
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 34
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 32
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 35
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 32
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 37
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 36
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 38
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 12
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 32
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 36
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 39
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 12
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 32
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 36
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 40
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 41
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 42
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 43
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 44
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 45
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 46
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 47
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 48
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 49
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 50
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 51
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 52
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 53
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 54
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 55
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 56
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 57
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
      ref.cast (ref 8)
      struct.get 8 1
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
