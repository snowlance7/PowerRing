using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace FXV.ShieldEditorUtils
{
    public partial class fxvIntroWindow : EditorWindow
    {
#pragma warning disable CS0414
        static string hdrpVersion = "version 2.2.7";
#pragma warning restore CS0414

        void GUI_HDRP_AfterImport()
        {
            HDRenderPipelineAsset currentRP = (HDRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                if (currentRP.currentPlatformRenderPipelineSettings.supportCustomPass)
                {
                    GUILayout.Label(" Custom Pass enabled.", greenStyle);
                }
                else
                {
                    GUILayout.Label(" Enable Custom Pass in pipeline asset for postprocess and hit effects with skinned mesh renderer.", redStyle);
                    if (GUILayout.Button("Show Asset", GUILayout.Width(buttonWidth)))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(currentRP));
                    }
                }
            }
            GUILayout.EndHorizontal();
            
            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Check demo scene for shield effect examples.");

                if (GUILayout.Button("Show", GUILayout.Width(buttonWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/HDRP/Demo_HDRP.unity");
                }
            }
            GUILayout.EndHorizontal();

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("All sample objects and materials are placed in Prefabs folder.");

                if (GUILayout.Button("Show", GUILayout.Width(buttonWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/Prefabs/Domes/ShieldDome1.prefab");
                }
            }
            GUILayout.EndHorizontal();

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("When implementing shield effect on your game scene add \nShieldPostprocess script to camera for postprocess effect.");

                if (GUILayout.Button("Show", GUILayout.Width(buttonWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/Scripts/ShieldPostprocess.cs");
                }
            }
            GUILayout.EndHorizontal();

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Please read documentation for implementation guidelines.");

                if (GUILayout.Button("Show", GUILayout.Width(buttonWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/Documentation.pdf");
                }
            }
            GUILayout.EndHorizontal();

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(" Found a bug, need a feature or help with implementation ? Send an email or join Discord server.");
            }
            GUILayout.EndHorizontal();
        }
    }
}