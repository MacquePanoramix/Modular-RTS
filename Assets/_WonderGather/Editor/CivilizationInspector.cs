using UnityEditor;
using UnityEngine;
namespace WonderGather.Editor
{
    [CustomEditor(typeof(CivilizationDefinition))]
    public sealed class CivilizationInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var data=(CivilizationDefinition)target;
            var report=CivilizationValidator.Validate(data);
            EditorGUILayout.Space();EditorGUILayout.LabelField("Starting network",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Structural reachability only. Supplies are the prototype resource. This does not prove affordability of a whole chain, map resources, survival after losses, or competitive balance.",MessageType.Info);
            foreach(var error in report.Errors) EditorGUILayout.HelpBox(error,MessageType.Error);
            foreach(var warning in report.Warnings) EditorGUILayout.HelpBox(warning,MessageType.Warning);
            if(report.Errors.Count==0 && report.Warnings.Count==0) EditorGUILayout.HelpBox("All listed blueprints are structurally reachable; a supply gatherer is reachable.",MessageType.Info);
            foreach(var building in data.Buildings) if(building!=null)
                EditorGUILayout.LabelField(building.DisplayName+(building==data.StartingBase?" (starting base)":""),building.Produces!=null?"produces "+building.Produces.DisplayName:"no production");
            foreach(var unit in data.Units) if(unit!=null)
            {
                EditorGUILayout.LabelField(unit.DisplayName,unit.GathersSupplies?"gathers supplies":"cannot gather supplies");
                foreach(var building in unit.Builds) if(building!=null) EditorGUILayout.LabelField("    constructs",building.DisplayName);
            }
        }
    }
}
