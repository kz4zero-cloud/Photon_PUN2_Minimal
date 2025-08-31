using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageList", menuName = "Game/Stage List")]
public class StageList : ScriptableObject
{
    [Tooltip("ゲームで使用可能なステージ名一覧。")]
    public List<string> stageNames = new List<string>();
}
