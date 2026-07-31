using UnityEngine;
[CreateAssetMenu(fileName = "Draganddropdata", menuName = "Drag And drop/Drag and drop Data")]
public class DraganddropLevelScriptable : ScriptableObject
{
     [System.Serializable]
     public class Dragdropdata
    {
        public int indicates;
        public Sprite firstimage;
        public Sprite secondimage;
        public Sprite thirdimage;
        public float firstimagexpos;
        public float secondimagexpos;
        public float thirdimagexpos;
        public float firstimageypos;
        public float secondimageypos;
        public float thirdimageypos;
        
    }
   public Dragdropdata[] data;
}
