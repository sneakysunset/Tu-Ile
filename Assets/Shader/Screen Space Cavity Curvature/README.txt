Screen Space Cavity & Curvature v1.0
Created by Jolly Theory.
Email: jollytheorygames@gmail.com

-----
USAGE:

Built-in render pipeline:
1) Add SSCC component to your camera. Adjust settings. Done.

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

----
TIPS:

- Use View Normals debug mode to see what objects are contributing their normals data to the effect, 
if you have a custom shader and it does not show up in this view, you'll need to either add a Depth pass to it and use Reconstructed normals, or add a DepthNormals pass to it.
(Please consult Google on how to do this for your render pipeline.)
- If you want to exclude a certain object from being affected by the effect, take a look at the _SSCCTexture output mode (read its tooltip), and the "_SSCCTexture Examples" folder.