//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UITweener), true), CanEditMultipleObjects]
public class UITweenerEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(110f);
		base.OnInspectorGUI();
		DrawCommonProperties();
	}

	protected void DrawCommonProperties ()
	{
		UITweener tw = target as UITweener;

		if (NGUIEditorTools.DrawHeader("Tweener"))
		{
			NGUIEditorTools.BeginContents();
			NGUIEditorTools.SetLabelWidth(110f);

			GUI.changed = false;

			UITweener.Style style = (UITweener.Style)EditorGUILayout.EnumPopup("Play Style", tw.style);
			AnimationCurve curve = EditorGUILayout.CurveField("Animation Curve", tw.animationCurve, GUILayout.Width(170f), GUILayout.Height(62f));
			//UITweener.Method method = (UITweener.Method)EditorGUILayout.EnumPopup("Play Method", tw.method);

			GUILayout.BeginHorizontal();
			float dur = EditorGUILayout.FloatField("Duration", tw.duration, GUILayout.Width(170f));
			GUILayout.Label("seconds");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			float del = EditorGUILayout.FloatField("Start Delay", tw.delay, GUILayout.Width(170f));
			GUILayout.Label("seconds");
			GUILayout.EndHorizontal();

			int tg = EditorGUILayout.IntField("Tween Group", tw.tweenGroup, GUILayout.Width(170f));
			bool ts = EditorGUILayout.Toggle("Ignore TimeScale", tw.ignoreTimeScale);
			bool fx = EditorGUILayout.Toggle("Use Fixed Update", tw.useFixedUpdate);

			bool forVisual = EditorGUILayout.Toggle("Tween For Visual", tw.ForVisual);

			if (GUI.changed)
			{
				NGUIEditorTools.RegisterUndo("Tween Change", tw);
				tw.animationCurve = curve;
				//tw.method = method;
				tw.style = style;
				tw.ignoreTimeScale = ts;
				tw.tweenGroup = tg;
				tw.duration = dur;
				tw.delay = del;
				tw.useFixedUpdate = fx;
				tw.ForVisual = forVisual;
				NGUITools.SetDirty(tw);

				if (targets != null && targets.Length > 1)
				{
					foreach (var t in targets)
					{
						var tw2 = t as TweenPosition;
						if (tw2 && tw2 != tw)
						{
							NGUIEditorTools.RegisterUndo("Tween Change", tw2);
							tw2.animationCurve = curve;
							//tw.method = method;
							tw2.style = style;
							tw2.ignoreTimeScale = ts;
							tw2.tweenGroup = tg;
							tw2.duration = dur;
							tw2.delay = del;
							tw2.useFixedUpdate = fx;
							tw2.ForVisual = forVisual;
							NGUITools.SetDirty(tw2);
						}
					}
				}
			}
			NGUIEditorTools.EndContents();
		}

		NGUIEditorTools.SetLabelWidth(80f);
		NGUIEditorTools.DrawEvents("On Finished", tw, tw.onFinished);
	}
}
