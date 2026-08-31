using UnityEngine;

// ROMBAK dari DraganddropLevelScriptable.cs.
// Perubahan: cuma rename class & field (levels -> LevelEntry, Levels -> level)
// biar casing-nya konsisten sama konvensi C# (PascalCase untuk class,
// camelCase untuk field). Isi & fungsinya sama persis seperti sebelumnya.
[CreateAssetMenu(fileName = "DragDropLevelData", menuName = "Drag And Drop/Level Data")]
public class DragDropLevelData : ScriptableObject {
    [System.Serializable]
    public class LevelEntry {
        public int level;
        [Header("Gambar Drag")]
        public Sprite firstImage;
        public Sprite secondImage;
        public Sprite thirdImage;
        [Header("Siluet")]
        public Sprite firstsiluet;
        public Sprite secondsiluet;
        public Sprite thridsiluet;
        [Header("Background")]
        public Sprite BackgroundImage;
        [Header("Posisi Tujuan")]
        public Vector2 firstImagePos;
        public Vector2 secondImagePos;
        public Vector2 thirdImagePos;
        [Header("Ukuran Gambar Drag")]
        public Vector2 firstimageScale;
        public Vector2 secondimageScale;
        public Vector2 thirdimageScale;
        [Header("Ukuran Gambar tujuan")]
        public Vector2 firstsiluetsize;
        public Vector2 secondsiluetsize;
        public Vector2 thridsiluetsize;
    }

    public LevelEntry[] levels;
}
