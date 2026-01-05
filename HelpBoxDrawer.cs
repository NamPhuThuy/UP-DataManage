#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NamPhuThuy.DataManage
{
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            HelpBoxAttribute helpBox = (HelpBoxAttribute)attribute;
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            float height = style.CalcHeight(new GUIContent(helpBox.Message), EditorGUIUtility.currentViewWidth);
            return height + 4f;
        }

        public override void OnGUI(Rect position)
        {
            HelpBoxAttribute helpBox = (HelpBoxAttribute)attribute;
            MessageType messageType = MessageType.Info;

            switch (helpBox.MessageType)
            {
                case HelpBoxMessageType.Warning:
                    messageType = MessageType.Warning;
                    break;
                case HelpBoxMessageType.Error:
                    messageType = MessageType.Error;
                    break;
            }

            EditorGUI.HelpBox(position, helpBox.Message, messageType);
        }
    }
}
#endif