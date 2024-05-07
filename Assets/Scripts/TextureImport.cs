#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureImport : AssetPostprocessor
{
    protected void OnPreprocessTexture()
    {
        if (Path.GetFileName(assetPath).StartsWith("f_")) return;

        if (assetImporter is TextureImporter importer)
        {
            SetImporterSettings(importer);
            importer.SaveAndReimport();
        }
    }

    private static void SetImporterSettings(TextureImporter importer)
    {
        importer.mipmapEnabled = true;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.crunchedCompression = true;
        importer.compressionQuality = 100;
        importer.maxTextureSize = 2048;
        importer.isReadable = false;
    }

    protected void OnPostprocessTexture(Texture2D texture)
    {
        if (texture.width <= 64 && texture.height<=64 && assetImporter is TextureImporter importer)
        {
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 64;
            importer.SaveAndReimport();
        }
    }
}
#endif