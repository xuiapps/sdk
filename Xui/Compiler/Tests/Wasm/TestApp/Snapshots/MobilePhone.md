# Persistent phone preview

[Open standalone preview](MobilePhone.html)

The white browser surface hosts the emulator middleware's real PhoneFrameGeometry and PhoneChromeRenderer, with TestApp's unchanged FillRectTest inside. An explicit fixture subclass adds a draggable contact marker and one-second fade. **This is not the complete MobileTestAppShell or RootView/EventRouter.** See [full-shell readiness](MobileShellReadiness.md).

Sources: [Program.cs](Fixtures/TestApp/Mobile/Program.cs), [PhoneHost.cs](Fixtures/TestApp/Mobile/PhoneHost.cs), [CanvasContext.cs](Fixtures/TestApp/CanvasContext.cs).

## Execution

```text
Node: persistent session, frames, pointer mapping, capture identity, fade completion, resize passed.
Browser: requestAnimationFrame, DPR-aware resize, pointer capture/cancel, idle scheduling and cleanup.
```

The browser holds one opaque managed session, schedules frames only after invalidation or while View requests animation, and releases capture on up/cancel. Coordinates are logical CSS pixels; conversion into device coordinates runs in WASM. Source-profile immutable defaults avoid unsupported static initialization; normal emulator builds retain their cached defaults.

## Exit Code

```text
0
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param externref f64 f64 f64 f64 externref)))
  (type (;3;) (struct (field i32) (field externref)))
  (type (;4;) (func (param eqref)))
  (type (;5;) (func (result eqref)))
  (type (;6;) (func (result eqref exnref)))
  (type (;7;) (func (result exnref)))
  (type (;8;) (func))
  (type (;9;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;10;) (sub (struct (field (mut eqref)))))
  (type (;11;) (func (param eqref eqref) (result eqref)))
  (type (;12;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i64)) (field (mut i32)))))
  (type (;13;) (sub 10 (struct (field (mut eqref)))))
  (type (;14;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut f64)) (field (mut f64)) (field (mut f64)) (field (mut f64)) (field (mut f64)))))
  (type (;15;) (sub 10 (struct (field (mut eqref)))))
  (type (;16;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref)) (field (mut eqref)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)))))
  (type (;17;) (struct (field (mut f32)) (field (mut f32))))
  (type (;18;) (struct (field (mut i32))))
  (type (;19;) (struct (field (mut f32)) (field (mut f32))))
  (type (;20;) (struct (field (mut i32))))
  (type (;21;) (struct (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;22;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;23;) (struct (field (mut eqref))))
  (type (;24;) (struct (field (mut i32))))
  (type (;25;) (struct (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref))))
  (type (;26;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)) (field (mut i64)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)))))
  (type (;27;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;28;) (struct (field (mut i32))))
  (type (;29;) (struct (field (mut i32))))
  (type (;30;) (struct (field (mut i32))))
  (type (;31;) (struct (field (mut i32))))
  (type (;32;) (struct (field (mut i32))))
  (type (;33;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)) (field (mut i64)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut eqref)) (field (mut eqref)))))
  (type (;34;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)))))
  (type (;35;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)))))
  (type (;36;) (sub 16 (struct (field (mut eqref)) (field (mut eqref)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref)) (field (mut eqref)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)))))
  (type (;37;) (sub 36 (struct (field (mut eqref)) (field (mut eqref)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref)) (field (mut eqref)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f64)) (field (mut f64)) (field (mut f64)) (field (mut i32)))))
  (type (;38;) (struct (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i64)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref))))
  (type (;39;) (struct (field (mut i32)) (field (mut i32))))
  (type (;40;) (struct (field (mut i32))))
  (type (;41;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32))))
  (type (;42;) (struct (field (mut f32)) (field (mut f32))))
  (type (;43;) (struct (field (mut i32))))
  (type (;44;) (struct (field (mut i32))))
  (type (;45;) (struct (field (mut i32))))
  (type (;46;) (struct (field (mut i32)) (field (mut i32))))
  (type (;47;) (struct (field (mut i64)) (field (mut i64)) (field (mut eqref)) (field (mut eqref)) (field (mut eqref))))
  (type (;48;) (struct (field (mut i64))))
  (type (;49;) (struct (field (mut externref)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;50;) (struct (field (mut i32))))
  (type (;51;) (struct (field (mut externref)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;52;) (struct (field (mut i32)) (field (mut i32)) (field (mut f32))))
  (type (;53;) (struct (field (mut f32))))
  (type (;54;) (struct (field (mut f32))))
  (type (;55;) (sub 10 (struct (field (mut eqref)))))
  (type (;56;) (struct (field (mut eqref)) (field (mut eqref))))
  (type (;57;) (struct (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut eqref))))
  (type (;58;) (struct (field (mut i32)) (field (mut i32)) (field (mut externref))))
  (type (;59;) (array (mut i32)))
  (type (;60;) (array (mut eqref)))
  (type (;61;) (array (mut eqref)))
  (type (;62;) (func (result i32)))
  (type (;63;) (func (result eqref)))
  (type (;64;) (func (param eqref f64 f64 i64)))
  (type (;65;) (func))
  (type (;66;) (func))
  (type (;67;) (func))
  (type (;68;) (func (param eqref)))
  (type (;69;) (func (param eqref) (result i32)))
  (type (;70;) (func (param eqref i32 i32 f64 f64 f64) (result i32)))
  (type (;71;) (func))
  (type (;72;) (func))
  (type (;73;) (func))
  (type (;74;) (func))
  (type (;75;) (func))
  (type (;76;) (func))
  (type (;77;) (func))
  (type (;78;) (func))
  (type (;79;) (func (result i32)))
  (type (;80;) (func (result i32)))
  (type (;81;) (func))
  (type (;82;) (func))
  (type (;83;) (func (param eqref) (result f64)))
  (type (;84;) (func (param eqref) (result f64)))
  (type (;85;) (func (param eqref) (result f64)))
  (type (;86;) (func (param eqref) (result f64)))
  (type (;87;) (func (param eqref) (result f64)))
  (type (;88;) (func (param eqref) (result i32)))
  (type (;89;) (func (param eqref) (result i32)))
  (type (;90;) (func (param eqref) (result i32)))
  (type (;91;) (func (param eqref)))
  (type (;92;) (func (param i32 i64)))
  (type (;93;) (func (param eqref i32 i32)))
  (type (;94;) (func (param eqref eqref f64 f64)))
  (type (;95;) (func (param i32 f32 f32)))
  (type (;96;) (func (param i32 i32) (result i32)))
  (type (;97;) (func (param eqref i32 eqref i32) (result i32)))
  (type (;98;) (func (param i32 f32 f32 f32 f32)))
  (type (;99;) (func (param i32 i32) (result i32)))
  (type (;100;) (func (param i32 i32)))
  (type (;101;) (func (param eqref i32 eqref i32 i32) (result i32)))
  (type (;102;) (func (result i32)))
  (type (;103;) (func (param eqref eqref)))
  (type (;104;) (func (param eqref eqref)))
  (type (;105;) (func (param eqref) (result i32)))
  (type (;106;) (func (param i32 f32 f32)))
  (type (;107;) (func (param i32 i32) (result i32)))
  (type (;108;) (func (param i32 i32 i32 f32 f32 i32 f32 f32 f32 i32 i32 i32)))
  (type (;109;) (func (param i32 i32 i32 i64 i32 i32 i32 i32 eqref)))
  (type (;110;) (func (param eqref)))
  (type (;111;) (func (param eqref)))
  (type (;112;) (func (param eqref)))
  (type (;113;) (func (param i32 i32 i32 eqref eqref i32)))
  (type (;114;) (func (param eqref i32)))
  (type (;115;) (func (result eqref)))
  (type (;116;) (func (param i32) (result i32)))
  (type (;117;) (func (param i32 f32 f32 f32 f32)))
  (type (;118;) (func (param i32 eqref i32 i32) (result i32)))
  (type (;119;) (func (param i32 i32) (result i32)))
  (type (;120;) (func (param i32) (result f32)))
  (type (;121;) (func (param eqref eqref eqref i32 i32)))
  (type (;122;) (func (param eqref) (result f32)))
  (type (;123;) (func (param i32 i32) (result i32)))
  (type (;124;) (func (param i32 i32) (result i32)))
  (type (;125;) (func (param i32 f32 f32)))
  (type (;126;) (func (param i32 i32 i32 i32)))
  (type (;127;) (func (param eqref i32 i32 i32)))
  (type (;128;) (func))
  (type (;129;) (func (result i32)))
  (type (;130;) (func (result i32)))
  (type (;131;) (func (result i32)))
  (type (;132;) (func (result i32)))
  (type (;133;) (func))
  (type (;134;) (func))
  (type (;135;) (func))
  (type (;136;) (func (param i32) (result i32)))
  (type (;137;) (func (param i32 i32) (result i32)))
  (type (;138;) (func (param i32 i32) (result i32)))
  (type (;139;) (func (param i32 i32 i32 i32 i32 i32 i32 i32)))
  (type (;140;) (func (param eqref i32 i32 i32)))
  (type (;141;) (func))
  (type (;142;) (func (result f64)))
  (type (;143;) (func (result f64)))
  (type (;144;) (func))
  (type (;145;) (func))
  (type (;146;) (func))
  (type (;147;) (func))
  (type (;148;) (func))
  (type (;149;) (func))
  (type (;150;) (func))
  (type (;151;) (func (param eqref i32 i32)))
  (type (;152;) (func))
  (type (;153;) (func (result i32)))
  (type (;154;) (func))
  (type (;155;) (func))
  (type (;156;) (func (result i32)))
  (type (;157;) (func (param eqref eqref eqref i32)))
  (type (;158;) (func))
  (type (;159;) (func))
  (type (;160;) (func (param i32 f32 f32)))
  (type (;161;) (func (param eqref i32 i32)))
  (type (;162;) (func (param eqref)))
  (type (;163;) (func (param eqref)))
  (type (;164;) (func (param eqref i32 i32)))
  (type (;165;) (func))
  (type (;166;) (func (result i32)))
  (type (;167;) (func))
  (type (;168;) (func (param eqref)))
  (type (;169;) (func (param i32 i32) (result i32)))
  (type (;170;) (func (param i32 i32) (result i32)))
  (type (;171;) (func (param eqref externref)))
  (type (;172;) (func (param eqref i32)))
  (type (;173;) (func (param i32 f32 f32 f32 f32)))
  (type (;174;) (func (param eqref i32)))
  (type (;175;) (func (param eqref i32)))
  (type (;176;) (func (param eqref i32)))
  (type (;177;) (func (param eqref i32)))
  (type (;178;) (func (param eqref f32)))
  (type (;179;) (func (param i32 f32 f32 f32)))
  (type (;180;) (func (param i32) (result f32)))
  (type (;181;) (func (param eqref i32) (result i32)))
  (type (;182;) (func (param i32) (result f32)))
  (type (;183;) (func (param i32 f32 i32) (result i32)))
  (type (;184;) (func (param i32) (result f32)))
  (type (;185;) (func (param i32) (result f32)))
  (type (;186;) (func (param i32 f32 i32 i32 i32 i32)))
  (type (;187;) (func (param eqref i32 i32)))
  (type (;188;) (func (param i32 i32) (result i32)))
  (type (;189;) (func (param i32) (result i32)))
  (type (;190;) (func (param i32 i32) (result i32)))
  (type (;191;) (func (param i32) (result i32)))
  (type (;192;) (func (param i32 i32) (result i32)))
  (type (;193;) (func (param eqref)))
  (type (;194;) (func (param eqref)))
  (type (;195;) (func (param eqref)))
  (type (;196;) (func (param eqref)))
  (type (;197;) (func (param eqref i32 f32)))
  (type (;198;) (func (param eqref i32 f32)))
  (type (;199;) (func (param eqref)))
  (type (;200;) (func (param eqref)))
  (type (;201;) (func (param eqref i32)))
  (type (;202;) (func (param eqref i32)))
  (type (;203;) (func (param eqref i32)))
  (type (;204;) (func (param eqref i32)))
  (type (;205;) (func (param eqref i32 i32) (result i32)))
  (type (;206;) (func))
  (type (;207;) (func))
  (type (;208;) (func))
  (type (;209;) (func))
  (type (;210;) (func))
  (type (;211;) (func (param i32 i32) (result i32)))
  (type (;212;) (func (param i32 i32 i32) (result i32)))
  (type (;213;) (func (param i32) (result i32)))
  (type (;214;) (func (param eqref) (result i32)))
  (type (;215;) (func (param i32) (result i32)))
  (type (;216;) (func (param eqref) (result i32)))
  (type (;217;) (func (param eqref i32) (result i32)))
  (type (;218;) (func (param i32 i32 i32) (result i32)))
  (type (;219;) (func (param i32) (result eqref)))
  (type (;220;) (func (param eqref) (result f32)))
  (type (;221;) (func (param eqref) (result f32)))
  (type (;222;) (func (param eqref) (result f32)))
  (type (;223;) (func (param eqref) (result f32)))
  (type (;224;) (func (param i32 i32 i32) (result i32)))
  (type (;225;) (func (param eqref)))
  (type (;226;) (func (param eqref i32 i32) (result i32)))
  (type (;227;) (func))
  (type (;228;) (func))
  (type (;229;) (func))
  (type (;230;) (func))
  (type (;231;) (func))
  (type (;232;) (func))
  (type (;233;) (func))
  (type (;234;) (func))
  (type (;235;) (func))
  (type (;236;) (func (param eqref i32) (result i32)))
  (type (;237;) (func (param i32 i32) (result i32)))
  (type (;238;) (func (param i32 i32) (result i32)))
  (type (;239;) (func (param i32) (result i32)))
  (type (;240;) (func (param i32) (result i32)))
  (type (;241;) (func (param i32 i32) (result i32)))
  (type (;242;) (func (param i32) (result i32)))
  (type (;243;) (func (param i32) (result i32)))
  (type (;244;) (func (param i32 i32 i32) (result i32)))
  (type (;245;) (func (param i32 i32) (result i32)))
  (type (;246;) (func))
  (type (;247;) (func))
  (type (;248;) (func))
  (type (;249;) (func (param eqref)))
  (type (;250;) (func))
  (type (;251;) (func (param eqref)))
  (type (;252;) (func (param eqref i32)))
  (type (;253;) (func (param eqref)))
  (type (;254;) (func (param i32) (result i32)))
  (type (;255;) (func (param eqref) (result eqref)))
  (type (;256;) (func (param i32 i32)))
  (type (;257;) (func (param i32 i32) (result i32)))
  (type (;258;) (func (param i32 i32) (result i32)))
  (type (;259;) (func))
  (type (;260;) (func))
  (type (;261;) (func))
  (type (;262;) (func (param i32) (result eqref)))
  (type (;263;) (func (param eqref) (result i32)))
  (type (;264;) (func))
  (type (;265;) (func (param eqref)))
  (type (;266;) (func (param eqref)))
  (type (;267;) (func (param eqref)))
  (type (;268;) (func (param eqref) (result i32)))
  (type (;269;) (func (param eqref i32) (result i32)))
  (type (;270;) (func (param i32 i32) (result i32)))
  (type (;271;) (func (param eqref)))
  (type (;272;) (func (param eqref)))
  (type (;273;) (func))
  (type (;274;) (func (param eqref)))
  (type (;275;) (func (param eqref i32)))
  (type (;276;) (func (param eqref i32 i32)))
  (type (;277;) (func))
  (type (;278;) (func))
  (type (;279;) (func (param eqref i32 i32)))
  (type (;280;) (func))
  (type (;281;) (func))
  (type (;282;) (func (param i32 i32) (result i32)))
  (type (;283;) (func (param i32) (result f32)))
  (type (;284;) (func (param i32) (result f32)))
  (type (;285;) (func (param i32) (result f32)))
  (type (;286;) (func (param eqref i32 f32)))
  (type (;287;) (func (param i32 i32)))
  (type (;288;) (func (param eqref i32)))
  (type (;289;) (func (param eqref i32)))
  (type (;290;) (func (param eqref i32)))
  (type (;291;) (func (param eqref i32)))
  (type (;292;) (func (param i32 i32) (result i32)))
  (type (;293;) (func))
  (type (;294;) (func (param eqref i32 eqref i32) (result i32)))
  (type (;295;) (func))
  (type (;296;) (func))
  (type (;297;) (func (param i32 i32) (result i32)))
  (type (;298;) (func))
  (type (;299;) (func (param eqref i32 eqref)))
  (type (;300;) (func))
  (type (;301;) (func))
  (type (;302;) (func (param eqref i32) (result i32)))
  (type (;303;) (func (param i32) (result f32)))
  (type (;304;) (func (param eqref i32 i32) (result i32)))
  (type (;305;) (func (param eqref eqref)))
  (type (;306;) (func (param eqref eqref)))
  (type (;307;) (func))
  (type (;308;) (func))
  (type (;309;) (func (param eqref eqref)))
  (type (;310;) (func))
  (type (;311;) (func (param eqref i32 f32 f32 f32 f32 f32 i32)))
  (type (;312;) (func (param eqref i32 f32 f32 f32 f32 f32 i32)))
  (type (;313;) (func (param i32 i32) (result i32)))
  (type (;314;) (func (param i32) (result i64)))
  (type (;315;) (func (param eqref eqref)))
  (type (;316;) (func))
  (type (;317;) (func (param eqref)))
  (type (;318;) (func (param eqref f32 externref i32 i32 i32 i32)))
  (type (;319;) (func (result i32)))
  (type (;320;) (func (result i32)))
  (type (;321;) (func (result i32)))
  (type (;322;) (func (result f64)))
  (type (;323;) (func (param eqref eqref eqref)))
  (type (;324;) (func))
  (type (;325;) (func (param eqref eqref)))
  (type (;326;) (func (param eqref eqref)))
  (type (;327;) (func))
  (type (;328;) (func (param eqref)))
  (type (;329;) (func (param eqref i32) (result eqref)))
  (type (;330;) (func (param eqref) (result i32)))
  (type (;331;) (func (param eqref f32)))
  (type (;332;) (func (param eqref f32)))
  (type (;333;) (func (param eqref i32)))
  (type (;334;) (func (param eqref i32)))
  (type (;335;) (func (param eqref i32)))
  (type (;336;) (func (param eqref i32)))
  (type (;337;) (func (param eqref f32)))
  (type (;338;) (func (param eqref f32)))
  (type (;339;) (func (param eqref f32)))
  (type (;340;) (func (param eqref f32)))
  (type (;341;) (func (param eqref f32)))
  (type (;342;) (func (param eqref f32)))
  (type (;343;) (func (param eqref i32)))
  (type (;344;) (func (param eqref i32)))
  (type (;345;) (func))
  (type (;346;) (func (param eqref i32)))
  (type (;347;) (func (param eqref i32)))
  (type (;348;) (func (param eqref externref)))
  (type (;349;) (func (param eqref f32)))
  (type (;350;) (func (param i32) (result i32)))
  (type (;351;) (func (param i32 f32)))
  (type (;352;) (func (param i32 i32) (result i32)))
  (type (;353;) (func (param eqref i32)))
  (type (;354;) (func (param i32) (result i32)))
  (type (;355;) (func (param i32) (result i32)))
  (type (;356;) (func (param i32 i32) (result i32)))
  (type (;357;) (func (param eqref i32)))
  (type (;358;) (func (param i32) (result i32)))
  (type (;359;) (func (param i32 f32)))
  (type (;360;) (func (param i32 i32) (result i32)))
  (type (;361;) (func (param eqref i32)))
  (type (;362;) (func (param i32) (result i32)))
  (type (;363;) (func (param i32) (result f32)))
  (type (;364;) (func (param eqref f32)))
  (type (;365;) (func (param eqref eqref)))
  (type (;366;) (func (param eqref eqref)))
  (type (;367;) (func (param eqref i32)))
  (type (;368;) (func (param eqref i32)))
  (type (;369;) (func (param eqref i32)))
  (type (;370;) (func (param eqref i32)))
  (type (;371;) (func (param eqref eqref i32)))
  (type (;372;) (func (param i32 i32 i32 f32)))
  (type (;373;) (func (param eqref) (result f32)))
  (type (;374;) (func (param eqref i32) (result i32)))
  (type (;375;) (func (param eqref i32) (result i32)))
  (type (;376;) (func (param eqref) (result f32)))
  (type (;377;) (func (param eqref) (result externref)))
  (type (;378;) (func (param eqref i32) (result i32)))
  (type (;379;) (func (param i32) (result i32)))
  (type (;380;) (func (param i32) (result i32)))
  (type (;381;) (func (param i32) (result f32)))
  (type (;382;) (func (param eqref i32)))
  (type (;383;) (func (param eqref i32)))
  (import "canvas" "Command" (func (;0;) (type 2) (param externref f64 f64 f64 f64 externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 519 519 funcref)
  (table (;1;) 65536 65536 eqref)
  (table (;2;) 65536 65536 externref)
  (memory (;0;) 16 16)
  (tag (;0;) (type 4) (param eqref))
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
  (global (;14;) (mut externref) ref.null extern)
  (global (;15;) (mut externref) ref.null extern)
  (global (;16;) (mut externref) ref.null extern)
  (global (;17;) (mut externref) ref.null extern)
  (global (;18;) (mut externref) ref.null extern)
  (global (;19;) (mut externref) ref.null extern)
  (global (;20;) (mut externref) ref.null extern)
  (global (;21;) (mut externref) ref.null extern)
  (global (;22;) (mut externref) ref.null extern)
  (global (;23;) (mut externref) ref.null extern)
  (global (;24;) (mut externref) ref.null extern)
  (global (;25;) (mut externref) ref.null extern)
  (global (;26;) (mut externref) ref.null extern)
  (global (;27;) (mut externref) ref.null extern)
  (global (;28;) (mut externref) ref.null extern)
  (global (;29;) (mut externref) ref.null extern)
  (global (;30;) (mut externref) ref.null extern)
  (global (;31;) (mut externref) ref.null extern)
  (global (;32;) (mut externref) ref.null extern)
  (global (;33;) (mut externref) ref.null extern)
  (global (;34;) (mut i32) i32.const 16)
  (global (;35;) (mut i32) i32.const 0)
  (global (;36;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 77 i32.const 111 i32.const 98 i32.const 105 i32.const 108 i32.const 101 i32.const 83 i32.const 101 i32.const 115 i32.const 115 i32.const 105 i32.const 111 i32.const 110 array.new_fixed 0 13 i32.const 0 i32.const 10 struct.new 9)
  (global (;37;) eqref i32.const 2 ref.null eq i32.const 22 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 65 i32.const 112 i32.const 112 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 27 i32.const 52 i32.const 1 struct.new 9)
  (global (;38;) eqref i32.const 3 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 24 i32.const 0 i32.const 0 struct.new 9)
  (global (;39;) eqref i32.const 4 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 77 i32.const 101 i32.const 97 i32.const 115 i32.const 117 i32.const 114 i32.const 101 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 31 i32.const 0 i32.const 0 struct.new 9)
  (global (;40;) eqref i32.const 5 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 84 i32.const 101 i32.const 120 i32.const 116 i32.const 77 i32.const 101 i32.const 97 i32.const 115 i32.const 117 i32.const 114 i32.const 101 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;41;) eqref i32.const 6 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 83 i32.const 116 i32.const 97 i32.const 116 i32.const 101 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 29 i32.const 0 i32.const 0 struct.new 9)
  (global (;42;) eqref i32.const 7 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 83 i32.const 121 i32.const 109 i32.const 98 i32.const 111 i32.const 108 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;43;) eqref i32.const 8 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 111 i32.const 115 i32.const 105 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;44;) eqref i32.const 9 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 70 i32.const 105 i32.const 108 i32.const 116 i32.const 101 i32.const 114 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;45;) eqref i32.const 10 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 101 i32.const 110 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 27 i32.const 0 i32.const 0 struct.new 9)
  (global (;46;) eqref i32.const 11 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;47;) eqref i32.const 12 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 66 i32.const 117 i32.const 105 i32.const 108 i32.const 100 i32.const 101 i32.const 114 array.new_fixed 0 28 i32.const 0 i32.const 0 struct.new 9)
  (global (;48;) eqref i32.const 13 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 71 i32.const 108 i32.const 121 i32.const 112 i32.const 104 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 66 i32.const 117 i32.const 105 i32.const 108 i32.const 100 i32.const 101 i32.const 114 array.new_fixed 0 33 i32.const 0 i32.const 0 struct.new 9)
  (global (;49;) eqref i32.const 14 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 array.new_fixed 0 28 i32.const 0 i32.const 0 struct.new 9)
  (global (;50;) eqref i32.const 15 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 67 i32.const 108 i32.const 105 i32.const 112 i32.const 112 i32.const 105 i32.const 110 i32.const 103 array.new_fixed 0 29 i32.const 0 i32.const 0 struct.new 9)
  (global (;51;) eqref i32.const 16 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 99 i32.const 116 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;52;) eqref i32.const 17 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 84 i32.const 101 i32.const 120 i32.const 116 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;53;) eqref i32.const 18 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 73 i32.const 109 i32.const 97 i32.const 103 i32.const 101 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 36 i32.const 0 i32.const 0 struct.new 9)
  (global (;54;) eqref i32.const 19 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 84 i32.const 114 i32.const 97 i32.const 110 i32.const 115 i32.const 102 i32.const 111 i32.const 114 i32.const 109 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 33 i32.const 0 i32.const 0 struct.new 9)
  (global (;55;) eqref i32.const 20 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 73 i32.const 68 i32.const 105 i32.const 115 i32.const 112 i32.const 111 i32.const 115 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 18 i32.const 0 i32.const 0 struct.new 9)
  (global (;56;) eqref i32.const 21 ref.null eq i32.const 96 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 77 i32.const 105 i32.const 100 i32.const 100 i32.const 108 i32.const 101 i32.const 119 i32.const 97 i32.const 114 i32.const 101 i32.const 46 i32.const 69 i32.const 109 i32.const 117 i32.const 108 i32.const 97 i32.const 116 i32.const 111 i32.const 114 i32.const 46 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 105 i32.const 108 i32.const 101 i32.const 114 i32.const 80 i32.const 104 i32.const 111 i32.const 110 i32.const 101 i32.const 72 i32.const 111 i32.const 115 i32.const 116 array.new_fixed 0 41 i32.const 0 i32.const 7 struct.new 9)
  (global (;57;) eqref i32.const 22 ref.null eq i32.const 118 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 77 i32.const 105 i32.const 100 i32.const 100 i32.const 108 i32.const 101 i32.const 119 i32.const 97 i32.const 114 i32.const 101 i32.const 46 i32.const 69 i32.const 109 i32.const 117 i32.const 108 i32.const 97 i32.const 116 i32.const 111 i32.const 114 i32.const 46 i32.const 85 i32.const 73 i32.const 46 i32.const 80 i32.const 104 i32.const 111 i32.const 110 i32.const 101 i32.const 46 i32.const 80 i32.const 104 i32.const 111 i32.const 110 i32.const 101 i32.const 67 i32.const 104 i32.const 114 i32.const 111 i32.const 109 i32.const 101 i32.const 82 i32.const 101 i32.const 110 i32.const 100 i32.const 101 i32.const 114 i32.const 101 i32.const 114 array.new_fixed 0 52 i32.const 0 i32.const 1 struct.new 9)
  (global (;58;) eqref i32.const 23 ref.null eq i32.const 140 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 85 i32.const 73 i32.const 46 i32.const 86 i32.const 105 i32.const 101 i32.const 119 array.new_fixed 0 16 i32.const 40 i32.const 55 struct.new 9)
  (global (;59;) eqref i32.const 24 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 73 i32.const 83 i32.const 101 i32.const 114 i32.const 118 i32.const 105 i32.const 99 i32.const 101 i32.const 80 i32.const 114 i32.const 111 i32.const 118 i32.const 105 i32.const 100 i32.const 101 i32.const 114 array.new_fixed 0 23 i32.const 0 i32.const 0 struct.new 9)
  (global (;60;) eqref i32.const 25 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 85 i32.const 73 i32.const 46 i32.const 76 i32.const 97 i32.const 121 i32.const 101 i32.const 114 i32.const 46 i32.const 73 i32.const 76 i32.const 97 i32.const 121 i32.const 101 i32.const 114 i32.const 72 i32.const 111 i32.const 115 i32.const 116 array.new_fixed 0 28 i32.const 0 i32.const 0 struct.new 9)
  (global (;61;) eqref i32.const 26 global.get 58 i32.const 370 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 65 i32.const 112 i32.const 112 i32.const 115 i32.const 46 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 65 i32.const 112 i32.const 112 i32.const 46 i32.const 80 i32.const 97 i32.const 103 i32.const 101 i32.const 115 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 115 i32.const 46 i32.const 70 i32.const 105 i32.const 108 i32.const 108 i32.const 82 i32.const 101 i32.const 99 i32.const 116 i32.const 84 i32.const 101 i32.const 115 i32.const 116 array.new_fixed 0 48 i32.const 41 i32.const 55 struct.new 9)
  (global (;62;) eqref i32.const 27 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 65 i32.const 112 i32.const 112 i32.const 115 i32.const 46 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 65 i32.const 112 i32.const 112 i32.const 46 i32.const 80 i32.const 97 i32.const 103 i32.const 101 i32.const 115 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 69 i32.const 120 i32.const 97 i32.const 109 i32.const 112 i32.const 108 i32.const 101 array.new_fixed 0 51 i32.const 0 i32.const 0 struct.new 9)
  (global (;63;) eqref i32.const 28 global.get 61 i32.const 433 i32.const 73 i32.const 110 i32.const 116 i32.const 101 i32.const 114 i32.const 97 i32.const 99 i32.const 116 i32.const 105 i32.const 118 i32.const 101 i32.const 70 i32.const 105 i32.const 108 i32.const 108 i32.const 82 i32.const 101 i32.const 99 i32.const 116 array.new_fixed 0 19 i32.const 41 i32.const 59 struct.new 9)
  (global (;64;) eqref i32.const 29 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 68 i32.const 101 i32.const 98 i32.const 117 i32.const 103 i32.const 46 i32.const 73 i32.const 86 i32.const 105 i32.const 101 i32.const 119 i32.const 73 i32.const 110 i32.const 115 i32.const 116 i32.const 114 i32.const 117 i32.const 109 i32.const 101 i32.const 110 i32.const 116 i32.const 115 i32.const 83 i32.const 105 i32.const 110 i32.const 107 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;65;) eqref i32.const 30 ref.null eq i32.const 202 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 72 i32.const 97 i32.const 115 i32.const 104 i32.const 83 i32.const 101 i32.const 116 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 42 i32.const 22 i32.const 9 struct.new 9)
  (global (;66;) eqref i32.const 31 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 9)
  (global (;67;) eqref i32.const 32 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 9)
  (global (;68;) eqref i32.const 33 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;69;) eqref i32.const 34 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 83 i32.const 101 i32.const 116 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 39 i32.const 0 i32.const 0 struct.new 9)
  (global (;70;) eqref i32.const 35 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 54 i32.const 0 i32.const 0 struct.new 9)
  (global (;71;) eqref i32.const 36 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 83 i32.const 101 i32.const 116 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 47 i32.const 0 i32.const 0 struct.new 9)
  (global (;72;) eqref i32.const 37 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 82 i32.const 117 i32.const 110 i32.const 116 i32.const 105 i32.const 109 i32.const 101 i32.const 46 i32.const 83 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 46 i32.const 73 i32.const 83 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 42 i32.const 0 i32.const 0 struct.new 9)
  (global (;73;) eqref i32.const 38 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 82 i32.const 117 i32.const 110 i32.const 116 i32.const 105 i32.const 109 i32.const 101 i32.const 46 i32.const 83 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 46 i32.const 73 i32.const 68 i32.const 101 i32.const 115 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 67 i32.const 97 i32.const 108 i32.const 108 i32.const 98 i32.const 97 i32.const 99 i32.const 107 array.new_fixed 0 53 i32.const 0 i32.const 0 struct.new 9)
  (global (;74;) eqref i32.const 39 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 113 i32.const 117 i32.const 97 i32.const 108 i32.const 105 i32.const 116 i32.const 121 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 97 i32.const 114 i32.const 101 i32.const 114 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 52 i32.const 0 i32.const 0 struct.new 9)
  (global (;75;) eqref i32.const 40 ref.null eq i32.const 246 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 52 i32.const 34 i32.const 11 struct.new 9)
  (global (;76;) eqref i32.const 41 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 53 i32.const 0 i32.const 0 struct.new 9)
  (global (;77;) eqref i32.const 42 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 75 i32.const 101 i32.const 121 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 80 i32.const 97 i32.const 105 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 i32.const 62 array.new_fixed 0 94 i32.const 0 i32.const 0 struct.new 9)
  (global (;78;) eqref i32.const 43 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 75 i32.const 101 i32.const 121 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 80 i32.const 97 i32.const 105 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 i32.const 62 array.new_fixed 0 94 i32.const 0 i32.const 0 struct.new 9)
  (global (;79;) eqref i32.const 44 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 73 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;80;) eqref i32.const 45 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;81;) eqref i32.const 46 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 61 i32.const 0 i32.const 0 struct.new 9)
  (global (;82;) eqref i32.const 47 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 75 i32.const 101 i32.const 121 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 80 i32.const 97 i32.const 105 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 i32.const 62 array.new_fixed 0 102 i32.const 0 i32.const 0 struct.new 9)
  (global (;83;) eqref i32.const 48 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 113 i32.const 117 i32.const 97 i32.const 108 i32.const 105 i32.const 116 i32.const 121 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 97 i32.const 114 i32.const 101 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 51 i32.const 0 i32.const 0 struct.new 9)
  (global (;84;) eqref i32.const 49 ref.null eq i32.const 302 i32.const 75 i32.const 101 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 28 i32.const 12 i32.const 2 struct.new 9)
  (global (;85;) eqref i32.const 50 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 45 i32.const 0 i32.const 0 struct.new 9)
  (global (;86;) eqref i32.const 51 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 45 i32.const 0 i32.const 0 struct.new 9)
  (global (;87;) eqref i32.const 52 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 53 i32.const 0 i32.const 0 struct.new 9)
  (global (;88;) eqref i32.const 53 ref.null eq i32.const 336 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 30 i32.const 12 i32.const 2 struct.new 9)
  (global (;89;) eqref i32.const 54 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 9)
  (global (;90;) eqref i32.const 55 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 9)
  (global (;91;) eqref i32.const 56 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 54 i32.const 0 i32.const 0 struct.new 9)
  (global (;92;) eqref i32.const 57 ref.null eq i32.const 496 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 array.new_fixed 0 5 i32.const 1 i32.const 1 struct.new 9)
  (global (;93;) eqref i32.const 58 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 85 i32.const 73 i32.const 46 i32.const 73 i32.const 110 i32.const 112 i32.const 117 i32.const 116 i32.const 46 i32.const 73 i32.const 80 i32.const 111 i32.const 105 i32.const 110 i32.const 116 i32.const 101 i32.const 114 i32.const 71 i32.const 101 i32.const 115 i32.const 116 i32.const 117 i32.const 114 i32.const 101 array.new_fixed 0 33 i32.const 0 i32.const 0 struct.new 9)
  (export "main" (func 2))
  (export "Create" (func 3))
  (export "Frame" (func 4))
  (export "NeedsFrame" (func 5))
  (export "Pointer" (func 6))
  (export "ScreenX" (func 7))
  (export "ScreenY" (func 8))
  (export "ScreenScale" (func 9))
  (export "MarkerX" (func 10))
  (export "MarkerY" (func 11))
  (export "Frames" (func 12))
  (export "PointerEvents" (func 13))
  (export "Animations" (func 14))
  (elem (;0;) (i32.const 44) func 88)
  (elem (;1;) (i32.const 45) func 132)
  (elem (;2;) (i32.const 46) func 169)
  (elem (;3;) (i32.const 47) func 170)
  (elem (;4;) (i32.const 48) func 171)
  (elem (;5;) (i32.const 49) func 172)
  (elem (;6;) (i32.const 50) func 173)
  (elem (;7;) (i32.const 51) func 174)
  (elem (;8;) (i32.const 52) func 195)
  (elem (;9;) (i32.const 53) func 196)
  (elem (;10;) (i32.const 54) func 175)
  (elem (;11;) (i32.const 55) func 176)
  (elem (;12;) (i32.const 56) func 147)
  (elem (;13;) (i32.const 57) func 208)
  (elem (;14;) (i32.const 58) func 194)
  (elem (;15;) (i32.const 59) func 89)
  (elem (;16;) (i32.const 76) func 92)
  (elem (;17;) (i32.const 78) func 93)
  (elem (;18;) (i32.const 88) func 159)
  (elem (;19;) (i32.const 90) func 90)
  (elem (;20;) (i32.const 92) func 148)
  (elem (;21;) (i32.const 94) func 91)
  (elem (;22;) (i32.const 22) func 88)
  (elem (;23;) (i32.const 23) func 89)
  (elem (;24;) (i32.const 24) func 90)
  (elem (;25;) (i32.const 25) func 91)
  (elem (;26;) (i32.const 26) func 92)
  (elem (;27;) (i32.const 27) func 93)
  (elem (;28;) (i32.const 28) func 132)
  (elem (;29;) (i32.const 29) func 147)
  (elem (;30;) (i32.const 30) func 148)
  (elem (;31;) (i32.const 31) func 159)
  (elem (;32;) (i32.const 32) func 169)
  (elem (;33;) (i32.const 33) func 170)
  (elem (;34;) (i32.const 34) func 171)
  (elem (;35;) (i32.const 35) func 172)
  (elem (;36;) (i32.const 36) func 173)
  (elem (;37;) (i32.const 37) func 174)
  (elem (;38;) (i32.const 38) func 175)
  (elem (;39;) (i32.const 39) func 176)
  (elem (;40;) (i32.const 40) func 194)
  (elem (;41;) (i32.const 41) func 195)
  (elem (;42;) (i32.const 42) func 196)
  (elem (;43;) (i32.const 43) func 208)
  (elem (;44;) (i32.const 169) func 168)
  (elem (;45;) (i32.const 170) func 167)
  (elem (;46;) (i32.const 171) func 59)
  (elem (;47;) (i32.const 176) func 139)
  (elem (;48;) (i32.const 177) func 150)
  (elem (;49;) (i32.const 178) func 152)
  (elem (;50;) (i32.const 179) func 157)
  (elem (;51;) (i32.const 180) func 156)
  (elem (;52;) (i32.const 185) func 137)
  (elem (;53;) (i32.const 186) func 166)
  (elem (;54;) (i32.const 188) func 121)
  (elem (;55;) (i32.const 191) func 162)
  (elem (;56;) (i32.const 195) func 153)
  (elem (;57;) (i32.const 198) func 155)
  (elem (;58;) (i32.const 200) func 111)
  (elem (;59;) (i32.const 399) func 168)
  (elem (;60;) (i32.const 400) func 167)
  (elem (;61;) (i32.const 401) func 59)
  (elem (;62;) (i32.const 406) func 139)
  (elem (;63;) (i32.const 407) func 150)
  (elem (;64;) (i32.const 408) func 152)
  (elem (;65;) (i32.const 409) func 165)
  (elem (;66;) (i32.const 410) func 156)
  (elem (;67;) (i32.const 415) func 137)
  (elem (;68;) (i32.const 416) func 166)
  (elem (;69;) (i32.const 418) func 121)
  (elem (;70;) (i32.const 421) func 162)
  (elem (;71;) (i32.const 425) func 153)
  (elem (;72;) (i32.const 428) func 155)
  (elem (;73;) (i32.const 430) func 111)
  (elem (;74;) (i32.const 462) func 168)
  (elem (;75;) (i32.const 463) func 167)
  (elem (;76;) (i32.const 464) func 62)
  (elem (;77;) (i32.const 469) func 140)
  (elem (;78;) (i32.const 470) func 150)
  (elem (;79;) (i32.const 471) func 152)
  (elem (;80;) (i32.const 472) func 158)
  (elem (;81;) (i32.const 473) func 156)
  (elem (;82;) (i32.const 478) func 137)
  (elem (;83;) (i32.const 479) func 166)
  (elem (;84;) (i32.const 481) func 121)
  (elem (;85;) (i32.const 484) func 162)
  (elem (;86;) (i32.const 488) func 153)
  (elem (;87;) (i32.const 491) func 155)
  (elem (;88;) (i32.const 493) func 111)
  (func (;2;) (type 62) (result i32)
    i32.const 0
    return
  )
  (func (;3;) (type 63) (result eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    struct.new_default 12
    local.set 0
    local.get 0
    ref.cast (ref 12)
    global.get 36
    struct.set 12 0
    local.get 0
    call 15
    local.get 0
    return
  )
  (func (;4;) (type 64) (param eqref f64 f64 i64)
    (local eqref eqref i32 f32 i32 i64 f64 externref i32 i64 eqref i32 i32 eqref eqref f64 f64 i32 f32 f32 eqref i32 eqref i32 f32 f32 f32 f32 i32 i32 eqref i32 eqref i32 eqref eqref eqref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 47
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 47
    i32.const 128
    i32.add
    global.set 34
    local.get 47
    i32.const 0
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 8
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        f64.const 0x1.8p+5 (;=48;)
        f64.le
        if (type 65) ;; label = @3
        else
          local.get 2
          f64.const 0x1.8p+5 (;=48;)
          f64.le
          i32.eqz
          if (type 66) ;; label = @4
            local.get 0
            local.tee 53
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 53
            ref.cast (ref 12)
            struct.get 12 3
            local.set 4
            local.get 0
            local.tee 53
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 53
            ref.cast (ref 12)
            struct.get 12 1
            local.set 5
            local.get 0
            call 5
            i32.eqz
            if (type 67) ;; label = @5
              local.get 0
              local.get 3
              local.set 55
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 12)
              local.set 53
              local.get 53
              ref.cast (ref 12)
              local.get 55
              struct.set 12 8
              local.get 0
              i32.const 0
              local.set 54
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 12)
              local.set 53
              local.get 53
              ref.cast (ref 12)
              local.get 54
              i32.const 255
              i32.and
              struct.set 12 9
              local.get 0
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 12)
              struct.get 12 2
              local.get 5
              local.get 1
              local.get 2
              local.set 20
              local.set 19
              local.set 18
              local.set 17
              local.get 17
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 14)
              local.get 18
              local.get 19
              local.get 20
              call 18
              local.get 4
              i32.const 393
              f32.convert_i32_s
              f64.promote_f32
              i32.const 759
              f32.convert_i32_s
              f64.promote_f32
              f32.demote_f64
              local.set 23
              f32.demote_f64
              local.set 22
              local.get 47
              i32.const 24
              i32.add
              local.tee 21
              i32.const 0
              i32.const 8
              memory.fill
              local.get 21
              local.get 22
              local.get 23
              call 19
              local.get 21
              local.get 47
              i32.const 32
              i32.add
              call 20
              local.get 5
              local.set 26
              local.set 25
              local.set 24
              local.get 24
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 16)
              local.get 25
              local.get 26
              local.get 47
              i32.const 40
              i32.add
              call 21
              local.set 48
              local.get 6
              local.get 48
              i32.const 8
              memory.copy
              local.get 4
              i32.const 0
              f32.convert_i32_s
              f64.promote_f32
              i32.const 59
              f32.convert_i32_s
              f64.promote_f32
              i32.const 393
              f32.convert_i32_s
              f64.promote_f32
              i32.const 759
              f32.convert_i32_s
              f64.promote_f32
              f32.demote_f64
              local.set 31
              f32.demote_f64
              local.set 30
              f32.demote_f64
              local.set 29
              f32.demote_f64
              local.set 28
              local.get 47
              i32.const 48
              i32.add
              local.tee 27
              i32.const 0
              i32.const 16
              memory.fill
              local.get 27
              local.get 28
              local.get 29
              local.get 30
              local.get 31
              call 22
              local.get 27
              local.get 47
              i32.const 64
              i32.add
              call 23
              local.get 5
              local.get 6
              local.set 48
              local.get 47
              i32.const 80
              i32.add
              local.get 48
              i32.const 8
              memory.copy
              local.get 47
              i32.const 80
              i32.add
              local.set 33
              local.get 47
              i32.const 88
              i32.add
              local.tee 32
              i32.const 0
              i32.const 12
              memory.fill
              local.get 32
              local.get 33
              call 24
              local.get 32
              local.set 37
              local.set 36
              local.set 35
              local.set 34
              local.get 34
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 16)
              local.get 35
              local.get 36
              local.get 37
              local.get 47
              i32.const 100
              i32.add
              call 25
              drop
              local.get 4
              local.get 5
              local.set 39
              local.set 38
              local.get 38
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 16)
              local.get 39
              call 26
              local.get 0
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 12)
              struct.get 12 2
              local.get 5
              local.set 41
              local.set 40
              local.get 40
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 14)
              local.get 41
              call 27
              local.get 0
              local.tee 52
              local.get 52
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 12)
              struct.get 12 4
              i32.const 1
              i32.add
              local.set 54
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 12)
              local.set 53
              local.get 53
              ref.cast (ref 12)
              local.get 54
              struct.set 12 4
              local.get 47
              global.set 34
              return
            else
              local.get 4
              local.get 0
              local.tee 53
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 53
              ref.cast (ref 12)
              struct.get 12 9
              if (type 68) (param eqref) ;; label = @6
                local.get 3
                local.set 13
                local.get 47
                i32.const 8
                i32.add
                local.tee 12
                i32.const 0
                i32.const 8
                memory.fill
                local.get 12
                local.get 13
                call 16
                local.get 12
                local.get 3
                local.set 13
                local.get 47
                i32.const 16
                i32.add
                local.tee 12
                i32.const 0
                i32.const 8
                memory.fill
                local.get 12
                local.get 13
                call 16
                local.get 12
                local.set 16
                local.set 15
                local.set 14
                local.get 14
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 16)
                local.get 15
                local.get 16
                call 17
                local.get 0
                local.tee 52
                local.get 52
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                struct.get 12 6
                i32.const 1
                i32.add
                local.set 54
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                local.set 53
                local.get 53
                ref.cast (ref 12)
                local.get 54
                struct.set 12 6
                local.get 0
                local.get 3
                local.set 55
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                local.set 53
                local.get 53
                ref.cast (ref 12)
                local.get 55
                struct.set 12 8
                local.get 0
                i32.const 0
                local.set 54
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                local.set 53
                local.get 53
                ref.cast (ref 12)
                local.get 54
                i32.const 255
                i32.and
                struct.set 12 9
                local.get 0
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                struct.get 12 2
                local.get 5
                local.get 1
                local.get 2
                local.set 20
                local.set 19
                local.set 18
                local.set 17
                local.get 17
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 14)
                local.get 18
                local.get 19
                local.get 20
                call 18
                local.get 4
                i32.const 393
                f32.convert_i32_s
                f64.promote_f32
                i32.const 759
                f32.convert_i32_s
                f64.promote_f32
                f32.demote_f64
                local.set 23
                f32.demote_f64
                local.set 22
                local.get 47
                i32.const 24
                i32.add
                local.tee 21
                i32.const 0
                i32.const 8
                memory.fill
                local.get 21
                local.get 22
                local.get 23
                call 19
                local.get 21
                local.get 47
                i32.const 32
                i32.add
                call 20
                local.get 5
                local.set 26
                local.set 25
                local.set 24
                local.get 24
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 16)
                local.get 25
                local.get 26
                local.get 47
                i32.const 40
                i32.add
                call 21
                local.set 48
                local.get 6
                local.get 48
                i32.const 8
                memory.copy
                local.get 4
                i32.const 0
                f32.convert_i32_s
                f64.promote_f32
                i32.const 59
                f32.convert_i32_s
                f64.promote_f32
                i32.const 393
                f32.convert_i32_s
                f64.promote_f32
                i32.const 759
                f32.convert_i32_s
                f64.promote_f32
                f32.demote_f64
                local.set 31
                f32.demote_f64
                local.set 30
                f32.demote_f64
                local.set 29
                f32.demote_f64
                local.set 28
                local.get 47
                i32.const 48
                i32.add
                local.tee 27
                i32.const 0
                i32.const 16
                memory.fill
                local.get 27
                local.get 28
                local.get 29
                local.get 30
                local.get 31
                call 22
                local.get 27
                local.get 47
                i32.const 64
                i32.add
                call 23
                local.get 5
                local.get 6
                local.set 48
                local.get 47
                i32.const 80
                i32.add
                local.get 48
                i32.const 8
                memory.copy
                local.get 47
                i32.const 80
                i32.add
                local.set 33
                local.get 47
                i32.const 88
                i32.add
                local.tee 32
                i32.const 0
                i32.const 12
                memory.fill
                local.get 32
                local.get 33
                call 24
                local.get 32
                local.set 37
                local.set 36
                local.set 35
                local.set 34
                local.get 34
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 16)
                local.get 35
                local.get 36
                local.get 37
                local.get 47
                i32.const 100
                i32.add
                call 25
                drop
                local.get 4
                local.get 5
                local.set 39
                local.set 38
                local.get 38
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 16)
                local.get 39
                call 26
                local.get 0
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                struct.get 12 2
                local.get 5
                local.set 41
                local.set 40
                local.get 40
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 14)
                local.get 41
                call 27
                local.get 0
                local.tee 52
                local.get 52
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                struct.get 12 4
                i32.const 1
                i32.add
                local.set 54
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                local.set 53
                local.get 53
                ref.cast (ref 12)
                local.get 54
                struct.set 12 4
                local.get 47
                global.set 34
                return
              else
                local.get 0
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                struct.get 12 8
                local.set 13
                local.get 47
                i32.const 8
                i32.add
                local.tee 12
                i32.const 0
                i32.const 8
                memory.fill
                local.get 12
                local.get 13
                call 16
                local.get 12
                local.get 3
                local.set 13
                local.get 47
                i32.const 16
                i32.add
                local.tee 12
                i32.const 0
                i32.const 8
                memory.fill
                local.get 12
                local.get 13
                call 16
                local.get 12
                local.set 16
                local.set 15
                local.set 14
                local.get 14
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 16)
                local.get 15
                local.get 16
                call 17
                local.get 0
                local.tee 52
                local.get 52
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                struct.get 12 6
                i32.const 1
                i32.add
                local.set 54
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                local.set 53
                local.get 53
                ref.cast (ref 12)
                local.get 54
                struct.set 12 6
                local.get 0
                local.get 3
                local.set 55
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                local.set 53
                local.get 53
                ref.cast (ref 12)
                local.get 55
                struct.set 12 8
                local.get 0
                i32.const 0
                local.set 54
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                local.set 53
                local.get 53
                ref.cast (ref 12)
                local.get 54
                i32.const 255
                i32.and
                struct.set 12 9
                local.get 0
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                struct.get 12 2
                local.get 5
                local.get 1
                local.get 2
                local.set 20
                local.set 19
                local.set 18
                local.set 17
                local.get 17
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 14)
                local.get 18
                local.get 19
                local.get 20
                call 18
                local.get 4
                i32.const 393
                f32.convert_i32_s
                f64.promote_f32
                i32.const 759
                f32.convert_i32_s
                f64.promote_f32
                f32.demote_f64
                local.set 23
                f32.demote_f64
                local.set 22
                local.get 47
                i32.const 24
                i32.add
                local.tee 21
                i32.const 0
                i32.const 8
                memory.fill
                local.get 21
                local.get 22
                local.get 23
                call 19
                local.get 21
                local.get 47
                i32.const 32
                i32.add
                call 20
                local.get 5
                local.set 26
                local.set 25
                local.set 24
                local.get 24
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 16)
                local.get 25
                local.get 26
                local.get 47
                i32.const 40
                i32.add
                call 21
                local.set 48
                local.get 6
                local.get 48
                i32.const 8
                memory.copy
                local.get 4
                i32.const 0
                f32.convert_i32_s
                f64.promote_f32
                i32.const 59
                f32.convert_i32_s
                f64.promote_f32
                i32.const 393
                f32.convert_i32_s
                f64.promote_f32
                i32.const 759
                f32.convert_i32_s
                f64.promote_f32
                f32.demote_f64
                local.set 31
                f32.demote_f64
                local.set 30
                f32.demote_f64
                local.set 29
                f32.demote_f64
                local.set 28
                local.get 47
                i32.const 48
                i32.add
                local.tee 27
                i32.const 0
                i32.const 16
                memory.fill
                local.get 27
                local.get 28
                local.get 29
                local.get 30
                local.get 31
                call 22
                local.get 27
                local.get 47
                i32.const 64
                i32.add
                call 23
                local.get 5
                local.get 6
                local.set 48
                local.get 47
                i32.const 80
                i32.add
                local.get 48
                i32.const 8
                memory.copy
                local.get 47
                i32.const 80
                i32.add
                local.set 33
                local.get 47
                i32.const 88
                i32.add
                local.tee 32
                i32.const 0
                i32.const 12
                memory.fill
                local.get 32
                local.get 33
                call 24
                local.get 32
                local.set 37
                local.set 36
                local.set 35
                local.set 34
                local.get 34
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 16)
                local.get 35
                local.get 36
                local.get 37
                local.get 47
                i32.const 100
                i32.add
                call 25
                drop
                local.get 4
                local.get 5
                local.set 39
                local.set 38
                local.get 38
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 16)
                local.get 39
                call 26
                local.get 0
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                struct.get 12 2
                local.get 5
                local.set 41
                local.set 40
                local.get 40
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 14)
                local.get 41
                call 27
                local.get 0
                local.tee 52
                local.get 52
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                struct.get 12 4
                i32.const 1
                i32.add
                local.set 54
                local.tee 53
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 53
                ref.cast (ref 12)
                local.set 53
                local.get 53
                ref.cast (ref 12)
                local.get 54
                struct.set 12 4
                local.get 47
                global.set 34
                return
              end
            end
          else
          end
        end
        local.get 47
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 42
    local.get 47
    global.set 34
    local.get 42
    throw_ref
  )
  (func (;5;) (type 69) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 12)
    struct.get 12 3
    local.set 1
    local.get 1
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 16)
    call 28
    i32.const 1
    i32.and
    i32.const 0
    i32.gt_u
    return
  )
  (func (;6;) (type 70) (param eqref i32 i32 f64 f64 f64) (result i32)
    (local i32 i32 i32 f32 i32 i64 f64 externref i32 f32 f32 i32 f32 f32 i32 i32 i32 f32 f32 i32 f32 f32 f32 i32 i32 i32 i32 i32 i32 i64 i32 i32 i32 i32 eqref eqref i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 49
    i32.const 1048288
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 49
    i32.const 288
    i32.add
    global.set 34
    local.get 49
    i32.const 0
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 52
    memory.fill
    local.get 49
    i32.const 56
    i32.add
    local.set 7
    local.get 7
    i32.const 0
    i32.const 112
    memory.fill
    local.get 49
    i32.const 168
    i32.add
    local.set 8
    local.get 8
    i32.const 0
    i32.const 8
    memory.fill
    global.get 35
    local.tee 46
    i32.const 65535
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 46
    i32.const 1
    i32.add
    global.set 35
    local.get 49
    i32.const 160
    i32.add
    local.get 46
    i32.const 0
    i32.add
    i32.store align=1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 55
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 55
        ref.cast (ref 12)
        struct.get 12 2
        local.tee 55
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 55
        ref.cast (ref 14)
        struct.get 14 4
        f64.const 0x0p+0 (;=0;)
        f64.le
        i32.eqz
        if (type 71) ;; label = @3
          local.get 3
          local.get 0
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          struct.get 12 2
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 14)
          struct.get 14 2
          f64.sub
          local.get 0
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          struct.get 12 2
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 14)
          struct.get 14 4
          f64.div
          local.set 3
          local.get 4
          local.get 0
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          struct.get 12 2
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 14)
          struct.get 14 3
          f64.sub
          local.get 0
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          struct.get 12 2
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 14)
          struct.get 14 4
          f64.div
          local.set 4
          local.get 1
          i32.const 2
          i32.eq
          i32.eqz
          if (type 72) ;; label = @4
            local.get 0
            local.tee 55
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 55
            ref.cast (ref 12)
            struct.get 12 7
            local.get 2
            i32.eq
            if (type 73) ;; label = @5
            else
              i32.const 0
              local.get 46
              ref.null eq
              i32.const 1
              table.fill 1
              local.get 46
              ref.null extern
              i32.const 1
              table.fill 2
              local.get 46
              global.set 35
              local.get 49
              global.set 34
              return
            end
          else
            local.get 0
            local.tee 55
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 55
            ref.cast (ref 12)
            struct.get 12 7
            i32.const -1
            i32.eq
            i32.eqz
            if (type 74) ;; label = @5
              i32.const 0
              local.get 46
              ref.null eq
              i32.const 1
              table.fill 1
              local.get 46
              ref.null extern
              i32.const 1
              table.fill 2
              local.get 46
              global.set 35
              local.get 49
              global.set 34
              return
            else
              local.get 3
              f64.const 0x0p+0 (;=0;)
              f64.lt
              if (type 75) ;; label = @6
                i32.const 0
                local.get 46
                ref.null eq
                i32.const 1
                table.fill 1
                local.get 46
                ref.null extern
                i32.const 1
                table.fill 2
                local.get 46
                global.set 35
                local.get 49
                global.set 34
                return
              else
                local.get 3
                f64.const 0x1.89p+8 (;=393;)
                f64.ge
                if (type 76) ;; label = @7
                  i32.const 0
                  local.get 46
                  ref.null eq
                  i32.const 1
                  table.fill 1
                  local.get 46
                  ref.null extern
                  i32.const 1
                  table.fill 2
                  local.get 46
                  global.set 35
                  local.get 49
                  global.set 34
                  return
                else
                  local.get 4
                  f64.const 0x1.d8p+5 (;=59;)
                  f64.lt
                  if (type 77) ;; label = @8
                    i32.const 0
                    local.get 46
                    ref.null eq
                    i32.const 1
                    table.fill 1
                    local.get 46
                    ref.null extern
                    i32.const 1
                    table.fill 2
                    local.get 46
                    global.set 35
                    local.get 49
                    global.set 34
                    return
                  else
                    local.get 4
                    f64.const 0x1.99p+9 (;=818;)
                    f64.ge
                    i32.eqz
                    if (type 78) ;; label = @9
                      local.get 0
                      local.get 2
                      local.set 56
                      local.tee 55
                      ref.is_null
                      if ;; label = @10
                        i32.const 4
                        global.get 30
                        ref.is_null
                        if ;; label = @11
                          i32.const 0
                          i32.const 53
                          array.new_data 0 30
                          i32.const 0
                          i32.const 53
                          call 1
                          global.set 30
                        end
                        global.get 30
                        struct.new 3
                        throw 0
                      end
                      local.get 55
                      ref.cast (ref 12)
                      local.set 55
                      local.get 55
                      ref.cast (ref 12)
                      local.get 56
                      struct.set 12 7
                    else
                      i32.const 0
                      local.get 46
                      ref.null eq
                      i32.const 1
                      table.fill 1
                      local.get 46
                      ref.null extern
                      i32.const 1
                      table.fill 2
                      local.get 46
                      global.set 35
                      local.get 49
                      global.set 34
                      return
                    end
                  end
                end
              end
            end
          end
          local.get 6
          local.get 3
          f32.demote_f64
          f64.promote_f32
          local.get 4
          f32.demote_f64
          f64.promote_f32
          f32.demote_f64
          local.set 16
          f32.demote_f64
          local.set 15
          local.get 49
          i32.const 176
          i32.add
          local.tee 14
          i32.const 0
          i32.const 8
          memory.fill
          local.get 14
          local.get 15
          local.get 16
          call 29
          local.get 14
          i32.const 1
          f32.convert_i32_s
          f64.promote_f32
          i32.const 1
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 19
          f32.demote_f64
          local.set 18
          local.get 49
          i32.const 184
          i32.add
          local.tee 17
          i32.const 0
          i32.const 8
          memory.fill
          local.get 17
          local.get 18
          local.get 19
          call 19
          local.get 17
          local.get 49
          i32.const 192
          i32.add
          call 20
          local.get 5
          f32.demote_f64
          f64.promote_f32
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 19
          f32.demote_f64
          local.set 18
          local.get 49
          i32.const 200
          i32.add
          local.tee 17
          i32.const 0
          i32.const 8
          memory.fill
          local.get 17
          local.get 18
          local.get 19
          call 19
          local.get 17
          local.get 49
          i32.const 208
          i32.add
          call 30
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          i32.const 1
          i32.const 0
          local.get 1
          i32.const 2
          i32.eq
          if (type 79) (result i32) ;; label = @4
            i32.const 1
          else
            local.get 1
            i32.const 3
            i32.eq
            if (type 80) (result i32) ;; label = @5
              i32.const 1
            else
              i32.const 0
            end
          end
          i32.extend16_s
          local.set 31
          i32.extend16_s
          local.set 30
          local.set 29
          f32.demote_f64
          local.set 28
          f32.demote_f64
          local.set 27
          f32.demote_f64
          local.set 26
          local.set 25
          f32.demote_f64
          local.set 24
          f32.demote_f64
          local.set 23
          local.set 22
          local.set 21
          local.set 20
          local.get 20
          local.get 21
          local.get 22
          local.get 23
          local.get 24
          local.get 25
          local.get 26
          local.get 27
          local.get 28
          local.get 29
          local.get 30
          local.get 31
          call 31
          local.get 7
          local.get 1
          local.get 2
          i32.const 0
          i64.extend_i32_s
          i32.const 1
          local.get 6
          local.set 50
          local.get 49
          i32.const 216
          i32.add
          local.get 50
          i32.const 52
          memory.copy
          local.get 49
          i32.const 216
          i32.add
          local.get 8
          i32.const 0
          i32.const 8
          memory.fill
          local.get 8
          local.set 50
          local.get 49
          i32.const 268
          i32.add
          local.get 50
          i32.const 8
          memory.copy
          local.get 49
          i32.const 268
          i32.add
          local.get 8
          i32.const 0
          i32.const 8
          memory.fill
          local.get 8
          local.set 50
          local.get 49
          i32.const 276
          i32.add
          local.get 50
          i32.const 8
          memory.copy
          local.get 49
          i32.const 276
          i32.add
          local.get 0
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          struct.get 12 1
          local.set 40
          local.set 39
          local.set 38
          local.set 37
          i32.const 255
          i32.and
          local.set 36
          local.set 35
          local.set 34
          local.set 33
          local.set 32
          local.get 32
          local.get 33
          local.get 34
          local.get 35
          local.get 36
          local.get 37
          local.get 38
          local.get 39
          local.get 40
          call 32
          local.get 0
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          struct.get 12 3
          local.get 7
          i32.const 1
          local.set 43
          local.set 42
          local.set 41
          local.get 41
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 16)
          local.get 42
          local.get 43
          local.get 41
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 31
          i32.add
          call_indirect (type 161)
          local.get 0
          local.tee 54
          local.get 54
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          struct.get 12 5
          i32.const 1
          i32.add
          local.set 56
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          local.set 55
          local.get 55
          ref.cast (ref 12)
          local.get 56
          struct.set 12 5
          local.get 1
          i32.const 4
          i32.eq
          if (type 81) ;; label = @4
          else
            local.get 1
            i32.const 5
            i32.eq
            i32.eqz
            if (type 82) ;; label = @5
              i32.const 1
              local.get 46
              ref.null eq
              i32.const 1
              table.fill 1
              local.get 46
              ref.null extern
              i32.const 1
              table.fill 2
              local.get 46
              global.set 35
              local.get 49
              global.set 34
              return
            else
            end
          end
          local.get 0
          i32.const -1
          local.set 56
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          local.set 55
          local.get 55
          ref.cast (ref 12)
          local.get 56
          struct.set 12 7
          local.get 0
          i32.const 1
          local.set 56
          local.tee 55
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 55
          ref.cast (ref 12)
          local.set 55
          local.get 55
          ref.cast (ref 12)
          local.get 56
          i32.const 255
          i32.and
          struct.set 12 9
          i32.const 1
          local.get 46
          ref.null eq
          i32.const 1
          table.fill 1
          local.get 46
          ref.null extern
          i32.const 1
          table.fill 2
          local.get 46
          global.set 35
          local.get 49
          global.set 34
          return
        else
          i32.const 0
          local.get 46
          ref.null eq
          i32.const 1
          table.fill 1
          local.get 46
          ref.null extern
          i32.const 1
          table.fill 2
          local.get 46
          global.set 35
          local.get 49
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 44
    local.get 46
    ref.null eq
    i32.const 1
    table.fill 1
    local.get 46
    ref.null extern
    i32.const 1
    table.fill 2
    local.get 46
    global.set 35
    local.get 49
    global.set 34
    local.get 44
    throw_ref
  )
  (func (;7;) (type 83) (param eqref) (result f64)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 12)
    struct.get 12 2
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 14)
    struct.get 14 2
    return
  )
  (func (;8;) (type 84) (param eqref) (result f64)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 12)
    struct.get 12 2
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 14)
    struct.get 14 3
    return
  )
  (func (;9;) (type 85) (param eqref) (result f64)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 12)
    struct.get 12 2
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 14)
    struct.get 14 4
    return
  )
  (func (;10;) (type 86) (param eqref) (result f64)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 12)
    struct.get 12 3
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 37)
    struct.get 37 55
    return
  )
  (func (;11;) (type 87) (param eqref) (result f64)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 12)
    struct.get 12 3
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 37)
    struct.get 37 56
    return
  )
  (func (;12;) (type 88) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 12)
    struct.get 12 4
    return
  )
  (func (;13;) (type 89) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 12)
    struct.get 12 5
    return
  )
  (func (;14;) (type 90) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 12)
    struct.get 12 6
    return
  )
  (func (;15;) (type 91) (param eqref)
    (local eqref eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    struct.new_default 13
    local.set 1
    local.get 1
    ref.cast (ref 13)
    global.get 37
    struct.set 13 0
    local.get 1
    call 33
    local.get 1
    local.set 4
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 5
    ref.cast (ref 12)
    local.set 5
    local.get 5
    ref.cast (ref 12)
    local.get 4
    struct.set 12 1
    local.get 0
    struct.new_default 14
    local.set 2
    local.get 2
    ref.cast (ref 14)
    global.get 56
    struct.set 14 0
    local.get 2
    call 34
    local.get 2
    local.set 4
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 5
    ref.cast (ref 12)
    local.set 5
    local.get 5
    ref.cast (ref 12)
    local.get 4
    struct.set 12 2
    local.get 0
    struct.new_default 37
    local.set 3
    local.get 3
    ref.cast (ref 37)
    global.get 63
    struct.set 37 0
    local.get 3
    call 35
    local.get 3
    local.set 4
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 5
    ref.cast (ref 12)
    local.set 5
    local.get 5
    ref.cast (ref 12)
    local.get 4
    struct.set 12 3
    local.get 0
    i32.const -1
    local.set 6
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 5
    ref.cast (ref 12)
    local.set 5
    local.get 5
    ref.cast (ref 12)
    local.get 6
    struct.set 12 7
    local.get 0
    drop
    return
  )
  (func (;16;) (type 92) (param i32 i64)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 10
        i32.const 0
        i32.add
        local.get 10
        i64.store align=1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;17;) (type 93) (param eqref i32 i32)
    (local i32 i32 i32 i32 eqref eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 17
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 80
    i32.add
    global.set 34
    local.get 17
    i32.const 16
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 32
    memory.fill
    global.get 35
    local.tee 14
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 4
    i32.add
    global.set 35
    local.get 17
    i32.const 32
    i32.add
    local.get 14
    i32.const 0
    i32.add
    i32.store align=1
    local.get 17
    i32.const 36
    i32.add
    local.get 14
    i32.const 1
    i32.add
    i32.store align=1
    local.get 17
    i32.const 40
    i32.add
    local.get 14
    i32.const 2
    i32.add
    i32.store align=1
    local.get 17
    i32.const 64
    i32.add
    local.get 14
    i32.const 3
    i32.add
    i32.store align=1
    local.get 17
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 17
    i32.const 0
    i32.add
    local.set 1
    local.get 17
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 17
    i32.const 8
    i32.add
    local.set 2
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 3
        local.get 1
        local.set 18
        local.get 17
        i32.const 48
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 48
        i32.add
        local.get 2
        local.set 18
        local.get 17
        i32.const 56
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 56
        i32.add
        ref.null eq
        ref.null eq
        local.get 0
        local.tee 23
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 23
        ref.cast (ref 16)
        local.set 23
        local.get 17
        i32.const 64
        i32.add
        local.get 23
        ref.cast (ref 16)
        struct.get 16 28
        local.set 13
        i32.load align=1
        local.get 13
        table.set 1
        local.get 17
        i32.const 64
        i32.add
        local.set 9
        local.set 8
        local.set 7
        local.set 6
        local.set 5
        local.set 4
        local.get 4
        local.get 5
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        call 36
        local.get 0
        local.get 3
        local.set 11
        local.set 10
        local.get 10
        local.get 11
        call 37
        local.get 14
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 14
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 14
        global.set 35
        local.get 17
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 14
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 14
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 14
    global.set 35
    local.get 17
    global.set 34
    local.get 12
    throw_ref
  )
  (func (;18;) (type 94) (param eqref eqref f64 f64)
    (local eqref i32 i32 i32 f32 i32 f32 f32 f32 f32 i32 i32 eqref eqref eqref i32 i32 eqref eqref eqref eqref i32 f32 eqref i32 eqref i32 i32 f32 f32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 41
    i32.const 1048256
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 41
    i32.const 320
    i32.add
    global.set 34
    local.get 41
    i32.const 0
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 12
    memory.fill
    local.get 41
    i32.const 12
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 68
    memory.fill
    local.get 41
    i32.const 80
    i32.add
    local.set 7
    local.get 7
    i32.const 0
    i32.const 16
    memory.fill
    struct.new_default 49
    local.set 4
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.set 50
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 14)
        local.set 47
        local.get 47
        ref.cast (ref 14)
        local.get 50
        struct.set 14 5
        local.get 0
        local.get 3
        local.set 50
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 14)
        local.set 47
        local.get 47
        ref.cast (ref 14)
        local.get 50
        struct.set 14 6
        call 38
        local.set 46
        local.get 4
        local.set 47
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 0
        struct.set 49 0
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 1
        struct.set 49 1
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 2
        struct.set 49 2
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 3
        struct.set 49 3
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 4
        struct.set 49 4
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 5
        struct.set 49 5
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 6
        struct.set 49 6
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 7
        struct.set 49 7
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 8
        struct.set 49 8
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 9
        struct.set 49 9
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 10
        struct.set 49 10
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 11
        struct.set 49 11
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 12
        struct.set 49 12
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 13
        struct.set 49 13
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 14
        struct.set 49 14
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 15
        struct.set 49 15
        local.get 47
        ref.cast (ref 49)
        local.get 46
        ref.cast (ref 49)
        struct.get 49 16
        struct.set 49 16
        local.get 41
        i32.const 96
        i32.add
        call 39
        local.set 42
        local.get 5
        local.get 42
        i32.const 12
        memory.copy
        i32.const 24
        f32.convert_i32_s
        f64.promote_f32
        i32.const 24
        f32.convert_i32_s
        f64.promote_f32
        local.get 2
        f32.demote_f64
        f64.promote_f32
        i32.const 48
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.sub
        f64.promote_f32
        local.get 3
        f32.demote_f64
        f64.promote_f32
        i32.const 48
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.sub
        f64.promote_f32
        f32.demote_f64
        local.set 13
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.set 10
        local.get 41
        i32.const 108
        i32.add
        local.tee 9
        i32.const 0
        i32.const 16
        memory.fill
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        call 40
        local.get 9
        local.get 4
        ref.cast (ref 49)
        local.set 46
        local.get 46
        ref.cast (ref 49)
        struct.get 49 0
        local.get 46
        ref.cast (ref 49)
        struct.get 49 1
        local.get 46
        ref.cast (ref 49)
        struct.get 49 2
        local.get 46
        ref.cast (ref 49)
        struct.get 49 3
        local.get 46
        ref.cast (ref 49)
        struct.get 49 4
        local.get 46
        ref.cast (ref 49)
        struct.get 49 5
        local.get 46
        ref.cast (ref 49)
        struct.get 49 6
        local.get 46
        ref.cast (ref 49)
        struct.get 49 7
        local.get 46
        ref.cast (ref 49)
        struct.get 49 8
        local.get 46
        ref.cast (ref 49)
        struct.get 49 9
        local.get 46
        ref.cast (ref 49)
        struct.get 49 10
        local.get 46
        ref.cast (ref 49)
        struct.get 49 11
        local.get 46
        ref.cast (ref 49)
        struct.get 49 12
        local.get 46
        ref.cast (ref 49)
        struct.get 49 13
        local.get 46
        ref.cast (ref 49)
        struct.get 49 14
        local.get 46
        ref.cast (ref 49)
        struct.get 49 15
        local.get 46
        ref.cast (ref 49)
        struct.get 49 16
        struct.new 49
        local.get 5
        local.set 42
        local.get 41
        i32.const 124
        i32.add
        local.get 42
        i32.const 12
        memory.copy
        local.get 41
        i32.const 124
        i32.add
        local.get 41
        i32.const 136
        i32.add
        call 41
        local.set 42
        local.get 6
        local.get 42
        i32.const 68
        memory.copy
        local.get 0
        local.get 6
        local.set 14
        local.get 14
        local.get 41
        i32.const 204
        i32.add
        call 42
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.set 50
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 14)
        local.set 47
        local.get 47
        ref.cast (ref 14)
        local.get 50
        struct.set 14 2
        local.get 0
        local.get 6
        local.set 14
        local.get 14
        local.get 41
        i32.const 220
        i32.add
        call 42
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.set 50
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 14)
        local.set 47
        local.get 47
        ref.cast (ref 14)
        local.get 50
        struct.set 14 3
        local.get 0
        local.get 6
        local.set 15
        local.get 15
        call 43
        f64.promote_f32
        local.set 50
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 14)
        local.set 47
        local.get 47
        ref.cast (ref 14)
        local.get 50
        struct.set 14 4
        local.get 0
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 14)
        struct.get 14 1
        local.get 1
        local.get 4
        ref.cast (ref 49)
        local.set 46
        local.get 46
        ref.cast (ref 49)
        struct.get 49 0
        local.get 46
        ref.cast (ref 49)
        struct.get 49 1
        local.get 46
        ref.cast (ref 49)
        struct.get 49 2
        local.get 46
        ref.cast (ref 49)
        struct.get 49 3
        local.get 46
        ref.cast (ref 49)
        struct.get 49 4
        local.get 46
        ref.cast (ref 49)
        struct.get 49 5
        local.get 46
        ref.cast (ref 49)
        struct.get 49 6
        local.get 46
        ref.cast (ref 49)
        struct.get 49 7
        local.get 46
        ref.cast (ref 49)
        struct.get 49 8
        local.get 46
        ref.cast (ref 49)
        struct.get 49 9
        local.get 46
        ref.cast (ref 49)
        struct.get 49 10
        local.get 46
        ref.cast (ref 49)
        struct.get 49 11
        local.get 46
        ref.cast (ref 49)
        struct.get 49 12
        local.get 46
        ref.cast (ref 49)
        struct.get 49 13
        local.get 46
        ref.cast (ref 49)
        struct.get 49 14
        local.get 46
        ref.cast (ref 49)
        struct.get 49 15
        local.get 46
        ref.cast (ref 49)
        struct.get 49 16
        struct.new 49
        local.get 5
        local.set 42
        local.get 41
        i32.const 236
        i32.add
        local.get 42
        i32.const 12
        memory.copy
        local.get 41
        i32.const 236
        i32.add
        local.get 6
        local.set 20
        local.set 19
        local.set 18
        local.set 17
        local.set 16
        local.get 16
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 15)
        local.get 17
        local.get 18
        local.get 19
        local.get 20
        call 44
        local.get 1
        local.set 21
        local.get 21
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 10)
        local.get 21
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 0
        i32.add
        call_indirect (type 193)
        local.get 1
        local.set 22
        local.get 22
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 10)
        local.get 22
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 1
        i32.add
        call_indirect (type 195)
        local.get 1
        local.get 6
        local.set 14
        local.get 14
        local.get 41
        i32.const 248
        i32.add
        call 42
        local.get 4
        local.set 23
        local.get 23
        call 45
        f64.promote_f32
        local.get 6
        local.set 15
        local.get 15
        call 43
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 26
        local.set 25
        local.set 24
        local.get 24
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 10)
        local.get 25
        local.get 26
        local.get 24
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 2
        i32.add
        call_indirect (type 197)
        local.get 1
        local.set 27
        local.get 27
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 10)
        local.get 27
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 3
        i32.add
        call_indirect (type 199)
        local.get 1
        local.get 6
        local.set 14
        local.get 14
        local.get 41
        i32.const 264
        i32.add
        call 42
        local.set 42
        local.get 7
        local.get 42
        i32.const 16
        memory.copy
        local.get 7
        local.set 28
        local.get 28
        local.get 41
        i32.const 280
        i32.add
        call 46
        local.get 41
        i32.const 288
        i32.add
        call 47
        local.set 30
        local.set 29
        local.get 29
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 10)
        local.get 30
        local.get 29
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 4
        i32.add
        call_indirect (type 201)
        local.get 1
        local.get 6
        local.set 15
        local.get 15
        call 43
        f64.promote_f32
        local.get 6
        local.set 15
        local.get 15
        call 43
        f64.promote_f32
        f32.demote_f64
        local.set 33
        f32.demote_f64
        local.set 32
        local.get 41
        i32.const 296
        i32.add
        local.tee 31
        i32.const 0
        i32.const 8
        memory.fill
        local.get 31
        local.get 32
        local.get 33
        call 19
        local.get 31
        local.get 41
        i32.const 304
        i32.add
        call 30
        local.set 35
        local.set 34
        local.get 34
        local.tee 47
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 47
        ref.cast (ref 10)
        local.get 35
        local.get 34
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 5
        i32.add
        call_indirect (type 203)
        local.get 41
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 36
    local.get 41
    global.set 34
    local.get 36
    throw_ref
  )
  (func (;19;) (type 95) (param i32 f32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 12
        i32.const 0
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 12
        i32.const 4
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;20;) (type 96) (param i32 i32) (result i32)
    (local i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 32
    i32.add
    global.set 34
    local.get 10
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 10
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 11
        local.get 10
        i32.const 8
        i32.add
        local.get 11
        i32.const 8
        memory.copy
        local.get 10
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 11
        local.get 10
        i32.const 16
        i32.add
        local.get 11
        i32.const 8
        memory.copy
        local.get 10
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 10
        i32.const 24
        i32.add
        local.tee 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        call 48
        local.get 2
        local.set 11
        local.get 1
        local.get 11
        i32.const 8
        memory.copy
        local.get 1
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;21;) (type 97) (param eqref i32 eqref i32) (result i32)
    (local i32 i32 i32 i32 i32 i32 eqref eqref i32 i32 i32 i32 i32 eqref i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 26
    i32.const 1048432
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 26
    i32.const 144
    i32.add
    global.set 34
    local.get 26
    i32.const 8
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 32
    memory.fill
    local.get 26
    i32.const 40
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 12
    memory.fill
    local.get 26
    i32.const 52
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 24
    memory.fill
    global.get 35
    local.tee 23
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 23
    i32.const 4
    i32.add
    global.set 35
    local.get 26
    i32.const 24
    i32.add
    local.get 23
    i32.const 0
    i32.add
    i32.store align=1
    local.get 26
    i32.const 28
    i32.add
    local.get 23
    i32.const 1
    i32.add
    i32.store align=1
    local.get 26
    i32.const 32
    i32.add
    local.get 23
    i32.const 2
    i32.add
    i32.store align=1
    local.get 26
    i32.const 96
    i32.add
    local.get 23
    i32.const 3
    i32.add
    i32.store align=1
    local.get 26
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 26
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 4
        local.get 26
        i32.const 80
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 26
        i32.const 80
        i32.add
        local.get 26
        i32.const 88
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 26
        i32.const 88
        i32.add
        local.get 2
        ref.null eq
        local.get 0
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 16)
        local.set 32
        local.get 26
        i32.const 96
        i32.add
        local.get 32
        ref.cast (ref 16)
        struct.get 16 28
        local.set 22
        i32.load align=1
        local.get 22
        table.set 1
        local.get 26
        i32.const 96
        i32.add
        local.set 12
        local.set 11
        local.set 10
        local.set 9
        local.set 8
        local.set 7
        local.get 7
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        call 36
        local.get 5
        local.get 1
        local.set 27
        local.get 26
        i32.const 100
        i32.add
        local.get 27
        i32.const 8
        memory.copy
        local.get 26
        i32.const 100
        i32.add
        i32.const 1
        i32.const 1
        i32.const 255
        i32.and
        local.set 16
        i32.const 255
        i32.and
        local.set 15
        local.set 14
        local.set 13
        local.get 13
        local.get 14
        local.get 15
        local.get 16
        call 49
        local.get 6
        i32.const 0
        i32.const 24
        memory.fill
        local.get 0
        local.get 4
        local.get 5
        local.get 6
        local.set 20
        local.set 19
        local.set 18
        local.set 17
        local.get 17
        local.get 18
        local.get 19
        local.get 20
        call 50
        local.get 6
        local.set 27
        local.get 26
        i32.const 108
        i32.add
        local.get 27
        i32.const 24
        memory.copy
        local.get 26
        i32.const 108
        i32.add
        i32.const 0
        i32.add
        local.set 27
        local.get 26
        i32.const 132
        i32.add
        local.get 27
        i32.const 8
        memory.copy
        local.get 26
        i32.const 132
        i32.add
        local.set 27
        local.get 3
        local.get 27
        i32.const 8
        memory.copy
        local.get 3
        local.get 23
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 23
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 23
        global.set 35
        local.get 26
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 21
    local.get 23
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 23
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 23
    global.set 35
    local.get 26
    global.set 34
    local.get 21
    throw_ref
  )
  (func (;22;) (type 98) (param i32 f32 f32 f32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 14
        i32.const 0
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 14
        i32.const 4
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 3
        f64.promote_f32
        local.set 14
        i32.const 8
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 4
        f64.promote_f32
        local.set 14
        i32.const 12
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;23;) (type 99) (param i32 i32) (result i32)
    (local i32 f32 f32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 12
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 96
    i32.add
    global.set 34
    local.get 12
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 13
        local.get 12
        i32.const 16
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 13
        local.get 12
        i32.const 32
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 13
        local.get 12
        i32.const 48
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 13
        local.get 12
        i32.const 64
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 12
        i32.const 80
        i32.add
        local.tee 2
        i32.const 0
        i32.const 16
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        local.get 5
        local.get 6
        call 40
        local.get 2
        local.set 13
        local.get 1
        local.get 13
        i32.const 16
        memory.copy
        local.get 1
        local.get 12
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 34
    local.get 7
    throw_ref
  )
  (func (;24;) (type 100) (param i32 i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 8
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 8
        i32.add
        local.set 9
        i32.const 4
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 0
        i32.const 1
        local.set 9
        i32.const 0
        i32.add
        local.get 9
        i32.store8
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;25;) (type 101) (param eqref i32 eqref i32 i32) (result i32)
    (local i32 i32 i32 i32 i32 i32 i32 i32 eqref eqref i32 i32 i32 eqref i32 eqref i32 i32 i32 i32 i32 i32 i32 i32 i32 i32 eqref i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 40
    i32.const 1048320
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 40
    i32.const 256
    i32.add
    global.set 34
    local.get 40
    i32.const 32
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 32
    memory.fill
    local.get 40
    i32.const 64
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 8
    memory.fill
    local.get 40
    i32.const 72
    i32.add
    local.set 7
    local.get 7
    i32.const 0
    i32.const 28
    memory.fill
    local.get 40
    i32.const 100
    i32.add
    local.set 8
    local.get 8
    i32.const 0
    i32.const 24
    memory.fill
    local.get 40
    i32.const 124
    i32.add
    local.set 9
    local.get 9
    i32.const 0
    i32.const 12
    memory.fill
    global.get 35
    local.tee 37
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 37
    i32.const 4
    i32.add
    global.set 35
    local.get 40
    i32.const 48
    i32.add
    local.get 37
    i32.const 0
    i32.add
    i32.store align=1
    local.get 40
    i32.const 52
    i32.add
    local.get 37
    i32.const 1
    i32.add
    i32.store align=1
    local.get 40
    i32.const 56
    i32.add
    local.get 37
    i32.const 2
    i32.add
    i32.store align=1
    local.get 40
    i32.const 152
    i32.add
    local.get 37
    i32.const 3
    i32.add
    i32.store align=1
    local.get 40
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 40
    i32.const 0
    i32.add
    local.set 1
    local.get 40
    i32.const 16
    i32.add
    local.get 3
    i32.const 12
    memory.copy
    local.get 40
    i32.const 16
    i32.add
    local.set 3
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 5
        local.get 40
        i32.const 136
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 40
        i32.const 136
        i32.add
        local.get 40
        i32.const 144
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 40
        i32.const 144
        i32.add
        local.get 2
        ref.null eq
        local.get 0
        local.tee 46
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 46
        ref.cast (ref 16)
        local.set 46
        local.get 40
        i32.const 152
        i32.add
        local.get 46
        ref.cast (ref 16)
        struct.get 16 28
        local.set 36
        i32.load align=1
        local.get 36
        table.set 1
        local.get 40
        i32.const 152
        i32.add
        local.set 15
        local.set 14
        local.set 13
        local.set 12
        local.set 11
        local.set 10
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        call 36
        local.get 3
        local.set 41
        local.get 40
        i32.const 156
        i32.add
        local.get 41
        i32.const 12
        memory.copy
        local.get 40
        i32.const 156
        i32.add
        local.set 41
        local.get 9
        local.get 41
        i32.const 12
        memory.copy
        local.get 9
        local.set 16
        local.get 16
        call 51
        if (type 102) (result i32) ;; label = @3
          local.get 9
          local.set 21
          local.get 21
          local.get 40
          i32.const 184
          i32.add
          call 53
        else
          local.get 0
          local.get 1
          local.set 17
          local.get 17
          local.get 40
          i32.const 168
          i32.add
          call 52
          local.get 2
          local.set 20
          local.set 19
          local.set 18
          local.get 18
          local.get 19
          local.get 20
          local.get 40
          i32.const 176
          i32.add
          call 21
        end
        local.set 41
        local.get 6
        local.get 41
        i32.const 8
        memory.copy
        local.get 7
        local.get 1
        local.set 17
        local.get 17
        local.get 40
        i32.const 192
        i32.add
        call 52
        local.get 6
        local.set 41
        local.get 40
        i32.const 200
        i32.add
        local.get 41
        i32.const 8
        memory.copy
        local.get 40
        i32.const 200
        i32.add
        local.get 1
        local.set 22
        local.get 22
        local.get 40
        i32.const 208
        i32.add
        call 46
        i32.const 0
        i32.const 0
        i32.const 0
        i32.const 0
        i32.const 255
        i32.and
        local.set 30
        i32.const 255
        i32.and
        local.set 29
        i32.const 255
        i32.and
        local.set 28
        i32.const 255
        i32.and
        local.set 27
        local.set 26
        local.set 25
        local.set 24
        local.set 23
        local.get 23
        local.get 24
        local.get 25
        local.get 26
        local.get 27
        local.get 28
        local.get 29
        local.get 30
        call 54
        local.get 8
        i32.const 0
        i32.const 24
        memory.fill
        local.get 0
        local.get 5
        local.get 7
        local.get 8
        local.set 34
        local.set 33
        local.set 32
        local.set 31
        local.get 31
        local.get 32
        local.get 33
        local.get 34
        call 55
        local.get 8
        local.set 41
        local.get 40
        i32.const 216
        i32.add
        local.get 41
        i32.const 24
        memory.copy
        local.get 40
        i32.const 216
        i32.add
        i32.const 8
        i32.add
        local.set 41
        local.get 40
        i32.const 240
        i32.add
        local.get 41
        i32.const 16
        memory.copy
        local.get 40
        i32.const 240
        i32.add
        local.set 41
        local.get 4
        local.get 41
        i32.const 16
        memory.copy
        local.get 4
        local.get 37
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 37
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 37
        global.set 35
        local.get 40
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 35
    local.get 37
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 37
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 37
    global.set 35
    local.get 40
    global.set 34
    local.get 35
    throw_ref
  )
  (func (;26;) (type 103) (param eqref eqref)
    (local i32 i32 i32 i32 i32 eqref eqref i32 eqref i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 18
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 18
    i32.const 96
    i32.add
    global.set 34
    local.get 18
    i32.const 0
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 32
    memory.fill
    local.get 18
    i32.const 32
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 20
    memory.fill
    global.get 35
    local.tee 15
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 4
    i32.add
    global.set 35
    local.get 18
    i32.const 16
    i32.add
    local.get 15
    i32.const 0
    i32.add
    i32.store align=1
    local.get 18
    i32.const 20
    i32.add
    local.get 15
    i32.const 1
    i32.add
    i32.store align=1
    local.get 18
    i32.const 24
    i32.add
    local.get 15
    i32.const 2
    i32.add
    i32.store align=1
    local.get 18
    i32.const 72
    i32.add
    local.get 15
    i32.const 3
    i32.add
    i32.store align=1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 2
        local.get 18
        i32.const 56
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 18
        i32.const 56
        i32.add
        local.get 18
        i32.const 64
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 18
        i32.const 64
        i32.add
        local.get 1
        local.get 1
        local.get 0
        local.tee 24
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 24
        ref.cast (ref 16)
        local.set 24
        local.get 18
        i32.const 72
        i32.add
        local.get 24
        ref.cast (ref 16)
        struct.get 16 28
        local.set 14
        i32.load align=1
        local.get 14
        table.set 1
        local.get 18
        i32.const 72
        i32.add
        local.set 9
        local.set 8
        local.set 7
        local.set 6
        local.set 5
        local.set 4
        local.get 4
        local.get 5
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        call 36
        local.get 0
        local.get 2
        local.get 3
        i32.const 0
        i32.const 20
        memory.fill
        local.get 3
        local.set 19
        local.get 18
        i32.const 76
        i32.add
        local.get 19
        i32.const 20
        memory.copy
        local.get 18
        i32.const 76
        i32.add
        local.set 12
        local.set 11
        local.set 10
        local.get 10
        local.get 11
        local.get 12
        call 56
        local.get 15
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 15
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 15
        global.set 35
        local.get 18
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 13
    local.get 15
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 15
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 15
    global.set 35
    local.get 18
    global.set 34
    local.get 13
    throw_ref
  )
  (func (;27;) (type 104) (param eqref eqref)
    (local eqref i32 f32 eqref i32 f32 f32 f32 f32 eqref eqref eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 20
    i32.const 1048400
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 20
    i32.const 176
    i32.add
    global.set 34
    local.get 20
    i32.const 0
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 68
    memory.fill
    struct.new_default 49
    local.set 2
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        local.set 5
        local.get 5
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 5
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 6
        i32.add
        call_indirect (type 266)
        call 38
        local.set 25
        local.get 2
        local.set 26
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 0
        struct.set 49 0
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 1
        struct.set 49 1
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 2
        struct.set 49 2
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 3
        struct.set 49 3
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 4
        struct.set 49 4
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 5
        struct.set 49 5
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 6
        struct.set 49 6
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 7
        struct.set 49 7
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 8
        struct.set 49 8
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 9
        struct.set 49 9
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 10
        struct.set 49 10
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 11
        struct.set 49 11
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 12
        struct.set 49 12
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 13
        struct.set 49 13
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 14
        struct.set 49 14
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 15
        struct.set 49 15
        local.get 26
        ref.cast (ref 49)
        local.get 25
        ref.cast (ref 49)
        struct.get 49 16
        struct.set 49 16
        i32.const 24
        f32.convert_i32_s
        f64.promote_f32
        i32.const 24
        f32.convert_i32_s
        f64.promote_f32
        local.get 0
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 14)
        struct.get 14 5
        f32.demote_f64
        f64.promote_f32
        i32.const 48
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.sub
        f64.promote_f32
        local.get 0
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 14)
        struct.get 14 6
        f32.demote_f64
        f64.promote_f32
        i32.const 48
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.sub
        f64.promote_f32
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        local.get 20
        i32.const 68
        i32.add
        local.tee 6
        i32.const 0
        i32.const 16
        memory.fill
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        local.get 10
        call 40
        local.get 6
        local.get 2
        ref.cast (ref 49)
        local.set 25
        local.get 25
        ref.cast (ref 49)
        struct.get 49 0
        local.get 25
        ref.cast (ref 49)
        struct.get 49 1
        local.get 25
        ref.cast (ref 49)
        struct.get 49 2
        local.get 25
        ref.cast (ref 49)
        struct.get 49 3
        local.get 25
        ref.cast (ref 49)
        struct.get 49 4
        local.get 25
        ref.cast (ref 49)
        struct.get 49 5
        local.get 25
        ref.cast (ref 49)
        struct.get 49 6
        local.get 25
        ref.cast (ref 49)
        struct.get 49 7
        local.get 25
        ref.cast (ref 49)
        struct.get 49 8
        local.get 25
        ref.cast (ref 49)
        struct.get 49 9
        local.get 25
        ref.cast (ref 49)
        struct.get 49 10
        local.get 25
        ref.cast (ref 49)
        struct.get 49 11
        local.get 25
        ref.cast (ref 49)
        struct.get 49 12
        local.get 25
        ref.cast (ref 49)
        struct.get 49 13
        local.get 25
        ref.cast (ref 49)
        struct.get 49 14
        local.get 25
        ref.cast (ref 49)
        struct.get 49 15
        local.get 25
        ref.cast (ref 49)
        struct.get 49 16
        struct.new 49
        local.get 20
        i32.const 84
        i32.add
        call 39
        local.get 20
        i32.const 96
        i32.add
        call 41
        local.set 21
        local.get 3
        local.get 21
        i32.const 68
        memory.copy
        local.get 0
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 14)
        struct.get 14 1
        local.get 1
        local.get 2
        ref.cast (ref 49)
        local.set 25
        local.get 25
        ref.cast (ref 49)
        struct.get 49 0
        local.get 25
        ref.cast (ref 49)
        struct.get 49 1
        local.get 25
        ref.cast (ref 49)
        struct.get 49 2
        local.get 25
        ref.cast (ref 49)
        struct.get 49 3
        local.get 25
        ref.cast (ref 49)
        struct.get 49 4
        local.get 25
        ref.cast (ref 49)
        struct.get 49 5
        local.get 25
        ref.cast (ref 49)
        struct.get 49 6
        local.get 25
        ref.cast (ref 49)
        struct.get 49 7
        local.get 25
        ref.cast (ref 49)
        struct.get 49 8
        local.get 25
        ref.cast (ref 49)
        struct.get 49 9
        local.get 25
        ref.cast (ref 49)
        struct.get 49 10
        local.get 25
        ref.cast (ref 49)
        struct.get 49 11
        local.get 25
        ref.cast (ref 49)
        struct.get 49 12
        local.get 25
        ref.cast (ref 49)
        struct.get 49 13
        local.get 25
        ref.cast (ref 49)
        struct.get 49 14
        local.get 25
        ref.cast (ref 49)
        struct.get 49 15
        local.get 25
        ref.cast (ref 49)
        struct.get 49 16
        struct.new 49
        local.get 3
        local.set 14
        local.set 13
        local.set 12
        local.set 11
        local.get 11
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 15)
        local.get 12
        local.get 13
        local.get 14
        call 57
        local.get 20
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 15
    local.get 20
    global.set 34
    local.get 15
    throw_ref
  )
  (func (;28;) (type 105) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 29
    return
  )
  (func (;29;) (type 106) (param i32 f32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 12
        i32.const 0
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 12
        i32.const 4
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;30;) (type 107) (param i32 i32) (result i32)
    (local i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 32
    i32.add
    global.set 34
    local.get 10
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 10
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 11
        local.get 10
        i32.const 8
        i32.add
        local.get 11
        i32.const 8
        memory.copy
        local.get 10
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 11
        local.get 10
        i32.const 16
        i32.add
        local.get 11
        i32.const 8
        memory.copy
        local.get 10
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 10
        i32.const 24
        i32.add
        local.tee 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        call 58
        local.get 2
        local.set 11
        local.get 1
        local.get 11
        i32.const 8
        memory.copy
        local.get 1
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;31;) (type 108) (param i32 i32 i32 f32 f32 i32 f32 f32 f32 i32 i32 i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 17
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 48
    i32.add
    global.set 34
    local.get 17
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 17
    i32.const 0
    i32.add
    local.set 1
    local.get 17
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 17
    i32.const 8
    i32.add
    local.set 2
    local.get 17
    i32.const 16
    i32.add
    local.get 5
    i32.const 8
    memory.copy
    local.get 17
    i32.const 16
    i32.add
    local.set 5
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 18
        local.get 17
        i32.const 24
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 24
        i32.add
        local.set 19
        i32.const 0
        i32.add
        local.get 19
        i32.const 8
        memory.copy
        local.get 0
        local.get 2
        local.set 18
        local.get 17
        i32.const 32
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 32
        i32.add
        local.set 19
        i32.const 8
        i32.add
        local.get 19
        i32.const 8
        memory.copy
        local.get 0
        local.get 3
        f64.promote_f32
        local.set 21
        i32.const 16
        i32.add
        local.get 21
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 4
        f64.promote_f32
        local.set 21
        i32.const 20
        i32.add
        local.get 21
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 5
        local.set 18
        local.get 17
        i32.const 40
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 40
        i32.add
        local.set 19
        i32.const 24
        i32.add
        local.get 19
        i32.const 8
        memory.copy
        local.get 0
        local.get 6
        f64.promote_f32
        local.set 21
        i32.const 32
        i32.add
        local.get 21
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 7
        f64.promote_f32
        local.set 21
        i32.const 36
        i32.add
        local.get 21
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 8
        f64.promote_f32
        local.set 21
        i32.const 40
        i32.add
        local.get 21
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 9
        local.set 19
        i32.const 44
        i32.add
        local.get 19
        i32.store align=1
        local.get 0
        local.get 10
        local.set 19
        i32.const 48
        i32.add
        local.get 19
        i32.store16 align=1
        local.get 0
        local.get 11
        local.set 19
        i32.const 50
        i32.add
        local.get 19
        i32.store16 align=1
        local.get 17
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 17
    global.set 34
    local.get 12
    throw_ref
  )
  (func (;32;) (type 109) (param i32 i32 i32 i64 i32 i32 i32 i32 eqref)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 14
    i32.const 1048432
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 144
    i32.add
    global.set 34
    local.get 14
    i32.const 0
    i32.add
    local.get 5
    i32.const 52
    memory.copy
    local.get 14
    i32.const 0
    i32.add
    local.set 5
    local.get 14
    i32.const 52
    i32.add
    local.get 6
    i32.const 8
    memory.copy
    local.get 14
    i32.const 52
    i32.add
    local.set 6
    local.get 14
    i32.const 60
    i32.add
    local.get 7
    i32.const 8
    memory.copy
    local.get 14
    i32.const 60
    i32.add
    local.set 7
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 0
        i32.const 0
        local.set 16
        i32.const 8
        i32.add
        local.get 16
        i32.store8
        local.get 0
        local.get 1
        local.set 16
        i32.const 12
        i32.add
        local.get 16
        i32.store align=1
        local.get 0
        local.get 2
        local.set 16
        i32.const 16
        i32.add
        local.get 16
        i32.store align=1
        local.get 0
        local.get 3
        local.set 17
        i32.const 24
        i32.add
        local.get 17
        i64.store align=1
        local.get 0
        local.get 4
        local.set 16
        i32.const 32
        i32.add
        local.get 16
        i32.store8
        local.get 0
        local.get 5
        local.set 15
        local.get 14
        i32.const 68
        i32.add
        local.get 15
        i32.const 52
        memory.copy
        local.get 14
        i32.const 68
        i32.add
        local.set 16
        i32.const 36
        i32.add
        local.get 16
        i32.const 52
        memory.copy
        local.get 0
        local.get 6
        local.set 15
        local.get 14
        i32.const 120
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 14
        i32.const 120
        i32.add
        local.set 16
        i32.const 88
        i32.add
        local.get 16
        i32.const 8
        memory.copy
        local.get 0
        local.get 7
        local.set 15
        local.get 14
        i32.const 128
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 14
        i32.const 128
        i32.add
        local.set 16
        i32.const 96
        i32.add
        local.get 16
        i32.const 8
        memory.copy
        local.get 0
        local.get 8
        local.set 19
        i32.const 104
        i32.add
        local.get 19
        local.set 10
        i32.load align=1
        local.get 10
        table.set 1
        local.get 14
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 14
    global.set 34
    local.get 9
    throw_ref
  )
  (func (;33;) (type 110) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;34;) (type 111) (param eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    struct.new_default 15
    local.set 1
    local.get 1
    ref.cast (ref 15)
    global.get 57
    struct.set 15 0
    local.get 1
    call 60
    local.get 1
    local.set 2
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 14)
    local.set 3
    local.get 3
    ref.cast (ref 14)
    local.get 2
    struct.set 14 1
    local.get 0
    drop
    return
  )
  (func (;35;) (type 112) (param eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    f64.const 0x1p+0 (;=1;)
    local.set 6
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 37)
    local.set 3
    local.get 3
    ref.cast (ref 37)
    local.get 6
    struct.set 37 57
    local.get 0
    local.set 1
    local.get 1
    call 61
    return
  )
  (func (;36;) (type 113) (param i32 i32 i32 eqref eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 11
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 48
    i32.add
    global.set 34
    global.get 35
    local.tee 8
    i32.const 65534
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 2
    i32.add
    global.set 35
    local.get 11
    i32.const 16
    i32.add
    local.get 8
    i32.const 0
    i32.add
    i32.store align=1
    local.get 11
    i32.const 40
    i32.add
    local.get 8
    i32.const 1
    i32.add
    i32.store align=1
    local.get 11
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 11
    i32.const 0
    i32.add
    local.set 1
    local.get 11
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 11
    i32.const 8
    i32.add
    local.set 2
    local.get 11
    i32.const 16
    i32.add
    local.get 5
    local.set 9
    local.set 10
    local.get 10
    i32.const 0
    i32.add
    local.get 9
    i32.const 0
    i32.add
    i32.load align=1
    table.get 1
    local.set 7
    i32.load align=1
    local.get 7
    table.set 1
    local.get 11
    i32.const 16
    i32.add
    local.set 5
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 12
        local.get 11
        i32.const 24
        i32.add
        local.get 12
        i32.const 8
        memory.copy
        local.get 11
        i32.const 24
        i32.add
        local.set 13
        i32.const 0
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 0
        local.get 2
        local.set 12
        local.get 11
        i32.const 32
        i32.add
        local.get 12
        i32.const 8
        memory.copy
        local.get 11
        i32.const 32
        i32.add
        local.set 13
        i32.const 8
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 0
        local.get 3
        local.set 16
        i32.const 16
        i32.add
        local.get 16
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 0
        local.get 4
        local.set 16
        i32.const 20
        i32.add
        local.get 16
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 0
        local.get 5
        local.set 12
        local.get 11
        i32.const 40
        i32.add
        local.get 12
        local.set 9
        local.set 10
        local.get 10
        i32.const 0
        i32.add
        local.get 9
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 11
        i32.const 40
        i32.add
        local.set 13
        i32.const 24
        i32.add
        local.get 13
        local.set 9
        local.set 10
        local.get 10
        i32.const 0
        i32.add
        local.get 9
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 8
        ref.null eq
        i32.const 2
        table.fill 1
        local.get 8
        ref.null extern
        i32.const 2
        table.fill 2
        local.get 8
        global.set 35
        local.get 11
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 8
    ref.null eq
    i32.const 2
    table.fill 1
    local.get 8
    ref.null extern
    i32.const 2
    table.fill 2
    local.get 8
    global.set 35
    local.get 11
    global.set 34
    local.get 6
    throw_ref
  )
  (func (;37;) (type 114) (param eqref i32)
    (local eqref i32 i32 eqref i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 13
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 2
        local.get 2
        call 63
        local.get 0
        local.get 1
        local.set 3
        local.get 3
        local.get 13
        i32.const 0
        i32.add
        call 64
        local.get 1
        local.set 4
        local.get 4
        local.get 13
        i32.const 8
        i32.add
        call 65
        local.set 7
        local.set 6
        local.set 5
        local.get 5
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 19
        ref.cast (ref 16)
        local.get 6
        local.get 7
        local.get 5
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 36
        i32.add
        call_indirect (type 276)
        local.get 13
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 34
    local.get 8
    throw_ref
  )
  (func (;38;) (type 115) (result eqref)
    (local eqref f32 eqref externref i32 f32 f32 eqref i32 i32 f32 f32 f32 f32 eqref i32 eqref i32 eqref i32 i32 f32 f32 f32 f32 eqref i32 eqref f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 34
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 34
    i32.const 64
    i32.add
    global.set 34
    struct.new_default 49
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        ref.cast (ref 49)
        local.set 40
        local.get 40
        ref.cast (ref 49)
        ref.null extern
        struct.set 49 0
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 1
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 2
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 3
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 4
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 5
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 6
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 7
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 8
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 9
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 10
        local.get 40
        ref.cast (ref 49)
        i32.const 0
        struct.set 49 11
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 12
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 13
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 14
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 15
        local.get 40
        ref.cast (ref 49)
        f32.const 0x0p+0 (;=0;)
        struct.set 49 16
        local.get 0
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 13
          array.new_data 0 0
          i32.const 0
          i32.const 13
          call 1
          global.set 0
        end
        global.get 0
        local.set 3
        local.set 2
        local.get 2
        local.get 3
        call 66
        local.get 0
        i32.const 393
        f32.convert_i32_s
        f64.promote_f32
        i32.const 852
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 34
        i32.const 0
        i32.add
        local.tee 4
        i32.const 0
        i32.const 8
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        call 48
        local.get 4
        local.set 8
        local.set 7
        local.get 7
        local.get 8
        call 67
        local.get 0
        i32.const 59
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const 34
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 13
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.set 10
        local.get 34
        i32.const 8
        i32.add
        local.tee 9
        i32.const 0
        i32.const 16
        memory.fill
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        call 68
        local.get 9
        local.set 15
        local.set 14
        local.get 14
        local.get 15
        call 69
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const 59
        f32.convert_i32_s
        f64.promote_f32
        i32.const 21
        f32.convert_i32_s
        f64.promote_f32
        i32.const 59
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 13
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.set 10
        local.get 34
        i32.const 24
        i32.add
        local.tee 9
        i32.const 0
        i32.const 16
        memory.fill
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        call 68
        local.get 9
        local.set 17
        local.set 16
        local.get 16
        local.get 17
        call 70
        local.get 0
        i32.const 3
        local.set 19
        local.set 18
        local.get 18
        local.get 19
        call 71
        local.get 0
        f32.const 0x1.0bp+7 (;=133.5;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        i32.const 11
        f32.convert_i32_s
        f64.promote_f32
        i32.const 126
        f32.convert_i32_s
        f64.promote_f32
        i32.const 36
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 24
        f32.demote_f64
        local.set 23
        f32.demote_f64
        local.set 22
        f32.demote_f64
        local.set 21
        local.get 34
        i32.const 40
        i32.add
        local.tee 20
        i32.const 0
        i32.const 16
        memory.fill
        local.get 20
        local.get 21
        local.get 22
        local.get 23
        local.get 24
        call 40
        local.get 20
        local.set 26
        local.set 25
        local.get 25
        local.get 26
        call 72
        local.get 0
        i32.const 44
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 28
        local.set 27
        local.get 27
        local.get 28
        call 73
        local.get 0
        ref.cast (ref 49)
        local.set 39
        local.get 39
        ref.cast (ref 49)
        struct.get 49 0
        local.get 39
        ref.cast (ref 49)
        struct.get 49 1
        local.get 39
        ref.cast (ref 49)
        struct.get 49 2
        local.get 39
        ref.cast (ref 49)
        struct.get 49 3
        local.get 39
        ref.cast (ref 49)
        struct.get 49 4
        local.get 39
        ref.cast (ref 49)
        struct.get 49 5
        local.get 39
        ref.cast (ref 49)
        struct.get 49 6
        local.get 39
        ref.cast (ref 49)
        struct.get 49 7
        local.get 39
        ref.cast (ref 49)
        struct.get 49 8
        local.get 39
        ref.cast (ref 49)
        struct.get 49 9
        local.get 39
        ref.cast (ref 49)
        struct.get 49 10
        local.get 39
        ref.cast (ref 49)
        struct.get 49 11
        local.get 39
        ref.cast (ref 49)
        struct.get 49 12
        local.get 39
        ref.cast (ref 49)
        struct.get 49 13
        local.get 39
        ref.cast (ref 49)
        struct.get 49 14
        local.get 39
        ref.cast (ref 49)
        struct.get 49 15
        local.get 39
        ref.cast (ref 49)
        struct.get 49 16
        struct.new 49
        local.get 34
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 29
    local.get 34
    global.set 34
    local.get 29
    throw_ref
  )
  (func (;39;) (type 116) (param i32) (result i32)
    (local f32 i32 f32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 11
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 4
        f32.convert_i32_s
        f64.promote_f32
        i32.const 4
        f32.convert_i32_s
        f64.promote_f32
        i32.const 8
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 11
        i32.const 0
        i32.add
        local.tee 2
        i32.const 0
        i32.const 12
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        local.get 5
        call 74
        local.get 2
        local.set 12
        local.get 0
        local.get 12
        i32.const 12
        memory.copy
        local.get 0
        local.get 11
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 34
    local.get 6
    throw_ref
  )
  (func (;40;) (type 117) (param i32 f32 f32 f32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 14
        i32.const 0
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 14
        i32.const 4
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 3
        f64.promote_f32
        local.set 14
        i32.const 8
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 4
        f64.promote_f32
        local.set 14
        i32.const 12
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;41;) (type 118) (param i32 eqref i32 i32) (result i32)
    (local f32 i32 f32 i32 i32 i32 i32 f32 i32 eqref i32 f32 f32 i32 f32 f32 f32 f32 i32 i32 f32 i32 i32 i32 f32 i32 i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 38
    i32.const 1048128
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 38
    i32.const 448
    i32.add
    global.set 34
    local.get 38
    i32.const 28
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 8
    memory.fill
    local.get 38
    i32.const 36
    i32.add
    local.set 7
    local.get 7
    i32.const 0
    i32.const 16
    memory.fill
    local.get 38
    i32.const 52
    i32.add
    local.set 8
    local.get 8
    i32.const 0
    i32.const 16
    memory.fill
    local.get 38
    i32.const 68
    i32.add
    local.set 9
    local.get 9
    i32.const 0
    i32.const 16
    memory.fill
    local.get 38
    i32.const 84
    i32.add
    local.set 10
    local.get 10
    i32.const 0
    i32.const 16
    memory.fill
    local.get 38
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 38
    i32.const 0
    i32.add
    local.set 0
    local.get 38
    i32.const 16
    i32.add
    local.get 2
    i32.const 12
    memory.copy
    local.get 38
    i32.const 16
    i32.add
    local.set 2
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 2
        local.set 12
        local.get 12
        call 75
        f64.promote_f32
        f32.demote_f64
        local.set 4
        local.get 5
        local.get 1
        local.set 13
        local.get 13
        local.get 38
        i32.const 100
        i32.add
        call 76
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 4
        f64.promote_f32
        i32.const 2
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.add
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 13
        local.get 38
        i32.const 108
        i32.add
        call 76
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 4
        f64.promote_f32
        i32.const 2
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.set 16
        f32.demote_f64
        local.set 15
        local.set 14
        local.get 14
        local.get 15
        local.get 16
        call 48
        local.get 0
        local.set 39
        local.get 38
        i32.const 116
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 116
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 5
        local.set 39
        local.get 38
        i32.const 132
        i32.add
        local.get 39
        i32.const 8
        memory.copy
        local.get 38
        i32.const 132
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.div
        f64.promote_f32
        local.get 0
        local.set 39
        local.get 38
        i32.const 140
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 140
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 5
        local.set 39
        local.get 38
        i32.const 156
        i32.add
        local.get 39
        i32.const 8
        memory.copy
        local.get 38
        i32.const 156
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.div
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.min
        f64.promote_f32
        f32.demote_f64
        local.set 6
        local.get 7
        local.get 0
        local.set 39
        local.get 38
        i32.const 164
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 164
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 39
        local.get 38
        i32.const 180
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 180
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 5
        local.set 39
        local.get 38
        i32.const 196
        i32.add
        local.get 39
        i32.const 8
        memory.copy
        local.get 38
        i32.const 196
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 6
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.sub
        f64.promote_f32
        f32.const 0x1p-1 (;=0.5;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.add
        f64.promote_f32
        local.get 0
        local.set 39
        local.get 38
        i32.const 204
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 204
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 39
        local.get 38
        i32.const 220
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 220
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 5
        local.set 39
        local.get 38
        i32.const 236
        i32.add
        local.get 39
        i32.const 8
        memory.copy
        local.get 38
        i32.const 236
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 6
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.sub
        f64.promote_f32
        f32.const 0x1p-1 (;=0.5;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.add
        f64.promote_f32
        local.get 5
        local.set 39
        local.get 38
        i32.const 244
        i32.add
        local.get 39
        i32.const 8
        memory.copy
        local.get 38
        i32.const 244
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 6
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        local.get 5
        local.set 39
        local.get 38
        i32.const 252
        i32.add
        local.get 39
        i32.const 8
        memory.copy
        local.get 38
        i32.const 252
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 6
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 21
        f32.demote_f64
        local.set 20
        f32.demote_f64
        local.set 19
        f32.demote_f64
        local.set 18
        local.set 17
        local.get 17
        local.get 18
        local.get 19
        local.get 20
        local.get 21
        call 40
        local.get 7
        local.get 2
        local.set 22
        local.get 22
        call 77
        f64.promote_f32
        f32.demote_f64
        f32.neg
        f64.promote_f32
        local.get 6
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 24
        local.set 23
        local.get 23
        local.get 24
        local.get 38
        i32.const 260
        i32.add
        call 78
        local.set 39
        local.get 8
        local.get 39
        i32.const 16
        memory.copy
        local.get 8
        local.get 2
        local.set 25
        local.get 25
        call 79
        f64.promote_f32
        f32.demote_f64
        f32.neg
        f64.promote_f32
        local.get 6
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 24
        local.set 23
        local.get 23
        local.get 24
        local.get 38
        i32.const 276
        i32.add
        call 78
        local.set 39
        local.get 9
        local.get 39
        i32.const 16
        memory.copy
        local.get 9
        local.get 2
        local.set 26
        local.get 26
        call 80
        f64.promote_f32
        f32.demote_f64
        f32.neg
        f64.promote_f32
        local.get 6
        f64.promote_f32
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.get 11
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 24
        local.set 23
        local.get 23
        local.get 24
        local.get 38
        i32.const 292
        i32.add
        call 78
        local.set 39
        local.get 10
        local.get 39
        i32.const 16
        memory.copy
        local.get 6
        f64.promote_f32
        local.get 7
        local.set 39
        local.get 38
        i32.const 308
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 308
        i32.add
        local.get 8
        local.set 39
        local.get 38
        i32.const 324
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 324
        i32.add
        local.get 9
        local.set 39
        local.get 38
        i32.const 340
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 340
        i32.add
        local.get 10
        local.set 39
        local.get 38
        i32.const 356
        i32.add
        local.get 39
        i32.const 16
        memory.copy
        local.get 38
        i32.const 356
        i32.add
        local.set 32
        local.set 31
        local.set 30
        local.set 29
        f32.demote_f64
        local.set 28
        local.get 38
        i32.const 372
        i32.add
        local.tee 27
        i32.const 0
        i32.const 68
        memory.fill
        local.get 27
        local.get 28
        local.get 29
        local.get 30
        local.get 31
        local.get 32
        call 81
        local.get 27
        local.set 39
        local.get 3
        local.get 39
        i32.const 68
        memory.copy
        local.get 3
        local.get 38
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 33
    local.get 38
    global.set 34
    local.get 33
    throw_ref
  )
  (func (;42;) (type 119) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 52
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;43;) (type 120) (param i32) (result f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;44;) (type 121) (param eqref eqref eqref i32 i32)
    (local f32 eqref i32 eqref i32 i32 i32 eqref i32 f32 eqref i32 eqref i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 26
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 26
    i32.const 128
    i32.add
    global.set 34
    local.get 26
    i32.const 0
    i32.add
    local.get 3
    i32.const 12
    memory.copy
    local.get 26
    i32.const 0
    i32.add
    local.set 3
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        local.get 3
        local.set 27
        local.get 26
        i32.const 12
        i32.add
        local.get 27
        i32.const 12
        memory.copy
        local.get 26
        i32.const 12
        i32.add
        local.get 4
        call 82
        local.get 1
        local.set 6
        local.get 6
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 6
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 1
        i32.add
        call_indirect (type 195)
        local.get 1
        local.get 4
        local.set 7
        local.get 7
        local.get 26
        i32.const 24
        i32.add
        call 83
        local.get 2
        local.set 8
        local.get 8
        call 45
        f64.promote_f32
        local.get 3
        local.set 9
        local.get 9
        call 80
        f64.promote_f32
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.get 5
        f32.add
        f64.promote_f32
        local.get 3
        local.set 10
        local.get 10
        call 79
        f64.promote_f32
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.get 5
        f32.add
        f64.promote_f32
        local.get 4
        local.set 11
        local.get 11
        call 43
        f64.promote_f32
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.get 5
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 14
        local.set 13
        local.set 12
        local.get 12
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 13
        local.get 14
        local.get 12
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 2
        i32.add
        call_indirect (type 197)
        local.get 1
        local.get 26
        i32.const 40
        i32.add
        call 84
        local.set 16
        local.set 15
        local.get 15
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 16
        local.get 15
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 1
        i32.const 0
        local.set 18
        local.set 17
        local.get 17
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 18
        local.get 17
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 8
        i32.add
        call_indirect (type 290)
        local.get 1
        local.set 6
        local.get 6
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 6
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 1
        i32.add
        call_indirect (type 195)
        local.get 1
        local.get 4
        local.set 19
        local.get 19
        local.get 26
        i32.const 56
        i32.add
        call 85
        local.get 2
        local.set 8
        local.get 8
        call 45
        f64.promote_f32
        local.get 3
        local.set 9
        local.get 9
        call 80
        f64.promote_f32
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.get 5
        f32.add
        f64.promote_f32
        local.get 4
        local.set 11
        local.get 11
        call 43
        f64.promote_f32
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.get 5
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 14
        local.set 13
        local.set 12
        local.get 12
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 13
        local.get 14
        local.get 12
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 2
        i32.add
        call_indirect (type 197)
        local.get 1
        local.get 26
        i32.const 72
        i32.add
        call 86
        local.set 16
        local.set 15
        local.get 15
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 16
        local.get 15
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 1
        i32.const 0
        local.set 18
        local.set 17
        local.get 17
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 18
        local.get 17
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 8
        i32.add
        call_indirect (type 290)
        local.get 1
        local.set 6
        local.get 6
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 6
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 1
        i32.add
        call_indirect (type 195)
        local.get 1
        local.get 4
        local.set 20
        local.get 20
        local.get 26
        i32.const 88
        i32.add
        call 42
        local.get 2
        local.set 8
        local.get 8
        call 45
        f64.promote_f32
        local.get 4
        local.set 11
        local.get 11
        call 43
        f64.promote_f32
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.get 5
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 14
        local.set 13
        local.set 12
        local.get 12
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 13
        local.get 14
        local.get 12
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 2
        i32.add
        call_indirect (type 197)
        local.get 1
        i32.const -1
        local.get 26
        i32.const 104
        i32.add
        call 87
        local.set 16
        local.set 15
        local.get 15
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 16
        local.get 15
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 1
        i32.const 0
        local.set 18
        local.set 17
        local.get 17
        local.tee 32
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 32
        ref.cast (ref 10)
        local.get 18
        local.get 17
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 8
        i32.add
        call_indirect (type 290)
        local.get 26
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 21
    local.get 26
    global.set 34
    local.get 21
    throw_ref
  )
  (func (;45;) (type 122) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 49)
    struct.get 49 16
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;46;) (type 123) (param i32 i32) (result i32)
    (local i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 10
        i32.const 0
        i32.add
        local.tee 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        call 29
        local.get 2
        local.set 11
        local.get 1
        local.get 11
        i32.const 8
        memory.copy
        local.get 1
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;47;) (type 124) (param i32 i32) (result i32)
    (local i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 32
    i32.add
    global.set 34
    local.get 10
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 10
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 11
        local.get 10
        i32.const 8
        i32.add
        local.get 11
        i32.const 8
        memory.copy
        local.get 10
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 11
        local.get 10
        i32.const 16
        i32.add
        local.get 11
        i32.const 8
        memory.copy
        local.get 10
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 10
        i32.const 24
        i32.add
        local.tee 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        call 58
        local.get 2
        local.set 11
        local.get 1
        local.get 11
        i32.const 8
        memory.copy
        local.get 1
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;48;) (type 125) (param i32 f32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 12
        i32.const 0
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 12
        i32.const 4
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;49;) (type 126) (param i32 i32 i32 i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 9
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 9
    i32.const 16
    i32.add
    global.set 34
    local.get 9
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 9
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 10
        local.get 9
        i32.const 8
        i32.add
        local.get 10
        i32.const 8
        memory.copy
        local.get 9
        i32.const 8
        i32.add
        local.set 11
        i32.const 0
        i32.add
        local.get 11
        i32.const 8
        memory.copy
        local.get 0
        local.get 2
        local.set 11
        i32.const 8
        i32.add
        local.get 11
        i32.store8
        local.get 0
        local.get 3
        local.set 11
        i32.const 9
        i32.add
        local.get 11
        i32.store8
        local.get 9
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 4
    local.get 9
    global.set 34
    local.get 4
    throw_ref
  )
  (func (;50;) (type 127) (param eqref i32 i32 i32)
    (local i32 i32 i32 i32 f32 f32 f32 eqref i32 i32 i32 f32 f32 i32 i32 eqref i32 eqref eqref i32 eqref i32 eqref eqref eqref eqref eqref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 37
    i32.const 1048336
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 37
    i32.const 240
    i32.add
    global.set 34
    local.get 37
    i32.const 0
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 8
    memory.fill
    local.get 37
    i32.const 8
    i32.add
    local.set 7
    local.get 7
    i32.const 0
    i32.const 8
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.get 2
        local.set 13
        local.set 12
        local.set 11
        local.get 11
        local.get 12
        local.get 13
        call 94
        i32.eqz
        if (type 128) ;; label = @3
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 16
          f32.demote_f64
          local.set 15
          local.get 37
          i32.const 24
          i32.add
          local.tee 14
          i32.const 0
          i32.const 8
          memory.fill
          local.get 14
          local.get 15
          local.get 16
          call 19
          local.get 14
          local.get 37
          i32.const 32
          i32.add
          call 20
          local.get 2
          local.set 17
          local.get 17
          local.get 37
          i32.const 40
          i32.add
          call 95
          local.get 37
          i32.const 48
          i32.add
          call 96
          local.set 38
          local.get 4
          local.get 38
          i32.const 8
          memory.copy
          local.get 2
          local.set 18
          local.get 18
          call 97
          if (type 129) (result i32) ;; label = @4
            i32.const 0
          else
            local.get 2
            local.set 17
            local.get 17
            local.get 37
            i32.const 56
            i32.add
            call 95
            i32.const 0
            i32.add
            f32.load align=1
            f64.promote_f32
            f32.demote_f64
            f32.abs
            f32.const inf (;=inf;)
            f32.lt
            i32.eqz
            if (type 130) (result i32) ;; label = @5
              i32.const 0
            else
              local.get 0
              local.set 19
              local.get 19
              call 98
              i32.const 0
              i32.eq
            end
          end
          i32.const 255
          i32.and
          local.set 5
          local.get 2
          local.set 20
          local.get 20
          call 99
          if (type 131) (result i32) ;; label = @4
            i32.const 0
          else
            local.get 2
            local.set 17
            local.get 17
            local.get 37
            i32.const 64
            i32.add
            call 95
            i32.const 4
            i32.add
            f32.load align=1
            f64.promote_f32
            f32.demote_f64
            f32.abs
            f32.const inf (;=inf;)
            f32.lt
            i32.eqz
            if (type 132) (result i32) ;; label = @5
              i32.const 0
            else
              local.get 0
              local.set 21
              local.get 21
              call 100
              i32.const 0
              i32.eq
            end
          end
          i32.const 255
          i32.and
          local.set 6
          local.get 5
          local.get 6
          i32.and
          i32.eqz
          if (type 133) ;; label = @4
            local.get 0
            local.get 4
            local.set 38
            local.get 37
            i32.const 104
            i32.add
            local.get 38
            i32.const 8
            memory.copy
            local.get 37
            i32.const 104
            i32.add
            local.get 0
            local.set 22
            local.get 22
            local.get 37
            i32.const 112
            i32.add
            call 101
            local.get 37
            i32.const 128
            i32.add
            call 102
            local.get 1
            local.set 23
            local.get 23
            call 103
            local.set 26
            local.set 25
            local.set 24
            local.get 24
            local.tee 43
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 43
            ref.cast (ref 16)
            local.get 25
            local.get 26
            local.get 37
            i32.const 136
            i32.add
            local.get 24
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 37
            i32.add
            call_indirect (type 294)
            local.set 38
            local.get 7
            local.get 38
            i32.const 8
            memory.copy
            local.get 5
            i32.eqz
            if (type 134) ;; label = @5
            else
              local.get 7
              local.get 2
              local.set 17
              local.get 17
              local.get 37
              i32.const 144
              i32.add
              call 95
              i32.const 0
              i32.add
              f32.load align=1
              f64.promote_f32
              local.set 41
              i32.const 0
              i32.add
              local.get 41
              f32.demote_f64
              f32.store align=1
            end
            local.get 6
            i32.eqz
            if (type 135) ;; label = @5
            else
              local.get 7
              local.get 2
              local.set 17
              local.get 17
              local.get 37
              i32.const 152
              i32.add
              call 95
              i32.const 4
              i32.add
              f32.load align=1
              f64.promote_f32
              local.set 41
              i32.const 4
              i32.add
              local.get 41
              f32.demote_f64
              f32.store align=1
            end
          else
            local.get 4
            local.set 38
            local.get 37
            i32.const 72
            i32.add
            local.get 38
            i32.const 8
            memory.copy
            local.get 37
            i32.const 72
            i32.add
            local.get 0
            local.set 22
            local.get 22
            local.get 37
            i32.const 80
            i32.add
            call 101
            local.get 37
            i32.const 96
            i32.add
            call 102
            local.set 38
            local.get 7
            local.get 38
            i32.const 8
            memory.copy
          end
          local.get 7
          local.set 38
          local.get 37
          i32.const 160
          i32.add
          local.get 38
          i32.const 8
          memory.copy
          local.get 37
          i32.const 160
          i32.add
          i32.const 0
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 0
          local.set 27
          local.get 27
          call 104
          f64.promote_f32
          local.get 0
          local.set 28
          local.get 28
          call 105
          f64.promote_f32
          f32.demote_f64
          local.set 10
          f32.demote_f64
          local.set 9
          f32.demote_f64
          local.set 8
          local.get 9
          local.get 10
          f32.gt
          if ;; label = @4
            i32.const 6
            global.get 32
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 46
              array.new_data 0 32
              i32.const 0
              i32.const 46
              call 1
              global.set 32
            end
            global.get 32
            struct.new 3
            throw 0
          end
          local.get 8
          local.get 9
          f32.max
          local.get 10
          f32.min
          f64.promote_f32
          local.get 7
          local.set 38
          local.get 37
          i32.const 168
          i32.add
          local.get 38
          i32.const 8
          memory.copy
          local.get 37
          i32.const 168
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 0
          local.set 29
          local.get 29
          call 106
          f64.promote_f32
          local.get 0
          local.set 30
          local.get 30
          call 107
          f64.promote_f32
          f32.demote_f64
          local.set 10
          f32.demote_f64
          local.set 9
          f32.demote_f64
          local.set 8
          local.get 9
          local.get 10
          f32.gt
          if ;; label = @4
            i32.const 6
            global.get 32
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 46
              array.new_data 0 32
              i32.const 0
              i32.const 46
              call 1
              global.set 32
            end
            global.get 32
            struct.new 3
            throw 0
          end
          local.get 8
          local.get 9
          f32.max
          local.get 10
          f32.min
          f64.promote_f32
          f32.demote_f64
          local.set 16
          f32.demote_f64
          local.set 15
          local.get 37
          i32.const 176
          i32.add
          local.tee 14
          i32.const 0
          i32.const 8
          memory.fill
          local.get 14
          local.get 15
          local.get 16
          call 19
          local.get 14
          local.get 37
          i32.const 184
          i32.add
          call 20
          local.set 38
          local.get 7
          local.get 38
          i32.const 8
          memory.copy
          local.get 3
          local.get 7
          local.set 38
          local.get 37
          i32.const 192
          i32.add
          local.get 38
          i32.const 8
          memory.copy
          local.get 37
          i32.const 192
          i32.add
          local.get 0
          local.set 22
          local.get 22
          local.get 37
          i32.const 200
          i32.add
          call 101
          local.get 37
          i32.const 216
          i32.add
          call 108
          local.set 39
          i32.const 0
          i32.add
          local.get 39
          i32.const 8
          memory.copy
          local.get 0
          local.get 2
          local.set 17
          local.get 17
          local.get 37
          i32.const 224
          i32.add
          call 95
          local.set 44
          local.tee 43
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 43
          ref.cast (ref 16)
          local.set 43
          local.get 43
          ref.cast (ref 16)
          local.get 44
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 16 3
          local.get 43
          ref.cast (ref 16)
          local.get 44
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 16 4
          local.get 0
          local.get 3
          i32.const 0
          i32.add
          local.set 38
          local.get 37
          i32.const 232
          i32.add
          local.get 38
          i32.const 8
          memory.copy
          local.get 37
          i32.const 232
          i32.add
          local.set 44
          local.tee 43
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 43
          ref.cast (ref 16)
          local.set 43
          local.get 43
          ref.cast (ref 16)
          local.get 44
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 16 5
          local.get 43
          ref.cast (ref 16)
          local.get 44
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 16 6
          local.get 0
          local.get 1
          local.set 23
          local.get 23
          call 103
          local.set 42
          local.tee 43
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 43
          ref.cast (ref 16)
          local.set 43
          local.get 43
          ref.cast (ref 16)
          local.get 42
          struct.set 16 7
          local.get 0
          local.get 2
          local.set 18
          local.get 18
          call 97
          local.set 44
          local.tee 43
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 43
          ref.cast (ref 16)
          local.set 43
          local.get 43
          ref.cast (ref 16)
          local.get 44
          i32.const 255
          i32.and
          struct.set 16 8
          local.get 0
          local.get 2
          local.set 20
          local.get 20
          call 99
          local.set 44
          local.tee 43
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 43
          ref.cast (ref 16)
          local.set 43
          local.get 43
          ref.cast (ref 16)
          local.get 44
          i32.const 255
          i32.and
          struct.set 16 9
          local.get 0
          i32.const 1
          local.set 44
          local.tee 43
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 43
          ref.cast (ref 16)
          local.set 43
          local.get 43
          ref.cast (ref 16)
          local.get 44
          i32.const 255
          i32.and
          struct.set 16 10
          local.get 0
          local.set 31
          local.get 31
          call 109
          local.get 37
          global.set 34
          return
        else
          local.get 3
          local.get 0
          local.tee 43
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 43
          ref.cast (ref 16)
          local.set 43
          local.get 37
          i32.const 16
          i32.add
          local.get 43
          ref.cast (ref 16)
          struct.get 16 5
          f32.store align=1
          local.get 37
          i32.const 20
          i32.add
          local.get 43
          ref.cast (ref 16)
          struct.get 16 6
          f32.store align=1
          local.get 37
          i32.const 16
          i32.add
          local.set 39
          i32.const 0
          i32.add
          local.get 39
          i32.const 8
          memory.copy
          local.get 37
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 32
    local.get 37
    global.set 34
    local.get 32
    throw_ref
  )
  (func (;51;) (type 136) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;52;) (type 137) (param i32 i32) (result i32)
    (local i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 10
        i32.const 0
        i32.add
        local.tee 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        call 48
        local.get 2
        local.set 11
        local.get 1
        local.get 11
        i32.const 8
        memory.copy
        local.get 1
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;53;) (type 138) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;54;) (type 139) (param i32 i32 i32 i32 i32 i32 i32 i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 13
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 48
    i32.add
    global.set 34
    local.get 13
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 13
    i32.const 0
    i32.add
    local.set 1
    local.get 13
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 13
    i32.const 8
    i32.add
    local.set 2
    local.get 13
    i32.const 16
    i32.add
    local.get 3
    i32.const 8
    memory.copy
    local.get 13
    i32.const 16
    i32.add
    local.set 3
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
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
        local.set 15
        i32.const 0
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 0
        local.get 2
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
        local.set 15
        i32.const 8
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 0
        local.get 3
        local.set 14
        local.get 13
        i32.const 40
        i32.add
        local.get 14
        i32.const 8
        memory.copy
        local.get 13
        i32.const 40
        i32.add
        local.set 15
        i32.const 16
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 0
        local.get 4
        local.set 15
        i32.const 24
        i32.add
        local.get 15
        i32.store8
        local.get 0
        local.get 5
        local.set 15
        i32.const 25
        i32.add
        local.get 15
        i32.store8
        local.get 0
        local.get 6
        local.set 15
        i32.const 26
        i32.add
        local.get 15
        i32.store8
        local.get 0
        local.get 7
        local.set 15
        i32.const 27
        i32.add
        local.get 15
        i32.store8
        local.get 13
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 34
    local.get 8
    throw_ref
  )
  (func (;55;) (type 140) (param eqref i32 i32 i32)
    (local f32 f32 f32 f32 f32 f32 f32 eqref i32 i32 eqref i32 i32 i32 i32 eqref i32 eqref i32 i32 i32 f32 f32 f32 f32 eqref eqref eqref i32 eqref i32 eqref i32 eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 43
    i32.const 1048304
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 43
    i32.const 272
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.get 2
        local.set 13
        local.set 12
        local.set 11
        local.get 11
        local.get 12
        local.get 13
        call 110
        i32.eqz
        if (type 141) ;; label = @3
          local.get 2
          local.set 15
          local.get 15
          local.get 43
          i32.const 16
          i32.add
          call 112
          i32.const 0
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.set 16
          local.get 16
          local.get 43
          i32.const 24
          i32.add
          call 113
          i32.const 0
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.set 17
          local.get 17
          call 114
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 10
          f32.demote_f64
          local.get 10
          f32.mul
          f64.promote_f32
          f64.const 0x1p-1 (;=0.5;)
          f32.demote_f64
          f64.promote_f32
          f32.demote_f64
          local.set 10
          f32.demote_f64
          local.get 10
          f32.mul
          f64.promote_f32
          f32.demote_f64
          local.set 10
          f32.demote_f64
          local.get 10
          f32.sub
          f64.promote_f32
          f32.demote_f64
          local.set 4
          local.get 2
          local.set 15
          local.get 15
          local.get 43
          i32.const 32
          i32.add
          call 112
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.set 16
          local.get 16
          local.get 43
          i32.const 40
          i32.add
          call 113
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.set 18
          local.get 18
          call 115
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 10
          f32.demote_f64
          local.get 10
          f32.mul
          f64.promote_f32
          f64.const 0x1p-1 (;=0.5;)
          f32.demote_f64
          f64.promote_f32
          f32.demote_f64
          local.set 10
          f32.demote_f64
          local.get 10
          f32.mul
          f64.promote_f32
          f32.demote_f64
          local.set 10
          f32.demote_f64
          local.get 10
          f32.sub
          f64.promote_f32
          f32.demote_f64
          local.set 5
          local.get 0
          local.set 19
          local.get 19
          call 98
          i32.eqz
          if (type 142) (result f64) ;; label = @4
            local.get 2
            local.set 20
            local.get 20
            local.get 43
            i32.const 56
            i32.add
            call 116
            i32.const 0
            i32.add
            f32.load align=1
            f64.promote_f32
          else
            local.get 2
            local.set 16
            local.get 16
            local.get 43
            i32.const 48
            i32.add
            call 113
            i32.const 0
            i32.add
            f32.load align=1
            f64.promote_f32
          end
          f32.demote_f64
          local.set 6
          local.get 0
          local.set 21
          local.get 21
          call 100
          i32.eqz
          if (type 143) (result f64) ;; label = @4
            local.get 2
            local.set 20
            local.get 20
            local.get 43
            i32.const 72
            i32.add
            call 116
            i32.const 4
            i32.add
            f32.load align=1
            f64.promote_f32
          else
            local.get 2
            local.set 16
            local.get 16
            local.get 43
            i32.const 64
            i32.add
            call 113
            i32.const 4
            i32.add
            f32.load align=1
            f64.promote_f32
          end
          f32.demote_f64
          local.set 7
          local.get 2
          local.set 22
          local.get 22
          call 117
          if (type 144) ;; label = @4
          else
            local.get 0
            local.set 19
            local.get 19
            call 98
            i32.eqz
            if (type 145) ;; label = @5
            else
              local.get 2
              local.set 20
              local.get 20
              local.get 43
              i32.const 80
              i32.add
              call 116
              i32.const 0
              i32.add
              f32.load align=1
              f64.promote_f32
              f32.demote_f64
              f32.abs
              f32.const inf (;=inf;)
              f32.lt
              i32.eqz
              if (type 146) ;; label = @6
              else
                local.get 0
                local.set 19
                local.get 19
                call 98
                i32.const 1
                i32.sub
                local.get 2
                local.set 17
                local.get 17
                call 114
                i32.sub
                f32.convert_i32_s
                f64.promote_f32
                f64.const 0x1p-1 (;=0.5;)
                f32.demote_f64
                f64.promote_f32
                f32.demote_f64
                local.set 10
                f32.demote_f64
                local.get 10
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 8
                local.get 4
                f64.promote_f32
                local.get 2
                local.set 20
                local.get 20
                local.get 43
                i32.const 88
                i32.add
                call 116
                i32.const 0
                i32.add
                f32.load align=1
                f64.promote_f32
                local.get 2
                local.set 16
                local.get 16
                local.get 43
                i32.const 96
                i32.add
                call 113
                i32.const 0
                i32.add
                f32.load align=1
                f64.promote_f32
                f32.demote_f64
                local.set 10
                f32.demote_f64
                local.get 10
                f32.sub
                f64.promote_f32
                local.get 8
                f64.promote_f32
                f32.demote_f64
                local.set 10
                f32.demote_f64
                local.get 10
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 10
                f32.demote_f64
                local.get 10
                f32.add
                f64.promote_f32
                f32.demote_f64
                local.set 4
              end
            end
          end
          local.get 2
          local.set 23
          local.get 23
          call 118
          if (type 147) ;; label = @4
          else
            local.get 0
            local.set 21
            local.get 21
            call 100
            i32.eqz
            if (type 148) ;; label = @5
            else
              local.get 2
              local.set 20
              local.get 20
              local.get 43
              i32.const 104
              i32.add
              call 116
              i32.const 4
              i32.add
              f32.load align=1
              f64.promote_f32
              f32.demote_f64
              f32.abs
              f32.const inf (;=inf;)
              f32.lt
              i32.eqz
              if (type 149) ;; label = @6
              else
                local.get 0
                local.set 21
                local.get 21
                call 100
                i32.const 1
                i32.sub
                local.get 2
                local.set 18
                local.get 18
                call 115
                i32.sub
                f32.convert_i32_s
                f64.promote_f32
                f64.const 0x1p-1 (;=0.5;)
                f32.demote_f64
                f64.promote_f32
                f32.demote_f64
                local.set 10
                f32.demote_f64
                local.get 10
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 9
                local.get 5
                f64.promote_f32
                local.get 2
                local.set 20
                local.get 20
                local.get 43
                i32.const 112
                i32.add
                call 116
                i32.const 4
                i32.add
                f32.load align=1
                f64.promote_f32
                local.get 2
                local.set 16
                local.get 16
                local.get 43
                i32.const 120
                i32.add
                call 113
                i32.const 4
                i32.add
                f32.load align=1
                f64.promote_f32
                f32.demote_f64
                local.set 10
                f32.demote_f64
                local.get 10
                f32.sub
                f64.promote_f32
                local.get 9
                f64.promote_f32
                f32.demote_f64
                local.set 10
                f32.demote_f64
                local.get 10
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 10
                f32.demote_f64
                local.get 10
                f32.add
                f64.promote_f32
                f32.demote_f64
                local.set 5
              end
            end
          end
          local.get 3
          local.get 4
          f64.promote_f32
          local.get 5
          f64.promote_f32
          local.get 6
          f64.promote_f32
          local.get 7
          f64.promote_f32
          f32.demote_f64
          local.set 28
          f32.demote_f64
          local.set 27
          f32.demote_f64
          local.set 26
          f32.demote_f64
          local.set 25
          local.get 43
          i32.const 128
          i32.add
          local.tee 24
          i32.const 0
          i32.const 16
          memory.fill
          local.get 24
          local.get 25
          local.get 26
          local.get 27
          local.get 28
          call 40
          local.get 24
          local.get 0
          local.set 29
          local.get 29
          local.get 43
          i32.const 144
          i32.add
          call 101
          local.get 43
          i32.const 160
          i32.add
          call 119
          local.set 45
          i32.const 8
          i32.add
          local.get 45
          i32.const 16
          memory.copy
          local.get 0
          local.set 14
          local.get 14
          local.get 43
          i32.const 176
          i32.add
          call 111
          local.get 3
          i32.const 8
          i32.add
          local.set 44
          local.get 43
          i32.const 192
          i32.add
          local.get 44
          i32.const 16
          memory.copy
          local.get 43
          i32.const 192
          i32.add
          call 120
          i32.eqz
          if (type 150) ;; label = @4
          else
            local.get 0
            local.set 30
            local.get 30
            call 121
          end
          local.get 0
          local.get 3
          i32.const 8
          i32.add
          local.set 44
          local.get 43
          i32.const 208
          i32.add
          local.get 44
          i32.const 16
          memory.copy
          local.get 43
          i32.const 208
          i32.add
          local.set 32
          local.set 31
          local.get 31
          local.get 32
          call 122
          local.get 0
          local.set 33
          local.get 33
          call 123
          local.get 0
          local.get 3
          i32.const 8
          i32.add
          local.set 44
          local.get 43
          i32.const 224
          i32.add
          local.get 44
          i32.const 16
          memory.copy
          local.get 43
          i32.const 224
          i32.add
          local.get 1
          local.set 34
          local.get 34
          call 103
          local.set 37
          local.set 36
          local.set 35
          local.get 35
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.get 36
          local.get 37
          local.get 35
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 38
          i32.add
          call_indirect (type 299)
          local.get 0
          local.get 2
          local.set 20
          local.get 20
          local.get 43
          i32.const 240
          i32.add
          call 116
          local.set 50
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.set 49
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 16 11
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 16 12
          local.get 0
          local.get 2
          local.set 16
          local.get 16
          local.get 43
          i32.const 248
          i32.add
          call 113
          local.set 50
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.set 49
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 16 13
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 16 14
          local.get 0
          local.get 2
          local.set 15
          local.get 15
          local.get 43
          i32.const 256
          i32.add
          call 112
          local.set 50
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.set 49
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 16 15
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 16 16
          local.get 0
          local.get 2
          local.set 22
          local.get 22
          call 117
          local.set 50
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.set 49
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 255
          i32.and
          struct.set 16 17
          local.get 0
          local.get 2
          local.set 23
          local.get 23
          call 118
          local.set 50
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.set 49
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 255
          i32.and
          struct.set 16 18
          local.get 0
          local.get 2
          local.set 17
          local.get 17
          call 114
          local.set 50
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.set 49
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 255
          i32.and
          struct.set 16 19
          local.get 0
          local.get 2
          local.set 18
          local.get 18
          call 115
          local.set 50
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.set 49
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 255
          i32.and
          struct.set 16 20
          local.get 0
          local.get 1
          local.set 34
          local.get 34
          call 103
          local.set 48
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.set 49
          local.get 49
          ref.cast (ref 16)
          local.get 48
          struct.set 16 21
          local.get 0
          i32.const 1
          local.set 50
          local.tee 49
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 49
          ref.cast (ref 16)
          local.set 49
          local.get 49
          ref.cast (ref 16)
          local.get 50
          i32.const 255
          i32.and
          struct.set 16 22
          local.get 43
          global.set 34
          return
        else
          local.get 3
          local.get 0
          local.set 14
          local.get 14
          local.get 43
          i32.const 0
          i32.add
          call 111
          local.set 45
          i32.const 8
          i32.add
          local.get 45
          i32.const 16
          memory.copy
          local.get 43
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 38
    local.get 43
    global.set 34
    local.get 38
    throw_ref
  )
  (func (;56;) (type 151) (param eqref i32 i32)
    (local i32 i32 i32 eqref i32 i32 i32 i32 i32 eqref eqref i32 i32 i32 eqref i32 i32 eqref i32 i32 eqref eqref eqref eqref eqref eqref eqref eqref eqref eqref exnref exnref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 40
    i32.const 1048192
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 40
    i32.const 384
    i32.add
    global.set 34
    local.get 40
    i32.const 20
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 16
    memory.fill
    local.get 40
    i32.const 36
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 20
    memory.fill
    local.get 40
    i32.const 56
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 16
    memory.fill
    local.get 40
    i32.const 72
    i32.add
    local.set 8
    local.get 8
    i32.const 0
    i32.const 20
    memory.fill
    local.get 40
    i32.const 92
    i32.add
    local.set 9
    local.get 9
    i32.const 0
    i32.const 20
    memory.fill
    local.get 40
    i32.const 112
    i32.add
    local.set 10
    local.get 10
    i32.const 0
    i32.const 16
    memory.fill
    local.get 40
    i32.const 0
    i32.add
    local.get 2
    i32.const 20
    memory.copy
    local.get 40
    i32.const 0
    i32.add
    local.set 2
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 2
        local.set 41
        local.get 40
        i32.const 128
        i32.add
        local.get 41
        i32.const 20
        memory.copy
        local.get 40
        i32.const 128
        i32.add
        local.set 41
        local.get 8
        local.get 41
        i32.const 20
        memory.copy
        local.get 8
        local.set 11
        local.get 11
        call 124
        if (type 152) ;; label = @3
        else
          local.get 0
          local.set 12
          local.get 12
          call 125
          ref.is_null
          if (type 153) (result i32) ;; label = @4
            local.get 0
            local.set 13
            local.get 13
            local.get 40
            i32.const 168
            i32.add
            call 111
            local.set 15
            local.get 40
            i32.const 184
            i32.add
            local.tee 14
            i32.const 0
            i32.const 20
            memory.fill
            local.get 14
            local.get 15
            call 126
            local.get 14
          else
            local.get 9
            i32.const 0
            i32.const 20
            memory.fill
            local.get 9
            local.set 41
            local.get 40
            i32.const 148
            i32.add
            local.get 41
            i32.const 20
            memory.copy
            local.get 40
            i32.const 148
            i32.add
          end
          local.set 41
          local.get 2
          local.get 41
          i32.const 20
          memory.copy
        end
        local.get 2
        local.set 11
        local.get 11
        call 124
        i32.eqz
        if (type 154) ;; label = @3
        else
          local.get 2
          local.set 16
          local.get 16
          local.get 40
          i32.const 204
          i32.add
          call 127
          local.set 41
          local.get 3
          local.get 41
          i32.const 16
          memory.copy
          local.get 0
          local.set 17
          local.get 17
          local.tee 46
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 46
          ref.cast (ref 16)
          local.get 40
          i32.const 220
          i32.add
          local.get 17
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 55
          i32.add
          call_indirect (type 302)
          local.set 41
          local.get 10
          local.get 41
          i32.const 16
          memory.copy
          local.get 10
          local.get 3
          local.set 41
          local.get 40
          i32.const 236
          i32.add
          local.get 41
          i32.const 16
          memory.copy
          local.get 40
          i32.const 236
          i32.add
          local.set 19
          local.set 18
          local.get 18
          local.get 19
          call 128
          if (type 155) ;; label = @4
          else
            local.get 40
            global.set 34
            return
          end
        end
        local.get 0
        local.tee 46
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 46
        ref.cast (ref 16)
        local.set 46
        local.get 40
        i32.const 252
        i32.add
        local.get 46
        ref.cast (ref 16)
        struct.get 16 23
        i32.store8
        local.get 40
        i32.const 256
        i32.add
        local.get 46
        ref.cast (ref 16)
        struct.get 16 24
        f32.store align=1
        local.get 40
        i32.const 260
        i32.add
        local.get 46
        ref.cast (ref 16)
        struct.get 16 25
        f32.store align=1
        local.get 40
        i32.const 264
        i32.add
        local.get 46
        ref.cast (ref 16)
        struct.get 16 26
        f32.store align=1
        local.get 40
        i32.const 268
        i32.add
        local.get 46
        ref.cast (ref 16)
        struct.get 16 27
        f32.store align=1
        local.get 40
        i32.const 252
        i32.add
        local.set 41
        local.get 4
        local.get 41
        i32.const 20
        memory.copy
        local.get 0
        local.get 2
        local.set 11
        local.get 11
        call 124
        i32.eqz
        if (type 156) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.const 20
          memory.fill
          local.get 8
          local.set 41
          local.get 40
          i32.const 288
          i32.add
          local.get 41
          i32.const 20
          memory.copy
          local.get 40
          i32.const 288
          i32.add
        else
          local.get 2
          local.set 16
          local.get 16
          local.get 40
          i32.const 272
          i32.add
          call 127
          local.set 41
          local.get 5
          local.get 41
          i32.const 16
          memory.copy
          local.get 0
          local.get 5
          local.set 41
          local.get 40
          i32.const 308
          i32.add
          local.get 41
          i32.const 16
          memory.copy
          local.get 40
          i32.const 308
          i32.add
          local.set 21
          local.set 20
          local.get 20
          local.tee 46
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 46
          ref.cast (ref 16)
          local.get 21
          local.get 40
          i32.const 324
          i32.add
          local.get 20
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 58
          i32.add
          call_indirect (type 304)
          local.set 15
          local.get 40
          i32.const 340
          i32.add
          local.tee 14
          i32.const 0
          i32.const 20
          memory.fill
          local.get 14
          local.get 15
          call 126
          local.get 14
        end
        local.set 47
        local.tee 46
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 46
        ref.cast (ref 16)
        local.set 46
        local.get 46
        ref.cast (ref 16)
        local.get 47
        i32.const 0
        i32.add
        i32.load8_u
        struct.set 16 23
        local.get 46
        ref.cast (ref 16)
        local.get 47
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 16 24
        local.get 46
        ref.cast (ref 16)
        local.get 47
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 16 25
        local.get 46
        ref.cast (ref 16)
        local.get 47
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 16 26
        local.get 46
        ref.cast (ref 16)
        local.get 47
        i32.const 16
        i32.add
        f32.load align=1
        struct.set 16 27
        local.get 1
        local.set 22
        local.get 22
        call 129
        local.set 6
        local.get 6
        local.set 23
        local.get 23
        local.tee 46
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 46
        ref.cast (ref 10)
        local.get 23
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 0
        i32.add
        call_indirect (type 193)
        local.get 0
        local.tee 46
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 46
        ref.cast (ref 16)
        struct.get 16 30
        i32.const 255
        i32.and
        local.set 7
        local.get 0
        local.get 0
        local.set 24
        local.get 24
        call 130
        local.set 47
        local.tee 46
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 46
        ref.cast (ref 16)
        local.set 46
        local.get 46
        ref.cast (ref 16)
        local.get 47
        i32.const 255
        i32.and
        struct.set 16 30
        block (type 8) ;; label = @3
          block (type 7) (result exnref) ;; label = @4
            try_table (catch_all_ref 0 (;@4;)) ;; label = @5
              block (type 8) ;; label = @6
                block (type 6) (result eqref exnref) ;; label = @7
                  try_table (catch_ref 0 0 (;@7;)) ;; label = @8
                    local.get 0
                    local.set 25
                    local.get 25
                    call 131
                    local.get 0
                    local.get 6
                    local.set 27
                    local.set 26
                    local.get 26
                    local.tee 46
                    ref.is_null
                    if ;; label = @9
                      i32.const 4
                      global.get 30
                      ref.is_null
                      if ;; label = @10
                        i32.const 0
                        i32.const 53
                        array.new_data 0 30
                        i32.const 0
                        i32.const 53
                        call 1
                        global.set 30
                      end
                      global.get 30
                      struct.new 3
                      throw 0
                    end
                    local.get 46
                    ref.cast (ref 16)
                    local.get 27
                    local.get 26
                    ref.cast (ref 10)
                    struct.get 10 0
                    ref.cast (ref 9)
                    struct.get 9 2
                    i32.const 40
                    i32.add
                    call_indirect (type 305)
                    local.get 0
                    local.get 6
                    local.set 29
                    local.set 28
                    local.get 28
                    local.tee 46
                    ref.is_null
                    if ;; label = @9
                      i32.const 4
                      global.get 30
                      ref.is_null
                      if ;; label = @10
                        i32.const 0
                        i32.const 53
                        array.new_data 0 30
                        i32.const 0
                        i32.const 53
                        call 1
                        global.set 30
                      end
                      global.get 30
                      struct.new 3
                      throw 0
                    end
                    local.get 46
                    ref.cast (ref 16)
                    local.get 29
                    local.get 28
                    ref.cast (ref 10)
                    struct.get 10 0
                    ref.cast (ref 9)
                    struct.get 9 2
                    i32.const 39
                    i32.add
                    call_indirect (type 306)
                    local.get 0
                    local.get 6
                    local.set 45
                    local.tee 46
                    ref.is_null
                    if ;; label = @9
                      i32.const 4
                      global.get 30
                      ref.is_null
                      if ;; label = @10
                        i32.const 0
                        i32.const 53
                        array.new_data 0 30
                        i32.const 0
                        i32.const 53
                        call 1
                        global.set 30
                      end
                      global.get 30
                      struct.new 3
                      throw 0
                    end
                    local.get 46
                    ref.cast (ref 16)
                    local.set 46
                    local.get 46
                    ref.cast (ref 16)
                    local.get 45
                    struct.set 16 31
                    br 2 (;@6;)
                  end
                  br 1 (;@6;)
                end
                local.set 33
                local.set 32
                i32.const 1
                if ;; label = @7
                  local.get 32
                  local.tee 32
                  local.get 33
                  local.set 33
                  drop
                  local.get 0
                  local.set 30
                  local.get 30
                  call 121
                  local.get 33
                  throw_ref
                  br 1 (;@6;)
                end
                local.get 33
                throw_ref
              end
            end
            br 1 (;@3;)
          end
          local.set 34
          block (type 8) ;; label = @4
            local.get 0
            local.get 7
            local.set 47
            local.tee 46
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 46
            ref.cast (ref 16)
            local.set 46
            local.get 46
            ref.cast (ref 16)
            local.get 47
            i32.const 255
            i32.and
            struct.set 16 30
            local.get 6
            local.set 31
            local.get 31
            local.tee 46
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 46
            ref.cast (ref 10)
            local.get 31
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 6
            i32.add
            call_indirect (type 266)
            local.get 0
            local.get 4
            local.set 41
            local.get 40
            i32.const 360
            i32.add
            local.get 41
            i32.const 20
            memory.copy
            local.get 40
            i32.const 360
            i32.add
            local.set 47
            local.tee 46
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 46
            ref.cast (ref 16)
            local.set 46
            local.get 46
            ref.cast (ref 16)
            local.get 47
            i32.const 0
            i32.add
            i32.load8_u
            struct.set 16 23
            local.get 46
            ref.cast (ref 16)
            local.get 47
            i32.const 4
            i32.add
            f32.load align=1
            struct.set 16 24
            local.get 46
            ref.cast (ref 16)
            local.get 47
            i32.const 8
            i32.add
            f32.load align=1
            struct.set 16 25
            local.get 46
            ref.cast (ref 16)
            local.get 47
            i32.const 12
            i32.add
            f32.load align=1
            struct.set 16 26
            local.get 46
            ref.cast (ref 16)
            local.get 47
            i32.const 16
            i32.add
            f32.load align=1
            struct.set 16 27
            br 0 (;@4;)
          end
          local.get 34
          throw_ref
        end
        block (type 8) ;; label = @3
          local.get 0
          local.get 7
          local.set 47
          local.tee 46
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 46
          ref.cast (ref 16)
          local.set 46
          local.get 46
          ref.cast (ref 16)
          local.get 47
          i32.const 255
          i32.and
          struct.set 16 30
          local.get 6
          local.set 31
          local.get 31
          local.tee 46
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 46
          ref.cast (ref 10)
          local.get 31
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 6
          i32.add
          call_indirect (type 266)
          local.get 0
          local.get 4
          local.set 41
          local.get 40
          i32.const 360
          i32.add
          local.get 41
          i32.const 20
          memory.copy
          local.get 40
          i32.const 360
          i32.add
          local.set 47
          local.tee 46
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 46
          ref.cast (ref 16)
          local.set 46
          local.get 46
          ref.cast (ref 16)
          local.get 47
          i32.const 0
          i32.add
          i32.load8_u
          struct.set 16 23
          local.get 46
          ref.cast (ref 16)
          local.get 47
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 16 24
          local.get 46
          ref.cast (ref 16)
          local.get 47
          i32.const 8
          i32.add
          f32.load align=1
          struct.set 16 25
          local.get 46
          ref.cast (ref 16)
          local.get 47
          i32.const 12
          i32.add
          f32.load align=1
          struct.set 16 26
          local.get 46
          ref.cast (ref 16)
          local.get 47
          i32.const 16
          i32.add
          f32.load align=1
          struct.set 16 27
          br 0 (;@3;)
        end
        local.get 40
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 35
    local.get 40
    global.set 34
    local.get 35
    throw_ref
  )
  (func (;57;) (type 157) (param eqref eqref eqref i32)
    (local i32 f32 f32 eqref i32 eqref i32 i32 f32 f32 f32 f32 eqref i32 eqref i32 f32 f32 f32 f32 f32 i32 eqref i32 f32 eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 38
    i32.const 1048368
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 38
    i32.const 208
    i32.add
    global.set 34
    local.get 38
    i32.const 0
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 16
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 2
        local.set 7
        local.get 7
        call 133
        if (type 158) ;; label = @3
          local.get 4
          local.get 3
          local.set 8
          local.get 8
          local.get 38
          i32.const 16
          i32.add
          call 42
          i32.const 0
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.set 9
          local.get 9
          local.get 38
          i32.const 32
          i32.add
          call 134
          i32.const 0
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 3
          local.set 10
          local.get 10
          call 43
          f64.promote_f32
          f32.demote_f64
          local.set 6
          f32.demote_f64
          local.get 6
          f32.mul
          f64.promote_f32
          f32.demote_f64
          local.set 6
          f32.demote_f64
          local.get 6
          f32.add
          f64.promote_f32
          local.get 3
          local.set 8
          local.get 8
          local.get 38
          i32.const 48
          i32.add
          call 42
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.set 9
          local.get 9
          local.get 38
          i32.const 64
          i32.add
          call 134
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 3
          local.set 10
          local.get 10
          call 43
          f64.promote_f32
          f32.demote_f64
          local.set 6
          f32.demote_f64
          local.get 6
          f32.mul
          f64.promote_f32
          f32.demote_f64
          local.set 6
          f32.demote_f64
          local.get 6
          f32.add
          f64.promote_f32
          local.get 2
          local.set 9
          local.get 9
          local.get 38
          i32.const 80
          i32.add
          call 134
          i32.const 8
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 3
          local.set 10
          local.get 10
          call 43
          f64.promote_f32
          f32.demote_f64
          local.set 6
          f32.demote_f64
          local.get 6
          f32.mul
          f64.promote_f32
          local.get 2
          local.set 9
          local.get 9
          local.get 38
          i32.const 96
          i32.add
          call 134
          i32.const 12
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 3
          local.set 10
          local.get 10
          call 43
          f64.promote_f32
          f32.demote_f64
          local.set 6
          f32.demote_f64
          local.get 6
          f32.mul
          f64.promote_f32
          f32.demote_f64
          local.set 15
          f32.demote_f64
          local.set 14
          f32.demote_f64
          local.set 13
          f32.demote_f64
          local.set 12
          local.set 11
          local.get 11
          local.get 12
          local.get 13
          local.get 14
          local.get 15
          call 40
          local.get 1
          local.set 16
          local.get 16
          local.tee 44
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 44
          ref.cast (ref 10)
          local.get 16
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 1
          i32.add
          call_indirect (type 195)
          local.get 2
          local.set 7
          local.get 7
          call 133
          i32.const 4
          i32.eq
          i32.eqz
          if (type 159) ;; label = @4
            local.get 1
            local.get 4
            local.set 39
            local.get 38
            i32.const 152
            i32.add
            local.get 39
            i32.const 16
            memory.copy
            local.get 38
            i32.const 152
            i32.add
            local.get 4
            local.set 39
            local.get 38
            i32.const 168
            i32.add
            local.get 39
            i32.const 16
            memory.copy
            local.get 38
            i32.const 168
            i32.add
            i32.const 12
            i32.add
            f32.load align=1
            f64.promote_f32
            f32.const 0x1p-1 (;=0.5;)
            f64.promote_f32
            f32.demote_f64
            f64.promote_f32
            f32.demote_f64
            local.set 6
            f32.demote_f64
            local.get 6
            f32.mul
            f64.promote_f32
            f32.demote_f64
            local.set 28
            local.set 27
            local.set 26
            local.get 26
            local.tee 44
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 44
            ref.cast (ref 10)
            local.get 27
            local.get 28
            local.get 26
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 2
            i32.add
            call_indirect (type 197)
          else
            local.get 4
            local.set 39
            local.get 38
            i32.const 112
            i32.add
            local.get 39
            i32.const 16
            memory.copy
            local.get 38
            i32.const 112
            i32.add
            i32.const 8
            i32.add
            f32.load align=1
            f64.promote_f32
            local.get 4
            local.set 39
            local.get 38
            i32.const 128
            i32.add
            local.get 39
            i32.const 16
            memory.copy
            local.get 38
            i32.const 128
            i32.add
            i32.const 12
            i32.add
            f32.load align=1
            f64.promote_f32
            f32.demote_f64
            local.set 6
            f32.demote_f64
            local.get 6
            f32.min
            f64.promote_f32
            f32.const 0x1p-1 (;=0.5;)
            f64.promote_f32
            f32.demote_f64
            f64.promote_f32
            f32.demote_f64
            local.set 6
            f32.demote_f64
            local.get 6
            f32.mul
            f64.promote_f32
            f32.demote_f64
            local.set 5
            local.get 1
            local.get 4
            local.set 17
            local.get 17
            local.get 38
            i32.const 144
            i32.add
            call 135
            local.get 5
            f64.promote_f32
            local.get 5
            f64.promote_f32
            i32.const 0
            f32.convert_i32_s
            f64.promote_f32
            i32.const 0
            f32.convert_i32_s
            f64.promote_f32
            f64.const 0x1.921fb6p+1 (;=3.1415927410125732;)
            i32.const 2
            f32.convert_i32_s
            f64.promote_f32
            f32.demote_f64
            local.set 6
            f32.demote_f64
            local.get 6
            f32.mul
            f64.promote_f32
            i32.const 1
            local.set 25
            f32.demote_f64
            local.set 24
            f32.demote_f64
            local.set 23
            f32.demote_f64
            local.set 22
            f32.demote_f64
            local.set 21
            f32.demote_f64
            local.set 20
            local.set 19
            local.set 18
            local.get 18
            local.tee 44
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 44
            ref.cast (ref 10)
            local.get 19
            local.get 20
            local.get 21
            local.get 22
            local.get 23
            local.get 24
            local.get 25
            local.get 18
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 9
            i32.add
            call_indirect (type 311)
          end
          local.get 1
          local.get 38
          i32.const 184
          i32.add
          call 86
          local.set 30
          local.set 29
          local.get 29
          local.tee 44
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 44
          ref.cast (ref 10)
          local.get 30
          local.get 29
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 7
          i32.add
          call_indirect (type 288)
          local.get 1
          i32.const 0
          local.set 32
          local.set 31
          local.get 31
          local.tee 44
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 44
          ref.cast (ref 10)
          local.get 32
          local.get 31
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 8
          i32.add
          call_indirect (type 290)
          local.get 38
          global.set 34
          return
        else
          local.get 38
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 33
    local.get 38
    global.set 34
    local.get 33
    throw_ref
  )
  (func (;58;) (type 160) (param i32 f32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 12
        i32.const 0
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 12
        i32.const 4
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;59;) (type 161) (param eqref i32 i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;60;) (type 162) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;61;) (type 163) (param eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 136
    return
  )
  (func (;62;) (type 164) (param eqref i32 i32)
    (local f32 eqref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 2
        i32.const 1
        i32.eq
        if (type 165) ;; label = @3
          local.get 0
          local.get 1
          i32.const 36
          i32.add
          i32.const 0
          i32.add
          i32.const 0
          i32.add
          f32.load align=1
          f64.promote_f32
          local.set 20
          local.tee 17
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 17
          ref.cast (ref 37)
          local.set 17
          local.get 17
          ref.cast (ref 37)
          local.get 20
          struct.set 37 55
          local.get 0
          local.get 1
          i32.const 36
          i32.add
          i32.const 0
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.set 20
          local.tee 17
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 17
          ref.cast (ref 37)
          local.set 17
          local.get 17
          ref.cast (ref 37)
          local.get 20
          struct.set 37 56
          local.get 0
          local.get 1
          i32.const 12
          i32.add
          i32.load align=1
          i32.const 2
          i32.eq
          if (type 166) (result i32) ;; label = @4
            i32.const 1
          else
            local.get 1
            i32.const 12
            i32.add
            i32.load align=1
            i32.const 3
            i32.eq
          end
          local.set 18
          local.tee 17
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 17
          ref.cast (ref 37)
          local.set 17
          local.get 17
          ref.cast (ref 37)
          local.get 18
          i32.const 255
          i32.and
          struct.set 37 58
          local.get 0
          f64.const 0x0p+0 (;=0;)
          local.set 20
          local.tee 17
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 17
          ref.cast (ref 37)
          local.set 17
          local.get 17
          ref.cast (ref 37)
          local.get 20
          struct.set 37 57
          local.get 0
          local.set 4
          local.get 4
          call 121
          local.get 0
          local.tee 17
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 17
          ref.cast (ref 37)
          struct.get 37 58
          if (type 167) ;; label = @4
          else
            local.get 0
            local.set 5
            local.get 5
            call 137
          end
          local.get 11
          global.set 34
          return
        else
          local.get 11
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 34
    local.get 6
    throw_ref
  )
  (func (;63;) (type 168) (param eqref)
    (local eqref eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 28
    i32.const -4
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 138
    return
  )
  (func (;64;) (type 169) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;65;) (type 170) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 8
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;66;) (type 171) (param eqref externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 7
    ref.cast (ref 49)
    local.set 3
    local.get 3
    ref.cast (ref 49)
    local.get 7
    struct.set 49 0
    return
  )
  (func (;67;) (type 172) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 8
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 8
        i32.add
        local.set 14
        ref.cast (ref 49)
        local.set 13
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 49 1
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 49 2
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;68;) (type 173) (param i32 f32 f32 f32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 14
        i32.const 0
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 14
        i32.const 4
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 3
        f64.promote_f32
        local.set 14
        i32.const 8
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 4
        f64.promote_f32
        local.set 14
        i32.const 12
        i32.add
        local.get 14
        f32.demote_f64
        f32.store align=1
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;69;) (type 174) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 32
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 16
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 16
        i32.add
        local.set 14
        ref.cast (ref 49)
        local.set 13
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 49 3
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 49 4
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 49 5
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 49 6
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;70;) (type 175) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 32
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 16
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 16
        i32.add
        local.set 14
        ref.cast (ref 49)
        local.set 13
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 49 7
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 49 8
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 49 9
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 49 10
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;71;) (type 176) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 4
    ref.cast (ref 49)
    local.set 3
    local.get 3
    ref.cast (ref 49)
    local.get 4
    struct.set 49 11
    return
  )
  (func (;72;) (type 177) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 32
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 16
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 16
        i32.add
        local.set 14
        ref.cast (ref 49)
        local.set 13
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 49 12
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 49 13
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 49 14
        local.get 13
        ref.cast (ref 49)
        local.get 14
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 49 15
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;73;) (type 178) (param eqref f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 6
    ref.cast (ref 49)
    local.set 3
    local.get 3
    ref.cast (ref 49)
    local.get 6
    f32.demote_f64
    struct.set 49 16
    return
  )
  (func (;74;) (type 179) (param i32 f32 f32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 9
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 9
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 13
        i32.const 0
        i32.add
        local.get 13
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 13
        i32.const 4
        i32.add
        local.get 13
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 3
        f64.promote_f32
        local.set 13
        i32.const 8
        i32.add
        local.get 13
        f32.demote_f64
        f32.store align=1
        local.get 9
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 4
    local.get 9
    global.set 34
    local.get 4
    throw_ref
  )
  (func (;75;) (type 180) (param i32) (result f32)
    (local f32 i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 2
        local.get 2
        call 77
        f64.promote_f32
        local.get 0
        local.set 3
        local.get 3
        call 79
        f64.promote_f32
        f32.demote_f64
        local.set 1
        f32.demote_f64
        local.get 1
        f32.add
        f64.promote_f32
        local.get 0
        local.set 4
        local.get 4
        call 80
        f64.promote_f32
        f32.demote_f64
        local.set 1
        f32.demote_f64
        local.get 1
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;76;) (type 181) (param eqref i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        ref.cast (ref 49)
        local.set 13
        local.get 7
        i32.const 0
        i32.add
        local.get 13
        ref.cast (ref 49)
        struct.get 49 1
        f32.store align=1
        local.get 7
        i32.const 4
        i32.add
        local.get 13
        ref.cast (ref 49)
        struct.get 49 2
        f32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;77;) (type 182) (param i32) (result f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;78;) (type 183) (param i32 f32 i32) (result i32)
    (local f32 i32 f32 f32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 14
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 0
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 0
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 14
        i32.const 0
        i32.add
        local.tee 4
        i32.const 0
        i32.const 16
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        local.get 7
        local.get 8
        call 40
        local.get 4
        local.set 15
        local.get 2
        local.get 15
        i32.const 16
        memory.copy
        local.get 2
        local.get 14
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 14
    global.set 34
    local.get 9
    throw_ref
  )
  (func (;79;) (type 184) (param i32) (result f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;80;) (type 185) (param i32) (result f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;81;) (type 186) (param i32 f32 i32 i32 i32 i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 11
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 128
    i32.add
    global.set 34
    local.get 11
    i32.const 0
    i32.add
    local.get 2
    i32.const 16
    memory.copy
    local.get 11
    i32.const 0
    i32.add
    local.set 2
    local.get 11
    i32.const 16
    i32.add
    local.get 3
    i32.const 16
    memory.copy
    local.get 11
    i32.const 16
    i32.add
    local.set 3
    local.get 11
    i32.const 32
    i32.add
    local.get 4
    i32.const 16
    memory.copy
    local.get 11
    i32.const 32
    i32.add
    local.set 4
    local.get 11
    i32.const 48
    i32.add
    local.get 5
    i32.const 16
    memory.copy
    local.get 11
    i32.const 48
    i32.add
    local.set 5
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 15
        i32.const 0
        i32.add
        local.get 15
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        local.set 12
        local.get 11
        i32.const 64
        i32.add
        local.get 12
        i32.const 16
        memory.copy
        local.get 11
        i32.const 64
        i32.add
        local.set 13
        i32.const 4
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 0
        local.get 3
        local.set 12
        local.get 11
        i32.const 80
        i32.add
        local.get 12
        i32.const 16
        memory.copy
        local.get 11
        i32.const 80
        i32.add
        local.set 13
        i32.const 20
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 0
        local.get 4
        local.set 12
        local.get 11
        i32.const 96
        i32.add
        local.get 12
        i32.const 16
        memory.copy
        local.get 11
        i32.const 96
        i32.add
        local.set 13
        i32.const 36
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 0
        local.get 5
        local.set 12
        local.get 11
        i32.const 112
        i32.add
        local.get 12
        i32.const 16
        memory.copy
        local.get 11
        i32.const 112
        i32.add
        local.set 13
        i32.const 52
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 11
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 34
    local.get 6
    throw_ref
  )
  (func (;82;) (type 187) (param eqref i32 i32)
    (local f32 f32 f32 f32 i32 f32 i32 i32 i32 i32 i32 i32 i32 f32 f32 f32 f32 eqref i32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 28
    i32.const 1048320
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 28
    i32.const 256
    i32.add
    global.set 34
    local.get 28
    i32.const 12
    i32.add
    local.set 7
    local.get 7
    i32.const 0
    i32.const 16
    memory.fill
    local.get 28
    i32.const 0
    i32.add
    local.get 1
    i32.const 12
    memory.copy
    local.get 28
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        local.set 9
        local.get 9
        call 77
        f64.promote_f32
        local.get 2
        local.set 10
        local.get 10
        call 43
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 3
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.get 3
        f64.promote_f32
        f32.const 0x1p-1 (;=0.5;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.max
        f64.promote_f32
        f32.demote_f64
        local.set 4
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 28
        i32.add
        call 141
        local.set 29
        local.get 7
        local.get 29
        i32.const 16
        memory.copy
        local.get 7
        local.set 12
        local.get 12
        call 142
        f64.promote_f32
        f32.demote_f64
        local.set 5
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 44
        i32.add
        call 141
        local.set 29
        local.get 7
        local.get 29
        i32.const 16
        memory.copy
        local.get 7
        local.set 13
        local.get 13
        call 143
        f64.promote_f32
        local.get 3
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.sub
        f64.promote_f32
        f32.demote_f64
        local.set 6
        local.get 0
        local.get 5
        f64.promote_f32
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 60
        i32.add
        call 141
        local.set 29
        local.get 7
        local.get 29
        i32.const 16
        memory.copy
        local.get 7
        local.set 14
        local.get 14
        call 144
        f64.promote_f32
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 76
        i32.add
        call 141
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.const 0x1.99999ap-3 (;=0.2;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 92
        i32.add
        call 141
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.const 0x1.70a3d8p-4 (;=0.09;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 19
        f32.demote_f64
        local.set 18
        f32.demote_f64
        local.set 17
        f32.demote_f64
        local.set 16
        local.get 28
        i32.const 108
        i32.add
        local.tee 15
        i32.const 0
        i32.const 16
        memory.fill
        local.get 15
        local.get 16
        local.get 17
        local.get 18
        local.get 19
        call 40
        local.get 15
        local.get 4
        f64.promote_f32
        f32.demote_f64
        local.set 22
        local.set 21
        local.set 20
        local.get 20
        local.get 21
        local.get 22
        call 145
        local.get 0
        local.get 5
        f64.promote_f32
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 124
        i32.add
        call 141
        local.set 29
        local.get 7
        local.get 29
        i32.const 16
        memory.copy
        local.get 7
        local.set 14
        local.get 14
        call 144
        f64.promote_f32
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 140
        i32.add
        call 141
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.const 0x1.51eb86p-2 (;=0.33;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 156
        i32.add
        call 141
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.const 0x1.0a3d7p-3 (;=0.13;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 19
        f32.demote_f64
        local.set 18
        f32.demote_f64
        local.set 17
        f32.demote_f64
        local.set 16
        local.get 28
        i32.const 172
        i32.add
        local.tee 15
        i32.const 0
        i32.const 16
        memory.fill
        local.get 15
        local.get 16
        local.get 17
        local.get 18
        local.get 19
        call 40
        local.get 15
        local.get 4
        f64.promote_f32
        f32.demote_f64
        local.set 22
        local.set 21
        local.set 20
        local.get 20
        local.get 21
        local.get 22
        call 145
        local.get 0
        local.get 6
        f64.promote_f32
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 188
        i32.add
        call 141
        local.set 29
        local.get 7
        local.get 29
        i32.const 16
        memory.copy
        local.get 7
        local.set 14
        local.get 14
        call 144
        f64.promote_f32
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 204
        i32.add
        call 141
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.const 0x1.3d70a4p-2 (;=0.31;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        local.get 2
        local.set 11
        local.get 11
        local.get 28
        i32.const 220
        i32.add
        call 141
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.const 0x1.ae147ap-3 (;=0.21;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.mul
        f64.promote_f32
        f32.demote_f64
        local.set 19
        f32.demote_f64
        local.set 18
        f32.demote_f64
        local.set 17
        f32.demote_f64
        local.set 16
        local.get 28
        i32.const 236
        i32.add
        local.tee 15
        i32.const 0
        i32.const 16
        memory.fill
        local.get 15
        local.get 16
        local.get 17
        local.get 18
        local.get 19
        call 40
        local.get 15
        local.get 4
        f64.promote_f32
        f32.demote_f64
        local.set 22
        local.set 21
        local.set 20
        local.get 20
        local.get 21
        local.get 22
        call 145
        local.get 28
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 23
    local.get 28
    global.set 34
    local.get 23
    throw_ref
  )
  (func (;83;) (type 188) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 20
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;84;) (type 189) (param i32) (result i32)
    (local i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const -1195852033
        local.set 2
        local.get 8
        i32.const 0
        i32.add
        local.tee 1
        i32.const 0
        i32.const 16
        memory.fill
        local.get 1
        local.get 2
        call 146
        local.get 1
        local.set 9
        local.get 0
        local.get 9
        i32.const 16
        memory.copy
        local.get 0
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;85;) (type 190) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 36
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;86;) (type 191) (param i32) (result i32)
    (local i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 286331391
        local.set 2
        local.get 8
        i32.const 0
        i32.add
        local.tee 1
        i32.const 0
        i32.const 16
        memory.fill
        local.get 1
        local.get 2
        call 146
        local.get 1
        local.set 9
        local.get 0
        local.get 9
        i32.const 16
        memory.copy
        local.get 0
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;87;) (type 192) (param i32 i32) (result i32)
    (local i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 9
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 9
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 3
        local.get 9
        i32.const 0
        i32.add
        local.tee 2
        i32.const 0
        i32.const 16
        memory.fill
        local.get 2
        local.get 3
        call 146
        local.get 2
        local.set 10
        local.get 1
        local.get 10
        i32.const 16
        memory.copy
        local.get 1
        local.get 9
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 4
    local.get 9
    global.set 34
    local.get 4
    throw_ref
  )
  (func (;88;) (type 194) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 4
      array.new_data 0 1
      i32.const 0
      i32.const 4
      call 1
      global.set 1
    end
    global.get 1
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;89;) (type 196) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 3
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 9
      array.new_data 0 3
      i32.const 0
      i32.const 9
      call 1
      global.set 3
    end
    global.get 3
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;90;) (type 198) (param eqref i32 f32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 9
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 9
    i32.const 80
    i32.add
    global.set 34
    local.get 9
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 9
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 4
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 8
          array.new_data 0 4
          i32.const 0
          i32.const 8
          call 1
          global.set 4
        end
        global.get 4
        local.get 1
        local.set 10
        local.get 9
        i32.const 16
        i32.add
        local.get 10
        i32.const 16
        memory.copy
        local.get 9
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 10
        local.get 9
        i32.const 32
        i32.add
        local.get 10
        i32.const 16
        memory.copy
        local.get 9
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 10
        local.get 9
        i32.const 48
        i32.add
        local.get 10
        i32.const 16
        memory.copy
        local.get 9
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 10
        local.get 9
        i32.const 64
        i32.add
        local.get 10
        i32.const 16
        memory.copy
        local.get 9
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 2
          i32.const 0
          i32.const 0
          call 1
          global.set 2
        end
        global.get 2
        call 0
        global.get 5
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 9
          array.new_data 0 5
          i32.const 0
          i32.const 9
          call 1
          global.set 5
        end
        global.get 5
        local.get 2
        f64.promote_f32
        f64.const 0x0p+0 (;=0;)
        f64.const 0x0p+0 (;=0;)
        f64.const 0x0p+0 (;=0;)
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 2
          i32.const 0
          i32.const 0
          call 1
          global.set 2
        end
        global.get 2
        call 0
        local.get 9
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 4
    local.get 9
    global.set 34
    local.get 4
    throw_ref
  )
  (func (;91;) (type 200) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 6
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 4
      array.new_data 0 6
      i32.const 0
      i32.const 4
      call 1
      global.set 6
    end
    global.get 6
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;92;) (type 202) (param eqref i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 32
    i32.add
    global.set 34
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 7
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 9
          array.new_data 0 7
          i32.const 0
          i32.const 9
          call 1
          global.set 7
        end
        global.get 7
        local.get 1
        local.set 9
        local.get 8
        i32.const 8
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f64.const 0x0p+0 (;=0;)
        f64.const 0x0p+0 (;=0;)
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 2
          i32.const 0
          i32.const 0
          call 1
          global.set 2
        end
        global.get 2
        call 0
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;93;) (type 204) (param eqref i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 32
    i32.add
    global.set 34
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 8
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 5
          array.new_data 0 8
          i32.const 0
          i32.const 5
          call 1
          global.set 8
        end
        global.get 8
        local.get 1
        local.set 9
        local.get 8
        i32.const 8
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f64.const 0x0p+0 (;=0;)
        f64.const 0x0p+0 (;=0;)
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 2
          i32.const 0
          i32.const 0
          call 1
          global.set 2
        end
        global.get 2
        call 0
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;94;) (type 205) (param eqref i32 i32) (result i32)
    (local eqref i32 i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 13
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 19
        ref.cast (ref 16)
        struct.get 16 10
        i32.eqz
        if (type 206) ;; label = @3
        else
          local.get 0
          local.set 3
          local.get 3
          call 28
          i32.const 4
          i32.and
          if (type 207) ;; label = @4
          else
            local.get 0
            local.tee 19
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 19
            ref.cast (ref 16)
            local.set 19
            local.get 13
            i32.const 0
            i32.add
            local.get 19
            ref.cast (ref 16)
            struct.get 16 3
            f32.store align=1
            local.get 13
            i32.const 4
            i32.add
            local.get 19
            ref.cast (ref 16)
            struct.get 16 4
            f32.store align=1
            local.get 13
            i32.const 0
            i32.add
            local.get 2
            local.set 4
            local.get 4
            local.get 13
            i32.const 8
            i32.add
            call 95
            call 149
            i32.eqz
            if (type 208) ;; label = @5
            else
              local.get 0
              local.tee 19
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 19
              ref.cast (ref 16)
              struct.get 16 8
              local.get 2
              local.set 5
              local.get 5
              call 97
              i32.eq
              i32.eqz
              if (type 209) ;; label = @6
              else
                local.get 0
                local.tee 19
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 19
                ref.cast (ref 16)
                struct.get 16 9
                local.get 2
                local.set 6
                local.get 6
                call 99
                i32.eq
                i32.eqz
                if (type 210) ;; label = @7
                else
                  local.get 0
                  local.tee 19
                  ref.is_null
                  if ;; label = @8
                    i32.const 4
                    global.get 30
                    ref.is_null
                    if ;; label = @9
                      i32.const 0
                      i32.const 53
                      array.new_data 0 30
                      i32.const 0
                      i32.const 53
                      call 1
                      global.set 30
                    end
                    global.get 30
                    struct.new 3
                    throw 0
                  end
                  local.get 19
                  ref.cast (ref 16)
                  struct.get 16 7
                  local.get 1
                  local.set 7
                  local.get 7
                  call 103
                  ref.eq
                  i32.const 255
                  i32.and
                  local.get 13
                  global.set 34
                  return
                end
              end
            end
          end
        end
        i32.const 0
        i32.const 255
        i32.and
        local.get 13
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 34
    local.get 8
    throw_ref
  )
  (func (;95;) (type 211) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;96;) (type 212) (param i32 i32 i32) (result i32)
    (local f32 i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 12
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 64
    i32.add
    global.set 34
    local.get 12
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 0
    local.get 12
    i32.const 8
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 12
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 13
        local.get 12
        i32.const 16
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 24
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.max
        f64.promote_f32
        local.get 0
        local.set 13
        local.get 12
        i32.const 32
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 40
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 40
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.max
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 12
        i32.const 48
        i32.add
        local.tee 4
        i32.const 0
        i32.const 8
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        call 19
        local.get 4
        local.get 12
        i32.const 56
        i32.add
        call 20
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 34
    local.get 7
    throw_ref
  )
  (func (;97;) (type 213) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 8
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;98;) (type 214) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 46
    i32.const 255
    i32.and
    return
  )
  (func (;99;) (type 215) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 9
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;100;) (type 216) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 47
    i32.const 255
    i32.and
    return
  )
  (func (;101;) (type 217) (param eqref i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 13
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 13
        ref.cast (ref 16)
        local.set 13
        local.get 7
        i32.const 0
        i32.add
        local.get 13
        ref.cast (ref 16)
        struct.get 16 42
        f32.store align=1
        local.get 7
        i32.const 4
        i32.add
        local.get 13
        ref.cast (ref 16)
        struct.get 16 43
        f32.store align=1
        local.get 7
        i32.const 8
        i32.add
        local.get 13
        ref.cast (ref 16)
        struct.get 16 44
        f32.store align=1
        local.get 7
        i32.const 12
        i32.add
        local.get 13
        ref.cast (ref 16)
        struct.get 16 45
        f32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;102;) (type 218) (param i32 i32 i32) (result i32)
    (local f32 i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 12
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 128
    i32.add
    global.set 34
    local.get 12
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 0
    local.get 12
    i32.const 8
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 12
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 13
        local.get 12
        i32.const 24
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 32
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 32
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 48
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 48
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 0
        local.set 13
        local.get 12
        i32.const 64
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 64
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 72
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 72
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 88
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 88
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 12
        i32.const 104
        i32.add
        local.tee 4
        i32.const 0
        i32.const 8
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        call 19
        local.get 4
        local.get 12
        i32.const 112
        i32.add
        call 20
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 34
    local.get 7
    throw_ref
  )
  (func (;103;) (type 219) (param i32) (result eqref)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 16
        i32.add
        i32.load align=1
        table.get 1
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;104;) (type 220) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 51
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;105;) (type 221) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 53
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;106;) (type 222) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 52
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;107;) (type 223) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 54
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;108;) (type 224) (param i32 i32 i32) (result i32)
    (local f32 i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 12
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 128
    i32.add
    global.set 34
    local.get 12
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 0
    local.get 12
    i32.const 8
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 12
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 13
        local.get 12
        i32.const 24
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 32
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 32
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 48
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 48
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 0
        local.set 13
        local.get 12
        i32.const 64
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 64
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 72
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 72
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 1
        local.set 13
        local.get 12
        i32.const 88
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 88
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 12
        i32.const 104
        i32.add
        local.tee 4
        i32.const 0
        i32.const 8
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        call 19
        local.get 4
        local.get 12
        i32.const 112
        i32.add
        call 20
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 34
    local.get 7
    throw_ref
  )
  (func (;109;) (type 225) (param eqref)
    (local eqref eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 28
    i32.const -5
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 138
    return
  )
  (func (;110;) (type 226) (param eqref i32 i32) (result i32)
    (local eqref i32 i32 i32 i32 i32 i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 17
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 48
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 23
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 23
        ref.cast (ref 16)
        struct.get 16 22
        i32.eqz
        if (type 227) ;; label = @3
        else
          local.get 0
          local.set 3
          local.get 3
          call 28
          i32.const 24
          i32.and
          if (type 228) ;; label = @4
          else
            local.get 0
            local.tee 23
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 23
            ref.cast (ref 16)
            local.set 23
            local.get 17
            i32.const 0
            i32.add
            local.get 23
            ref.cast (ref 16)
            struct.get 16 11
            f32.store align=1
            local.get 17
            i32.const 4
            i32.add
            local.get 23
            ref.cast (ref 16)
            struct.get 16 12
            f32.store align=1
            local.get 17
            i32.const 0
            i32.add
            local.get 2
            local.set 4
            local.get 4
            local.get 17
            i32.const 8
            i32.add
            call 116
            call 149
            i32.eqz
            if (type 229) ;; label = @5
            else
              local.get 0
              local.tee 23
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 23
              ref.cast (ref 16)
              local.set 23
              local.get 17
              i32.const 16
              i32.add
              local.get 23
              ref.cast (ref 16)
              struct.get 16 13
              f32.store align=1
              local.get 17
              i32.const 20
              i32.add
              local.get 23
              ref.cast (ref 16)
              struct.get 16 14
              f32.store align=1
              local.get 17
              i32.const 16
              i32.add
              local.get 2
              local.set 5
              local.get 5
              local.get 17
              i32.const 24
              i32.add
              call 113
              call 149
              i32.eqz
              if (type 230) ;; label = @6
              else
                local.get 0
                local.tee 23
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 30
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 30
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 30
                  end
                  global.get 30
                  struct.new 3
                  throw 0
                end
                local.get 23
                ref.cast (ref 16)
                local.set 23
                local.get 17
                i32.const 32
                i32.add
                local.get 23
                ref.cast (ref 16)
                struct.get 16 15
                f32.store align=1
                local.get 17
                i32.const 36
                i32.add
                local.get 23
                ref.cast (ref 16)
                struct.get 16 16
                f32.store align=1
                local.get 17
                i32.const 32
                i32.add
                local.get 2
                local.set 6
                local.get 6
                local.get 17
                i32.const 40
                i32.add
                call 112
                call 151
                i32.eqz
                if (type 231) ;; label = @7
                else
                  local.get 0
                  local.tee 23
                  ref.is_null
                  if ;; label = @8
                    i32.const 4
                    global.get 30
                    ref.is_null
                    if ;; label = @9
                      i32.const 0
                      i32.const 53
                      array.new_data 0 30
                      i32.const 0
                      i32.const 53
                      call 1
                      global.set 30
                    end
                    global.get 30
                    struct.new 3
                    throw 0
                  end
                  local.get 23
                  ref.cast (ref 16)
                  struct.get 16 17
                  local.get 2
                  local.set 7
                  local.get 7
                  call 117
                  i32.eq
                  i32.eqz
                  if (type 232) ;; label = @8
                  else
                    local.get 0
                    local.tee 23
                    ref.is_null
                    if ;; label = @9
                      i32.const 4
                      global.get 30
                      ref.is_null
                      if ;; label = @10
                        i32.const 0
                        i32.const 53
                        array.new_data 0 30
                        i32.const 0
                        i32.const 53
                        call 1
                        global.set 30
                      end
                      global.get 30
                      struct.new 3
                      throw 0
                    end
                    local.get 23
                    ref.cast (ref 16)
                    struct.get 16 18
                    local.get 2
                    local.set 8
                    local.get 8
                    call 118
                    i32.eq
                    i32.eqz
                    if (type 233) ;; label = @9
                    else
                      local.get 0
                      local.tee 23
                      ref.is_null
                      if ;; label = @10
                        i32.const 4
                        global.get 30
                        ref.is_null
                        if ;; label = @11
                          i32.const 0
                          i32.const 53
                          array.new_data 0 30
                          i32.const 0
                          i32.const 53
                          call 1
                          global.set 30
                        end
                        global.get 30
                        struct.new 3
                        throw 0
                      end
                      local.get 23
                      ref.cast (ref 16)
                      struct.get 16 19
                      local.get 2
                      local.set 9
                      local.get 9
                      call 114
                      i32.eq
                      i32.eqz
                      if (type 234) ;; label = @10
                      else
                        local.get 0
                        local.tee 23
                        ref.is_null
                        if ;; label = @11
                          i32.const 4
                          global.get 30
                          ref.is_null
                          if ;; label = @12
                            i32.const 0
                            i32.const 53
                            array.new_data 0 30
                            i32.const 0
                            i32.const 53
                            call 1
                            global.set 30
                          end
                          global.get 30
                          struct.new 3
                          throw 0
                        end
                        local.get 23
                        ref.cast (ref 16)
                        struct.get 16 20
                        local.get 2
                        local.set 10
                        local.get 10
                        call 115
                        i32.eq
                        i32.eqz
                        if (type 235) ;; label = @11
                        else
                          local.get 0
                          local.tee 23
                          ref.is_null
                          if ;; label = @12
                            i32.const 4
                            global.get 30
                            ref.is_null
                            if ;; label = @13
                              i32.const 0
                              i32.const 53
                              array.new_data 0 30
                              i32.const 0
                              i32.const 53
                              call 1
                              global.set 30
                            end
                            global.get 30
                            struct.new 3
                            throw 0
                          end
                          local.get 23
                          ref.cast (ref 16)
                          struct.get 16 21
                          local.get 1
                          local.set 11
                          local.get 11
                          call 103
                          ref.eq
                          i32.const 255
                          i32.and
                          local.get 17
                          global.set 34
                          return
                        end
                      end
                    end
                  end
                end
              end
            end
          end
        end
        i32.const 0
        i32.const 255
        i32.and
        local.get 17
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 17
    global.set 34
    local.get 12
    throw_ref
  )
  (func (;111;) (type 236) (param eqref i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 13
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 13
        ref.cast (ref 16)
        local.set 13
        local.get 7
        i32.const 0
        i32.add
        local.get 13
        ref.cast (ref 16)
        struct.get 16 38
        f32.store align=1
        local.get 7
        i32.const 4
        i32.add
        local.get 13
        ref.cast (ref 16)
        struct.get 16 39
        f32.store align=1
        local.get 7
        i32.const 8
        i32.add
        local.get 13
        ref.cast (ref 16)
        struct.get 16 40
        f32.store align=1
        local.get 7
        i32.const 12
        i32.add
        local.get 13
        ref.cast (ref 16)
        struct.get 16 41
        f32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;112;) (type 237) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 16
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;113;) (type 238) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 8
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;114;) (type 239) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 26
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;115;) (type 240) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 27
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;116;) (type 241) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;117;) (type 242) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 24
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;118;) (type 243) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 25
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;119;) (type 244) (param i32 i32 i32) (result i32)
    (local f32 i32 f32 f32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 14
    i32.const 1048368
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 208
    i32.add
    global.set 34
    local.get 14
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 14
    i32.const 0
    i32.add
    local.set 0
    local.get 14
    i32.const 16
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 14
    i32.const 16
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 15
        local.get 14
        i32.const 32
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 32
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 15
        local.get 14
        i32.const 48
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 48
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 0
        local.set 15
        local.get 14
        i32.const 64
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 64
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 15
        local.get 14
        i32.const 80
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 80
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 0
        local.set 15
        local.get 14
        i32.const 96
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 96
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 15
        local.get 14
        i32.const 112
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 112
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 1
        local.set 15
        local.get 14
        i32.const 128
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 128
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 0
        local.set 15
        local.get 14
        i32.const 144
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 144
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 15
        local.get 14
        i32.const 160
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 160
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 1
        local.set 15
        local.get 14
        i32.const 176
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 176
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 14
        i32.const 192
        i32.add
        local.tee 4
        i32.const 0
        i32.const 16
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        local.get 7
        local.get 8
        call 40
        local.get 4
        local.set 15
        local.get 2
        local.get 15
        i32.const 16
        memory.copy
        local.get 2
        local.get 14
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 14
    global.set 34
    local.get 9
    throw_ref
  )
  (func (;120;) (type 245) (param i32 i32) (result i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048416
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 160
    i32.add
    global.set 34
    local.get 8
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 0
    local.get 8
    i32.const 16
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 16
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 9
        local.get 8
        i32.const 32
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 32
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.ne
        if (type 246) ;; label = @3
        else
          local.get 0
          local.set 9
          local.get 8
          i32.const 64
          i32.add
          local.get 9
          i32.const 16
          memory.copy
          local.get 8
          i32.const 64
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 1
          local.set 9
          local.get 8
          i32.const 80
          i32.add
          local.get 9
          i32.const 16
          memory.copy
          local.get 8
          i32.const 80
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.ne
          if (type 247) ;; label = @4
          else
            local.get 0
            local.set 9
            local.get 8
            i32.const 96
            i32.add
            local.get 9
            i32.const 16
            memory.copy
            local.get 8
            i32.const 96
            i32.add
            i32.const 8
            i32.add
            f32.load align=1
            f64.promote_f32
            local.get 1
            local.set 9
            local.get 8
            i32.const 112
            i32.add
            local.get 9
            i32.const 16
            memory.copy
            local.get 8
            i32.const 112
            i32.add
            i32.const 8
            i32.add
            f32.load align=1
            f64.promote_f32
            f32.demote_f64
            local.set 2
            f32.demote_f64
            local.get 2
            f32.ne
            if (type 248) ;; label = @5
            else
              local.get 0
              local.set 9
              local.get 8
              i32.const 128
              i32.add
              local.get 9
              i32.const 16
              memory.copy
              local.get 8
              i32.const 128
              i32.add
              i32.const 12
              i32.add
              f32.load align=1
              f64.promote_f32
              local.get 1
              local.set 9
              local.get 8
              i32.const 144
              i32.add
              local.get 9
              i32.const 16
              memory.copy
              local.get 8
              i32.const 144
              i32.add
              i32.const 12
              i32.add
              f32.load align=1
              f64.promote_f32
              f32.demote_f64
              local.set 2
              f32.demote_f64
              local.get 2
              f32.ne
              i32.const 255
              i32.and
              local.get 8
              global.set 34
              return
            end
          end
        end
        i32.const 1
        i32.const 255
        i32.and
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;121;) (type 249) (param eqref)
    (local i32 i64 f64 externref eqref eqref i32 eqref eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 5
    local.get 5
    call 28
    i32.const 32
    i32.and
    i32.eqz
    if (type 250) ;; label = @1
      local.get 0
      local.get 0
      local.set 5
      local.get 5
      call 28
      i32.const 32
      i32.or
      local.set 7
      local.set 6
      local.get 6
      local.get 7
      call 138
      local.get 0
      local.set 8
      local.get 8
      call 125
      local.tee 11
      local.get 11
      ref.is_null
      i32.eqz
      if (type 251) (param eqref) ;; label = @2
        local.get 0
        local.set 10
        local.set 9
        local.get 9
        local.tee 12
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 12
        ref.cast (ref 16)
        local.get 10
        local.get 9
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 51
        i32.add
        call_indirect (type 315)
        return
      else
        drop
        return
      end
      unreachable
    else
      return
    end
    unreachable
  )
  (func (;122;) (type 252) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 32
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 16
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 16
        i32.add
        local.set 14
        local.tee 13
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 13
        ref.cast (ref 16)
        local.set 13
        local.get 13
        ref.cast (ref 16)
        local.get 14
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 16 38
        local.get 13
        ref.cast (ref 16)
        local.get 14
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 16 39
        local.get 13
        ref.cast (ref 16)
        local.get 14
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 16 40
        local.get 13
        ref.cast (ref 16)
        local.get 14
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 16 41
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;123;) (type 253) (param eqref)
    (local eqref eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 28
    i32.const -9
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 138
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 28
    i32.const -17
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 138
    return
  )
  (func (;124;) (type 254) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;125;) (type 255) (param eqref) (result eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 37
    return
  )
  (func (;126;) (type 256) (param i32 i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 32
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 16
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 16
        i32.add
        local.set 9
        i32.const 4
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 0
        i32.const 1
        local.set 9
        i32.const 0
        i32.add
        local.get 9
        i32.store8
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;127;) (type 257) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;128;) (type 258) (param i32 i32) (result i32)
    (local f32 i32 i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 34
    local.get 12
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 3
        local.get 3
        call 142
        f64.promote_f32
        local.get 1
        local.set 4
        local.get 4
        call 143
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.le
        i32.eqz
        if (type 259) ;; label = @3
        else
          local.get 0
          local.set 4
          local.get 4
          call 143
          f64.promote_f32
          local.get 1
          local.set 3
          local.get 3
          call 142
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.ge
          i32.eqz
          if (type 260) ;; label = @4
          else
            local.get 0
            local.set 5
            local.get 5
            call 144
            f64.promote_f32
            local.get 1
            local.set 6
            local.get 6
            call 154
            f64.promote_f32
            f32.demote_f64
            local.set 2
            f32.demote_f64
            local.get 2
            f32.le
            i32.eqz
            if (type 261) ;; label = @5
            else
              local.get 0
              local.set 6
              local.get 6
              call 154
              f64.promote_f32
              local.get 1
              local.set 5
              local.get 5
              call 144
              f64.promote_f32
              f32.demote_f64
              local.set 2
              f32.demote_f64
              local.get 2
              f32.ge
              i32.const 255
              i32.and
              local.get 12
              global.set 34
              return
            end
          end
        end
        i32.const 0
        i32.const 255
        i32.and
        local.get 12
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 34
    local.get 7
    throw_ref
  )
  (func (;129;) (type 262) (param i32) (result eqref)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 20
        i32.add
        i32.load align=1
        table.get 1
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;130;) (type 263) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 16)
    struct.get 16 30
    if (type 264) ;; label = @1
      i32.const 1
      i32.const 255
      i32.and
      return
    else
      local.get 0
      local.set 1
      local.get 1
      call 28
      i32.const 96
      i32.and
      i32.const 0
      i32.gt_u
      i32.const 255
      i32.and
      return
    end
    unreachable
  )
  (func (;131;) (type 265) (param eqref)
    (local eqref eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 28
    i32.const 96
    i32.and
    drop
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 28
    i32.const -33
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 138
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 28
    i32.const -65
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 138
    return
  )
  (func (;132;) (type 267) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 9
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 7
      array.new_data 0 9
      i32.const 0
      i32.const 7
      call 1
      global.set 9
    end
    global.get 9
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;133;) (type 268) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 49)
    struct.get 49 11
    return
  )
  (func (;134;) (type 269) (param eqref i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        ref.cast (ref 49)
        local.set 13
        local.get 7
        i32.const 0
        i32.add
        local.get 13
        ref.cast (ref 49)
        struct.get 49 12
        f32.store align=1
        local.get 7
        i32.const 4
        i32.add
        local.get 13
        ref.cast (ref 49)
        struct.get 49 13
        f32.store align=1
        local.get 7
        i32.const 8
        i32.add
        local.get 13
        ref.cast (ref 49)
        struct.get 49 14
        f32.store align=1
        local.get 7
        i32.const 12
        i32.add
        local.get 13
        ref.cast (ref 49)
        struct.get 49 15
        f32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;135;) (type 270) (param i32 i32) (result i32)
    (local f32 i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 11
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        i32.const 2
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.add
        f64.promote_f32
        local.get 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        i32.const 2
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.set 4
        local.get 11
        i32.const 0
        i32.add
        local.tee 3
        i32.const 0
        i32.const 8
        memory.fill
        local.get 3
        local.get 4
        local.get 5
        call 29
        local.get 3
        local.set 12
        local.get 1
        local.get 12
        i32.const 8
        memory.copy
        local.get 1
        local.get 11
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 34
    local.get 6
    throw_ref
  )
  (func (;136;) (type 271) (param eqref)
    (local f32 i32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 10
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 32
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 10
        i32.const 0
        i32.add
        local.tee 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        call 19
        local.get 2
        local.get 10
        i32.const 8
        i32.add
        call 160
        local.set 17
        local.tee 16
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 16
        ref.cast (ref 16)
        local.set 16
        local.get 16
        ref.cast (ref 16)
        local.get 17
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 16 42
        local.get 16
        ref.cast (ref 16)
        local.get 17
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 16 43
        local.get 16
        ref.cast (ref 16)
        local.get 17
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 16 44
        local.get 16
        ref.cast (ref 16)
        local.get 17
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 16 45
        local.get 0
        i32.const 1
        local.set 17
        local.tee 16
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 16
        ref.cast (ref 16)
        local.set 16
        local.get 16
        ref.cast (ref 16)
        local.get 17
        i32.const 255
        i32.and
        struct.set 16 49
        local.get 0
        i32.const 1
        local.set 17
        local.tee 16
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 16
        ref.cast (ref 16)
        local.set 16
        local.get 16
        ref.cast (ref 16)
        local.get 17
        i32.const 255
        i32.and
        struct.set 16 50
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        local.set 19
        local.tee 16
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 16
        ref.cast (ref 16)
        local.set 16
        local.get 16
        ref.cast (ref 16)
        local.get 19
        f32.demote_f64
        struct.set 16 51
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        local.set 19
        local.tee 16
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 16
        ref.cast (ref 16)
        local.set 16
        local.get 16
        ref.cast (ref 16)
        local.get 19
        f32.demote_f64
        struct.set 16 52
        local.get 0
        f64.const inf (;=inf;)
        local.set 19
        local.tee 16
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 16
        ref.cast (ref 16)
        local.set 16
        local.get 16
        ref.cast (ref 16)
        local.get 19
        f32.demote_f64
        struct.set 16 53
        local.get 0
        f64.const inf (;=inf;)
        local.set 19
        local.tee 16
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 16
        ref.cast (ref 16)
        local.set 16
        local.get 16
        ref.cast (ref 16)
        local.get 19
        f32.demote_f64
        struct.set 16 54
        local.get 0
        drop
        local.get 10
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 10
    global.set 34
    local.get 5
    throw_ref
  )
  (func (;137;) (type 272) (param eqref)
    (local i32 i64 f64 externref eqref eqref i32 eqref eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 5
    local.get 5
    call 28
    i32.const 1
    i32.and
    i32.eqz
    if (type 273) ;; label = @1
      local.get 0
      local.get 0
      local.set 5
      local.get 5
      call 28
      i32.const 1
      i32.or
      local.set 7
      local.set 6
      local.get 6
      local.get 7
      call 138
      local.get 0
      local.set 8
      local.get 8
      call 125
      local.tee 11
      local.get 11
      ref.is_null
      i32.eqz
      if (type 274) (param eqref) ;; label = @2
        local.get 0
        local.set 10
        local.set 9
        local.get 9
        local.tee 12
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 12
        ref.cast (ref 16)
        local.get 10
        local.get 9
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 46
        i32.add
        call_indirect (type 326)
        return
      else
        drop
        return
      end
      unreachable
    else
      return
    end
    unreachable
  )
  (func (;138;) (type 275) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 4
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 16)
    local.set 3
    local.get 3
    ref.cast (ref 16)
    local.get 4
    struct.set 16 29
    return
  )
  (func (;139;) (type 276) (param eqref i32 i32)
    (local i32 eqref i32 eqref i32 i32 eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 15
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 32
    i32.add
    global.set 34
    local.get 15
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 15
    i32.const 0
    i32.add
    local.set 1
    local.get 15
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 15
    i32.const 8
    i32.add
    local.set 2
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 0
        local.set 3
        block (type 277) ;; label = @3
          loop (type 277) ;; label = @4
            local.get 3
            local.get 0
            local.set 9
            local.get 9
            local.tee 21
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 21
            ref.cast (ref 16)
            local.get 9
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 29
            i32.add
            call_indirect (type 330)
            i32.lt_s
            if (type 278) ;; label = @5
              local.get 0
              local.get 3
              local.set 5
              local.set 4
              local.get 4
              local.tee 21
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 21
              ref.cast (ref 16)
              local.get 5
              local.get 4
              ref.cast (ref 10)
              struct.get 10 0
              ref.cast (ref 9)
              struct.get 9 2
              i32.const 30
              i32.add
              call_indirect (type 329)
              local.get 1
              local.set 16
              local.get 15
              i32.const 16
              i32.add
              local.get 16
              i32.const 8
              memory.copy
              local.get 15
              i32.const 16
              i32.add
              local.get 2
              local.set 16
              local.get 15
              i32.const 24
              i32.add
              local.get 16
              i32.const 8
              memory.copy
              local.get 15
              i32.const 24
              i32.add
              local.set 8
              local.set 7
              local.set 6
              local.get 6
              local.tee 21
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 21
              ref.cast (ref 16)
              local.get 7
              local.get 8
              call 17
              local.get 3
              i32.const 1
              i32.add
              local.set 3
              br 1 (;@4;)
            else
              br 2 (;@3;)
            end
            unreachable
          end
        end
        local.get 15
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 15
    global.set 34
    local.get 10
    throw_ref
  )
  (func (;140;) (type 279) (param eqref i32 i32)
    (local i32 eqref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 11
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 16
    i32.add
    global.set 34
    local.get 11
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 11
    i32.const 0
    i32.add
    local.set 1
    local.get 11
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 11
    i32.const 8
    i32.add
    local.set 2
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 17
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 17
        ref.cast (ref 37)
        struct.get 37 58
        i32.eqz
        if (type 280) ;; label = @3
          local.get 0
          local.get 0
          local.tee 17
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 17
          ref.cast (ref 37)
          struct.get 37 57
          local.get 2
          local.set 3
          local.get 3
          call 161
          local.get 1
          local.set 3
          local.get 3
          call 161
          i64.sub
          f64.convert_i64_s
          f64.const 0x1.ad7f29abcaf48p-24 (;=0.0000001;)
          f64.mul
          f64.add
          local.set 20
          local.tee 17
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 17
          ref.cast (ref 37)
          local.set 17
          local.get 17
          ref.cast (ref 37)
          local.get 20
          struct.set 37 57
          local.get 0
          local.set 4
          local.get 4
          call 121
          local.get 0
          local.tee 17
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 17
          ref.cast (ref 37)
          struct.get 37 57
          f64.const 0x1p+0 (;=1;)
          f64.lt
          i32.eqz
          if (type 281) ;; label = @4
          else
            local.get 0
            local.set 5
            local.get 5
            call 137
          end
          local.get 11
          global.set 34
          return
        else
          local.get 11
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 34
    local.get 6
    throw_ref
  )
  (func (;141;) (type 282) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;142;) (type 283) (param i32) (result f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;143;) (type 284) (param i32) (result f32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 1
        f32.demote_f64
        local.get 1
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;144;) (type 285) (param i32) (result f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;145;) (type 286) (param eqref i32 f32)
    (local eqref eqref i32 f32 eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 16
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 16
    i32.const 48
    i32.add
    global.set 34
    local.get 16
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 16
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 3
        local.get 3
        local.tee 22
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 22
        ref.cast (ref 10)
        local.get 3
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 1
        i32.add
        call_indirect (type 195)
        local.get 0
        local.get 1
        local.set 17
        local.get 16
        i32.const 16
        i32.add
        local.get 17
        i32.const 16
        memory.copy
        local.get 16
        i32.const 16
        i32.add
        local.get 2
        f64.promote_f32
        f32.demote_f64
        local.set 6
        local.set 5
        local.set 4
        local.get 4
        local.tee 22
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 22
        ref.cast (ref 10)
        local.get 5
        local.get 6
        local.get 4
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 2
        i32.add
        call_indirect (type 197)
        local.get 0
        local.get 16
        i32.const 32
        i32.add
        call 84
        local.set 8
        local.set 7
        local.get 7
        local.tee 22
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 22
        ref.cast (ref 10)
        local.get 8
        local.get 7
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 0
        i32.const 0
        local.set 10
        local.set 9
        local.get 9
        local.tee 22
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 22
        ref.cast (ref 10)
        local.get 10
        local.get 9
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 8
        i32.add
        call_indirect (type 290)
        local.get 16
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 11
    local.get 16
    global.set 34
    local.get 11
    throw_ref
  )
  (func (;146;) (type 287) (param i32 i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        i32.const -16777216
        i32.and
        i32.const 24
        i32.shr_u
        f64.convert_i32_u
        f32.demote_f64
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        local.set 12
        i32.const 0
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 1
        i32.const 16711680
        i32.and
        i32.const 16
        i32.shr_u
        f64.convert_i32_u
        f32.demote_f64
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        local.set 12
        i32.const 4
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 1
        i32.const 65280
        i32.and
        i32.const 8
        i32.shr_u
        f64.convert_i32_u
        f32.demote_f64
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        local.set 12
        i32.const 8
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 1
        i32.const 255
        i32.and
        f64.convert_i32_u
        f32.demote_f64
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        local.set 12
        i32.const 12
        i32.add
        local.get 12
        f32.demote_f64
        f32.store align=1
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;147;) (type 289) (param eqref i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 80
    i32.add
    global.set 34
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 10
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 7
          array.new_data 0 10
          i32.const 0
          i32.const 7
          call 1
          global.set 10
        end
        global.get 10
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 32
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 64
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 2
          i32.const 0
          i32.const 0
          call 1
          global.set 2
        end
        global.get 2
        call 0
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;148;) (type 291) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 11
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 4
      array.new_data 0 11
      i32.const 0
      i32.const 4
      call 1
      global.set 11
    end
    global.get 11
    local.get 1
    f64.convert_i32_s
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;149;) (type 292) (param i32 i32) (result i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 48
    i32.add
    global.set 34
    local.get 8
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 0
    local.get 8
    i32.const 8
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 8
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 24
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.eq
        i32.eqz
        if (type 293) ;; label = @3
          i32.const 0
          i32.const 255
          i32.and
          local.get 8
          global.set 34
          return
        else
          local.get 0
          local.set 9
          local.get 8
          i32.const 32
          i32.add
          local.get 9
          i32.const 8
          memory.copy
          local.get 8
          i32.const 32
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 1
          local.set 9
          local.get 8
          i32.const 40
          i32.add
          local.get 9
          i32.const 8
          memory.copy
          local.get 8
          i32.const 40
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.eq
          i32.const 255
          i32.and
          local.get 8
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;150;) (type 294) (param eqref i32 eqref i32) (result i32)
    (local i32 i32 f32 i32 f32 f32 eqref i32 eqref i32 eqref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 21
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 21
    i32.const 80
    i32.add
    global.set 34
    local.get 21
    i32.const 8
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 8
    memory.fill
    local.get 21
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 21
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.set 8
        local.get 21
        i32.const 16
        i32.add
        local.tee 7
        i32.const 0
        i32.const 8
        memory.fill
        local.get 7
        local.get 8
        local.get 9
        call 19
        local.get 7
        local.get 21
        i32.const 24
        i32.add
        call 20
        local.set 22
        local.get 4
        local.get 22
        i32.const 8
        memory.copy
        i32.const 0
        local.set 5
        block (type 295) ;; label = @3
          loop (type 295) ;; label = @4
            local.get 5
            local.get 0
            local.set 15
            local.get 15
            local.tee 27
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 27
            ref.cast (ref 16)
            local.get 15
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 29
            i32.add
            call_indirect (type 330)
            i32.lt_s
            if (type 296) ;; label = @5
              local.get 4
              local.set 22
              local.get 21
              i32.const 32
              i32.add
              local.get 22
              i32.const 8
              memory.copy
              local.get 21
              i32.const 32
              i32.add
              local.get 0
              local.get 5
              local.set 11
              local.set 10
              local.get 10
              local.tee 27
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 27
              ref.cast (ref 16)
              local.get 11
              local.get 10
              ref.cast (ref 10)
              struct.get 10 0
              ref.cast (ref 9)
              struct.get 9 2
              i32.const 30
              i32.add
              call_indirect (type 329)
              local.get 1
              local.set 22
              local.get 21
              i32.const 40
              i32.add
              local.get 22
              i32.const 8
              memory.copy
              local.get 21
              i32.const 40
              i32.add
              local.get 2
              local.set 14
              local.set 13
              local.set 12
              local.get 12
              local.tee 27
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 27
              ref.cast (ref 16)
              local.get 13
              local.get 14
              local.get 21
              i32.const 48
              i32.add
              call 21
              local.get 21
              i32.const 56
              i32.add
              call 96
              local.set 22
              local.get 4
              local.get 22
              i32.const 8
              memory.copy
              local.get 5
              i32.const 1
              i32.add
              local.set 5
              br 1 (;@4;)
            else
              br 2 (;@3;)
            end
            unreachable
          end
        end
        local.get 4
        local.set 22
        local.get 21
        i32.const 64
        i32.add
        local.get 22
        i32.const 8
        memory.copy
        local.get 21
        i32.const 64
        i32.add
        local.set 22
        local.get 3
        local.get 22
        i32.const 8
        memory.copy
        local.get 3
        local.get 21
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 16
    local.get 21
    global.set 34
    local.get 16
    throw_ref
  )
  (func (;151;) (type 297) (param i32 i32) (result i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 48
    i32.add
    global.set 34
    local.get 8
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 0
    local.get 8
    i32.const 8
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 8
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 24
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.eq
        i32.eqz
        if (type 298) ;; label = @3
          i32.const 0
          i32.const 255
          i32.and
          local.get 8
          global.set 34
          return
        else
          local.get 0
          local.set 9
          local.get 8
          i32.const 32
          i32.add
          local.get 9
          i32.const 8
          memory.copy
          local.get 8
          i32.const 32
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 1
          local.set 9
          local.get 8
          i32.const 40
          i32.add
          local.get 9
          i32.const 8
          memory.copy
          local.get 8
          i32.const 40
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.eq
          i32.const 255
          i32.and
          local.get 8
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;152;) (type 299) (param eqref i32 eqref)
    (local i32 i32 eqref i32 eqref i32 eqref i32 eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 17
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 80
    i32.add
    global.set 34
    local.get 17
    i32.const 16
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 12
    memory.fill
    local.get 17
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 17
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 0
        local.set 3
        block (type 300) ;; label = @3
          loop (type 300) ;; label = @4
            local.get 3
            local.get 0
            local.set 11
            local.get 11
            local.tee 23
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 30
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 30
                i32.const 0
                i32.const 53
                call 1
                global.set 30
              end
              global.get 30
              struct.new 3
              throw 0
            end
            local.get 23
            ref.cast (ref 16)
            local.get 11
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 29
            i32.add
            call_indirect (type 330)
            i32.lt_s
            if (type 301) ;; label = @5
              local.get 0
              local.get 3
              local.set 6
              local.set 5
              local.get 5
              local.tee 23
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 23
              ref.cast (ref 16)
              local.get 6
              local.get 5
              ref.cast (ref 10)
              struct.get 10 0
              ref.cast (ref 9)
              struct.get 9 2
              i32.const 30
              i32.add
              call_indirect (type 329)
              local.get 1
              local.set 18
              local.get 17
              i32.const 28
              i32.add
              local.get 18
              i32.const 16
              memory.copy
              local.get 17
              i32.const 28
              i32.add
              local.get 2
              local.get 4
              i32.const 0
              i32.const 12
              memory.fill
              local.get 4
              local.set 18
              local.get 17
              i32.const 44
              i32.add
              local.get 18
              i32.const 12
              memory.copy
              local.get 17
              i32.const 44
              i32.add
              local.set 10
              local.set 9
              local.set 8
              local.set 7
              local.get 7
              local.tee 23
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 30
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 30
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 30
                end
                global.get 30
                struct.new 3
                throw 0
              end
              local.get 23
              ref.cast (ref 16)
              local.get 8
              local.get 9
              local.get 10
              local.get 17
              i32.const 56
              i32.add
              call 25
              drop
              local.get 3
              i32.const 1
              i32.add
              local.set 3
              br 1 (;@4;)
            else
              br 2 (;@3;)
            end
            unreachable
          end
        end
        local.get 17
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 17
    global.set 34
    local.get 12
    throw_ref
  )
  (func (;153;) (type 302) (param eqref i32) (result i32)
    (local eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 2
        local.get 2
        local.get 8
        i32.const 0
        i32.add
        call 111
        local.set 9
        local.get 1
        local.get 9
        i32.const 16
        memory.copy
        local.get 1
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;154;) (type 303) (param i32) (result f32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 1
        f32.demote_f64
        local.get 1
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;155;) (type 304) (param eqref i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 32
    i32.add
    global.set 34
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        local.set 9
        local.get 2
        local.get 9
        i32.const 16
        memory.copy
        local.get 2
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;156;) (type 305) (param eqref eqref)
    (local i32 i32 i32 i32 i32 f32 eqref f32 eqref i32 eqref i32 eqref f32 eqref f32 eqref f32 eqref i32 eqref i32 eqref i32 eqref f32 externref i32 i32 i32 i32 eqref eqref eqref i32 eqref i32 eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 45
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 45
    i32.const 128
    i32.add
    global.set 34
    local.get 45
    i32.const 0
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 8
    memory.fill
    local.get 45
    i32.const 8
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 8
    memory.fill
    local.get 45
    i32.const 16
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 12
    memory.fill
    local.get 45
    i32.const 28
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 8
    memory.fill
    local.get 45
    i32.const 36
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 8
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 9
        local.set 8
        local.get 8
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 9
        local.get 8
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 10
        i32.add
        call_indirect (type 331)
        local.get 1
        i32.const 0
        local.set 11
        local.set 10
        local.get 10
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 11
        local.get 10
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 11
        i32.add
        call_indirect (type 333)
        local.get 1
        i32.const 0
        local.set 13
        local.set 12
        local.get 12
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 13
        local.get 12
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 12
        i32.add
        call_indirect (type 335)
        local.get 1
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 15
        local.set 14
        local.get 14
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 15
        local.get 14
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 13
        i32.add
        call_indirect (type 337)
        local.get 1
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 17
        local.set 16
        local.get 16
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 17
        local.get 16
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 14
        i32.add
        call_indirect (type 339)
        local.get 1
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 19
        local.set 18
        local.get 18
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 19
        local.get 18
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 15
        i32.add
        call_indirect (type 341)
        local.get 1
        local.get 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.set 46
        local.get 45
        i32.const 44
        i32.add
        local.get 46
        i32.const 8
        memory.copy
        local.get 45
        i32.const 44
        i32.add
        local.set 21
        local.set 20
        local.get 20
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 21
        local.get 20
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 16
        i32.add
        call_indirect (type 343)
        local.get 1
        i32.const 255
        local.get 45
        i32.const 52
        i32.add
        call 87
        local.set 23
        local.set 22
        local.get 22
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 23
        local.get 22
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 17
        i32.add
        call_indirect (type 346)
        local.get 1
        i32.const -2139062017
        local.get 45
        i32.const 68
        i32.add
        call 87
        local.set 25
        local.set 24
        local.get 24
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 25
        local.get 24
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 1
        i32.const 12
        f32.convert_i32_s
        f64.promote_f32
        global.get 12
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 5
          array.new_data 0 12
          i32.const 0
          i32.const 5
          call 1
          global.set 12
        end
        global.get 12
        local.get 3
        i32.const 0
        i32.const 8
        memory.fill
        local.get 3
        local.set 46
        local.get 45
        i32.const 84
        i32.add
        local.get 46
        i32.const 8
        memory.copy
        local.get 45
        i32.const 84
        i32.add
        local.get 4
        i32.const 0
        i32.const 12
        memory.fill
        local.get 4
        local.set 46
        local.get 45
        i32.const 92
        i32.add
        local.get 46
        i32.const 12
        memory.copy
        local.get 45
        i32.const 92
        i32.add
        local.get 5
        i32.const 0
        i32.const 8
        memory.fill
        local.get 5
        local.set 46
        local.get 45
        i32.const 104
        i32.add
        local.get 46
        i32.const 8
        memory.copy
        local.get 45
        i32.const 104
        i32.add
        local.get 6
        i32.const 0
        i32.const 8
        memory.fill
        local.get 6
        local.set 46
        local.get 45
        i32.const 112
        i32.add
        local.get 46
        i32.const 8
        memory.copy
        local.get 45
        i32.const 112
        i32.add
        local.set 32
        local.set 31
        local.set 30
        local.set 29
        local.set 28
        f32.demote_f64
        local.set 27
        struct.new_default 51
        local.set 26
        local.get 26
        local.get 27
        local.get 28
        local.get 29
        local.get 30
        local.get 31
        local.get 32
        call 163
        local.get 26
        local.set 34
        local.set 33
        local.get 33
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 34
        local.get 33
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 18
        i32.add
        call_indirect (type 365)
        local.get 1
        i32.const 0
        local.set 36
        local.set 35
        local.get 35
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 36
        local.get 35
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 19
        i32.add
        call_indirect (type 367)
        local.get 1
        i32.const 3
        local.set 38
        local.set 37
        local.get 37
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 38
        local.get 37
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 20
        i32.add
        call_indirect (type 369)
        local.get 1
        local.set 39
        local.get 39
        local.tee 51
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 51
        ref.cast (ref 10)
        local.get 39
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 1
        i32.add
        call_indirect (type 195)
        local.get 45
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 40
    local.get 45
    global.set 34
    local.get 40
    throw_ref
  )
  (func (;157;) (type 306) (param eqref eqref)
    (local i32 eqref i32 eqref eqref eqref eqref eqref eqref i32 i64 f64 externref)
    i32.const 0
    local.set 2
    block (type 307) ;; label = @1
      loop (type 307) ;; label = @2
        local.get 2
        local.get 0
        local.set 8
        local.get 8
        local.tee 10
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 10
        ref.cast (ref 16)
        local.get 8
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 29
        i32.add
        call_indirect (type 330)
        i32.lt_s
        if (type 308) ;; label = @3
          local.get 0
          local.get 1
          local.get 0
          local.get 2
          local.set 4
          local.set 3
          local.get 3
          local.tee 10
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 10
          ref.cast (ref 16)
          local.get 4
          local.get 3
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 30
          i32.add
          call_indirect (type 329)
          local.set 7
          local.set 6
          local.set 5
          local.get 5
          local.get 6
          local.get 7
          call 164
          local.get 2
          i32.const 1
          i32.add
          local.set 2
          br 1 (;@2;)
        else
          br 2 (;@1;)
        end
        unreachable
      end
    end
    return
  )
  (func (;158;) (type 309) (param eqref eqref)
    (local f32 eqref eqref eqref f32 i32 i32 eqref i32 eqref i32 f32 f32 f32 f32 eqref i32 f32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 27
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 27
    i32.const 32
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        local.get 4
        call 165
        local.get 0
        local.tee 33
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 33
        ref.cast (ref 37)
        struct.get 37 57
        f64.const 0x1p+0 (;=1;)
        f64.ge
        i32.eqz
        if (type 310) ;; label = @3
          local.get 1
          f64.const 0x1p+0 (;=1;)
          local.get 0
          local.tee 33
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 33
          ref.cast (ref 37)
          struct.get 37 57
          f64.sub
          f32.demote_f64
          f64.promote_f32
          f32.demote_f64
          local.set 6
          local.set 5
          local.get 5
          local.tee 33
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 33
          ref.cast (ref 10)
          local.get 6
          local.get 5
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 10
          i32.add
          call_indirect (type 331)
          local.get 1
          i32.const 1714675199
          local.set 8
          local.get 27
          i32.const 0
          i32.add
          local.tee 7
          i32.const 0
          i32.const 16
          memory.fill
          local.get 7
          local.get 8
          call 146
          local.get 7
          local.set 10
          local.set 9
          local.get 9
          local.tee 33
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 33
          ref.cast (ref 10)
          local.get 10
          local.get 9
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 7
          i32.add
          call_indirect (type 288)
          local.get 1
          local.set 11
          local.get 11
          local.tee 33
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 33
          ref.cast (ref 10)
          local.get 11
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 1
          i32.add
          call_indirect (type 195)
          local.get 1
          local.get 0
          local.tee 33
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 33
          ref.cast (ref 37)
          struct.get 37 55
          f32.demote_f64
          f64.promote_f32
          i32.const 20
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.sub
          f64.promote_f32
          local.get 0
          local.tee 33
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 33
          ref.cast (ref 37)
          struct.get 37 56
          f32.demote_f64
          f64.promote_f32
          i32.const 20
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.sub
          f64.promote_f32
          i32.const 40
          f32.convert_i32_s
          f64.promote_f32
          i32.const 40
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 16
          f32.demote_f64
          local.set 15
          f32.demote_f64
          local.set 14
          f32.demote_f64
          local.set 13
          local.get 27
          i32.const 16
          i32.add
          local.tee 12
          i32.const 0
          i32.const 16
          memory.fill
          local.get 12
          local.get 13
          local.get 14
          local.get 15
          local.get 16
          call 40
          local.get 12
          i32.const 20
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 19
          local.set 18
          local.set 17
          local.get 17
          local.tee 33
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 33
          ref.cast (ref 10)
          local.get 18
          local.get 19
          local.get 17
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 2
          i32.add
          call_indirect (type 197)
          local.get 1
          i32.const 0
          local.set 21
          local.set 20
          local.get 20
          local.tee 33
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 33
          ref.cast (ref 10)
          local.get 21
          local.get 20
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 8
          i32.add
          call_indirect (type 290)
          local.get 27
          global.set 34
          return
        else
          local.get 27
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 22
    local.get 27
    global.set 34
    local.get 22
    throw_ref
  )
  (func (;159;) (type 312) (param eqref i32 f32 f32 f32 f32 f32 i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 13
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 16
    i32.add
    global.set 34
    local.get 13
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 13
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 13
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 53
          array.new_data 0 13
          i32.const 0
          i32.const 53
          call 1
          global.set 13
        end
        global.get 13
        local.set 23
        local.get 23
        ref.is_null
        if ;; label = @3
          global.get 31
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 62
            array.new_data 0 31
            i32.const 0
            i32.const 62
            call 1
            global.set 31
          end
          global.get 31
          local.set 23
        end
        i32.const 5
        local.get 23
        struct.new 3
        local.tee 19
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 19
        throw 0
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 34
    local.get 8
    throw_ref
  )
  (func (;160;) (type 313) (param i32 i32) (result i32)
    (local i32 f32 f32 f32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 12
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 64
    i32.add
    global.set 34
    local.get 12
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 13
        local.get 12
        i32.const 8
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 13
        local.get 12
        i32.const 16
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 13
        local.get 12
        i32.const 24
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 13
        local.get 12
        i32.const 32
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 12
        i32.const 40
        i32.add
        local.tee 2
        i32.const 0
        i32.const 16
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        local.get 5
        local.get 6
        call 68
        local.get 2
        local.set 13
        local.get 1
        local.get 13
        i32.const 16
        memory.copy
        local.get 1
        local.get 12
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 34
    local.get 7
    throw_ref
  )
  (func (;161;) (type 314) (param i32) (result i64)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i64.load align=1
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;162;) (type 315) (param eqref eqref)
    (local i32 i64 f64 externref eqref eqref i32 eqref eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 6
    local.get 6
    call 28
    i32.const 64
    i32.and
    i32.eqz
    if (type 316) ;; label = @1
      local.get 0
      local.get 0
      local.set 6
      local.get 6
      call 28
      i32.const 64
      i32.or
      local.set 8
      local.set 7
      local.get 7
      local.get 8
      call 138
      local.get 0
      local.set 9
      local.get 9
      call 125
      local.tee 12
      local.get 12
      ref.is_null
      i32.eqz
      if (type 317) (param eqref) ;; label = @2
        local.get 0
        local.set 11
        local.set 10
        local.get 10
        local.tee 13
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 13
        ref.cast (ref 16)
        local.get 11
        local.get 10
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 51
        i32.add
        call_indirect (type 315)
        return
      else
        drop
        return
      end
      unreachable
    else
      return
    end
    unreachable
  )
  (func (;163;) (type 318) (param eqref f32 externref i32 i32 i32 i32)
    (local i32 i32 i32 i32 f32 eqref externref eqref f32 i32 i32 f32 i32 eqref i32 i32 i32 eqref i32 i32 i32 f32 i32 eqref i32 i32 i32 eqref f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 41
    i32.const 1048432
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 41
    i32.const 144
    i32.add
    global.set 34
    local.get 41
    i32.const 36
    i32.add
    local.set 7
    local.get 7
    i32.const 0
    i32.const 8
    memory.fill
    local.get 41
    i32.const 44
    i32.add
    local.set 8
    local.get 8
    i32.const 0
    i32.const 12
    memory.fill
    local.get 41
    i32.const 56
    i32.add
    local.set 9
    local.get 9
    i32.const 0
    i32.const 8
    memory.fill
    local.get 41
    i32.const 64
    i32.add
    local.set 10
    local.get 10
    i32.const 0
    i32.const 8
    memory.fill
    local.get 41
    i32.const 0
    i32.add
    local.get 3
    i32.const 8
    memory.copy
    local.get 41
    i32.const 0
    i32.add
    local.set 3
    local.get 41
    i32.const 8
    i32.add
    local.get 4
    i32.const 12
    memory.copy
    local.get 41
    i32.const 8
    i32.add
    local.set 4
    local.get 41
    i32.const 20
    i32.add
    local.get 5
    i32.const 8
    memory.copy
    local.get 41
    i32.const 20
    i32.add
    local.set 5
    local.get 41
    i32.const 28
    i32.add
    local.get 6
    i32.const 8
    memory.copy
    local.get 41
    i32.const 28
    i32.add
    local.set 6
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.set 13
        local.set 12
        local.get 12
        local.get 13
        call 177
        local.get 0
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 15
        local.set 14
        local.get 14
        local.get 15
        call 178
        local.get 0
        local.get 3
        local.set 42
        local.get 41
        i32.const 72
        i32.add
        local.get 42
        i32.const 8
        memory.copy
        local.get 41
        i32.const 72
        i32.add
        local.set 42
        local.get 7
        local.get 42
        i32.const 8
        memory.copy
        local.get 7
        local.set 16
        local.get 16
        call 179
        if (type 319) (result i32) ;; label = @3
          local.get 7
          local.set 19
          local.get 19
          local.get 41
          i32.const 84
          i32.add
          call 181
        else
          i32.const 400
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 18
          local.get 41
          i32.const 80
          i32.add
          local.tee 17
          i32.const 0
          i32.const 4
          memory.fill
          local.get 17
          local.get 18
          call 180
          local.get 17
        end
        local.set 21
        local.set 20
        local.get 20
        local.get 21
        call 182
        local.get 0
        local.get 4
        local.set 42
        local.get 41
        i32.const 88
        i32.add
        local.get 42
        i32.const 12
        memory.copy
        local.get 41
        i32.const 88
        i32.add
        local.set 42
        local.get 8
        local.get 42
        i32.const 12
        memory.copy
        local.get 8
        local.set 22
        local.get 22
        call 183
        if (type 320) (result i32) ;; label = @3
          local.get 8
          local.set 23
          local.get 23
          local.get 41
          i32.const 108
          i32.add
          call 185
        else
          local.get 41
          i32.const 100
          i32.add
          call 184
        end
        local.set 25
        local.set 24
        local.get 24
        local.get 25
        call 186
        local.get 0
        local.get 5
        local.set 42
        local.get 41
        i32.const 116
        i32.add
        local.get 42
        i32.const 8
        memory.copy
        local.get 41
        i32.const 116
        i32.add
        local.set 42
        local.get 9
        local.get 42
        i32.const 8
        memory.copy
        local.get 9
        local.set 26
        local.get 26
        call 187
        if (type 321) (result i32) ;; label = @3
          local.get 9
          local.set 29
          local.get 29
          local.get 41
          i32.const 128
          i32.add
          call 189
        else
          i32.const 100
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 28
          local.get 41
          i32.const 124
          i32.add
          local.tee 27
          i32.const 0
          i32.const 4
          memory.fill
          local.get 27
          local.get 28
          call 188
          local.get 27
        end
        local.set 31
        local.set 30
        local.get 30
        local.get 31
        call 190
        local.get 0
        local.get 6
        local.set 42
        local.get 41
        i32.const 132
        i32.add
        local.get 42
        i32.const 8
        memory.copy
        local.get 41
        i32.const 132
        i32.add
        local.set 42
        local.get 10
        local.get 42
        i32.const 8
        memory.copy
        local.get 10
        local.set 32
        local.get 32
        call 191
        if (type 322) (result f64) ;; label = @3
          local.get 10
          local.set 33
          local.get 33
          call 192
          f64.promote_f32
        else
          local.get 1
          f64.promote_f32
          f32.const 0x1.333334p+0 (;=1.2;)
          f64.promote_f32
          f32.demote_f64
          f64.promote_f32
          f32.demote_f64
          local.set 11
          f32.demote_f64
          local.get 11
          f32.mul
          f64.promote_f32
        end
        f32.demote_f64
        local.set 35
        local.set 34
        local.get 34
        local.get 35
        call 193
        local.get 41
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 36
    local.get 41
    global.set 34
    local.get 36
    throw_ref
  )
  (func (;164;) (type 323) (param eqref eqref eqref)
    (local i32 i32 i32 i32 eqref eqref i32 eqref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 17
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 96
    i32.add
    global.set 34
    local.get 17
    i32.const 0
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 16
    memory.fill
    local.get 17
    i32.const 16
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 20
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 23
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 23
        ref.cast (ref 16)
        local.set 23
        local.get 17
        i32.const 36
        i32.add
        local.get 23
        ref.cast (ref 16)
        struct.get 16 23
        i32.store8
        local.get 17
        i32.const 40
        i32.add
        local.get 23
        ref.cast (ref 16)
        struct.get 16 24
        f32.store align=1
        local.get 17
        i32.const 44
        i32.add
        local.get 23
        ref.cast (ref 16)
        struct.get 16 25
        f32.store align=1
        local.get 17
        i32.const 48
        i32.add
        local.get 23
        ref.cast (ref 16)
        struct.get 16 26
        f32.store align=1
        local.get 17
        i32.const 52
        i32.add
        local.get 23
        ref.cast (ref 16)
        struct.get 16 27
        f32.store align=1
        local.get 17
        i32.const 36
        i32.add
        local.set 18
        local.get 4
        local.get 18
        i32.const 20
        memory.copy
        local.get 4
        local.set 5
        local.get 5
        call 124
        i32.eqz
        if (type 324) ;; label = @3
          local.get 2
          local.get 1
          local.set 11
          local.set 10
          local.get 10
          local.tee 23
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 23
          ref.cast (ref 16)
          local.get 11
          call 26
          local.get 17
          global.set 34
          return
        else
          local.get 4
          local.set 6
          local.get 6
          local.get 17
          i32.const 56
          i32.add
          call 127
          local.set 18
          local.get 3
          local.get 18
          i32.const 16
          memory.copy
          local.get 2
          local.get 1
          local.get 3
          local.set 18
          local.get 17
          i32.const 72
          i32.add
          local.get 18
          i32.const 16
          memory.copy
          local.get 17
          i32.const 72
          i32.add
          local.set 9
          local.set 8
          local.set 7
          local.get 7
          local.tee 23
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 23
          ref.cast (ref 16)
          local.get 8
          local.get 9
          call 197
          local.get 17
          global.set 34
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 17
    global.set 34
    local.get 12
    throw_ref
  )
  (func (;165;) (type 325) (param eqref eqref)
    (local f32 f32 f32 eqref eqref i32 i32 f32 f32 f32 f32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 20
    i32.const 1048384
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 20
    i32.const 192
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 5
        local.get 5
        local.get 20
        i32.const 0
        i32.add
        call 111
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 2
        local.get 0
        local.set 5
        local.get 5
        local.get 20
        i32.const 16
        i32.add
        call 111
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        local.get 1
        i32.const -16776961
        local.get 20
        i32.const 32
        i32.add
        call 87
        local.set 7
        local.set 6
        local.get 6
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 7
        local.get 6
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 80
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 20
        i32.const 48
        i32.add
        local.tee 8
        i32.const 0
        i32.const 16
        memory.fill
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        call 40
        local.get 8
        local.set 14
        local.set 13
        local.get 13
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 14
        local.get 13
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 21
        i32.add
        call_indirect (type 382)
        local.get 1
        i32.const 8388863
        local.get 20
        i32.const 64
        i32.add
        call 87
        local.set 7
        local.set 6
        local.get 6
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 7
        local.get 6
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 130
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 80
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 20
        i32.const 80
        i32.add
        local.tee 8
        i32.const 0
        i32.const 16
        memory.fill
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        call 40
        local.get 8
        local.set 14
        local.set 13
        local.get 13
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 14
        local.get 13
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 21
        i32.add
        call_indirect (type 382)
        local.get 1
        i32.const 65535
        local.get 20
        i32.const 96
        i32.add
        call 87
        local.set 7
        local.set 6
        local.get 6
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 7
        local.get 6
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 250
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 80
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 20
        i32.const 112
        i32.add
        local.tee 8
        i32.const 0
        i32.const 16
        memory.fill
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        call 40
        local.get 8
        local.set 14
        local.set 13
        local.get 13
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 14
        local.get 13
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 21
        i32.add
        call_indirect (type 382)
        local.get 1
        i32.const -5963521
        local.get 20
        i32.const 128
        i32.add
        call 87
        local.set 7
        local.set 6
        local.get 6
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 7
        local.get 6
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 60
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 50
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 80
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 20
        i32.const 144
        i32.add
        local.tee 8
        i32.const 0
        i32.const 16
        memory.fill
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        call 40
        local.get 8
        local.set 14
        local.set 13
        local.get 13
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 14
        local.get 13
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 21
        i32.add
        call_indirect (type 382)
        local.get 1
        i32.const 255
        local.get 20
        i32.const 160
        i32.add
        call 87
        local.set 7
        local.set 6
        local.get 6
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 7
        local.get 6
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 288)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 150
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.get 4
        f32.add
        f64.promote_f32
        i32.const 20
        f32.convert_i32_s
        f64.promote_f32
        i32.const 20
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 20
        i32.const 176
        i32.add
        local.tee 8
        i32.const 0
        i32.const 16
        memory.fill
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        call 40
        local.get 8
        local.set 14
        local.set 13
        local.get 13
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 10)
        local.get 14
        local.get 13
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 21
        i32.add
        call_indirect (type 382)
        local.get 20
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 15
    local.get 20
    global.set 34
    local.get 15
    throw_ref
  )
  (func (;166;) (type 326) (param eqref eqref)
    (local i32 i64 f64 externref eqref eqref i32 eqref eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 6
    local.get 6
    call 28
    i32.const 2
    i32.and
    i32.eqz
    if (type 327) ;; label = @1
      local.get 0
      local.get 0
      local.set 6
      local.get 6
      call 28
      i32.const 2
      i32.or
      local.set 8
      local.set 7
      local.get 7
      local.get 8
      call 138
      local.get 0
      local.set 9
      local.get 9
      call 125
      local.tee 12
      local.get 12
      ref.is_null
      i32.eqz
      if (type 328) (param eqref) ;; label = @2
        local.get 0
        local.set 11
        local.set 10
        local.get 10
        local.tee 13
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 13
        ref.cast (ref 16)
        local.get 11
        local.get 10
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 46
        i32.add
        call_indirect (type 326)
        return
      else
        drop
        return
      end
      unreachable
    else
      return
    end
    unreachable
  )
  (func (;167;) (type 329) (param eqref i32) (result eqref)
    (local eqref eqref i32 i64 f64 externref)
    i32.const 7
    global.get 33
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 42
      array.new_data 0 33
      i32.const 0
      i32.const 42
      call 1
      global.set 33
    end
    global.get 33
    struct.new 3
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 3
    throw 0
  )
  (func (;168;) (type 330) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 30
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 30
        i32.const 0
        i32.const 53
        call 1
        global.set 30
      end
      global.get 30
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 2
    return
  )
  (func (;169;) (type 332) (param eqref f32)
    (local f32 eqref eqref i32 i64 f64 externref)
    global.get 14
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 11
      array.new_data 0 14
      i32.const 0
      i32.const 11
      call 1
      global.set 14
    end
    global.get 14
    local.get 1
    f64.promote_f32
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;170;) (type 334) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 15
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 7
      array.new_data 0 15
      i32.const 0
      i32.const 7
      call 1
      global.set 15
    end
    global.get 15
    local.get 1
    f64.convert_i32_s
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;171;) (type 336) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 16
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 8
      array.new_data 0 16
      i32.const 0
      i32.const 8
      call 1
      global.set 16
    end
    global.get 16
    local.get 1
    f64.convert_i32_s
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;172;) (type 338) (param eqref f32)
    (local f32 eqref eqref i32 i64 f64 externref)
    global.get 17
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 9
      array.new_data 0 17
      i32.const 0
      i32.const 9
      call 1
      global.set 17
    end
    global.get 17
    local.get 1
    f64.promote_f32
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;173;) (type 340) (param eqref f32)
    (local f32 eqref eqref i32 i64 f64 externref)
    global.get 18
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 10
      array.new_data 0 18
      i32.const 0
      i32.const 10
      call 1
      global.set 18
    end
    global.get 18
    local.get 1
    f64.promote_f32
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;174;) (type 342) (param eqref f32)
    (local f32 eqref eqref i32 i64 f64 externref)
    global.get 19
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 14
      array.new_data 0 19
      i32.const 0
      i32.const 14
      call 1
      global.set 19
    end
    global.get 19
    local.get 1
    f64.promote_f32
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;175;) (type 344) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        i32.const 4
        i32.add
        i32.load align=1
        i32.eqz
        if (type 345) ;; label = @3
          global.get 20
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 11
            array.new_data 0 20
            i32.const 0
            i32.const 11
            call 1
            global.set 20
          end
          global.get 20
          f64.const 0x0p+0 (;=0;)
          f64.const 0x0p+0 (;=0;)
          f64.const 0x0p+0 (;=0;)
          f64.const 0x0p+0 (;=0;)
          global.get 2
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 0
            array.new_data 0 2
            i32.const 0
            i32.const 0
            call 1
            global.set 2
          end
          global.get 2
          call 0
          local.get 7
          global.set 34
          return
        else
          global.get 13
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 13
            i32.const 0
            i32.const 53
            call 1
            global.set 13
          end
          global.get 13
          local.set 17
          local.get 17
          ref.is_null
          if ;; label = @4
            global.get 31
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 62
              array.new_data 0 31
              i32.const 0
              i32.const 62
              call 1
              global.set 31
            end
            global.get 31
            local.set 17
          end
          i32.const 5
          local.get 17
          struct.new 3
          local.tee 13
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 30
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 30
              i32.const 0
              i32.const 53
              call 1
              global.set 30
            end
            global.get 30
            struct.new 3
            throw 0
          end
          local.get 13
          throw 0
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;176;) (type 347) (param eqref i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 80
    i32.add
    global.set 34
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 21
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 9
          array.new_data 0 21
          i32.const 0
          i32.const 9
          call 1
          global.set 21
        end
        global.get 21
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 32
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 64
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 2
          i32.const 0
          i32.const 0
          call 1
          global.set 2
        end
        global.get 2
        call 0
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;177;) (type 348) (param eqref externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 7
    ref.cast (ref 51)
    local.set 3
    local.get 3
    ref.cast (ref 51)
    local.get 7
    struct.set 51 0
    return
  )
  (func (;178;) (type 349) (param eqref f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 6
    ref.cast (ref 51)
    local.set 3
    local.get 3
    ref.cast (ref 51)
    local.get 6
    f32.demote_f64
    struct.set 51 1
    return
  )
  (func (;179;) (type 350) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;180;) (type 351) (param i32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 11
        i32.const 0
        i32.add
        local.get 11
        f32.demote_f64
        f32.store align=1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;181;) (type 352) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 4
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 4
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;182;) (type 353) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 4
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 4
        i32.add
        local.get 8
        i32.const 4
        memory.copy
        local.get 7
        i32.const 4
        i32.add
        local.set 14
        ref.cast (ref 51)
        local.set 13
        local.get 13
        ref.cast (ref 51)
        local.get 14
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 51 5
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;183;) (type 354) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;184;) (type 355) (param i32) (result i32)
    (local f32 i32 i32 i32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 11
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 0
        i32.const 0
        f32.const 0x1.cp+3 (;=14;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 5
        i32.const 255
        i32.and
        local.set 4
        i32.const 255
        i32.and
        local.set 3
        local.get 11
        i32.const 0
        i32.add
        local.tee 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        local.get 5
        call 198
        local.get 2
        local.set 12
        local.get 0
        local.get 12
        i32.const 8
        memory.copy
        local.get 0
        local.get 11
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 34
    local.get 6
    throw_ref
  )
  (func (;185;) (type 356) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;186;) (type 357) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 8
        i32.add
        local.get 8
        i32.const 8
        memory.copy
        local.get 7
        i32.const 8
        i32.add
        local.set 14
        ref.cast (ref 51)
        local.set 13
        local.get 13
        ref.cast (ref 51)
        local.get 14
        i32.const 0
        i32.add
        i32.load8_u
        struct.set 51 2
        local.get 13
        ref.cast (ref 51)
        local.get 14
        i32.const 1
        i32.add
        i32.load8_u
        struct.set 51 3
        local.get 13
        ref.cast (ref 51)
        local.get 14
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 51 4
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;187;) (type 358) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;188;) (type 359) (param i32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 11
        i32.const 0
        i32.add
        local.get 11
        f32.demote_f64
        f32.store align=1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;189;) (type 360) (param i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        local.set 8
        local.get 7
        i32.const 0
        i32.add
        local.get 8
        i32.const 4
        memory.copy
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 4
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;190;) (type 361) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 4
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 4
        i32.add
        local.get 8
        i32.const 4
        memory.copy
        local.get 7
        i32.const 4
        i32.add
        local.set 14
        ref.cast (ref 51)
        local.set 13
        local.get 13
        ref.cast (ref 51)
        local.get 14
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 51 6
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;191;) (type 362) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;192;) (type 363) (param i32) (result f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;193;) (type 364) (param eqref f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 6
    ref.cast (ref 51)
    local.set 3
    local.get 3
    ref.cast (ref 51)
    local.get 6
    f32.demote_f64
    struct.set 51 7
    return
  )
  (func (;194;) (type 366) (param eqref eqref)
    (local i32 f32 eqref eqref eqref eqref eqref eqref i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 18
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 18
    i32.const 48
    i32.add
    global.set 34
    local.get 18
    i32.const 0
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 8
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 22
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 4
          array.new_data 0 22
          i32.const 0
          i32.const 4
          call 1
          global.set 22
        end
        global.get 22
        local.get 1
        local.set 4
        local.get 4
        call 199
        f64.promote_f32
        local.get 1
        local.set 5
        local.get 5
        local.get 18
        i32.const 8
        i32.add
        call 200
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 6
        local.get 6
        local.get 18
        i32.const 12
        i32.add
        call 201
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 7
        local.get 7
        call 202
        f64.promote_f32
        local.get 1
        local.set 8
        local.get 8
        call 203
        call 0
        global.get 23
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 9
          array.new_data 0 23
          i32.const 0
          i32.const 9
          call 1
          global.set 23
        end
        global.get 23
        local.get 1
        local.set 9
        local.get 9
        local.get 18
        i32.const 16
        i32.add
        call 204
        local.set 19
        local.get 2
        local.get 19
        i32.const 8
        memory.copy
        local.get 2
        local.set 10
        local.get 10
        call 205
        i32.const 0
        i32.gt_u
        f64.convert_i32_s
        local.get 1
        local.set 9
        local.get 9
        local.get 18
        i32.const 24
        i32.add
        call 204
        local.set 19
        local.get 2
        local.get 19
        i32.const 8
        memory.copy
        local.get 2
        local.set 11
        local.get 11
        call 206
        i32.const 0
        i32.gt_u
        f64.convert_i32_s
        local.get 1
        local.set 9
        local.get 9
        local.get 18
        i32.const 32
        i32.add
        call 204
        local.set 19
        local.get 2
        local.get 19
        i32.const 8
        memory.copy
        local.get 2
        local.set 12
        local.get 12
        call 207
        f64.promote_f32
        f64.const 0x0p+0 (;=0;)
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 2
          i32.const 0
          i32.const 0
          call 1
          global.set 2
        end
        global.get 2
        call 0
        local.get 18
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 13
    local.get 18
    global.set 34
    local.get 13
    throw_ref
  )
  (func (;195;) (type 368) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 24
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 9
      array.new_data 0 24
      i32.const 0
      i32.const 9
      call 1
      global.set 24
    end
    global.get 24
    local.get 1
    f64.convert_i32_s
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;196;) (type 370) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 25
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 12
      array.new_data 0 25
      i32.const 0
      i32.const 12
      call 1
      global.set 25
    end
    global.get 25
    local.get 1
    f64.convert_i32_s
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 2
      i32.const 0
      i32.const 0
      call 1
      global.set 2
    end
    global.get 2
    call 0
    return
  )
  (func (;197;) (type 371) (param eqref eqref i32)
    (local i32 i32 i32 i32 eqref eqref i32 i32 i32 eqref i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 20
    i32.const 1048464
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 20
    i32.const 112
    i32.add
    global.set 34
    local.get 20
    i32.const 16
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 32
    memory.fill
    global.get 35
    local.tee 17
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 4
    i32.add
    global.set 35
    local.get 20
    i32.const 32
    i32.add
    local.get 17
    i32.const 0
    i32.add
    i32.store align=1
    local.get 20
    i32.const 36
    i32.add
    local.get 17
    i32.const 1
    i32.add
    i32.store align=1
    local.get 20
    i32.const 40
    i32.add
    local.get 17
    i32.const 2
    i32.add
    i32.store align=1
    local.get 20
    i32.const 64
    i32.add
    local.get 17
    i32.const 3
    i32.add
    i32.store align=1
    local.get 20
    i32.const 0
    i32.add
    local.get 2
    i32.const 16
    memory.copy
    local.get 20
    i32.const 0
    i32.add
    local.set 2
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 3
        local.get 20
        i32.const 48
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 20
        i32.const 48
        i32.add
        local.get 20
        i32.const 56
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 20
        i32.const 56
        i32.add
        local.get 1
        local.get 1
        local.get 0
        local.tee 26
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 30
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 30
            i32.const 0
            i32.const 53
            call 1
            global.set 30
          end
          global.get 30
          struct.new 3
          throw 0
        end
        local.get 26
        ref.cast (ref 16)
        local.set 26
        local.get 20
        i32.const 64
        i32.add
        local.get 26
        ref.cast (ref 16)
        struct.get 16 28
        local.set 16
        i32.load align=1
        local.get 16
        table.set 1
        local.get 20
        i32.const 64
        i32.add
        local.set 9
        local.set 8
        local.set 7
        local.set 6
        local.set 5
        local.set 4
        local.get 4
        local.get 5
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        call 36
        local.get 0
        local.get 3
        local.get 2
        local.set 21
        local.get 20
        i32.const 68
        i32.add
        local.get 21
        i32.const 16
        memory.copy
        local.get 20
        i32.const 68
        i32.add
        local.set 11
        local.get 20
        i32.const 84
        i32.add
        local.tee 10
        i32.const 0
        i32.const 20
        memory.fill
        local.get 10
        local.get 11
        call 126
        local.get 10
        local.set 14
        local.set 13
        local.set 12
        local.get 12
        local.get 13
        local.get 14
        call 56
        local.get 17
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 17
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 17
        global.set 35
        local.get 20
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 15
    local.get 17
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 17
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 17
    global.set 35
    local.get 20
    global.set 34
    local.get 15
    throw_ref
  )
  (func (;198;) (type 372) (param i32 i32 i32 f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 9
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 9
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 11
        i32.const 0
        i32.add
        local.get 11
        i32.store8
        local.get 0
        local.get 2
        local.set 11
        i32.const 1
        i32.add
        local.get 11
        i32.store8
        local.get 0
        local.get 3
        f64.promote_f32
        local.set 13
        i32.const 4
        i32.add
        local.get 13
        f32.demote_f64
        f32.store align=1
        local.get 9
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 4
    local.get 9
    global.set 34
    local.get 4
    throw_ref
  )
  (func (;199;) (type 373) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 51)
    struct.get 51 1
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;200;) (type 374) (param eqref i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        ref.cast (ref 51)
        local.set 13
        local.get 7
        i32.const 0
        i32.add
        local.get 13
        ref.cast (ref 51)
        struct.get 51 5
        f32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 4
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;201;) (type 375) (param eqref i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        ref.cast (ref 51)
        local.set 13
        local.get 7
        i32.const 0
        i32.add
        local.get 13
        ref.cast (ref 51)
        struct.get 51 6
        f32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 4
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;202;) (type 376) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 51)
    struct.get 51 7
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;203;) (type 377) (param eqref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 51)
    struct.get 51 0
    return
  )
  (func (;204;) (type 378) (param eqref i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        ref.cast (ref 51)
        local.set 13
        local.get 7
        i32.const 0
        i32.add
        local.get 13
        ref.cast (ref 51)
        struct.get 51 2
        i32.store8
        local.get 7
        i32.const 1
        i32.add
        local.get 13
        ref.cast (ref 51)
        struct.get 51 3
        i32.store8
        local.get 7
        i32.const 4
        i32.add
        local.get 13
        ref.cast (ref 51)
        struct.get 51 4
        f32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 8
        memory.copy
        local.get 1
        local.get 7
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 34
    local.get 2
    throw_ref
  )
  (func (;205;) (type 379) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;206;) (type 380) (param i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 1
        i32.add
        i32.load8_u
        i32.const 255
        i32.and
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;207;) (type 381) (param i32) (result f32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 34
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.get 6
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 34
    local.get 1
    throw_ref
  )
  (func (;208;) (type 383) (param eqref i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 34
    local.tee 8
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 80
    i32.add
    global.set 34
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 26
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 8
          array.new_data 0 26
          i32.const 0
          i32.const 8
          call 1
          global.set 26
        end
        global.get 26
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 32
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 64
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 2
          i32.const 0
          i32.const 0
          call 1
          global.set 2
        end
        global.get 2
        call 0
        local.get 8
        global.set 34
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 34
    local.get 3
    throw_ref
  )
  (func (;209;) (type 11) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 10)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 10)
    struct.get 10 0
    local.set 2
    local.get 1
    global.get 38
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
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
      global.get 37
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
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 41
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 42
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 43
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 44
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 45
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 46
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 47
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 48
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 49
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 50
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 51
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 52
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 53
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 54
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 55
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 37
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 59
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 58
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 61
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 63
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 60
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 58
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 61
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 63
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 62
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 61
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 63
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 64
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 66
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 67
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 68
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 84
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 88
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 69
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 70
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 71
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 72
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 73
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 74
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 76
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 77
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 78
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 79
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 80
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 84
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 88
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 81
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 82
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 75
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 83
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 85
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 84
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 86
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 84
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 87
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 84
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 89
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 88
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 90
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 84
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 88
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 91
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 65
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 84
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 88
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 93
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
      ref.cast (ref 9)
      struct.get 9 1
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
  (data (;0;) "i\00P\00h\00o\00n\00e\00 \001\005\00 \00P\00r\00o\00")
  (data (;1;) "S\00a\00v\00e\00")
  (data (;2;) "")
  (data (;3;) "B\00e\00g\00i\00n\00P\00a\00t\00h\00")
  (data (;4;) "P\00a\00t\00h\00R\00e\00c\00t\00")
  (data (;5;) "R\00o\00u\00n\00d\00R\00e\00c\00t\00")
  (data (;6;) "C\00l\00i\00p\00")
  (data (;7;) "T\00r\00a\00n\00s\00l\00a\00t\00e\00")
  (data (;8;) "S\00c\00a\00l\00e\00")
  (data (;9;) "R\00e\00s\00t\00o\00r\00e\00")
  (data (;10;) "S\00e\00t\00F\00i\00l\00l\00")
  (data (;11;) "F\00i\00l\00l\00")
  (data (;12;) "I\00n\00t\00e\00r\00")
  (data (;13;) "U\00n\00s\00u\00p\00p\00o\00r\00t\00e\00d\00 \00C\00a\00n\00v\00a\00s\00 \00o\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00n\00 \00t\00h\00e\00 \00f\00i\00r\00s\00t\00-\00f\00r\00a\00m\00e\00 \00h\00o\00s\00t\00.\00")
  (data (;14;) "G\00l\00o\00b\00a\00l\00A\00l\00p\00h\00a\00")
  (data (;15;) "L\00i\00n\00e\00C\00a\00p\00")
  (data (;16;) "L\00i\00n\00e\00J\00o\00i\00n\00")
  (data (;17;) "L\00i\00n\00e\00W\00i\00d\00t\00h\00")
  (data (;18;) "M\00i\00t\00e\00r\00L\00i\00m\00i\00t\00")
  (data (;19;) "L\00i\00n\00e\00D\00a\00s\00h\00O\00f\00f\00s\00e\00t\00")
  (data (;20;) "S\00e\00t\00L\00i\00n\00e\00D\00a\00s\00h\00")
  (data (;21;) "S\00e\00t\00S\00t\00r\00o\00k\00e\00")
  (data (;22;) "F\00o\00n\00t\00")
  (data (;23;) "F\00o\00n\00t\00S\00t\00y\00l\00e\00")
  (data (;24;) "T\00e\00x\00t\00A\00l\00i\00g\00n\00")
  (data (;25;) "T\00e\00x\00t\00B\00a\00s\00e\00l\00i\00n\00e\00")
  (data (;26;) "F\00i\00l\00l\00R\00e\00c\00t\00")
  (data (;27;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;28;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;29;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;30;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;31;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;32;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
  (data (;33;) "I\00n\00d\00e\00x\00 \00w\00a\00s\00 \00o\00u\00t\00s\00i\00d\00e\00 \00t\00h\00e\00 \00b\00o\00u\00n\00d\00s\00 \00o\00f\00 \00t\00h\00e\00 \00a\00r\00r\00a\00y\00.\00")
)
```
