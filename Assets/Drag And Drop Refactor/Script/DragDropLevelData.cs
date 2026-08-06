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
        public Sprite firstImage;
        public Sprite secondImage;
        public Sprite thirdImage;
        public Vector2 firstImagePos;
        public Vector2 secondImagePos;
        public Vector2 thirdImagePos;
    }

    public LevelEntry[] levels;
}
