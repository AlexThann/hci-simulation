using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ScenarioEngine : MonoBehaviour
{
    [Header("Scenario Source")]
    [SerializeField] private string scenarioFileName = "icu_scenario_hypoxia_v1";
    [SerializeField] private string scenariosFolderName = "scenarios";

    [Header("Scene References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ToastManager toastManager;
    [SerializeField] private HUDController hudController;
    [SerializeField] private LogManager logManager;
    [SerializeField] private EHRFormManager ehrFormManager;
    [SerializeField] private Canvas gameplayCanvas;

    private ScenarioUIRuntime runtimeUI;
    private ScenarioData scenario;
    private ScenarioNode currentNode;
    private readonly Dictionary<string, ScenarioNode> nodeLookup = new Dictionary<string, ScenarioNode>();
    private readonly Dictionary<string, ScenarioHotspot> hotspotLookup = new Dictionary<string, ScenarioHotspot>();
    private readonly HashSet<string> activeHotspotIds = new HashSet<string>();
    private readonly List<string> decisionPath = new List<string>();
    private readonly HashSet<string> completedDocumentationFlags = new HashSet<string>();

    private ScenarioVitalsSnapshot currentVitals;
    private int currentScore;
    private Dictionary<string, bool> currentFlags = new Dictionary<string, bool>();

    private ScenarioNode activeGateNode;
    private ScenarioGateRequirements activeGateRequirements;
    private Coroutine timeoutRoutine;
    private bool awaitingDecision;
    private bool primaryAvailable;
    private readonly List<ScenarioOption> currentOptions = new List<ScenarioOption>();

    private void Awake()
    {
        if (!uiManager) uiManager = GetComponent<UIManager>();
        if (!toastManager) toastManager = GetComponent<ToastManager>();
        if (!hudController) hudController = GetComponent<HUDController>();
        if (!logManager) logManager = GetComponent<LogManager>();
        if (!ehrFormManager) ehrFormManager = GetComponent<EHRFormManager>();

        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.scenarioEngine = this;
        }
    }

    private void Start()
    {
        if (!gameplayCanvas)
        {
#if UNITY_2023_1_OR_NEWER
            gameplayCanvas = FindFirstObjectByType<Canvas>();
#else
            gameplayCanvas = FindObjectOfType<Canvas>();
#endif
        }

        if (!gameplayCanvas)
        {
            Debug.LogError("ScenarioEngine: Unable to locate a Canvas for runtime UI.");
            enabled = false;
            return;
        }

        runtimeUI = new ScenarioUIRuntime(gameplayCanvas);
        runtimeUI.Show();

        if (ehrFormManager != null)
        {
            ehrFormManager.FormSaved += HandleFormSaved;
        }

        TryStartScenario();
    }

    private void Update()
    {
        if (runtimeUI == null)
        {
            return;
        }

        if (primaryAvailable && runtimeUI.HasPrimaryAction && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            runtimeUI.TriggerPrimary();
        }

        if (awaitingDecision && currentNode != null && currentOptions.Count > 0)
        {
            for (int i = 0; i < currentOptions.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    var option = currentOptions[i];
                    if (option != null)
                    {
                        SelectOption(currentNode, option);
                    }

                    break;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (ehrFormManager != null)
        {
            ehrFormManager.FormSaved -= HandleFormSaved;
        }
    }

    private void TryStartScenario()
    {
        try
        {
            LoadScenario();
            InitializeRuntimeState();
            EnterNode(scenario.Nodes.FirstOrDefault());
        }
        catch (Exception ex)
        {
            Debug.LogError($"ScenarioEngine: Failed to start scenario - {ex.Message}\n{ex.StackTrace}");
            toastManager?.ShowToast("Scenario failed to load. Check console for details.");
            runtimeUI?.Hide();
        }
    }

    private void LoadScenario()
    {
        var path = ResolveScenarioPath(scenarioFileName);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            throw new FileNotFoundException($"Scenario file not found for '{scenarioFileName}'. Searched path: {path ?? "<null>"}");
        }

        var json = File.ReadAllText(path);
        scenario = JsonConvert.DeserializeObject<ScenarioData>(json);

        nodeLookup.Clear();
        hotspotLookup.Clear();

        if (scenario?.Nodes != null)
        {
            foreach (var node in scenario.Nodes)
            {
                if (!string.IsNullOrWhiteSpace(node.Id))
                {
                    nodeLookup[node.Id] = node;
                }
            }
        }

        if (scenario?.Hotspots != null)
        {
            foreach (var hotspot in scenario.Hotspots)
            {
                if (!string.IsNullOrWhiteSpace(hotspot.Id))
                {
                    hotspotLookup[hotspot.Id] = hotspot;
                }
            }
        }

        LogEvent("SCENARIO_LOAD", scenario?.ScenarioMeta?.Id ?? scenarioFileName);
    }

    private string ResolveScenarioPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".json";
        }

        var candidatePaths = new List<string>();

        if (!string.IsNullOrEmpty(Application.streamingAssetsPath))
        {
            candidatePaths.Add(Path.Combine(Application.streamingAssetsPath, scenariosFolderName, fileName));
        }

        candidatePaths.Add(Path.Combine(Application.dataPath, "..", "icu_json_scenarios", fileName));
        candidatePaths.Add(Path.Combine(Application.dataPath, scenariosFolderName, fileName));

        foreach (var candidate in candidatePaths)
        {
            var normalized = Path.GetFullPath(candidate);
            if (File.Exists(normalized))
            {
                return normalized;
            }
        }

        // fallback to first candidate (even if missing) for diagnostic purposes
        return candidatePaths.FirstOrDefault();
    }

    private void InitializeRuntimeState()
    {
        if (scenario == null || scenario.InitialState == null)
        {
            throw new InvalidOperationException("Scenario data missing initial state.");
        }

        currentScore = scenario.InitialState.CurrentScore;
        currentFlags = scenario.InitialState.Flags != null
            ? new Dictionary<string, bool>(scenario.InitialState.Flags)
            : new Dictionary<string, bool>();

        if (scenario.InitialState.Vitals == null)
        {
            scenario.InitialState.Vitals = new ScenarioVitalsSnapshot
            {
                HeartRate = 80,
                OxygenSaturation = 96,
                RespiratoryRate = 18,
                BloodPressure = "120/80",
                Temperature = 36.8f
            };
        }

        currentVitals = new ScenarioVitalsSnapshot
        {
            HeartRate = scenario.InitialState.Vitals.HeartRate,
            OxygenSaturation = scenario.InitialState.Vitals.OxygenSaturation,
            RespiratoryRate = scenario.InitialState.Vitals.RespiratoryRate,
            BloodPressure = scenario.InitialState.Vitals.BloodPressure,
            Temperature = scenario.InitialState.Vitals.Temperature
        };

        ApplyVitalsToHUD();
        runtimeUI?.SetScore(currentScore);

        activeHotspotIds.Clear();
        if (scenario.InitialState.UI?.ActiveHotspots != null)
        {
            foreach (var id in scenario.InitialState.UI.ActiveHotspots)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    activeHotspotIds.Add(id);
                }
            }
        }

        decisionPath.Clear();
        completedDocumentationFlags.Clear();

        runtimeUI?.SetHeader(scenario.ScenarioMeta?.Title ?? "Scenario");
        runtimeUI?.SetInfo(scenario.ScenarioMeta?.Description);
        runtimeUI?.SetTimer(null);
    }

    private void EnterNode(ScenarioNode node)
    {
        if (node == null)
        {
            Debug.LogWarning("ScenarioEngine: Attempted to enter a null node.");
            return;
        }

        currentNode = node;
        awaitingDecision = false;
        activeGateNode = null;
        activeGateRequirements = null;
        primaryAvailable = false;
        currentOptions.Clear();

        if (timeoutRoutine != null)
        {
            StopCoroutine(timeoutRoutine);
            timeoutRoutine = null;
        }

        runtimeUI?.Show();
        runtimeUI?.SetBody(node.Text);
        runtimeUI?.SetTimer(null);

        LogEvent("NODE_ENTER", node.Id);

        switch (node.Type)
        {
            case "message":
                runtimeUI?.ShowPrimary("Continue", () => Advance(node.NextNodeId));
                runtimeUI?.SetInfo(node.Description);
                primaryAvailable = true;
                break;
            case "decision":
                ShowDecisionNode(node);
                break;
            case "gate":
                ActivateGate(node);
                break;
            case "end":
                CompleteScenario(node);
                break;
            default:
                Debug.LogWarning($"ScenarioEngine: Unsupported node type '{node.Type}'. Advancing by default.");
                Advance(node.NextNodeId);
                break;
        }

        EvaluateGlobalRules();
    }

    private void ShowDecisionNode(ScenarioNode node)
    {
        if (node.Options == null || node.Options.Count == 0)
        {
            Debug.LogWarning($"ScenarioEngine: Decision node '{node.Id}' has no options. Auto-advancing.");
            currentOptions.Clear();
            Advance(node.NextNodeId);
            return;
        }

        currentOptions.Clear();
        currentOptions.AddRange(node.Options);
        awaitingDecision = true;
        primaryAvailable = false;
        runtimeUI?.SetInfo(node.Description);

        var optionViews = new List<ScenarioUIRuntime.OptionViewData>();
        for (int i = 0; i < node.Options.Count; i++)
        {
            var option = node.Options[i];
            var prefix = node.Options.Count > 1 ? $"{i + 1}. " : string.Empty;
            var color = DetermineOptionColor(option?.Effects?.ScoreDelta);
            optionViews.Add(new ScenarioUIRuntime.OptionViewData
            {
                Label = prefix + (option?.Label ?? "Option"),
                Callback = () => SelectOption(node, option),
                Color = color
            });
        }

        runtimeUI?.ShowOptions(optionViews);

        if (node.Timeout != null && node.Timeout.Seconds > 0)
        {
            timeoutRoutine = StartCoroutine(DecisionTimeout(node));
        }
    }

    private void ActivateGate(ScenarioNode node)
    {
        activeGateNode = node;
        activeGateRequirements = node.GateRequirements;

        runtimeUI?.SetInfo(node.Description);
        runtimeUI?.ShowPrimary("Check Gate", CheckActiveGate);
        primaryAvailable = true;

        if (!string.IsNullOrWhiteSpace(node.GateRequirements?.TargetHotspot))
        {
            OpenHotspotPanel(node.GateRequirements.TargetHotspot);
        }

        if (!string.IsNullOrWhiteSpace(node.FeedbackBlocked))
        {
            toastManager?.ShowToast(node.FeedbackBlocked, 2.5f);
        }
    }

    private void CheckActiveGate()
    {
        if (activeGateNode == null || activeGateRequirements == null)
        {
            return;
        }

        var missing = CollectMissingDocumentation(activeGateRequirements);

        if (missing.Count > 0)
        {
            var message = string.Join("\n", missing.Select(m => "• " + m));
            runtimeUI?.SetInfo(message);
            toastManager?.ShowToast(activeGateNode.FeedbackBlocked ?? "Documentation incomplete.", 2.5f);
            LogEvent("GATE_BLOCKED", string.Join(",", missing));
            runtimeUI?.SetPrimaryLabel("Re-check Gate");
            return;
        }

        runtimeUI?.SetInfo(activeGateNode.FeedbackSuccess ?? "Gate passed.");
        toastManager?.ShowToast(activeGateNode.FeedbackSuccess ?? "Gate passed.");
        LogEvent("GATE_PASSED", activeGateNode.Id);

        ApplyEffects(activeGateNode.EffectsOnPass, "GATE_PASS");

        if (activeGateNode.EffectsOnPass?.StateUpdate != null)
        {
            ExtractDocumentationFlags(activeGateNode.EffectsOnPass.StateUpdate);
        }

        var next = activeGateNode.NextNodeId;
        activeGateNode = null;
        activeGateRequirements = null;
        Advance(next);
    }

    private void SelectOption(ScenarioNode node, ScenarioOption option)
    {
        if (!awaitingDecision || node != currentNode)
        {
            return;
        }

        awaitingDecision = false;
        if (timeoutRoutine != null)
        {
            StopCoroutine(timeoutRoutine);
            timeoutRoutine = null;
        }

        runtimeUI?.SetTimer(null);
        runtimeUI?.ClearOptions();
        currentOptions.Clear();

        var label = option?.Label ?? "Option";
        decisionPath.Add(label);

        LogEvent("OPTION_SELECTED", option?.Id ?? label);

        if (!string.IsNullOrWhiteSpace(option?.TargetHotspot))
        {
            OpenHotspotPanel(option.TargetHotspot);
        }

        ApplyEffects(option?.Effects, option?.Id);

        Advance(option?.NextNodeId ?? node.NextNodeId);
    }

    private Color? DetermineOptionColor(int? scoreDelta)
    {
        if (!scoreDelta.HasValue)
        {
            return null;
        }

        if (scoreDelta.Value > 0)
        {
            return new Color(0.23f, 0.56f, 0.32f, 0.95f);
        }

        if (scoreDelta.Value < 0)
        {
            return new Color(0.64f, 0.2f, 0.2f, 0.95f);
        }

        return null;
    }

    private IEnumerator DecisionTimeout(ScenarioNode node)
    {
        var seconds = Mathf.Max(1, node.Timeout.Seconds);
        var endTime = Time.time + seconds;

        while (Time.time < endTime && awaitingDecision && currentNode == node)
        {
            var remaining = Mathf.CeilToInt(endTime - Time.time);
            runtimeUI?.SetTimer($"Timeout in {remaining}s");
            yield return null;
        }

        runtimeUI?.SetTimer(null);

        if (!awaitingDecision || currentNode != node)
        {
            yield break;
        }

        awaitingDecision = false;
        ApplyEffects(node.Timeout?.OnTimeoutEffects, "TIMEOUT");
        LogEvent("NODE_TIMEOUT", node.Id);
        Advance(node.Timeout?.NextNodeId ?? node.NextNodeId);
    }

    private void Advance(string nextNodeId)
    {
        if (string.IsNullOrWhiteSpace(nextNodeId))
        {
            primaryAvailable = false;
            runtimeUI?.Hide();
            return;
        }

        if (!nodeLookup.TryGetValue(nextNodeId, out var node))
        {
            Debug.LogWarning($"ScenarioEngine: Unable to find node '{nextNodeId}'. Scenario will stop.");
            primaryAvailable = false;
            runtimeUI?.Hide();
            return;
        }

        EnterNode(node);
    }

    private void ApplyEffects(ScenarioEffects effects, string context)
    {
        if (effects == null)
        {
            return;
        }

        if (effects.ScoreDelta.HasValue)
        {
            currentScore += effects.ScoreDelta.Value;
            runtimeUI?.SetScore(currentScore);
            LogEvent("SCORE_UPDATE", $"{context}:{effects.ScoreDelta.Value}");
        }

        if (effects.VitalsUpdate != null)
        {
            ApplyVitalsUpdate(effects.VitalsUpdate);
            LogEvent("VITALS_CHANGE", context);
        }

        if (effects.StateUpdate != null)
        {
            ApplyStateUpdate(effects.StateUpdate);
        }

        if (!string.IsNullOrWhiteSpace(effects.Toast))
        {
            toastManager?.ShowToast(effects.Toast, 2.4f);
        }

        if (effects.AdditionalEffects != null)
        {
            foreach (var extra in effects.AdditionalEffects)
            {
                ApplyRuleEffect(extra, context);
            }
        }

        EvaluateGlobalRules();
    }

    private void ApplyStateUpdate(JObject stateUpdate)
    {
        foreach (var property in stateUpdate)
        {
            var key = property.Key;
            var token = property.Value;

            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (key.StartsWith("flags.", StringComparison.OrdinalIgnoreCase))
            {
                var flagKey = key.Substring(6);
                var value = token.Type switch
                {
                    JTokenType.Boolean => token.Value<bool>(),
                    JTokenType.Integer => token.Value<int>() != 0,
                    JTokenType.Float => Math.Abs(token.Value<float>()) > float.Epsilon,
                    JTokenType.String => string.Equals(token.Value<string>(), "true", StringComparison.OrdinalIgnoreCase),
                    _ => false
                };

                currentFlags[flagKey] = value;
                if (value)
                {
                    completedDocumentationFlags.Add(flagKey);
                }

                LogEvent("FLAG_UPDATE", $"{flagKey}:{value}");
            }
        }
    }

    private void ExtractDocumentationFlags(JObject stateUpdate)
    {
        foreach (var property in stateUpdate)
        {
            if (property.Key.StartsWith("flags.", StringComparison.OrdinalIgnoreCase) && property.Value.Type == JTokenType.Boolean && property.Value.Value<bool>())
            {
                var flagKey = property.Key.Substring(6);
                completedDocumentationFlags.Add(flagKey);
            }
        }
    }

    private void ApplyVitalsUpdate(ScenarioVitalsUpdate update)
    {
        if (update.HeartRate.HasValue)
        {
            currentVitals.HeartRate = update.HeartRate.Value;
        }

        if (update.OxygenSaturation.HasValue)
        {
            currentVitals.OxygenSaturation = update.OxygenSaturation.Value;
        }

        if (update.RespiratoryRate.HasValue)
        {
            currentVitals.RespiratoryRate = update.RespiratoryRate.Value;
        }

        if (!string.IsNullOrWhiteSpace(update.BloodPressure))
        {
            currentVitals.BloodPressure = update.BloodPressure;
        }

        if (update.Temperature.HasValue)
        {
            currentVitals.Temperature = update.Temperature.Value;
        }

        ApplyVitalsToHUD();
    }

    private void ApplyVitalsToHUD()
    {
        hudController?.UpdateVitals(
            currentVitals.HeartRate,
            currentVitals.OxygenSaturation,
            currentVitals.RespiratoryRate,
            currentVitals.BloodPressure,
            currentVitals.Temperature
        );
    }

    private void EvaluateGlobalRules()
    {
        if (scenario?.Rules?.GlobalRules == null)
        {
            return;
        }

        foreach (var rule in scenario.Rules.GlobalRules)
        {
            if (RuleConditionSatisfied(rule.Condition))
            {
                foreach (var effect in rule.Effects)
                {
                    ApplyRuleEffect(effect, rule.Id);
                }
            }
        }
    }

    private bool RuleConditionSatisfied(Dictionary<string, ScenarioComparison> condition)
    {
        if (condition == null || condition.Count == 0)
        {
            return true;
        }

        foreach (var kvp in condition)
        {
            var value = ResolveValueByPath(kvp.Key);
            if (!CompareValue(value, kvp.Value))
            {
                return false;
            }
        }

        return true;
    }

    private object ResolveValueByPath(string path)
    {
        switch (path)
        {
            case "vitals.hr":
                return currentVitals.HeartRate;
            case "vitals.spo2":
                return currentVitals.OxygenSaturation;
            case "vitals.rr":
                return currentVitals.RespiratoryRate;
            case "vitals.temp":
                return currentVitals.Temperature;
            case "flags.assessment_complete":
                return currentFlags.TryGetValue("assessment_complete", out var assess) && assess;
            default:
                if (path.StartsWith("flags.", StringComparison.OrdinalIgnoreCase))
                {
                    var key = path.Substring(6);
                    return currentFlags.TryGetValue(key, out var flag) && flag;
                }

                return null;
        }
    }

    private bool CompareValue(object value, ScenarioComparison comparison)
    {
        if (comparison == null)
        {
            return true;
        }

        if (value == null)
        {
            return false;
        }

        double numeric;

        if (value is bool booleanValue)
        {
            numeric = booleanValue ? 1 : 0;
        }
        else if (value is IConvertible convertible)
        {
            numeric = convertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            return false;
        }

        if (comparison.Equal.HasValue && Math.Abs(numeric - comparison.Equal.Value) > double.Epsilon)
        {
            return false;
        }

        if (comparison.LessThan.HasValue && !(numeric < comparison.LessThan.Value))
        {
            return false;
        }

        if (comparison.LessThanOrEqual.HasValue && !(numeric <= comparison.LessThanOrEqual.Value))
        {
            return false;
        }

        if (comparison.GreaterThan.HasValue && !(numeric > comparison.GreaterThan.Value))
        {
            return false;
        }

        if (comparison.GreaterThanOrEqual.HasValue && !(numeric >= comparison.GreaterThanOrEqual.Value))
        {
            return false;
        }

        return true;
    }

    private void ApplyRuleEffect(ScenarioRuleEffect effect, string origin)
    {
        if (effect == null)
        {
            return;
        }

        switch (effect.Type)
        {
            case "ui_toast":
                if (!string.IsNullOrWhiteSpace(effect.Message))
                {
                    toastManager?.ShowToast(effect.Message, 2.4f);
                }
                break;
            case "ui_visual":
                // Potential hook for visual feedback
                LogEvent("UI_VISUAL", $"{effect.Target}:{effect.State}");
                break;
            default:
                LogEvent("RULE_EFFECT", effect.Type ?? "unknown");
                break;
        }
    }

    private List<string> CollectMissingDocumentation(ScenarioGateRequirements gate)
    {
        var missing = new List<string>();

        if (gate?.RequiredForms == null)
        {
            return missing;
        }

        foreach (var form in gate.RequiredForms)
        {
            if (form?.Fields == null)
            {
                continue;
            }

            foreach (var field in form.Fields)
            {
                if (!ehrFormManager.HasField(field))
                {
                    missing.Add($"{form.FormId}: {field}");
                }
            }
        }

        return missing;
    }

    private void HandleFormSaved(string formId)
    {
        LogEvent("EHR_SUBMIT", formId);

        if (activeGateNode != null)
        {
            CheckActiveGate();
        }
    }

    private void CompleteScenario(ScenarioNode node)
    {
        ApplyEffects(node.EffectsOnPass, "END_NODE");

        var summaryLines = new List<string>();

        if (node.DebriefConfig?.ShowScore == true)
        {
            summaryLines.Add($"Τελικό Σκορ: {currentScore}");
        }

        if (node.DebriefConfig?.ShowDecisionPath == true && decisionPath.Count > 0)
        {
            summaryLines.Add("Διαδρομή Αποφάσεων:");
            summaryLines.AddRange(decisionPath.Select((step, index) => $"  {index + 1}. {step}"));
        }

        if (node.DebriefConfig?.HighlightMissedDocs == true)
        {
            var missingDocs = currentFlags
                .Where(flag => flag.Key.Contains("documentation", StringComparison.OrdinalIgnoreCase) && !flag.Value)
                .Select(flag => flag.Key)
                .Except(completedDocumentationFlags)
                .ToList();

            if (missingDocs.Count > 0)
            {
                summaryLines.Add("Εκκρεμείς Τεκμηριώσεις:");
                summaryLines.AddRange(missingDocs.Select(doc => "  - " + doc));
            }
        }

        runtimeUI?.SetBody(node.Text);
        runtimeUI?.SetInfo(string.Join("\n", summaryLines));
        runtimeUI?.ShowPrimary("Close Scenario", () => runtimeUI.Hide());
        primaryAvailable = true;

        if (node.DebriefConfig?.ExportLog == true)
        {
            ExportLogs();
        }

        LogEvent("SCENARIO_COMPLETE", scenario?.ScenarioMeta?.Id ?? scenarioFileName);
    }

    public void HandleHotspotInteraction(HotspotType hotspotType)
    {
        var hotspotId = ResolveHotspotId(hotspotType);
        if (string.IsNullOrWhiteSpace(hotspotId))
        {
            return;
        }

        LogEvent("HOTSPOT_INTERACTION", hotspotId);

        if (!activeHotspotIds.Contains(hotspotId))
        {
            toastManager?.ShowToast("Hotspot currently inactive for scenario", 1.8f);
        }
    }

    private void OpenHotspotPanel(string hotspotId)
    {
        if (string.IsNullOrWhiteSpace(hotspotId) || uiManager == null)
        {
            return;
        }

        switch (hotspotId)
        {
            case "hs_ehr":
                uiManager.OpenEHR();
                break;
            case "hs_monitor":
                uiManager.OpenVitals();
                break;
            case "hs_patient":
                uiManager.OpenPatient();
                break;
            case "hs_ventilator":
                uiManager.OpenVentilator();
                break;
            case "hs_call":
                uiManager.OpenCallSystem();
                break;
            default:
                break;
        }
    }

    private string ResolveHotspotId(HotspotType type)
    {
        return type switch
        {
            HotspotType.EHR => "hs_ehr",
            HotspotType.Monitor => "hs_monitor",
            HotspotType.Patient => "hs_patient",
            HotspotType.Ventilator => "hs_ventilator",
            HotspotType.CallSystem => "hs_call",
            _ => null
        };
    }

    private void LogEvent(string eventType, string details)
    {
        if (scenario?.Logging?.Enabled == true)
        {
            if (scenario.Logging.LogEvents == null || scenario.Logging.LogEvents.Count == 0 || scenario.Logging.LogEvents.Contains(eventType))
            {
                logManager?.Log(eventType, details ?? string.Empty);
            }
        }
    }

    private void ExportLogs()
    {
        if (logManager == null)
        {
            return;
        }

        var format = scenario?.Logging?.ExportFormat;
        switch (format)
        {
            case "JSON":
            case "json":
                logManager.ExportLogsToJson();
                break;
            case "CSV":
            case "csv":
                logManager.ExportLogsToCsv();
                break;
            default:
                logManager.ExportLogsToTxt();
                break;
        }
    }
}
