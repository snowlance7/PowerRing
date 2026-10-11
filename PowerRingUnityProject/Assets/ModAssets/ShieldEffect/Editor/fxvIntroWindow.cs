using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FXV.ShieldEditorUtils
{
    internal class fxAssetPostprocess : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            bool openedWindow = false;
            bool migratedShaders = false;
            foreach (string str in importedAssets)
            {
#if FX_DEBUG_LOGS
                Debug.Log("Reimported Asset: " + str);
#endif
                if (!openedWindow && str.Contains("fxvIntroWindow"))
                {
                    fxvIntroWindow.ShowPipelineIntro();
                    openedWindow = true;
                }
                else if (!migratedShaders && str.Contains("FXVShieldEffect.shader"))
                {
                    MigrateMaterialsTo226();
                    migratedShaders = true;
                }
            }

        }

        static void MigrateMaterialsTo226()
        {
            Debug.Log("FXV shader changed. Migrating materials...");

            string[] materialGuids = AssetDatabase.FindAssets("t:Material");

            foreach (string guid in materialGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat == null)
                    continue;

                if (mat.shader == null)
                    continue;

                if (mat.shader.name != "FXV/FXVShieldEffect")
                    continue;

                List<string> keyWords = new List<string>(mat.shaderKeywords);

                if (mat.GetFloat("_FEATURE_ActivationType") == -1.0f)
                {
                    float value = -1;

                    if (keyWords.Contains("ACTIVATION_TYPE_FINALCOLOR")) value = 0;
                    else if (keyWords.Contains("ACTIVATION_TYPE_FINALCOLOR_UVX")) value = 1;
                    else if (keyWords.Contains("ACTIVATION_TYPE_FINALCOLOR_UVY")) value = 2;
                    else if (keyWords.Contains("ACTIVATION_TYPE_CUSTOM_TEX")) value = 3;
                    else if (keyWords.Contains("ACTIVATION_TYPE_UVX")) value = 4;
                    else if (keyWords.Contains("ACTIVATION_TYPE_UVY")) value = 5;
                    else if (keyWords.Contains("ACTIVATION_TYPE_FINALCOLOR_POSY")) value = 6;
                    else if (keyWords.Contains("ACTIVATION_TYPE_FINALCOLOR_POSX")) value = 7;
                    else if (keyWords.Contains("ACTIVATION_TYPE_FINALCOLOR_POSZ")) value = 8;

                    if (value >= 0)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            mat.SetFloat("_FEATURE_ActivationType", value);
                            EditorUtility.SetDirty(mat);
                        };
                    }
                }

                if (mat.GetFloat("_FEATURE_ColorRim") == -1.0f)
                {
                    float value = -1;
                    if (keyWords.Contains("USE_COLOR_RIM")) value = 1;
                    else value = 0;

                    if (value >= 0)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            mat.SetFloat("_FEATURE_ColorRim", value);
                            EditorUtility.SetDirty(mat);
                        };
                    }
                }

                if (mat.GetFloat("_FEATURE_MainTextureAnimation") == -1.0f)
                {
                    float value = -1;
                    if (keyWords.Contains("USE_MAIN_TEXTURE_ANIMATION")) value = 1;
                    else value = 0;

                    if (value >= 0)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            mat.SetFloat("_FEATURE_MainTextureAnimation", value);
                            EditorUtility.SetDirty(mat);
                        };
                    }
                }

                if (mat.GetFloat("_FEATURE_PatternTextureAnimation") == -1.0f)
                {
                    float value = -1;
                    if (keyWords.Contains("USE_PATTERN_TEXTURE_ANIMATION")) value = 1;
                    else value = 0;

                    if (value >= 0)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            mat.SetFloat("_FEATURE_PatternTextureAnimation", value);
                            EditorUtility.SetDirty(mat);
                        };
                    }
                }

                if (mat.GetFloat("_FEATURE_DirectionalVisibility") == -1.0f)
                {
                    float value = -1;
                    if (keyWords.Contains("USE_DIRECTION_VISIBILITY")) value = 1;
                    else value = 0;

                    if (value >= 0)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            mat.SetFloat("_FEATURE_DirectionalVisibility", value);
                            EditorUtility.SetDirty(mat);
                        };
                    }
                }

                if (mat.GetFloat("_FEATURE_DepthOverlapRim") == -1.0f)
                {
                    float value = -1;
                    if (keyWords.Contains("USE_DEPTH_OVERLAP_RIM")) value = 1;
                    else value = 0;

                    if (value >= 0)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            mat.SetFloat("_FEATURE_DepthOverlapRim", value);
                            EditorUtility.SetDirty(mat);
                        };
                    }
                }
            }

            EditorApplication.delayCall += () =>
            {
                AssetDatabase.SaveAssets();
                Debug.Log("FXV material migration finished.");
            };
        }
    }

    public partial class fxvIntroWindow : EditorWindow
    {
        static string version = "version 2.2.7";

        static int windowWidth = 650;
        static int windowHeight = 450;
        static int buttonWidth = 100;

        [MenuItem("Window/FXV/ShieldEffect/Intro")]
        public static void ShowPipelineIntro()
        {
            var currentRP = GraphicsSettings.currentRenderPipeline;
            if (currentRP == null)
            {
                ShowIntroWindowBuiltIn();
                return;
            }

            var curPipeline = currentRP.GetType().ToString().ToLower();

            if (curPipeline.Contains("universal"))
            {
                ShowIntroWindowURP();
            }
            else if (curPipeline.Contains("high definition") || curPipeline.Contains("highdefinition"))
            {
                ShowIntroWindowHDRP();
            }
        }

        public static void ShowIntroWindowBuiltIn()
        {
#if FX_DEBUG_LOGS
            Debug.Log("FXV.Shield ShowIntroWindow");
#endif
            fxvIntroWindow wnd = GetWindow<fxvIntroWindow>();
            wnd.titleContent = new GUIContent("Welcome Built In");
            wnd.pipelineType = 0;

            wnd.minSize = new Vector2(windowWidth, windowHeight);
            wnd.maxSize = new Vector2(windowWidth, windowHeight);
        }

        public static void ShowIntroWindowURP()
        {
#if FX_DEBUG_LOGS
            Debug.Log("FXV.Shield ShowIntroWindow");
#endif
            fxvIntroWindow wnd = GetWindow<fxvIntroWindow>();
            wnd.titleContent = new GUIContent("Welcome URP");
            wnd.pipelineType = 1;

            wnd.minSize = new Vector2(windowWidth, windowHeight);
            wnd.maxSize = new Vector2(windowWidth, windowHeight);
        }

        public static void ShowIntroWindowHDRP()
        {
#if FX_DEBUG_LOGS
            Debug.Log("FXV.Shield ShowIntroWindow");
#endif
            fxvIntroWindow wnd = GetWindow<fxvIntroWindow>();
            wnd.titleContent = new GUIContent("Welcome HDRP");
            wnd.pipelineType = 2;

            wnd.minSize = new Vector2(windowWidth, windowHeight);
            wnd.maxSize = new Vector2(windowWidth, windowHeight);
        }

        int pipelineType = -1;
        string assetPath;

        GUIStyle titleStyle;
        GUIStyle greenStyle;
        GUIStyle redStyle;
        Texture2D fxvLogo;

        void Init()
        {
            var g = AssetDatabase.FindAssets($"t:Script {nameof(fxvIntroWindow)}");
            string scriptPath = AssetDatabase.GUIDToAssetPath(g[0]);

            assetPath = Path.GetDirectoryName(scriptPath);
            assetPath = Path.GetDirectoryName(assetPath);

            titleStyle = new GUIStyle();
            titleStyle.normal.textColor = new Color(0.6f, 0.8f, 1.0f, 1.0f);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;

            redStyle = new GUIStyle();
            redStyle.normal.textColor = new Color(0.9f, 0.9f, 0.4f, 1.0f);

            greenStyle = new GUIStyle();
            greenStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f, 1.0f);

            fxvLogo = (Texture2D)Resources.Load("IntroImg", typeof(Texture2D));
        }

        public void OnGUI()
        {
            //if (titleStyle == null)
            {
                Init();
            }

            GUILayout.BeginHorizontal();

            GUILayout.Box(fxvLogo);

            GUILayout.BeginVertical();
            GUILayout.Label(version);
            GUILayout.Space(4);
            GUILayout.Label(" Thank you for purchasing \n Shield Effect asset !!!", titleStyle);

            if (GUILayout.Button("fx.valley.contact@gmail.com", GUILayout.Width(buttonWidth * 2.0f)))
            {
                Application.OpenURL("mailto:fx.valley.contact@gmail.com");
            }

            if (GUILayout.Button("Join Discord", GUILayout.Width(buttonWidth * 2.0f)))
            {
                Application.OpenURL("https://discord.gg/3ssjcBcgpu");
            }

            if (GUILayout.Button("Leave Review on Asset Page", GUILayout.Width(buttonWidth * 2.0f)))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/energy-shield-effect-v2-with-hit-fx-and-activation-animation-273354");
            }

            GUILayout.Label("Below you can find configuration tips based on pipeline your project uses.");
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            if (pipelineType == 0)
            {
                GUIBuiltIn();
            }
            else if (pipelineType == 1)
            {
                GUIURP();
            }
            else if (pipelineType == 2)
            {
                GUIHDRP();
            }
        }

        public static void GUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        void GUIBuiltIn()
        {
            GUILayout.Space(5);

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Check demo scene for shield effect examples.");

                if (GUILayout.Button("Show", GUILayout.Width(buttonWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/Demo/Demo.unity");
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

        void GUIURP()
        {
            GUILayout.Space(5);

            bool urpUnpacked = false;
            if (File.Exists(assetPath + "/URP/Scripts/ShieldPostprocessRenderFeature.cs"))
            {
                urpUnpacked = true;

                var type = this.GetType();
                var fieldInfo = type.GetField("urpVersion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (fieldInfo == null || (string)fieldInfo.GetValue(this) != version)
                {
                    urpUnpacked = false;
                }
            }

            if (!urpUnpacked)
            {
                GUILine(Color.gray);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(" Import URP shield asset package.", redStyle);

                    if (GUILayout.Button("Show package", GUILayout.Width(buttonWidth)))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/InstallURP.unitypackage");
                    }

                    if (GUILayout.Button("Import", GUILayout.Width(buttonWidth)))
                    {
                        AssetDatabase.ImportPackage(assetPath + "/InstallURP.unitypackage", true);
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                var type = this.GetType();
                var method = type.GetMethod("GUI_URP_AfterImport", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(this, null);
                }
            }
        }

        void GUIHDRP()
        {
            GUILayout.Space(5);

            bool hdrpUnpacked = false;
            if (File.Exists(assetPath + "/HDRP/Scripts/ShieldPostprocessHDRPPass.cs"))
            {
                hdrpUnpacked = true;

                var type = this.GetType();
                var fieldInfo = type.GetField("hdrpVersion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (fieldInfo == null || (string)fieldInfo.GetValue(this) != version)
                {
                    hdrpUnpacked = false;
                }
            }

            if (!hdrpUnpacked)
            {
                GUILine(Color.gray);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(" Import HDRP shield asset package.", redStyle);

                    if (GUILayout.Button("Show package", GUILayout.Width(buttonWidth)))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/InstallHDRP.unitypackage");
                    }

                    if (GUILayout.Button("Import", GUILayout.Width(buttonWidth)))
                    {
                        AssetDatabase.ImportPackage(assetPath + "/InstallHDRP.unitypackage", true);
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                var type = this.GetType();
                var method = type.GetMethod("GUI_HDRP_AfterImport", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(this, null);
                }
            }
        }
    }
}
