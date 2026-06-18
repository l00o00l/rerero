using NUnit.Framework;
using UnityEditor;

namespace Thkim.DreamLaundromat.Tests.EditMode.ReleaseSlice
{
    public sealed class ReleaseUiArtImportTests
    {
        [Test]
        public void GeneratedReleaseBackgrounds_UseAndroidTextureCompression()
        {
            const string assetPath = "Assets/_Project/Art/UI/Backgrounds/release-gameplay-background.png";
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            Assert.That(importer, Is.Not.Null, assetPath);
            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");

            Assert.That(android.overridden, Is.True);
            Assert.That(android.textureCompression, Is.Not.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ETC2_RGBA8));
        }
    }
}
