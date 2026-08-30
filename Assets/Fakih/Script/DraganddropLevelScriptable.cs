using UnityEngine;
[CreateAssetMenu(fileName = "Draganddropdata", menuName = "Drag And drop/Drag and drop Data")]
public class DraganddropLevelScriptable : ScriptableObject
{
     [System.Serializable]
     public class Dragdropdata
    {
        public int Levels;
        [Header("Gambar Drag")]
        public Sprite firstimage;
        public Sprite secondimage;
        public Sprite thirdimage;
        [Header("Gambar siluet")]
        public Sprite firstshilloute;
        public Sprite secondshilloute;
        public Sprite thridshilloute;

        public Vector2 firstImagePos;
public Vector2 secondImagePos;
public Vector2 thirdImagePos;

        
    }
   public Dragdropdata[] levels;
}
