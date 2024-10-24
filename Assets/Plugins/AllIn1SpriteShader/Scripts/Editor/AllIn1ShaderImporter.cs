#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AllIn1SpriteShader
{
	[InitializeOnLoad]
	public static class AllIn1ShaderImporter
	{
		public enum UnityVersion
		{
			NONE,
			UNITY_2019,
			UNITY_2020,
			UNITY_2021,
			UNITY_2022,
			UNITY_6,
		}

		private const string LIT_SHADER_PATH		= "../../Shaders/Resources/AllIn1SpriteShaderLit.shader";

		private const string SHADER_PATH_STANDARD	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_Standard.txt";

		private const string SHADER_PATH_URP_2019	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_URP2019.txt";
		private const string SHADER_PATH_HDRP_2019	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_HDRP2019.txt";

		private const string SHADER_PATH_URP_2020	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_URP2020.txt";
		private const string SHADER_PATH_HDRP_2020	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_HDRP2020.txt";

		private const string SHADER_PATH_URP_2021	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_URP2021.txt";
		private const string SHADER_PATH_HDRP_2021	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_HDRP2021.txt";

		private const string SHADER_PATH_URP_2022	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_URP2022.txt";
		private const string SHADER_PATH_HDRP_2022	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_HDRP2022.txt";

		private const string SHADER_PATH_URP_2023	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_URP2023.txt";
		private const string SHADER_PATH_HDRP_2023	= "../../Shaders/LitShadersTextAssets/AllIn1SpriteShaderLit_HDRP2023.txt";

		private const string ACTIVE_SHADER = "../../Shaders/Resources/CurrentLitShader.shader";

		static AllIn1ShaderImporter()
		{
			ConfigureShader();
		}

		private static void ConfigureShader()
		{
			RenderPipelineChecker.RefreshData();

			string shaderPath = string.Empty;
			UnityVersion unityVersion = GetUnityVersion();
			if (RenderPipelineChecker.IsHDRP)
			{
				switch (unityVersion)
				{
					case UnityVersion.UNITY_2019:
						shaderPath = SHADER_PATH_HDRP_2019;
						break;
					case UnityVersion.UNITY_2020:
						shaderPath = SHADER_PATH_HDRP_2020;
						break;
					case UnityVersion.UNITY_2021:
						shaderPath = SHADER_PATH_HDRP_2021;
						break;
					case UnityVersion.UNITY_2022:
						shaderPath = SHADER_PATH_HDRP_2022;
						break;
					case UnityVersion.UNITY_6:
						shaderPath = SHADER_PATH_HDRP_2023;
						break;

				}
			}
			else if (RenderPipelineChecker.IsURP)
			{
				switch (unityVersion)
				{
					case UnityVersion.UNITY_2019:
						shaderPath = SHADER_PATH_URP_2019;
						break;
					case UnityVersion.UNITY_2020:
						shaderPath = SHADER_PATH_URP_2020;
						break;
					case UnityVersion.UNITY_2021:
						shaderPath = SHADER_PATH_URP_2021;
						break;
					case UnityVersion.UNITY_2022:
						shaderPath = SHADER_PATH_URP_2022;
						break;
					case UnityVersion.UNITY_6:
						shaderPath = SHADER_PATH_URP_2023;
						break;
				}
			}
			else
			{
				shaderPath = SHADER_PATH_STANDARD;
			} 
			try
			{
				var currentFileGUID = AssetDatabase.FindAssets($"t:Script {nameof(AllIn1ShaderImporter)}")[0];
				string currentFolder = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(currentFileGUID));

				string newShaderStr = File.ReadAllText(Path.Combine(currentFolder, shaderPath)); 
				File.WriteAllText(Path.Combine(currentFolder, LIT_SHADER_PATH), newShaderStr);

				AssetDatabase.SaveAssets();
			}
			catch(Exception e)
			{
				Debug.LogError("Shader not found: " + e);
			}
		}

		private static UnityVersion GetUnityVersion()
		{
			UnityVersion res = UnityVersion.NONE;

			string unityVersion = Application.unityVersion;

			if (unityVersion.Contains("2019"))
			{
				res = UnityVersion.UNITY_2019;
			}
			else if (unityVersion.Contains("2020"))
			{
				res = UnityVersion.UNITY_2020;
			}
			else if (unityVersion.Contains("2021"))
			{
				res = UnityVersion.UNITY_2021;
			}
			else if (unityVersion.Contains("2022"))
			{
				res = UnityVersion.UNITY_2022;
			}
			else if (unityVersion.Contains("6000"))
			{
				res = UnityVersion.UNITY_6;
			}

			return res;
		}
	}
}
#endif