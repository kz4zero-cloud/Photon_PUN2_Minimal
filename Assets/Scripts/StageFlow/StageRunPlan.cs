// Assets/Scripts/StageFlow/StageRunPlan.cs
// 目的: 「このプレイで回すステージ名のリスト」と「最後に戻るシーン」を資産として持つ。
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Stage Run Plan", fileName = "StageRunPlan")]
public sealed class StageRunPlan : ScriptableObject
{
    [Tooltip("今回プレイするステージ（Build Settingsに含まれるシーン名）を順に並べる")]
    public List<string> stages = new List<string>();

    [Tooltip("最後のステージを終えたら戻るシーン名（通常は Main）")]
    public string returnScene = "Main";
}
