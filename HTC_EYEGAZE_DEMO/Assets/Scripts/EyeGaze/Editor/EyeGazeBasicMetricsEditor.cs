#if UNITY_EDITOR
using UnityEditor;
using EyeGaze.Runtime.Modules;

namespace EyeGaze.Editor
{
    [CustomEditor(typeof(EyeGazeBasicMetrics))]
    public class EyeGazeBasicMetricsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Sincroniza el SerializedObject con el estado actual del componente
            serializedObject.Update();

            // -----------------------------------------------------------------
            // FIXATION
            // -----------------------------------------------------------------

            // Tiempo mínimo continuo necesario para considerar que una fijación ha comenzado
            SerializedProperty fixationThresholdProp = serializedObject.FindProperty("fixationThreshold");

            // -----------------------------------------------------------------
            // VISUAL FIXATION EMISSION
            // -----------------------------------------------------------------

            // Activa o desactiva la emisión repetida de fijaciones visuales
            SerializedProperty emitRepeatedVisualFixationsProp = serializedObject.FindProperty("emitRepeatedVisualFixations");

            // Intervalo entre emisiones repetidas de fijaciones visuales
            SerializedProperty repeatedVisualFixationIntervalProp = serializedObject.FindProperty("repeatedVisualFixationInterval");

            // -----------------------------------------------------------------
            // AOI FILTERING
            // -----------------------------------------------------------------

            // Si está activado, el objeto debe pertenecer a la máscara de capas de métricas
            SerializedProperty requireMetricsLayerMaskProp = serializedObject.FindProperty("requireMetricsLayerMask");

            // Máscara de capas válidas para considerar un objeto como candidato a métricas
            SerializedProperty metricsMaskProp = serializedObject.FindProperty("metricsMask");

            // Si está activado, se exige que exista un componente EyeGazeAOI válido
            SerializedProperty requireAOIComponentProp = serializedObject.FindProperty("requireAOIComponent");

            // Si está activado, el componente EyeGazeAOI también se puede buscar en los padres
            SerializedProperty searchAOIInParentsProp = serializedObject.FindProperty("searchAOIInParents");

            // -----------------------------------------------------------------
            // DEBUG
            // -----------------------------------------------------------------

            // Activa o desactiva el log cuando empieza una fijación
            SerializedProperty logFixationStartsProp = serializedObject.FindProperty("logFixationStarts");

            // Activa o desactiva el log periódico de resumen
            SerializedProperty logPeriodicSummaryProp = serializedObject.FindProperty("logPeriodicSummary");

            // Número de frames entre cada resumen periódico cuando está activado
            SerializedProperty summaryLogEveryNFramesProp = serializedObject.FindProperty("summaryLogEveryNFrames");

            // -----------------------------------------------------------------
            // EXPORT
            // -----------------------------------------------------------------

            // Activa o desactiva la exportación automática al cerrar la aplicación
            SerializedProperty exportOnApplicationQuitProp = serializedObject.FindProperty("exportOnApplicationQuit");

            // Activa o desactiva la exportación en formato TXT
            SerializedProperty exportToTxtProp = serializedObject.FindProperty("exportToTxt");

            // Activa o desactiva la exportación en formato CSV
            SerializedProperty exportToCsvProp = serializedObject.FindProperty("exportToCsv");

            // Permite usar un directorio de salida personalizado
            SerializedProperty useCustomOutputDirectoryProp = serializedObject.FindProperty("useCustomOutputDirectory");

            // Ruta del directorio de salida personalizado
            SerializedProperty customOutputDirectoryProp = serializedObject.FindProperty("customOutputDirectory");

            // Nombre base del archivo de salida
            SerializedProperty outputFileNameProp = serializedObject.FindProperty("outputFileName");

            // Activa o desactiva el uso de timestamp en el nombre del archivo
            SerializedProperty generateTimestampedFileNameProp = serializedObject.FindProperty("generateTimestampedFileName");

            // Indica si se debe incluir el InstanceID en la exportación
            SerializedProperty includeInstanceIdProp = serializedObject.FindProperty("includeInstanceId");

            // -----------------------------------------------------------------
            // FIXATION
            // -----------------------------------------------------------------

            EditorGUILayout.LabelField("Fixation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fixationThresholdProp);

            // -----------------------------------------------------------------
            // VISUAL FIXATION EMISSION
            // -----------------------------------------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visual Fixation Emission", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(emitRepeatedVisualFixationsProp);

            // Solo muestra el intervalo si la emisión repetida está activada
            if (emitRepeatedVisualFixationsProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(repeatedVisualFixationIntervalProp);
                EditorGUI.indentLevel--;
            }

            // -----------------------------------------------------------------
            // AOI FILTERING
            // -----------------------------------------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("AOI Filtering", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(requireMetricsLayerMaskProp);

            // Solo muestra la máscara de capas si este filtro está activado
            if (requireMetricsLayerMaskProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(metricsMaskProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(requireAOIComponentProp);
            EditorGUILayout.PropertyField(searchAOIInParentsProp);

            // -----------------------------------------------------------------
            // DEBUG
            // -----------------------------------------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(logFixationStartsProp);
            EditorGUILayout.PropertyField(logPeriodicSummaryProp);

            // Solo muestra el intervalo de resumen si el log periódico está activado
            if (logPeriodicSummaryProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(summaryLogEveryNFramesProp);
                EditorGUI.indentLevel--;
            }

            // -----------------------------------------------------------------
            // EXPORT
            // -----------------------------------------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(exportOnApplicationQuitProp);
            EditorGUILayout.PropertyField(exportToTxtProp);
            EditorGUILayout.PropertyField(exportToCsvProp);
            EditorGUILayout.PropertyField(useCustomOutputDirectoryProp);

            // Solo muestra la ruta si se ha activado el uso de directorio personalizado
            if (useCustomOutputDirectoryProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(customOutputDirectoryProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(outputFileNameProp);
            EditorGUILayout.PropertyField(generateTimestampedFileNameProp);
            EditorGUILayout.PropertyField(includeInstanceIdProp);

            // Aplica los cambios hechos en el inspector al objeto serializado
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif