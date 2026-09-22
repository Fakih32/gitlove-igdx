using UnityEngine;
using System.Collections.Generic;
// ROMBAK dari DraganddropLevelScriptable.cs.
// Perubahan: cuma rename class & field (levels -> LevelEntry, Levels -> level)
// biar casing-nya konsisten sama konvensi C# (PascalCase untuk class,
// camelCase untuk field). Isi & fungsinya sama persis seperti sebelumnya.
[CreateAssetMenu(fileName = "DragDropLevelData", menuName = "Drag And Drop/Level Data")]
public class DragDropLevelData : ScriptableObject {
    [System.Serializable]
    public class LevelEntry {
          public int level;
        public DDquiz[] dragdropquiz;
         [Header("Background")]
        public Sprite BackgroundImage;

    }
    [System.Serializable]
    public class DDquiz
    {
      
        [Header("Gambar Drag")]
        public List<Sprite> Dragobject;
        [Header("Siluet")]
        public List<Sprite> SiluetDrag;
       
        
        [Header("Posisi Tujuan")]
         public List<Vector2> Destinypos;
      
        [Header("Ukuran Gambar Drag")]
         public List<Vector2> Imagescale;
       
        [Header("Ukuran Gambar tujuan")]
        public List<Vector2> DestinyImagescale;
    }
    public DDquiz[] getquizdragdrop(int index)
    {
         foreach (var entry in levels) {
            if (entry.level == index) return entry.dragdropquiz;
        }
        return null;
    }
    public Sprite getspritebg(int index)
    {
         foreach (var entry in levels) {
            if (entry.level == index) return entry.BackgroundImage;
        }
        return null;
    }
    public LevelEntry[] levels;
}
