using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Text;

public class FixWebGLMouseBug : IPostprocessBuildWithReport
{
	public int callbackOrder => 0;

	public void OnPostprocessBuild(BuildReport report)
	{
		string buildOutputDirectory = report.summary.outputPath + "/Build/";
		string[] files = Directory.GetFiles(buildOutputDirectory, "*.js", SearchOption.AllDirectories);

		if (files.Length > 0)
		{
			foreach (var file in files)
			{
				Debug.Log("Modify file " + file);
				// Modify the file
				ModifyGzFile(file);
			}
		}
		else
		{
			Debug.LogError("No .js.gz file found in the build output directory.");
		}
	}

	void ModifyGzFile(string filePath)
	{
		string tempFile = Path.GetTempFileName();
		string modifiedContent;

		using (StreamReader reader = new StreamReader(filePath))
		{
			modifiedContent = reader.ReadToEnd().Replace("requestPointerLock()", "requestPointerLock({unadjustedMovement: true}).catch(function(error) {console.log(error);})");
		}

		// Write the modified content back to the temp file
		using (StreamWriter writer = new StreamWriter(filePath, false))
		{
			writer.Write(modifiedContent);
		}

		/*
		// Decompress
		using (FileStream originalFileStream = new FileStream(filePath, FileMode.Open))
		using (FileStream decompressedFileStream = File.Create(tempFile))
		using (GZipStream decompressionStream = new GZipStream(originalFileStream, System.IO.Compression.CompressionMode.Decompress))
		{
			decompressionStream.CopyTo(decompressedFileStream);
			Debug.Log("File decompressed.");
		}

		// Read, modify, and write the decompressed content
		using (StreamReader reader = new StreamReader(tempFile))
		{
			modifiedContent = reader.ReadToEnd().Replace("requestPointerLock()", "requestPointerLock({unadjustedMovement: true}).catch(function(error) {console.log(error);})");
		}

		// Write the modified content back to the temp file
		using (StreamWriter writer = new StreamWriter(tempFile, false))
		{
			writer.Write(modifiedContent);
		}

		// Compress the modified content back to the original file
		using (FileStream originalFileStream = new FileStream(filePath, FileMode.Create))
		using (GZipStream compressionStream = new GZipStream(originalFileStream, System.IO.Compression.CompressionLevel.Optimal))
		using (FileStream modifiedFileStream = File.OpenRead(tempFile))
		{
			modifiedFileStream.CopyTo(compressionStream);
			Debug.Log("File recompressed.");
		}

		// Cleanup the temporary file
		File.Delete(tempFile);*/
	}
}