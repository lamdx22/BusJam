//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(TweenPosition))]
public class TweenPositionEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(120f);

		TweenPosition tw = target as TweenPosition;
		GUI.changed = false;

		Vector3 from = EditorGUILayout.Vector3Field("From", tw.from);
		Vector3 to = EditorGUILayout.Vector3Field("To", tw.to);
		bool worldSpace = EditorGUILayout.Toggle("World space", tw.worldSpace);
		var rectTrans = tw.GetComponent<RectTransform>();
		bool rectPos = tw.rectPos;
		if (rectTrans)
		{
			rectPos = EditorGUILayout.Toggle("Use Anchor Pos", tw.rectPos);
		}

		if (GUI.changed)
		{
			NGUIEditorTools.RegisterUndo("Tween Change", tw);
			tw.from = from;
			tw.to = to;
			tw.worldSpace = worldSpace;
			tw.rectPos = rectPos;
			NGUITools.SetDirty(tw);

			if (targets != null && targets.Length > 1)
			{
				foreach (var t in targets)
				{
					var tw2 = t as TweenPosition;
					if (tw2 && tw2 != tw)
					{
						NGUIEditorTools.RegisterUndo("Tween Change", tw2);
						tw2.from = from;
						tw2.to = to;
						tw2.worldSpace = worldSpace;
						tw2.rectPos = rectPos;
						NGUITools.SetDirty(tw2);
					}
				}
			}
		}

		DrawCommonProperties();

		
	}
}
