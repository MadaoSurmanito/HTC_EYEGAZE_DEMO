# State of the Art: Eye Tracking, Area of Interest (AOI) Analytics, and Biometrics in Immersive Virtual Reality

**Abstract:** Human visual behavior analysis in Virtual Reality (VR) is undergoing a paradigm shift. With the standardization of Extended Reality (XR) development frameworks and the integration of native eye-tracking sensors in standalone headsets, research is moving away from proprietary solutions towards open and modular architectures [1]. This document reviews the state of the art divided into four fundamental layers: hardware platform, methodological framework, current tool landscape, and the identification of architectural gaps.

---

## 1. The Platform Layer: The Dual Nature of OpenXR and Physiological Tracking

The foundation of modern XR research relies on the Unity engine and the OpenXR standard. OpenXR democratizes device support by abstracting hardware specifics behind a unified API, providing standard gaze-based interaction through the `XR_EXT_eye_gaze_interaction` extension [2].

However, the literature highlights a critical dichotomy in the platform layer regarding the distinction between Eye Gaze and Full Eye Tracking:

* **Interaction-Oriented Gaze (The Standard):** The default OpenXR implementation applies low-pass filters and smoothing to the gaze vector to prevent UI jitter. This masks the microsaccadic movements necessary for physiological research and omits biometric data [2].
* **Research-Grade Gaze and Biometrics:** To bypass this limitation, manufacturers provide advanced SDKs. HTC's *VIVE OpenXR SDK* exposes an *Eye Tracker* interface that outputs raw data per eye, pupil diameter (pupillometry), and eyelid openness [3]. Similar paradigms are observed in the dual-tier data pipelines of the Varjo ecosystem [4].

## 2. The Methodological Layer: 3D Fixations, AOI Mapping, and Cognitive Load

The central challenge is translating raw spatial data into meaningful behavioral metrics, which is divided into spatial mapping and physiological correlation.

### 2.1 Spatial Gaze: Fixations and AOIs
Unlike eye tracking on 2D screens, VR introduces head-eye coordination, dynamic depth planes, and occlusion. This makes traditional 2D fixation algorithms (such as I-VT for velocity or I-DT for dispersion [5]) complex to implement as they require translating 3D coordinates to degrees of visual angle in real time. To solve this, current methodologies rely on Object-Linked Raycasting [6]. By treating 3D colliders as Areas of Interest (AOIs), the system registers a "Dwell Fixation" if the intersection is maintained continuously for a predefined threshold time (e.g., 150 ms).

### 2.2 Physiological Correlates: Pupillometry and Blink Dynamics
The literature underscores the importance of mapping ocular states directly to specific visual stimuli [7]. Pupil dilation is a validated indicator of cognitive load and arousal. The methodological gap lies in synchronizing data streams: the research standard requires knowing the exact pupil diameter *at the precise moment* the user fixes their gaze on a specific AOI, rather than as a mere temporal average.

## 3. Tool Landscape: Commercial Platforms vs. Open Research

To understand the need for a native C# architecture, existing solutions must be analyzed, paying special attention to their limitations regarding real-time metric calculation.

### 3.1 Commercial Platforms (The "Gold Standard")
These platforms dominate enterprise and clinical research but present significant methodological and economic barriers for open and agile science.

| Platform | Core Focus | Limitations vs. Proposed System |
| :--- | :--- | :--- |
| **iMotions VR** [8] | Massive biometric aggregation platform that links Unity AOIs to automated fixation and pupillary tracking algorithms. | **Cost and Dependency:** Strictly tied to paid licenses. Analysis occurs outside Unity in a desktop app; it is not a lightweight, in-engine C# module. |
| **Cognitive3D** [9] | Cloud-based spatial analytics. Exceptional at handling "Dynamic AOIs" and generating 3D heatmaps. | **Cloud Dependency:** Metric processing relies on external servers. Local (*offline*) research is highly restricted. |
| **Tobii XR SDK & Ocumen** [10] | Tobii's ecosystem. The *XR SDK* is free to access, but *Ocumen* is the premium suite with robust pupillometry pipelines and filters. | **Restricted Data & Lock-in:** The free SDK hides access to unfiltered biometric data. Real research requires the Ocumen license (approx. €1495/year per headset) and restricts you to Tobii hardware. |

### 3.2 Open-Source Research Frameworks
These projects, built by academia, usually solve specific data acquisition problems but lack integrated analytical engines.

| Project | Core Focus | Limitations vs. Proposed System |
| :--- | :--- | :--- |
| **TAUXR** [11] | Unity template for running experiments with rigorous, high-framerate data logging. | **No Real-Time Metrics:** Only logs raw vectors to CSV, requiring post-hoc scripts (Python/R) to calculate Time to First Fixation (TFF) or fixation durations. |
| **ORCL VR** [12] | Demonstrates how to extract raw data from OpenXR/Tobii in Unity for XML serialization. | **Lack of Modularity:** Acts as an extraction tool, lacking decoupled tracking and AOI calculation submodules. |
| **EDIA** [13] | Focuses on standardizing VR scene setup and semantic label assignment. | **Incomplete Analytics:** Lacks an explicit real-time accumulation architecture integrated into the game loop. |
| **GazeMetrics** [14] | Validates hardware precision (offset) and accuracy (jitter) of the headset. | **No Semantic Analysis:** Only evaluates sensors; does not analyze contexts, AOI logic, or gaze metrics. |
| **Pupil Labs** [15] | Modular blocks (*hmd-eyes*) for calibration and Unity integration. | **Limited Automation:** Highly focused on their add-on hardware, delegating complex semantic reporting to the researcher. |

## 4. Identification of the Architectural Gap

The analysis of the tools reveals a clear void in the ecosystem. Commercial options restrict raw data behind paywalls or rely on the cloud, while open-source software delegates the heavy computational load of metric calculation to *offline* post-processing.

There is a methodological need for a **native Unity architecture (C#)** that operates autonomously. A system that processes raw spatial and physiological data through dedicated submodules, calculates AOI metrics (TFF, Total Fixation Duration) in real-time, and simultaneously correlates biometric responses, exporting localized tabular reports without relying on third-party pipelines or incurring restrictive licenses.

## 5. Proposed Architectural Innovations

To elevate the proposed system and offer unprecedented methodological value to the scientific community, this project aims to implement novel features in open repositories:

1.  **Biometrics Synchronized with AOIs (Contextual Pupillometry):** Instead of blind continuous logging, the system includes a biometric module that computes average pupil dilation and blink rate *exclusively* during the active fixation window on an object, directly linking cognitive load with visual geometry.
2.  **Volumetric Probabilistic Gaze (Cone-Casting):** Replacing linear raycasting with a conical volume to absorb foveal dispersion error and hardware jitter. This enables calculating a "Confidence Index" that natively handles multi-object occlusion.
3.  **Cumulative Semantic Hierarchies:** Support for "Parent-Child" collider relationships (e.g., looking at a "Wheel" simultaneously accumulates attention time in the "Vehicle" semantic category), generating context-rich semantic exports.
4.  **Dynamic Thresholds:** Self-adjusting dwell thresholds based on the target's virtual distance and the user's head velocity, compensating for the Vestibulo-Ocular Reflex (VOR) and reducing false negatives on distant elements.

---

## References

1. Clay, V., König, P., & König, S. U. (2019). *Eye tracking in virtual reality*. Journal of Eye Movement Research, 12(1). [Link](https://doi.org/10.16910/jemr.12.1.3)
2. Khronos Group. (2021). *OpenXR Specification: XR_EXT_eye_gaze_interaction*. [Link](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html)
3. HTC Corporation. (2023). *VIVE OpenXR XR HTC Eye Tracker SDK Documentation*. [Link](https://hub.vive.com/apidoc/api/VIVE.OpenXR.XR_HTC_eye_tracker.html)
4. Varjo Technologies. (2023). *Varjo XR Developer Documentation: Eye Tracking*. [Link](https://developer.varjo.com/docs/openxr/eye-tracking)
5. Salvucci, D. D., & Goldberg, J. H. (2000). *Identifying fixations and saccades in eye-tracking protocols*. ETRA.
6. Duchowski, A. T. (2017). *Eye Tracking Methodology: Theory and Practice* (3rd ed.). Springer.
7. Eckstein, M. K., et al. (2017). *Beyond eye gaze: What else can eyetracking reveal about cognition and cognitive development?*. Developmental Cognitive Neuroscience, 25, 69-91.
8. iMotions A/S. (2024). *iMotions VR Integration & Biometric Research Platform*. [Link](https://imotions.com/products/imotions-lab/modules/eye-tracking-virtual-reality/)
9. Cognitive3D. (2024). *Spatial Analytics Platform Documentation for Unity*. [Link](https://cognitive3d.com/product/unity-analytics/)
10. Tobii AB. (2024). *Tobii Ocumen VR Developer Guide & XR SDK Licensing*. [Link](https://developer.tobii.com/tobii-pro/)
11. TAU XR Lab. (2023). *TAUXR Unity XR Toolkit*. GitHub. [Link](https://github.com/TAU-XR/TAUXR-Research-Template)
12. ORCL. (2022). *ORCL_VR_EyeTracking*. GitHub. [Link](https://github.com/XiangGuo1992/ORCL_VR_EyeTracking)
13. EDIA Framework. (2023). *Eye-tracking Data Integration App*. 
14. Adhanom, I. B., et al. (2020). *GazeMetrics: An Open-Source Tool for Measuring the Data Quality of HMD-based Eye Trackers*. ETRA. [Link](https://github.com/isayasMatter/GazeMetrics)
15. Pupil Labs. (2024). *hmd-eyes: VR/AR Eye Tracking integration for Unity*. GitHub. [Link](https://github.com/pupil-labs/hmd-eyes)