using UnityEngine;

// ROMBAK dari DraganddropLevelScriptable.cs.
// Perubahan: cuma rename class & field (levels -> LevelEntry, Levels -> level)
// biar casing-nya konsisten sama konvensi C# (PascalCase untuk class,
// camelCase untuk field). Isi & fungsinya sama persis seperti sebelumnya.
//Menambahkan Siluet untuk tiap level 
[CreateAssetMenu(fileName = "DragDropLevelData", menuName = "Drag And Drop/Level Data")]
public class DragDropLevelData : ScriptableObject {
    [System.Serializable]
    public class LevelEntry {
        public int level;
        public Sprite firstImage;
        public Sprite secondImage;
        public Sprite thirdImage;
        public Sprite firstshilloute;
        public Sprite secondshilloute;
        public Sprite thirdshilloute;
        public Sprite BackgroundImage;
        public Vector2 firstImagePos;
        public Vector2 secondImagePos;
        public Vector2 thirdImagePos;
        public Vector2 firstimageScale;
        public Vector2 secondimageScale;
        public Vector2 thirdimageScale;
    }

    public LevelEntry[] levels;
}
