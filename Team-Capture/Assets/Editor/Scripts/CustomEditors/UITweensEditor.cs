using Tweens;
using UnityEditor;

namespace Editor.Scripts.CustomEditors
{
	[CustomEditor(typeof(UITweenEvent))]
	public class UITweensEditor : UnityEditor.Editor
	{
		private UITweenEvent tweenEvent;

		private void OnEnable()
		{
			tweenEvent = (UITweenEvent) target;
		}

		public override void OnInspectorGUI()
		{
			//Base Tween Event Settings
			EditorGUILayout.LabelField("Base Tween Settings", EditorStyles.boldLabel);
			tweenEvent.duration = EditorGUILayout.FloatField("Duration:", tweenEvent.duration);
			tweenEvent.activeOnEnd = EditorGUILayout.Toggle("Active On end?", tweenEvent.activeOnEnd);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("UI Tween Settings", EditorStyles.boldLabel);

			//UI Tween Settings
			tweenEvent.moving = EditorGUILayout.Toggle("Moving?", tweenEvent.moving);
			if (tweenEvent.moving)
			{
				tweenEvent.moveFrom = EditorGUILayout.FloatField("Move from", tweenEvent.moveFrom);
				tweenEvent.moveTo = EditorGUILayout.FloatField("Move to", tweenEvent.moveTo);
			}

			EditorGUILayout.Space();

			tweenEvent.fading = EditorGUILayout.Toggle("Fading?", tweenEvent.fading);
			if (tweenEvent.fading)
			{
				tweenEvent.fadeFrom = EditorGUILayout.FloatField("Fade from", tweenEvent.fadeFrom);
				tweenEvent.fadeTo = EditorGUILayout.FloatField("Fade to", tweenEvent.fadeTo);
			}

			EditorUtility.SetDirty(tweenEvent);
		}
	}
}