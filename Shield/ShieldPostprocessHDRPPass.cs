using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace FXV
{
    public class ShieldPostprocessHDRPPass : CustomPass
    {
        static readonly string BufferName = "_FXVTemporaryBuffer_";

        int downSampleSteps = 3;

        Material blendAddMaterial;
        Material postprocessMaterial;

        RTHandle[] postprocessBuffer = null;
        RTHandle ssgiTargetBuffer = null;
        RTHandle ssgi2TargetBuffer = null;
        RTHandle positionFrontTargetBuffer = null;
        RTHandle positionBackTargetBuffer = null;

        float[] ssgiSampleDirections = null;

        ShieldPostprocess.PostprocessContext context;

        int samplesQualitySet = -1;


        public override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            context = new ShieldPostprocess.PostprocessContext();

            downSampleSteps = -1;

            blendAddMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/FXV/FXVPostprocessBlitAdd_HDRP"));
            postprocessMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/FXV/FXVPostprocessShield_HDRP"));

            ssgiSampleDirections = new float[32];
            for (int s = 0; s < 16; ++s)
            {
                Vector2 dir = UnityEngine.Random.insideUnitCircle;
                ssgiSampleDirections[s * 2] = dir.x;
                ssgiSampleDirections[s * 2 + 1] = dir.y;
            }
        }

        void UpdateSSGIPostprocessMaterial(ShieldPostprocess postprocess)
        {
            postprocessMaterial.DisableKeyword("FXV_NUM_SAMPLES_LOW");
            postprocessMaterial.DisableKeyword("FXV_NUM_SAMPLES_MEDIUM");
            postprocessMaterial.DisableKeyword("FXV_NUM_SAMPLES_HIGH");

            if (postprocess.giNumberOfSamples == ShieldPostprocess.GiNumSmplesCount.Low)
            {
                postprocessMaterial.EnableKeyword("FXV_NUM_SAMPLES_LOW");
            }
            else if (postprocess.giNumberOfSamples == ShieldPostprocess.GiNumSmplesCount.Medium)
            {
                postprocessMaterial.EnableKeyword("FXV_NUM_SAMPLES_MEDIUM");
            }
            else if (postprocess.giNumberOfSamples == ShieldPostprocess.GiNumSmplesCount.High)
            {
                postprocessMaterial.EnableKeyword("FXV_NUM_SAMPLES_HIGH");
            }

            samplesQualitySet = (int)postprocess.giNumberOfSamples;
        }

        void RecreateBuffers(ShieldPostprocess postprocess)
        {
            ReleaseBuffers();

            downSampleSteps = postprocess.numberOfIterations;

            postprocessBuffer = new RTHandle[downSampleSteps + 1];
            context.tempTarget = new RenderTargetIdentifier[downSampleSteps + 1];
            context.tmpTargetSizes = new Vector2[downSampleSteps + 1];

            float downsample = (1.0f - (postprocess != null ? postprocess.downSampleRate : 0.5f));

            context.downSampleRate = downsample;

            Vector2 size = Vector2.one;
            for (int i = 0; i < postprocessBuffer.Length; i++)
            {
                postprocessBuffer[i] = RTHandles.Alloc(
                    size, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
                    useDynamicScale: true, name: BufferName + i);

                context.tempTarget[i] = postprocessBuffer[i].nameID;
                context.tmpTargetSizes[i] = size;

                size *= downsample;
            }

            if (postprocess.IsGloballIlluminationEnabled())
            {
                Vector2 giSize = context.tmpTargetSizes[postprocess.giAtIteration];

                ssgiTargetBuffer = RTHandles.Alloc(
                   giSize, TextureXR.slices, dimension: TextureXR.dimension,
                   colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
                   useDynamicScale: true, name: "_SSGIBuffer");
                context.ssgiTarget = ssgiTargetBuffer.nameID;

                ssgi2TargetBuffer = RTHandles.Alloc(
                   giSize, TextureXR.slices, dimension: TextureXR.dimension,
                   colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
                   useDynamicScale: true, name: "_SSGIBuffer2");
                context.ssgiTarget2 = ssgi2TargetBuffer.nameID;

                positionFrontTargetBuffer = RTHandles.Alloc(
                   Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                   colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
                   useDynamicScale: true, name: "_FXVPositionFrontBuffer");
                context.positionFrontTarget = positionFrontTargetBuffer.nameID;

                positionBackTargetBuffer = RTHandles.Alloc(
                   Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                   colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
                   useDynamicScale: true, name: "_FXVPositionBackBuffer");
                context.positionBackTarget = positionBackTargetBuffer.nameID;
            }

            if (samplesQualitySet != (int)postprocess.giNumberOfSamples)
            {
                UpdateSSGIPostprocessMaterial(postprocess);
            }
        }

        public override void Execute(CustomPassContext ctx)
        {
            ShieldPostprocess postprocess = ctx.hdCamera.camera.GetComponent<ShieldPostprocess>();
            if (postprocess == null)
            {
                postprocess = ShieldPostprocess.GetMainInstance();
                if (postprocess == null)
                {
                    return;
                }
            }

            Shader.SetGlobalFloat("GLOBAL_ResolutionCompensation", Screen.height / 800.0f);

            float downsample = (1.0f - (postprocess != null ? postprocess.downSampleRate : 0.5f));

            if (context.downSampleSteps != postprocess.numberOfIterations || context.downSampleRate != downsample || (postprocess.IsGloballIlluminationEnabled() && ssgiTargetBuffer == null))
            {
                RecreateBuffers(postprocess);
            }

            if (samplesQualitySet != (int)postprocess.giNumberOfSamples)
            {
                UpdateSSGIPostprocessMaterial(postprocess);
            }

            context.downSampleSteps = postprocess.numberOfIterations;
            context.downSampleRate = downsample;
            context.currentTarget = ctx.cameraColorBuffer.nameID;
            context.targetWidth = ctx.hdCamera.actualWidth;
            context.targetHeight = ctx.hdCamera.actualHeight;
            context.blitToScreen = false;
            context.viewProjectInverse = (ctx.hdCamera.camera.projectionMatrix * ctx.hdCamera.camera.worldToCameraMatrix).inverse;

            for (int i = 1; i < postprocessBuffer.Length; ++i)
            {
                CoreUtils.SetRenderTarget(ctx.cmd, postprocessBuffer[i], ClearFlag.All);
            }

            if (postprocess.IsGloballIlluminationEnabled())
            {
                CoreUtils.SetRenderTarget(ctx.cmd, new RenderTargetIdentifier[] {
                                                context.tempTarget[0],
                                                context.positionFrontTarget,
                                                context.positionBackTarget,
                                            }, ctx.cameraDepthBuffer);

                ctx.cmd.SetGlobalTexture("_FXVPositionFrontBuffer", context.positionFrontTarget);
                ctx.cmd.SetGlobalTexture("_FXVPositionBackBuffer", context.positionBackTarget);
                ctx.cmd.SetGlobalTexture("_SSGIBuffer", context.ssgiTarget);
            }
            else
            {
                CoreUtils.SetRenderTarget(ctx.cmd, postprocessBuffer[0], ClearFlag.All);
            }

            postprocess._PushRenderObjectsToCommandBuffer(ctx.cmd);

            postprocess._SetGaussianParams(postprocessMaterial);

            if (postprocess.IsGloballIlluminationEnabled())
            {
                postprocess._SetSSGIProperties(ctx.cmd);
            }

            ctx.cmd.SetGlobalMatrix("_ViewProjectInverse", context.viewProjectInverse);

            int pass = 2;
            for (int i = 0; i < context.downSampleSteps; ++i)
            {
                int ipp = i + 1;
                Vector2 targetSize = new Vector2(context.tmpTargetSizes[ipp].x * context.targetWidth, context.tmpTargetSizes[ipp].y * context.targetHeight);
                ctx.cmd.SetGlobalVector("_GI_FrameSize", new Vector4(1.0f / targetSize.x, 1.0f / targetSize.y, targetSize.x, targetSize.y));

                HDUtils.BlitCameraTexture(ctx.cmd, postprocessBuffer[i], postprocessBuffer[ipp], postprocessMaterial, pass);
                pass = pass == 2 ? 3 : 2;
            }

            if (postprocess.IsGloballIlluminationEnabled())
            {
                Vector2 targetSize = new Vector2(context.tmpTargetSizes[postprocess.giAtIteration].x * context.targetWidth, context.tmpTargetSizes[postprocess.giAtIteration].y * context.targetHeight);
                ctx.cmd.SetGlobalVector("_GI_FrameSize", new Vector4(1.0f / targetSize.x, 1.0f / targetSize.y, targetSize.x, targetSize.y));

                HDUtils.BlitCameraTexture(ctx.cmd, postprocessBuffer[postprocess.giAtIteration], ssgiTargetBuffer, postprocessMaterial, 5);

                for (int i = 0; i < postprocess.giDenoiseIterations; ++i)
                {
                    ctx.cmd.SetGlobalFloat("_GI_Stepwidth", Mathf.Pow(postprocess.giDenoiseStepWidth, (float)i * postprocess.giDenoiseStepChange));
                    HDUtils.BlitCameraTexture(ctx.cmd, ssgiTargetBuffer, ssgi2TargetBuffer, postprocessMaterial, 6);
                    HDUtils.BlitCameraTexture(ctx.cmd, ssgi2TargetBuffer, ssgiTargetBuffer, postprocessMaterial, 6);
                }
            }

            int upscalePass = 1; //Box
            int upscaleSGGIPass = 7; //Box + SSGI
            if (postprocess.blurMethod == ShieldPostprocess.BlurMethod.GaussianOnly)
            {
                upscalePass = 8;
                upscaleSGGIPass = 9;
            }

            for (int i = 0; i < context.downSampleSteps; ++i)
            {
                int ipp = context.downSampleSteps - i - 1;

                Vector2 targetSize = new Vector2(context.tmpTargetSizes[ipp].x * context.targetWidth, context.tmpTargetSizes[ipp].y * context.targetHeight);
                ctx.cmd.SetGlobalVector("_GI_FrameSize", new Vector4(1.0f / targetSize.x, 1.0f / targetSize.y, targetSize.x, targetSize.y));
                HDUtils.BlitCameraTexture(ctx.cmd, postprocessBuffer[context.downSampleSteps - i], postprocessBuffer[ipp], postprocessMaterial, (postprocess.IsGloballIlluminationEnabled() && ipp == postprocess.giAtIteration) ? upscaleSGGIPass : upscalePass);
            }

            blendAddMaterial.SetFloat("_ColorMultiplier", postprocess.postprocessPower);

            HDUtils.BlitCameraTexture(ctx.cmd, postprocessBuffer[0], ctx.cameraColorBuffer, blendAddMaterial, 0);
        }

        void ReleaseBuffers()
        {
            if (postprocessBuffer != null)
            {
                for (int i = 0; i < postprocessBuffer.Length; i++)
                {
                    postprocessBuffer[i].Release();
                    postprocessBuffer[i] = null;
                }
                postprocessBuffer = null;
            }

            if (ssgiTargetBuffer != null)
            {
                ssgiTargetBuffer.Release();
                ssgiTargetBuffer = null;
            }

            if (ssgi2TargetBuffer != null)
            {
                ssgi2TargetBuffer.Release();
                ssgi2TargetBuffer = null;
            }

            if (positionFrontTargetBuffer != null)
            {
                positionFrontTargetBuffer.Release();
                positionFrontTargetBuffer = null;
            }

            if (positionBackTargetBuffer != null)
            {
                positionBackTargetBuffer.Release();
                positionBackTargetBuffer = null;
            }
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(blendAddMaterial);
            CoreUtils.Destroy(postprocessMaterial);

            ssgiSampleDirections = null;

            ReleaseBuffers();
        }
    }
}
