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
            foreach (string str in importedAssets)
            {
#if FX_DEBUG_LOGS
                Debug.Log("Reimported Asset: " + str);
#endif
                if (str.Contains("fxvIntroWindow"))
                {
                    fxvIntroWindow.ShowPipelineIntro();
                    return;
                }
            }
        }
    }

    public partial class fxvIntroWindow : EditorWindow
    {
        static int windowWidth = 650;
        static int windowHeight = 400;
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

            fxvLogo = (Texture2D)Resources.Load("FXV", typeof(Texture2D));
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
            GUILayout.Space(16);
            GUILayout.Label(" Thank you for buying Shield Effect asset !!!", titleStyle);
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
                GUILayout.Label(" Found a bug, need a feature or help with implementation. Contact at:\n   fx.valley.contact@gmail.com");
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

            bool urpUnpacked = false;
            if (File.Exists(assetPath + "/HDRP/Scripts/ShieldPostprocessHDRPPass.cs"))
            {
                urpUnpacked = true;
            }

            if (!urpUnpacked)
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
