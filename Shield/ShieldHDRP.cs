using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace FXV
{
    public partial class Shield : fxvRenderObject
    {
        static GameObject hitsPassGO = null;
        static CustomPassVolume hitsPassVolume = null;
        static ShieldHitsHDRPPass hitPass = null;

        static partial void SetupHitsPassHDRP(Camera camera)
        {
#if UNITY_EDITOR
            if (!Shield.HITS_PREVIEW_ENABLED && !Application.isPlaying)
            {
                hitsCommandBuffer_SRP = null;
                return;
            }
#endif
            if (hitsPassGO == null || hitsPassGO.scene == null || hitsPassGO.scene.isLoaded == false) 
            {
                hitsPassGO = null;

                {
#if UNITY_EDITOR
                    GameObject go = null;
                    while ((go = GameObject.Find("/fxvHitsPassVolume")) != null)
                    {
                        DestroyImmediate(go);
                    }

                    hitsPassGO = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags("fxvHitsPassVolume", HideFlags.DontSave | HideFlags.NotEditable);
#else
                    hitsPassGO = new GameObject("fxvHitsPassVolume");
                    hitsPassGO.hideFlags = HideFlags.DontSave;
#endif
                    hitsPassVolume = hitsPassGO.AddComponent<CustomPassVolume>();
                    hitsPassVolume.hideFlags = HideFlags.HideAndDontSave;
                    hitsPassVolume.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;
                    hitPass = (ShieldHitsHDRPPass)hitsPassVolume.AddPassOfType<ShieldHitsHDRPPass>();
                    hitPass.name = "ShieldHits";
                }
            }

            if (hitPass != null)
            {
                hitsCommandBuffer_SRP = hitPass.GetCommandBuffer();
            }
        }
    }
}