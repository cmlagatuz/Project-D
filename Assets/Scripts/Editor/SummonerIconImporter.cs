// Project D  |  Codename: Zantetsuken
// SummonerIconImporter.cs  (Editor-only)
//
// Any texture dropped into Assets/Resources/UI/SummonerSpells/ is imported as a UI Sprite
// automatically, so SummonerSpellIcons.Load() (Resources.Load<Sprite>) works with no manual setup.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public sealed class SummonerIconImporter : AssetPostprocessor
    {
        private const string Folder = "Assets/Resources/UI/SummonerSpells/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 256;
        }
    }
}
#endif
