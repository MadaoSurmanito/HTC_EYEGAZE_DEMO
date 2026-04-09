# Estado del Arte: Seguimiento Ocular, Analítica de Áreas de Interés (AOI) y Biometría en Realidad Virtual Inmersiva

**Resumen:** El análisis del comportamiento visual humano en la Realidad Virtual (RV) está experimentando un cambio de paradigma. Con la estandarización de los marcos de desarrollo de la Realidad Extendida (XR) y la integración de sensores de seguimiento ocular nativos en visores autónomos, la investigación se aleja de soluciones propietarias hacia arquitecturas abiertas y modulares [1]. Este documento revisa el estado del arte dividido en cuatro capas fundamentales: plataforma de hardware, marco metodológico, panorama de herramientas actuales y la identificación de brechas arquitectónicas.

---

## 1. La Capa de Plataforma: La Naturaleza Dual de OpenXR y el Seguimiento Fisiológico

La base de la investigación moderna en XR se sustenta sobre el motor Unity y el estándar OpenXR. OpenXR democratiza el soporte de dispositivos al abstraer las especificidades del hardware tras una API unificada, proporcionando interacción estándar basada en la mirada a través de la extensión `XR_EXT_eye_gaze_interaction` [2].

Sin embargo, la literatura destaca una dicotomía crítica en la capa de plataforma en cuanto a la distinción entre la Dirección de la Mirada (_Eye Gaze_) y el Seguimiento Ocular Completo (_Eye Tracking_):

- **Mirada Orientada a la Interacción (El Estándar):** La implementación predeterminada de OpenXR aplica filtros de paso bajo y suavizado al vector de la mirada para evitar temblores en la interfaz de usuario. Esto enmascara los movimientos microsacádicos necesarios para la investigación fisiológica y omite datos biométricos [2].
- **Mirada y Biometría de Grado de Investigación:** Para eludir esta limitación, los fabricantes proporcionan SDKs avanzados. El _SDK VIVE OpenXR_ de HTC expone una interfaz de _Eye Tracker_ que produce datos crudos por ojo, diámetro pupilar (pupilometría) y apertura de los párpados [3]. Paradigmas similares se observan en las canalizaciones de datos de doble nivel del ecosistema Varjo [4].

## 2. La Capa Metodológica: Fijaciones 3D, Mapeo de AOIs y Carga Cognitiva

El desafío central es traducir datos espaciales crudos en métricas conductuales significativas, lo cual se divide en mapeo espacial y correlación fisiológica.

### 2.1 Mirada Espacial: Fijaciones y AOIs

A diferencia del seguimiento ocular en pantallas 2D, la RV introduce la coordinación cabeza-ojo, planos de profundidad dinámicos y oclusión. Esto hace que los algoritmos tradicionales de fijación 2D (como I-VT para velocidad o I-DT para dispersión [5]) sean complejos de implementar al requerir traducción de coordenadas 3D a grados de ángulo visual en tiempo real. Para resolver esto, las metodologías actuales se basan en el _Raycasting_ Vinculado a Objetos [6]. Al tratar los colisionadores 3D como Áreas de Interés (AOIs), el sistema registra una "Fijación de Objeto" (_Dwell Fixation_) si la intersección se mantiene de manera continua durante un tiempo umbral predefinido (ej. 150 ms).

### 2.2 Correlatos Fisiológicos: Pupilometría y Dinámica de Parpadeo

La literatura subraya la importancia de mapear estados oculares directamente a estímulos visuales específicos [7]. La dilatación pupilar es un indicador validado de carga cognitiva y excitación. La brecha metodológica reside en la sincronización de flujos: el estándar de investigación exige conocer el diámetro pupilar exacto _en el momento preciso_ en que el usuario fija la mirada en un AOI específico, no como un mero promedio temporal.

## 3. Panorama de Herramientas: Plataformas Comerciales vs. Investigación Abierta

Para comprender la necesidad de una arquitectura C# nativa, es preciso analizar las soluciones existentes, prestando especial atención a sus limitaciones en el cálculo de métricas en tiempo real.

### 3.1 Plataformas Comerciales (El "Gold Standard")

Estas plataformas dominan la investigación empresarial y clínica, pero presentan barreras metodológicas y económicas significativas para la ciencia abierta y ágil.

| Plataforma                     | Enfoque Principal                                                                                                                        | Limitaciones vs. El Sistema Propuesto                                                                                                                                                                                    |
| :----------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **iMotions VR** [8]            | Plataforma masiva de agregación biométrica que vincula AOIs de Unity a algoritmos automatizados de fijación y seguimiento pupilar.       | **Coste y Dependencia:** Fuertemente sujeta a licencias de pago. El análisis ocurre fuera de Unity en una app de escritorio; no es un módulo C# ligero in-engine.                                                        |
| **Cognitive3D** [9]            | Analítica espacial basada en la nube. Excepcional en el manejo de "AOIs Dinámicos" y generación de mapas de calor 3D.                    | **Dependencia de la Nube:** El procesamiento de métricas recae en servidores externos. La investigación local (_offline_) está altamente restringida.                                                                    |
| **Tobii XR SDK & Ocumen** [10] | Ecosistema de Tobii. El _XR SDK_ es gratuito, pero _Ocumen_ es la suite _premium_ con robustas canalizaciones de pupilometría y filtros. | **Datos Restringidos y _Lock-in_:** El SDK gratuito oculta el acceso a datos biométricos sin filtrar. Para investigación real se exige la licencia Ocumen (aprox. 1495 €/año por visor) y se restringe a hardware Tobii. |

### 3.2 Marcos de Investigación de Código Abierto

Estos proyectos, construidos desde el ámbito académico, suelen resolver problemas específicos de adquisición de datos, pero carecen de motores analíticos integrados.

| Proyecto             | Enfoque Principal                                                                                                | Limitaciones vs. El Sistema Propuesto                                                                                                                            |
| :------------------- | :--------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **TAUXR** [11]       | Plantilla de Unity para ejecutar experimentos con un registro de datos riguroso a alta frecuencia de fotogramas. | **Sin Métricas en Tiempo Real:** Solo registra vectores crudos en CSV, requiriendo scripts post-hoc (Python/R) para calcular el TFF o la duración de fijaciones. |
| **ORCL VR** [12]     | Demuestra cómo extraer datos crudos de OpenXR/Tobii en Unity para su serialización en XML.                       | **Falta de Modularidad:** Actúa como una herramienta de extracción, careciendo de submódulos desacoplados de seguimiento y cálculo de AOI.                       |
| **EDIA** [13]        | Se enfoca en estandarizar la configuración de la escena VR y la asignación de etiquetas semánticas.              | **Analítica Incompleta:** Carece de una arquitectura explícita de acumulación en tiempo real integrada en el _game loop_.                                        |
| **GazeMetrics** [14] | Valida la precisión (offset) y exactitud (jitter) del hardware del visor.                                        | **Sin Análisis Semántico:** Solo evalúa los sensores; no analiza contextos, lógicas de AOI ni métricas de mirada.                                                |
| **Pupil Labs** [15]  | Bloques modulares (_hmd-eyes_) para calibración e integración en Unity.                                          | **Automatización Limitada:** Muy centrado en su hardware _add-on_, delegando los reportes semánticos complejos al investigador.                                  |

## 4. Identificación de la Brecha Arquitectónica

El análisis de las herramientas revela un vacío claro en el ecosistema. Las opciones comerciales restringen los datos crudos tras barreras de pago o dependen de la nube, mientras que el software abierto delega la pesada carga computacional del cálculo de métricas al procesamiento posterior _offline_.

Surge la necesidad metodológica de una **arquitectura nativa en Unity (C#)** que opere de forma autónoma. Un sistema que procese datos espaciales y fisiológicos crudos a través de submódulos dedicados, calcule métricas de AOI (TFF, Duración Total de Fijación) en tiempo real y correlacione respuestas biométricas en el mismo instante, exportando informes tabulares localizados sin depender de canalizaciones de terceros ni incurrir en licencias restrictivas.

## 5. Innovaciones Arquitectónicas Propuestas

Para elevar el sistema propuesto y ofrecer un valor metodológico sin precedentes a la comunidad científica, este proyecto puede aspirar a implementar características inéditas en repositorios abiertos:

1.  **Biometría Sincronizada con AOIs (Pupilometría Contextual):** En lugar de registros continuos ciegos, el sistema incluye un módulo biométrico que computa la dilatación pupilar promedio y el parpadeo _exclusivamente_ durante la ventana activa de fijación sobre un objeto, enlazando directamente la carga cognitiva con la geometría visual.
2.  **Mirada Probabilística Volumétrica (Cone-Casting):** Sustituir el _raycast_ lineal por un volumen cónico para absorber el error de dispersión foveal y el _jitter_ del hardware. Esto permite calcular un "Índice de Confianza" que maneja la oclusión multiobjeto de forma nativa.
3.  **Jerarquías Semánticas Acumulativas:** Soporte para relaciones de colisionadores "Padre-Hijo" (ej. mirar la "Rueda" acumula simultáneamente tiempo de atención en la categoría semántica "Vehículo"), generando exportaciones ricas en contexto semántico.
4.  **Umbrales Dinámicos:** Autoajuste del umbral de permanencia basándose en la distancia virtual del objetivo y la velocidad de la cabeza del usuario, compensando el Reflejo Vestíbulo-Ocular (VOR) y reduciendo falsos negativos en elementos distantes.

---

## Referencias Bibliográficas

1. Clay, V., König, P., & König, S. U. (2019). _Eye tracking in virtual reality_. Journal of Eye Movement Research, 12(1). [Enlace](https://doi.org/10.16910/jemr.12.1.3)
2. Khronos Group. (2021). _OpenXR Specification: XR_EXT_eye_gaze_interaction_. [Enlace](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html)
3. HTC Corporation. (2023). _VIVE OpenXR XR HTC Eye Tracker SDK Documentation_. [Enlace](https://hub.vive.com/apidoc/api/VIVE.OpenXR.XR_HTC_eye_tracker.html)
4. Varjo Technologies. (2023). _Varjo XR Developer Documentation: Eye Tracking_. [Enlace](https://developer.varjo.com/docs/openxr/eye-tracking)
5. Salvucci, D. D., & Goldberg, J. H. (2000). _Identifying fixations and saccades in eye-tracking protocols_. ETRA.
6. Duchowski, A. T. (2017). _Eye Tracking Methodology: Theory and Practice_ (3rd ed.). Springer.
7. Eckstein, M. K., et al. (2017). _Beyond eye gaze: What else can eyetracking reveal about cognition and cognitive development?_. Developmental Cognitive Neuroscience, 25, 69-91.
8. iMotions A/S. (2024). _iMotions VR Integration & Biometric Research Platform_. [Enlace](https://imotions.com/products/imotions-lab/modules/eye-tracking-virtual-reality/)
9. Cognitive3D. (2024). _Spatial Analytics Platform Documentation for Unity_. [Enlace](https://cognitive3d.com/product/unity-analytics/)
10. Tobii AB. (2024). _Tobii Ocumen VR Developer Guide & XR SDK Licensing_. [Enlace](https://developer.tobii.com/tobii-pro/)
11. TAU XR Lab. (2023). _TAUXR Unity XR Toolkit_. GitHub. [Enlace](https://github.com/TAU-XR/TAUXR-Research-Template)
12. ORCL. (2022). _ORCL_VR_EyeTracking_. GitHub. [Enlace](https://github.com/XiangGuo1992/ORCL_VR_EyeTracking)
13. EDIA Framework. (2023). _Eye-tracking Data Integration App_.
14. Adhanom, I. B., et al. (2020). _GazeMetrics: An Open-Source Tool for Measuring the Data Quality of HMD-based Eye Trackers_. ETRA. [Enlace](https://github.com/isayasMatter/GazeMetrics)
15. Pupil Labs. (2024). _hmd-eyes: VR/AR Eye Tracking integration for Unity_. GitHub. [Enlace](https://github.com/pupil-labs/hmd-eyes)
