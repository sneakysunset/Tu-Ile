From README.txt:
...

USAGE:

HDRP:
1) Make sure HDRP v7.1.8+ is installed, import HDRP.unitypackage by double clicking it.
2) Project Settings > HDRP Default Settings > Custom Post Process Orders (at the bottom) > After Opaque And Sky > add SSCC custom post process.
3) Effect will now be enabled by default. You can add SSCC to your Post Process Volume to adjust settings. To disable the effect set Effect Intensity to zero. Done.

URP:
1) Make sure URP v7.1.8+ is installed, import URP.unitypackage by double clicking it.
2) Your URP Renderer asset > Renderer Features > add SSCCRenderFeature.
3) Make sure your UniversalRenderPipelineAsset's > Depth Texture is on. Or locally toggle in on in your camera.
4) Make sure your camera's > Post Processing is on.
5) Effect will now be enabled by default. You can add SSCC to your Post Process Volume to adjust settings. To disable the effect set Effect Intensity to zero. Done.