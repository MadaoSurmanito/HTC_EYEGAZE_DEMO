using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using EyeGaze.Runtime.Core;
using UnityEngine;

namespace EyeGaze.Runtime.Modules
{
    public partial class EyeGazeBasicMetrics
    {
        // Writes the current aggregated metrics summary to the console
        public void LogSummary()
        {
            Debug.Log("[GAZE BASIC METRICS] ----- Summary Start -----");

            foreach (KeyValuePair<EyeGazeAOI, BasicMetricsData> pair in metricsByAOI)
            {
                EyeGazeAOI aoi = pair.Key;
                BasicMetricsData data = pair.Value;

                if (aoi == null)
                {
                    continue;
                }

                Debug.Log(
                    $"[GAZE BASIC METRICS] AOI='{aoi.AoiLabel}' | " +
                    $"FB={data.fixationsBefore} | " +
                    $"TFF={data.timeToFirstFixation.ToString("F3", CultureInfo.InvariantCulture)}s | " +
                    $"FD={data.GetAverageFixationDuration().ToString("F3", CultureInfo.InvariantCulture)}s | " +
                    $"TFD={data.totalFixationDuration.ToString("F3", CultureInfo.InvariantCulture)}s | " +
                    $"FC={data.fixationCount}"
                );
            }

            Debug.Log("[GAZE BASIC METRICS] ----- Summary End -----");
        }

        // Exports the current metrics to a human-readable TXT report
        public void ExportToTxt()
        {
            try
            {
                // Resolve/create the output directory
                string outputDirectory = EyeGazeUtils.GetOutputDirectory(useCustomOutputDirectory, customOutputDirectory);
                Directory.CreateDirectory(outputDirectory);

                // Build the output file path
                string filePath = Path.Combine(
                    outputDirectory,
                    EyeGazeUtils.GetOutputFileName(outputFileName, "eye_gaze_basic_metrics", generateTimestampedFileName)
                );

                StringBuilder sb = new StringBuilder();

                // Write report metadata
                sb.AppendLine("Eye Gaze Basic Metrics Report");
                sb.AppendLine($"ExportedAt={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"FixationThresholdSeconds={fixationThreshold.ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"EmitRepeatedVisualFixations={(emitRepeatedVisualFixations ? "1" : "0")}");
                sb.AppendLine($"RepeatedVisualFixationIntervalSeconds={repeatedVisualFixationInterval.ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"RequireMetricsLayerMask={(requireMetricsLayerMask ? "1" : "0")}");
                sb.AppendLine($"RequireAOIComponent={(requireAOIComponent ? "1" : "0")}");
                sb.AppendLine($"SearchAOIInParents={(searchAOIInParents ? "1" : "0")}");
                sb.AppendLine($"SessionElapsedSeconds={(Time.time - sessionStartTime).ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"CurrentMetricsAOI={(currentMetricsAOI != null ? currentMetricsAOI.AoiLabel : "<none>")}");
                sb.AppendLine($"CurrentVisualTarget={(currentVisualTarget != null ? currentVisualTarget.name : "<none>")}");
                sb.AppendLine($"CurrentVisualIsFallback={(currentVisualIsFallback ? "1" : "0")}");
                sb.AppendLine($"CurrentTargetContinuousTime={currentTargetContinuousTime.ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"TotalFixationsStarted={totalFixationsStarted}");
                sb.AppendLine();

                // Write the tabular header
                if (includeInstanceId)
                {
                    sb.AppendLine("AOI_Label\tAOI_Id\tInstanceID\tFB\tTFF_Seconds\tFD_Seconds\tTFD_Seconds\tFC\tIsCurrentMetricsAOI");
                }
                else
                {
                    sb.AppendLine("AOI_Label\tAOI_Id\tFB\tTFF_Seconds\tFD_Seconds\tTFD_Seconds\tFC\tIsCurrentMetricsAOI");
                }

                // Write one row per AOI
                foreach (KeyValuePair<EyeGazeAOI, BasicMetricsData> pair in metricsByAOI)
                {
                    WriteTxtExportLine(sb, pair.Key, pair.Value);
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                Debug.Log($"[GAZE BASIC METRICS] Exported TXT report to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GAZE BASIC METRICS] Failed to export TXT report: {ex.Message}");
            }
        }

        // Exports the current metrics to CSV format for spreadsheets or later analysis
        public void ExportToCsv()
        {
            try
            {
                // Resolve/create the output directory
                string outputDirectory = EyeGazeUtils.GetOutputDirectory(useCustomOutputDirectory, customOutputDirectory);
                Directory.CreateDirectory(outputDirectory);

                // Build a CSV file name based on the same base naming logic
                string baseFileName = EyeGazeUtils.GetOutputFileName(
                    outputFileName,
                    "eye_gaze_basic_metrics",
                    generateTimestampedFileName
                );

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
                string csvFileName = fileNameWithoutExtension + ".csv";

                string filePath = Path.Combine(outputDirectory, csvFileName);

                StringBuilder sb = new StringBuilder();

                // Write CSV header using semicolon as separator for better Excel compatibility in Spanish locales
                if (includeInstanceId)
                {
                    sb.AppendLine("AOI_Label;AOI_Id;InstanceID;FB;TFF_Seconds;FD_Seconds;TFD_Seconds;FC;IsCurrentMetricsAOI");
                }
                else
                {
                    sb.AppendLine("AOI_Label;AOI_Id;FB;TFF_Seconds;FD_Seconds;TFD_Seconds;FC;IsCurrentMetricsAOI");
                }

                // Write one row per AOI
                foreach (KeyValuePair<EyeGazeAOI, BasicMetricsData> pair in metricsByAOI)
                {
                    WriteCsvExportLine(sb, pair.Key, pair.Value);
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                Debug.Log($"[GAZE BASIC METRICS] Exported CSV report to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GAZE BASIC METRICS] Failed to export CSV report: {ex.Message}");
            }
        }

        // Called automatically when the application is closing
        // Exports the enabled output formats if configured
        private void OnApplicationQuit()
        {
            // Finalize the active segment first so the latest data is not lost
            FinalizeCurrentSegment();

            if (!exportOnApplicationQuit)
            {
                return;
            }

            if (exportToTxt)
            {
                ExportToTxt();
            }

            if (exportToCsv)
            {
                ExportToCsv();
            }
        }

        // Writes a single TXT report row for a given AOI
        private void WriteTxtExportLine(StringBuilder sb, EyeGazeAOI aoi, BasicMetricsData data)
        {
            if (aoi == null)
            {
                return;
            }

            bool isCurrentAOI = aoi == currentMetricsAOI;

            string tffText = data.timeToFirstFixation >= 0f
                ? data.timeToFirstFixation.ToString("F6", CultureInfo.InvariantCulture)
                : "-1";

            string fdText = data.GetAverageFixationDuration().ToString("F6", CultureInfo.InvariantCulture);
            string tfdText = data.totalFixationDuration.ToString("F6", CultureInfo.InvariantCulture);

            if (includeInstanceId)
            {
                sb.AppendLine(
                    $"{aoi.AoiLabel}\t" +
                    $"{aoi.AoiId}\t" +
                    $"{aoi.GetInstanceID()}\t" +
                    $"{data.fixationsBefore}\t" +
                    $"{tffText}\t" +
                    $"{fdText}\t" +
                    $"{tfdText}\t" +
                    $"{data.fixationCount}\t" +
                    $"{(isCurrentAOI ? "1" : "0")}"
                );
            }
            else
            {
                sb.AppendLine(
                    $"{aoi.AoiLabel}\t" +
                    $"{aoi.AoiId}\t" +
                    $"{data.fixationsBefore}\t" +
                    $"{tffText}\t" +
                    $"{fdText}\t" +
                    $"{tfdText}\t" +
                    $"{data.fixationCount}\t" +
                    $"{(isCurrentAOI ? "1" : "0")}"
                );
            }
        }

        // Writes a single CSV report row for a given AOI
        private void WriteCsvExportLine(StringBuilder sb, EyeGazeAOI aoi, BasicMetricsData data)
        {
            if (aoi == null)
            {
                return;
            }

            bool isCurrentAOI = aoi == currentMetricsAOI;

            // Escape string values so labels/ids containing special characters remain valid CSV
            string aoiLabel = EscapeCsvValue(aoi.AoiLabel);
            string aoiId = EscapeCsvValue(aoi.AoiId);

            string tffText = data.timeToFirstFixation >= 0f
                ? data.timeToFirstFixation.ToString("F6", CultureInfo.InvariantCulture)
                : "-1";

            string fdText = data.GetAverageFixationDuration().ToString("F6", CultureInfo.InvariantCulture);
            string tfdText = data.totalFixationDuration.ToString("F6", CultureInfo.InvariantCulture);

            if (includeInstanceId)
            {
                sb.AppendLine(
                    $"{aoiLabel};" +
                    $"{aoiId};" +
                    $"{aoi.GetInstanceID()};" +
                    $"{data.fixationsBefore};" +
                    $"{tffText};" +
                    $"{fdText};" +
                    $"{tfdText};" +
                    $"{data.fixationCount};" +
                    $"{(isCurrentAOI ? "1" : "0")}"
                );
            }
            else
            {
                sb.AppendLine(
                    $"{aoiLabel};" +
                    $"{aoiId};" +
                    $"{data.fixationsBefore};" +
                    $"{tffText};" +
                    $"{fdText};" +
                    $"{tfdText};" +
                    $"{data.fixationCount};" +
                    $"{(isCurrentAOI ? "1" : "0")}"
                );
            }
        }

        // Escapes a string for CSV output by quoting it and escaping internal quotes
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            string escapedValue = value.Replace("\"", "\"\"");
            return $"\"{escapedValue}\"";
        }
    }
}