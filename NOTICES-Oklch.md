# OKLCH Color Space Implementation Notices

## Oklab / OKLCH Color Space

### Implementation Details

The OKLCH perceptual color space implementation in `Xui.DevKit.UI.Design.Oklch` uses the Oklab color space created by Björn Ottosson.

**Implementation Date:** March 2026
**Author:** Xui Development Team
**License:** Same as Xui project license

### Reference

- **Author:** Björn Ottosson
- **Publication:** "A perceptual color space for image processing" (2020)
- **URL:** https://bottosson.github.io/posts/oklab/
- **License:** The Oklab color space definition and conversion matrices are in the public domain.

### Implementation Approach

The Xui OKLCH implementation uses the published Oklab conversion matrices (sRGB → linear sRGB → LMS → Oklab → OKLCH) and adds gamut-aware chroma clamping via bisection search against the sRGB gamut boundary. No code was copied from any implementation; only the mathematical definitions were used.

---

## Acknowledgments

We acknowledge Björn Ottosson for creating the Oklab perceptual color space, which provides the mathematical foundation for the Xui Design System's color palette generation.
