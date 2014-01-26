using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TouchScriptBuild : MonoBehaviour {
	
	private static string SOURCE_FOLDER = "TouchScript";
	private static string TMP_FOLDER = "TouchScript_";

	private static string[] FREE_PACKAGE = new string[] {
		"Textures",
		"Prefabs",
		"Editor/TouchScript.Editor.dll",
		"Plugins/TouchScript.dll",
		"Plugins/TouchScript.Windows.dll"
	};

	private static string[] MOBILE_PACKAGE = new string[] {
		"Textures",
		"Prefabs",
		"Editor/TouchScript.Editor.dll",
		"Plugins/TouchScript.dll"
	};

	private static string[] FULL_PACKAGE = new string[] {
		"Textures",
		"Prefabs",
		"Editor",
		"Plugins"
	};

	private static string[] EXAMPLES_PACKAGE = new string[] {
		"Examples"
	};
	
	[MenuItem("TouchScript/Prepare")]
	public static void Prepare()
	{
//		AssetDatabase.ExportPackage("Assets/" + SOURCE_FOLDER, "Assets/" + SOURCE_FOLDER + ".unitypackage", ExportPackageOptions.Recurse);

//		var tmpPath = Path.Combine(Application.dataPath, TMP_FOLDER);
//		if (Directory.Exists(tmpPath)) Directory.Delete(tmpPath);
//		Directory.CreateDirectory(tmpPath);

		var tmp = AssetDatabase.LoadMainAssetAtPath("Assets/" + TMP_FOLDER);
		if (tmp != null) AssetDatabase.DeleteAsset("Assets/" + TMP_FOLDER);
		AssetDatabase.CreateFolder("Assets", TMP_FOLDER);

		buildPackage("TouchScript Free", SOURCE_FOLDER, TMP_FOLDER, FREE_PACKAGE);
		buildPackage("TouchScript Mobile", SOURCE_FOLDER, TMP_FOLDER, MOBILE_PACKAGE);
		buildPackage("TouchScript Full", SOURCE_FOLDER, TMP_FOLDER, FULL_PACKAGE);
		buildPackage("TouchScript Examples", SOURCE_FOLDER, TMP_FOLDER, EXAMPLES_PACKAGE);

		AssetDatabase.DeleteAsset("Assets/" + SOURCE_FOLDER);
		AssetDatabase.RenameAsset("Assets/" + TMP_FOLDER, SOURCE_FOLDER);
//		Directory.Delete(Path.Combine(Application.dataPath, SOURCE_FOLDER), true);
//		Directory.Move(Path.Combine(Application.dataPath, TMP_FOLDER), Path.Combine(Application.dataPath, SOURCE_FOLDER));
		AssetDatabase.ImportPackage("Assets/" + SOURCE_FOLDER + "/TouchScript Mobile.unitypackage", false); 
	}
	
	private static void buildPackage(string name, string srcFolder, string dstFolder, string[] masks)
	{
		var files = getFiles(srcFolder, masks);

		var package = "Assets/" + dstFolder + "/" + name + ".unitypackage";
		AssetDatabase.ExportPackage(files.ToArray(), package, ExportPackageOptions.Recurse);
	}

	private static List<string> getFiles(string folder, string[] masks)
	{
		var files = new List<string>();

		var folderPath = Path.Combine(Application.dataPath, folder);
		foreach (var mask in masks)
		{
			var path = Path.Combine(folderPath, mask);
			files.Add("Assets" + path.Replace(Application.dataPath, ""));
		}

		return files;
	}

	private static void copyFolder(string src, string dst, bool copySubDirs)
	{
		DirectoryInfo dir = new DirectoryInfo(src);
		DirectoryInfo[] dirs = dir.GetDirectories();
		
		if (!dir.Exists)
		{
			throw new DirectoryNotFoundException(
				"Source directory does not exist or could not be found: "
				+ src);
		}
		
		if (Directory.Exists(dst)) Directory.Delete(dst);
		Directory.CreateDirectory(dst);
		
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files) file.CopyTo(Path.Combine(dst, file.Name), false);
		
		if (copySubDirs)
		{
			foreach (DirectoryInfo subdir in dirs) copyFolder(subdir.FullName, Path.Combine(dst, subdir.Name), copySubDirs);
		}
	}

}
